import * as THREE from 'three';
import { DecalGeometry } from 'three/addons/geometries/DecalGeometry.js';
import type { AssetPose, BackPoint, BackSheet, StickerSize } from '../kiosk/sheet';
import { backToWorld, footprintOnSheet, normalizeRotation, stickerCovers, worldToBack, type PhoneNest } from '../kiosk/sheet';
import { canonicalize, loadScene, loadTexture, seatPhone, type Fitted } from './canonical';
import { createViewer } from './viewer';

export interface StagePlacement {
  readonly key: string;
  readonly imageUrl: string;
  readonly widthMm: number;
  readonly heightMm: number;
  readonly at: BackPoint;
  readonly rotationDeg: number;
  readonly active: boolean;
}

export interface StageView {
  readonly caseModel: { readonly url: string; readonly pose: AssetPose; readonly sheet: BackSheet; readonly nest: PhoneNest };
  readonly phone: { readonly url: string; readonly pose: AssetPose } | null;
  readonly placements: readonly StagePlacement[];
  /** False on home and receipt pages: slow turn, no editing. */
  readonly editable: boolean;
}

export interface StageHooks {
  onCommit(key: string, at: BackPoint): void;
  onNudge(dx: number, dy: number): void;
  onRotateTo?(key: string, rotationDeg: number): void;
  onSelect?(key: string): void;
  onReject(): void;
  onStatus(status: 'loading' | 'ready' | 'error'): void;
}

export interface ProductStage {
  show(view: StageView): void;
  clear(): void;
  /** Point on the case back under a screen point, for drops from the sticker tray. */
  pointToBack(clientX: number, clientY: number): BackPoint | null;
  preview(clientX: number, clientY: number, sticker: ({ readonly imageUrl: string } & StickerSize) | null): void;
  resetView(): void;
  dispose(): void;
}

const VALID = new THREE.Color(0x0e7a45);
const INVALID = new THREE.Color(0xb4232a);
const DECAL_DEPTH_MM = 20;

/** Projector orientation: local X is the sticker width, local Z is the outward normal. */
function decalOrientation(rotationDeg: number, normal: THREE.Vector3): THREE.Euler {
  const rad = THREE.MathUtils.degToRad(normalizeRotation(rotationDeg));
  const width = new THREE.Vector3(Math.cos(rad), -Math.sin(rad), 0);
  width.addScaledVector(normal, -width.dot(normal));
  if (width.lengthSq() < 1e-8) width.set(1, 0, 0);
  width.normalize();
  const height = new THREE.Vector3().crossVectors(normal, width).normalize();
  return new THREE.Euler().setFromRotationMatrix(new THREE.Matrix4().makeBasis(width, height, normal));
}

export function mountProduct(host: HTMLElement, hooks: StageHooks): ProductStage {
  const viewer = createViewer(host);
  const { scene, controls, canvas } = viewer;

  let view: StageView | null = null;
  let builtKey = '';
  let seatedKey = '';
  let token = 0;
  let caseFit: Fitted | null = null;
  let phoneFit: Fitted | null = null;
  const decal = new THREE.Group();
  scene.add(decal);
  let decalKey = '';

  const ghost = makeGhost();
  scene.add(ghost.group);
  let tween: { from: THREE.Vector3; to: THREE.Vector3; t: number } | null = null;

  type MoveDrag = { kind: 'move'; pointerId: number; key: string; grab: { x: number; y: number }; candidate: BackPoint | null; rotationDeg: number; size: StickerSize; imageUrl: string };
  type SpinDrag = { kind: 'spin'; pointerId: number; key: string; rotationDeg: number; size: StickerSize; imageUrl: string };
  let drag: MoveDrag | SpinDrag | null = null;
  const handleMaterial = { depthTest: false, transparent: true, toneMapped: false } as const;
  const handle = new THREE.Group();
  const handleHalo = new THREE.Mesh(
    new THREE.SphereGeometry(7, 20, 16),
    new THREE.MeshBasicMaterial({ ...handleMaterial, color: 0xf4f5f8 }),
  );
  const handleKnob = new THREE.Mesh(
    new THREE.SphereGeometry(4.2, 20, 16),
    new THREE.MeshBasicMaterial({ ...handleMaterial, color: 0x14161c }),
  );
  const handleStem = new THREE.Line(
    new THREE.BufferGeometry().setFromPoints([new THREE.Vector3(), new THREE.Vector3(0, 1, 0)]),
    new THREE.LineBasicMaterial({ color: 0x14161c, depthTest: false, transparent: true, toneMapped: false }),
  );
  handleHalo.renderOrder = 998;
  handleKnob.renderOrder = 999;
  handleStem.renderOrder = 997;
  handle.add(handleStem, handleHalo, handleKnob);
  handle.visible = false;
  scene.add(handle);
  let press: { x: number; y: number; pointerId: number; touch: boolean } | null = null;

  function backZ(): number {
    return caseFit ? caseFit.box.max.z : 0;
  }

  function hitCase(clientX: number, clientY: number): THREE.Vector3 | null {
    if (!caseFit) return null;
    const hits = viewer.ray(clientX, clientY).intersectObjects(caseFit.meshes as THREE.Mesh[], false);
    return hits[0]?.point.clone() ?? null;
  }

  /** XY on the back: the case surface when the ray touches it, otherwise the back plane. */
  function backXY(clientX: number, clientY: number): { x: number; y: number } | null {
    const hit = hitCase(clientX, clientY);
    if (hit) return { x: hit.x, y: hit.y };
    const plane = new THREE.Plane(new THREE.Vector3(0, 0, 1), -backZ());
    const point = viewer.ray(clientX, clientY).ray.intersectPlane(plane, new THREE.Vector3());
    return point ? { x: point.x, y: point.y } : null;
  }

  function activePlacement(next: StageView | null = view): StagePlacement | null {
    if (!next) return null;
    return next.placements.find((item) => item.active) ?? next.placements.at(-1) ?? null;
  }

  function placementAt(x: number, y: number): StagePlacement | null {
    if (!view) return null;
    for (let index = view.placements.length - 1; index >= 0; index -= 1) {
      const item = view.placements[index]!;
      if (stickerCovers(x, y, item.at, item, view.caseModel.sheet, item.rotationDeg)) return item;
    }
    return null;
  }

  function handlePoint(item: StagePlacement): { x: number; y: number } {
    const center = backToWorld(item.at, view!.caseModel.sheet);
    const rad = THREE.MathUtils.degToRad(normalizeRotation(item.rotationDeg));
    const reach = item.heightMm / 2 + 14;
    return { x: center.x + Math.sin(rad) * reach, y: center.y + Math.cos(rad) * reach };
  }

  function nearHandle(x: number, y: number, item: StagePlacement): boolean {
    const point = handlePoint(item);
    return Math.hypot(x - point.x, y - point.y) < 12;
  }

  function placeHandle(item: StagePlacement | null, rotationDeg = item?.rotationDeg ?? 0) {
    if (!view?.editable || !item) {
      handle.visible = false;
      viewer.invalidate();
      return;
    }
    const center = backToWorld(item.at, view.caseModel.sheet);
    const rad = THREE.MathUtils.degToRad(normalizeRotation(rotationDeg));
    const reach = item.heightMm / 2 + 14;
    const z = backZ() + 2;
    const knob = new THREE.Vector3(center.x + Math.sin(rad) * reach, center.y + Math.cos(rad) * reach, z);
    const edge = new THREE.Vector3(center.x + Math.sin(rad) * (item.heightMm / 2), center.y + Math.cos(rad) * (item.heightMm / 2), z);
    handle.position.copy(knob);
    handleStem.geometry.setFromPoints([new THREE.Vector3(), edge.clone().sub(knob)]);
    handle.visible = true;
    viewer.invalidate();
  }

  function surfaceFrame(x: number, y: number): { z: number; normal: THREE.Vector3 } {
    const fallback = new THREE.Vector3(0, 0, 1);
    if (!caseFit) return { z: 0, normal: fallback };
    const ray = new THREE.Raycaster(new THREE.Vector3(x, y, caseFit.box.max.z + 50), new THREE.Vector3(0, 0, -1));
    const hit = ray.intersectObjects(caseFit.meshes as THREE.Mesh[], false)[0];
    if (!hit?.face) return { z: caseFit.box.max.z, normal: fallback };
    const normal = hit.face.normal.clone().transformDirection(hit.object.matrixWorld).normalize();
    if (normal.z < 0) normal.negate();
    return { z: hit.point.z, normal };
  }

  async function render(next: StageView) {
    const mine = ++token;
    const key = `${next.caseModel.url}|${next.caseModel.pose.back}${next.caseModel.pose.up}|${next.caseModel.sheet.heightMm}|${next.phone?.url ?? ''}|${next.phone?.pose.back ?? ''}${next.phone?.pose.up ?? ''}`;
    const nestKey = `${next.caseModel.nest.insetMm}|${next.caseModel.nest.scale}`;
    if (key !== builtKey) {
      hooks.onStatus('loading');
      try {
        const [caseSource, phoneSource] = await Promise.all([
          loadScene(next.caseModel.url),
          next.phone ? loadScene(next.phone.url).catch(() => null) : Promise.resolve(null),
        ]);
        if (mine !== token) return;
        if (caseFit) scene.remove(caseFit.object);
        if (phoneFit) scene.remove(phoneFit.object);
        caseFit = canonicalize(caseSource, next.caseModel.pose, next.caseModel.sheet.heightMm);
        phoneFit = phoneSource && next.phone ? canonicalize(phoneSource, next.phone.pose, next.caseModel.sheet.heightMm) : null;
        if (phoneFit) {
          seatPhone(phoneFit, caseFit.box, next.caseModel.nest);
          scene.add(phoneFit.object);
        }
        scene.add(caseFit.object);
        builtKey = key;
        seatedKey = nestKey;
        decalKey = '';
        viewer.frame(caseFit.box, next.editable ? 'angled' : 'angled');
        hooks.onStatus('ready');
      } catch (error) {
        console.error(error);
        if (mine === token) hooks.onStatus('error');
        return;
      }
    } else if (phoneFit && caseFit && nestKey !== seatedKey) {
      seatPhone(phoneFit, caseFit.box, next.caseModel.nest);
      seatedKey = nestKey;
      viewer.invalidate();
    }
    controls.autoRotate = !next.editable && !window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    controls.autoRotateSpeed = 0.8;
    await renderDecal(next, mine);
    if (!drag) placeHandle(activePlacement(next));
  }

  async function renderDecal(next: StageView, mine: number) {
    const key = `${builtKey}|${next.placements.map((item) => `${item.key}:${item.imageUrl}:${item.widthMm}x${item.heightMm}:${item.at.x},${item.at.y}:${item.rotationDeg}`).join(';')}`;
    if (key === decalKey) return;
    clearDecal();
    decalKey = key;
    if (!caseFit || next.placements.length === 0) {
      viewer.invalidate();
      return;
    }
    for (const [index, placement] of next.placements.entries()) {
      const texture = await loadTexture(placement.imageUrl).catch(() => null);
      if (mine !== token || key !== decalKey || !caseFit) return;
      const center = backToWorld(placement.at, next.caseModel.sheet);
      const frame = surfaceFrame(center.x, center.y);
      const position = new THREE.Vector3(center.x, center.y, frame.z);
      const orientation = decalOrientation(placement.rotationDeg, frame.normal);
      const size = new THREE.Vector3(placement.widthMm, placement.heightMm, DECAL_DEPTH_MM);
      const material = new THREE.MeshBasicMaterial({
        map: texture ?? undefined,
        color: texture ? 0xffffff : 0xdce4d2,
        transparent: true,
        depthWrite: false,
        polygonOffset: true,
        polygonOffsetFactor: -4 - index * 2,
        toneMapped: false,
      });
      for (const mesh of caseFit.meshes) {
        const geometry = new DecalGeometry(mesh, position, orientation, size);
        if (!geometry.getAttribute('position')?.count) {
          geometry.dispose();
          continue;
        }
        const sticker = new THREE.Mesh(geometry, material);
        sticker.renderOrder = index + 1;
        sticker.userData.placementKey = placement.key;
        decal.add(sticker);
      }
    }
    viewer.invalidate();
  }

  function clearDecal() {
    for (const child of [...decal.children]) {
      const mesh = child as THREE.Mesh;
      mesh.geometry.dispose();
      decal.remove(mesh);
    }
  }

  async function setGhostSticker(sticker: ({ readonly imageUrl: string } & StickerSize) | null) {
    if (!sticker) return;
    ghost.resize(sticker.widthMm, sticker.heightMm);
    const texture = await loadTexture(sticker.imageUrl).catch(() => null);
    ghost.setTexture(texture);
    viewer.invalidate();
  }

  function placeGhost(x: number, y: number, valid: boolean, rotationDeg = 0) {
    ghost.group.visible = true;
    ghost.group.position.set(x, y, backZ() + 0.6);
    ghost.group.rotation.z = -THREE.MathUtils.degToRad(normalizeRotation(rotationDeg));
    ghost.setTint(valid ? VALID : INVALID);
    viewer.invalidate();
  }

  function hidePlacement(key: string, hidden: boolean) {
    for (const child of decal.children) {
      if (child.userData.placementKey === key) child.visible = !hidden;
    }
  }

  function candidateAt(clientX: number, clientY: number, size: StickerSize, rotationDeg: number, offset = { x: 0, y: 0 }) {
    if (!view) return null;
    const xy = backXY(clientX, clientY);
    if (!xy) return null;
    const x = xy.x - offset.x;
    const y = xy.y - offset.y;
    const at = worldToBack(x, y, view.caseModel.sheet);
    return { x, y, at, valid: footprintOnSheet(at, size, view.caseModel.sheet, rotationDeg) };
  }

  function spinAngle(clientX: number, clientY: number, item: StagePlacement): number | null {
    const xy = backXY(clientX, clientY);
    if (!xy || !view) return null;
    const center = backToWorld(item.at, view.caseModel.sheet);
    const deg = THREE.MathUtils.radToDeg(Math.atan2(xy.x - center.x, xy.y - center.y));
    return normalizeRotation(deg);
  }

  function onPointerDown(event: PointerEvent) {
    if (!view?.editable || event.button !== 0) return;
    const xy = backXY(event.clientX, event.clientY);
    const touch = event.pointerType === 'touch';
    const current = activePlacement();
    if (xy && current && nearHandle(xy.x, xy.y, current)) {
      drag = { kind: 'spin', pointerId: event.pointerId, key: current.key, rotationDeg: current.rotationDeg, size: current, imageUrl: current.imageUrl };
      controls.enabled = false;
      canvas.setPointerCapture(event.pointerId);
      canvas.style.cursor = 'alias';
      void setGhostSticker(current);
      hidePlacement(current.key, true);
      const center = backToWorld(current.at, view.caseModel.sheet);
      placeGhost(center.x, center.y, true, current.rotationDeg);
      return;
    }
    const hit = xy ? placementAt(xy.x, xy.y) : null;
    if (xy && hit) {
      const center = backToWorld(hit.at, view.caseModel.sheet);
      drag = {
        kind: 'move',
        pointerId: event.pointerId,
        key: hit.key,
        grab: { x: xy.x - center.x, y: xy.y - center.y },
        candidate: hit.at,
        rotationDeg: hit.rotationDeg,
        size: hit,
        imageUrl: hit.imageUrl,
      };
      controls.enabled = false;
      canvas.setPointerCapture(event.pointerId);
      canvas.style.cursor = 'grabbing';
      hooks.onSelect?.(hit.key);
      void setGhostSticker(hit);
      hidePlacement(hit.key, true);
      placeGhost(center.x, center.y, true, hit.rotationDeg);
      return;
    }
    press = { x: event.clientX, y: event.clientY, pointerId: event.pointerId, touch };
  }

  function onPointerMove(event: PointerEvent) {
    if (drag && event.pointerId === drag.pointerId && drag.kind === 'spin') {
      const item = view?.placements.find((placement) => placement.key === drag?.key);
      if (!item || !view) return;
      const rotationDeg = spinAngle(event.clientX, event.clientY, item);
      if (rotationDeg == null) return;
      drag.rotationDeg = rotationDeg;
      const center = backToWorld(item.at, view.caseModel.sheet);
      const legal = footprintOnSheet(item.at, item, view.caseModel.sheet, rotationDeg);
      placeGhost(center.x, center.y, legal, rotationDeg);
      placeHandle({ ...item, rotationDeg }, rotationDeg);
      return;
    }
    if (drag && event.pointerId === drag.pointerId && drag.kind === 'move') {
      const next = candidateAt(event.clientX, event.clientY, drag.size, drag.rotationDeg, drag.grab);
      if (!next) return;
      drag.candidate = next.valid ? next.at : null;
      placeGhost(next.x, next.y, next.valid, drag.rotationDeg);
      return;
    }
    if (!view?.editable || press) return;
    const xy = backXY(event.clientX, event.clientY);
    const current = activePlacement();
    if (xy && current && nearHandle(xy.x, xy.y, current)) canvas.style.cursor = 'alias';
    else if (xy && placementAt(xy.x, xy.y)) canvas.style.cursor = 'grab';
    else canvas.style.cursor = hitCase(event.clientX, event.clientY) ? 'copy' : 'default';
  }

  function onPointerUp(event: PointerEvent) {
    if (drag && event.pointerId === drag.pointerId && drag.kind === 'spin') {
      const finished = drag;
      const item = view?.placements.find((placement) => placement.key === finished.key);
      drag = null;
      controls.enabled = true;
      ghost.group.visible = false;
      hidePlacement(finished.key, false);
      if (item && view && finished.rotationDeg !== item.rotationDeg && footprintOnSheet(item.at, item, view.caseModel.sheet, finished.rotationDeg)) {
        hooks.onRotateTo?.(finished.key, finished.rotationDeg);
      } else if (item && finished.rotationDeg !== item.rotationDeg) {
        hooks.onReject();
      }
      placeHandle(activePlacement());
      viewer.invalidate();
      return;
    }
    if (drag && event.pointerId === drag.pointerId && drag.kind === 'move') {
      const committed = drag.candidate;
      const finished = drag;
      drag = null;
      controls.enabled = true;
      canvas.style.cursor = 'grab';
      const item = view?.placements.find((placement) => placement.key === finished.key);
      if (committed && item && (committed.x !== item.at.x || committed.y !== item.at.y)) {
        ghost.group.visible = false;
        hidePlacement(finished.key, false);
        hooks.onCommit(finished.key, committed);
      } else if (item && view) {
        const home = backToWorld(item.at, view.caseModel.sheet);
        tween = { from: ghost.group.position.clone(), to: new THREE.Vector3(home.x, home.y, backZ() + 0.6), t: 0 };
        hidePlacement(finished.key, false);
        if (!committed) hooks.onReject();
      }
      return;
    }
    if (!press || event.pointerId !== press.pointerId) return;
    const moved = Math.hypot(event.clientX - press.x, event.clientY - press.y);
    const limit = press.touch ? 10 : 4;
    press = null;
    const current = activePlacement();
    if (moved > limit || !view?.editable || !current) return;
    if (!hitCase(event.clientX, event.clientY)) return;
    const next = candidateAt(event.clientX, event.clientY, current, current.rotationDeg);
    if (!next) return;
    if (next.valid) {
      hooks.onCommit(current.key, next.at);
      return;
    }
    void setGhostSticker(current);
    placeGhost(next.x, next.y, false, current.rotationDeg);
    hooks.onReject();
    window.setTimeout(() => {
      if (!drag) {
        ghost.group.visible = false;
        viewer.invalidate();
      }
    }, 650);
  }

  function onKey(event: KeyboardEvent) {
    if (!view?.editable || !activePlacement()) return;
    const step = event.shiftKey ? 50 : 10;
    const moves: Record<string, [number, number]> = {
      ArrowLeft: [-step, 0],
      ArrowRight: [step, 0],
      ArrowUp: [0, -step],
      ArrowDown: [0, step],
    };
    if (event.key === 'r' || event.key === 'R' || event.key === '[' || event.key === ']') {
      event.preventDefault();
      const current = activePlacement();
      if (!current) return;
      const counter = event.key === '[' || ((event.key === 'r' || event.key === 'R') && event.shiftKey);
      hooks.onRotateTo?.(current.key, normalizeRotation(current.rotationDeg + (counter ? -15 : 15)));
      return;
    }
    const move = moves[event.key];
    if (!move) return;
    event.preventDefault();
    hooks.onNudge(move[0], move[1]);
  }

  canvas.addEventListener('pointerdown', onPointerDown, { capture: true });
  canvas.addEventListener('pointermove', onPointerMove);
  canvas.addEventListener('pointerup', onPointerUp);
  canvas.addEventListener('pointercancel', onPointerUp);
  canvas.addEventListener('keydown', onKey);

  const stopTween = viewer.onFrame((dt) => {
    if (!tween) return false;
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    tween.t = reduced ? 1 : Math.min(1, tween.t + dt / 0.35);
    const eased = 1 - (1 - tween.t) ** 3;
    ghost.group.position.lerpVectors(tween.from, tween.to, eased);
    ghost.setTint(VALID);
    if (tween.t >= 1) {
      tween = null;
      ghost.group.visible = false;
      decal.visible = true;
    }
    return true;
  });

  return {
    show(next) {
      view = next;
      if (!drag && !tween) {
        ghost.group.visible = false;
        decal.visible = true;
      }
      void render(next);
    },
    clear() {
      token += 1;
      view = null;
      if (caseFit) scene.remove(caseFit.object);
      if (phoneFit) scene.remove(phoneFit.object);
      caseFit = null;
      phoneFit = null;
      builtKey = '';
      seatedKey = '';
      decalKey = '';
      clearDecal();
      handle.visible = false;
      ghost.group.visible = false;
      controls.autoRotate = false;
      viewer.invalidate();
    },
    pointToBack(clientX, clientY) {
      if (!view) return null;
      const xy = backXY(clientX, clientY);
      return xy ? worldToBack(xy.x, xy.y, view.caseModel.sheet) : null;
    },
    preview(clientX, clientY, sticker) {
      if (!sticker || !view) {
        ghost.group.visible = false;
        viewer.invalidate();
        return;
      }
      void setGhostSticker(sticker);
      const xy = backXY(clientX, clientY);
      if (!xy) return;
      const at = worldToBack(xy.x, xy.y, view.caseModel.sheet);
      placeGhost(xy.x, xy.y, footprintOnSheet(at, sticker, view.caseModel.sheet, 0), 0);
    },
    resetView() {
      if (caseFit) viewer.frame(caseFit.box, 'angled');
    },
    dispose() {
      token += 1;
      stopTween();
      canvas.removeEventListener('pointerdown', onPointerDown, { capture: true });
      canvas.removeEventListener('pointermove', onPointerMove);
      canvas.removeEventListener('pointerup', onPointerUp);
      canvas.removeEventListener('pointercancel', onPointerUp);
      canvas.removeEventListener('keydown', onKey);
      clearDecal();
      viewer.dispose();
    },
  };
}

function makeGhost() {
  const group = new THREE.Group();
  group.visible = false;
  const material = new THREE.MeshBasicMaterial({ transparent: true, opacity: 0.8, depthWrite: false, toneMapped: false });
  const plane = new THREE.Mesh(new THREE.PlaneGeometry(1, 1), material);
  const outlineMaterial = new THREE.LineBasicMaterial({ color: VALID, toneMapped: false });
  const outline = new THREE.LineLoop(
    new THREE.BufferGeometry().setFromPoints([
      new THREE.Vector3(-0.5, -0.5, 0.05),
      new THREE.Vector3(0.5, -0.5, 0.05),
      new THREE.Vector3(0.5, 0.5, 0.05),
      new THREE.Vector3(-0.5, 0.5, 0.05),
    ]),
    outlineMaterial,
  );
  group.add(plane, outline);
  return {
    group,
    resize(width: number, height: number) {
      plane.scale.set(width, height, 1);
      outline.scale.set(width, height, 1);
    },
    setTexture(texture: THREE.Texture | null) {
      material.map = texture;
      material.needsUpdate = true;
    },
    setTint(color: THREE.Color) {
      outlineMaterial.color.copy(color);
      material.color.copy(color.equals(VALID) ? new THREE.Color(0xffffff) : new THREE.Color(0xf0c4b8));
    },
  };
}
