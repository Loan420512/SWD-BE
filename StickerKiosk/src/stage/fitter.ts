import * as THREE from 'three';
import { cells, hundredths, maskBytes, DEFAULT_NEST, type AssetPose, type OccupancyMask, type PhoneNest } from '../kiosk/sheet';
import { canonicalize, cheapenPreview, loadScene, seatPhone, type Fitted } from './canonical';
import { createViewer } from './viewer';

export interface BakeResult {
  readonly widthMm: number;
  readonly heightMm: number;
  readonly mask: OccupancyMask;
  readonly printableCells: number;
}

export interface PhonePreview {
  readonly url: string;
  readonly pose: AssetPose;
}

export type BrushMode = 'open' | 'block';

export interface FitterStage {
  show(url: string, pose: AssetPose, heightMm: number, phone?: PhonePreview | null): Promise<{ widthMm: number; heightMm: number; depthMm: number }>;
  orbit(axis: 'x' | 'y' | 'z'): void;
  setNest(nest: PhoneNest): void;
  /** While a brush is set, dragging on the case reports fitted-world millimetres instead of orbiting. */
  setBrush(mode: BrushMode | null, radiusMm: number, onStroke: (worldX: number, worldY: number) => void): void;
  bake(): BakeResult;
  showMask(mask: OccupancyMask | null): void;
  dispose(): void;
}

/** Cells under a sticker must face the camera within this angle. */
const MAX_TILT_DEG = 30;
/** Depth change inside one cell that still counts as flat. */
const CELL_RELIEF_MM = 0.5;
/** Depth change between neighbor cells that counts as a step (camera ring, rim). */
const STEP_MM = 0.8;
const SAMPLES = 4;

export function mountFitter(host: HTMLElement): FitterStage {
  const viewer = createViewer(host, 1);
  let fitted: Fitted | null = null;
  let phoneFit: Fitted | null = null;
  let nest: PhoneNest = DEFAULT_NEST;
  let heightMm = 0;
  let overlay: THREE.Mesh | null = null;
  let brush: { mode: BrushMode; radiusMm: number; onStroke: (worldX: number, worldY: number) => void } | null = null;
  let painting = false;
  let generation = 0;
  const arrow = new THREE.ArrowHelper(new THREE.Vector3(0, 1, 0), new THREE.Vector3(), 20, 0xff4b1f, 6, 4);
  viewer.scene.add(arrow);

  function applyNest() {
    if (!phoneFit || !fitted) return;
    seatPhone(phoneFit, fitted.box, nest);
    viewer.invalidate();
  }

  function hitCase(clientX: number, clientY: number): THREE.Vector3 | null {
    if (!fitted) return null;
    return viewer.ray(clientX, clientY).intersectObjects(fitted.meshes as THREE.Mesh[], false)[0]?.point ?? null;
  }

  function onPointerDown(event: PointerEvent) {
    if (!brush || event.button !== 0) return;
    const point = hitCase(event.clientX, event.clientY);
    if (!point) return;
    painting = true;
    event.stopPropagation();
    viewer.controls.enabled = false;
    viewer.canvas.setPointerCapture(event.pointerId);
    brush.onStroke(point.x, point.y);
  }

  function onPointerMove(event: PointerEvent) {
    if (!painting || !brush) return;
    const point = hitCase(event.clientX, event.clientY);
    if (point) brush.onStroke(point.x, point.y);
  }

  function onPointerUp() {
    if (!painting) return;
    painting = false;
    viewer.controls.enabled = true;
  }

  viewer.canvas.addEventListener('pointerdown', onPointerDown, { capture: true });
  viewer.canvas.addEventListener('pointermove', onPointerMove);
  viewer.canvas.addEventListener('pointerup', onPointerUp);
  viewer.canvas.addEventListener('pointercancel', onPointerUp);

  function clearOverlay() {
    if (!overlay) return;
    viewer.scene.remove(overlay);
    (overlay.material as THREE.MeshBasicMaterial).map?.dispose();
    overlay.geometry.dispose();
    overlay = null;
  }

  return {
    async show(url, pose, height, phone = null) {
      const ticket = ++generation;
      const [source, phoneSource] = await Promise.all([
        loadScene(url),
        phone ? loadScene(phone.url).catch(() => null) : Promise.resolve(null),
      ]);
      if (ticket !== generation) return { widthMm: 0, heightMm: height, depthMm: 0 };
      if (fitted) viewer.scene.remove(fitted.object);
      if (phoneFit) viewer.scene.remove(phoneFit.object);
      clearOverlay();
      fitted = canonicalize(source, pose, height, 'longest');
      cheapenPreview(fitted.object);
      heightMm = height;
      phoneFit = phoneSource && phone ? canonicalize(phoneSource, phone.pose, height) : null;
      if (phoneFit) cheapenPreview(phoneFit.object);
      if (phoneFit) {
        seatPhone(phoneFit, fitted.box, nest);
        viewer.scene.add(phoneFit.object);
      }
      viewer.scene.add(fitted.object);
      const box = fitted.box;
      arrow.position.set(box.max.x + 12, box.max.y - 22, box.max.z);
      viewer.frame(box, 'straight');
      const size = box.getSize(new THREE.Vector3());
      return { widthMm: size.x, heightMm: size.y, depthMm: size.z };
    },

    orbit(axis) {
      viewer.orbit(axis);
    },

    setNest(next) {
      nest = next;
      applyNest();
    },

    setBrush(mode, radiusMm, onStroke) {
      brush = mode ? { mode, radiusMm, onStroke } : null;
      if (!brush) onPointerUp();
      viewer.canvas.style.cursor = brush ? 'crosshair' : '';
    },

    bake() {
      if (!fitted) throw new Error('Load a case model first');
      const box = fitted.box;
      const widthMm = Math.round((box.max.x - box.min.x) * 100) / 100;
      const cols = cells(widthMm);
      const rows = cells(heightMm);
      const wholeCols = Math.trunc(hundredths(widthMm) / 100);
      const wholeRows = Math.trunc(hundredths(heightMm) / 100);
      const width = cols * SAMPLES;
      const height = rows * SAMPLES;
      const depthRange = box.max.z - box.min.z + 2;

      const camera = new THREE.OrthographicCamera(box.min.x, box.min.x + cols, box.max.y, box.max.y - rows, 0.5, depthRange + 20);
      camera.position.set(0, 0, box.max.z + 10);
      camera.lookAt(0, 0, box.min.z);
      camera.position.set(0, 0, box.max.z + 10);
      camera.updateMatrixWorld(true);

      const material = new THREE.ShaderMaterial({
        side: THREE.DoubleSide,
        uniforms: { zMin: { value: box.min.z - 1 }, zRange: { value: depthRange } },
        vertexShader: `
          varying vec3 vWorld;
          void main() {
            vec4 world = modelMatrix * vec4(position, 1.0);
            vWorld = world.xyz;
            gl_Position = projectionMatrix * viewMatrix * world;
          }`,
        fragmentShader: `
          uniform float zMin;
          uniform float zRange;
          varying vec3 vWorld;
          void main() {
            vec3 n = normalize(cross(dFdx(vWorld), dFdy(vWorld)));
            gl_FragColor = vec4(abs(n.z), clamp((vWorld.z - zMin) / zRange, 0.0, 1.0), 0.0, 1.0);
          }`,
      });

      const target = new THREE.WebGLRenderTarget(width, height, { type: THREE.UnsignedByteType, depthBuffer: true });
      const bakeScene = new THREE.Scene();
      bakeScene.overrideMaterial = material;
      const parent = fitted.object.parent;
      bakeScene.add(fitted.object);
      const renderer = viewer.renderer;
      const previousClear = renderer.getClearAlpha();
      renderer.setRenderTarget(target);
      renderer.setClearColor(0x000000, 0);
      renderer.clear();
      renderer.render(bakeScene, camera);
      renderer.setRenderTarget(null);
      renderer.setClearColor(0x000000, previousClear);
      const pixels = new Uint8Array(width * height * 4);
      renderer.readRenderTargetPixels(target, 0, 0, width, height, pixels);
      parent?.add(fitted.object);
      target.dispose();
      material.dispose();

      const minNz = Math.cos(THREE.MathUtils.degToRad(MAX_TILT_DEG)) * 255;
      const relief = (CELL_RELIEF_MM / depthRange) * 255;
      const depth = new Float32Array(cols * rows).fill(NaN);
      const flat = new Uint8Array(cols * rows);
      for (let j = 0; j < wholeRows; j++) {
        for (let i = 0; i < wholeCols; i++) {
          let ok = true;
          let lo = 255;
          let hi = 0;
          let sum = 0;
          for (let sy = 0; sy < SAMPLES && ok; sy++) {
            const row = height - 1 - (j * SAMPLES + sy);
            for (let sx = 0; sx < SAMPLES; sx++) {
              const p = (row * width + i * SAMPLES + sx) * 4;
              if (pixels[p + 3]! < 128 || pixels[p]! < minNz) {
                ok = false;
                break;
              }
              const g = pixels[p + 1]!;
              lo = Math.min(lo, g);
              hi = Math.max(hi, g);
              sum += g;
            }
          }
          if (!ok || hi - lo > relief) continue;
          flat[j * cols + i] = 1;
          depth[j * cols + i] = sum / (SAMPLES * SAMPLES);
        }
      }

      const step = (STEP_MM / depthRange) * 255;
      const stepped = flat.slice();
      for (let j = 0; j < rows; j++) {
        for (let i = 0; i < cols; i++) {
          const here = depth[j * cols + i]!;
          if (!flat[j * cols + i]) continue;
          for (const [di, dj] of [[1, 0], [0, 1]] as const) {
            const ni = i + di;
            const nj = j + dj;
            if (ni >= cols || nj >= rows || !flat[nj * cols + ni]) continue;
            if (Math.abs(here - depth[nj * cols + ni]!) > step) {
              stepped[j * cols + i] = 0;
              stepped[nj * cols + ni] = 0;
            }
          }
        }
      }

      const bits = new Uint8Array(maskBytes(cols, rows));
      let printableCells = 0;
      for (let j = 0; j < rows; j++) {
        for (let i = 0; i < cols; i++) {
          let keep = stepped[j * cols + i] === 1;
          for (let dj = -1; dj <= 1 && keep; dj++) {
            for (let di = -1; di <= 1; di++) {
              const ni = i + di;
              const nj = j + dj;
              if (ni < 0 || nj < 0 || ni >= cols || nj >= rows || !stepped[nj * cols + ni]) {
                keep = false;
                break;
              }
            }
          }
          if (!keep) continue;
          const bit = j * cols + i;
          bits[bit >> 3]! |= 1 << (bit & 7);
          printableCells += 1;
        }
      }

      return { widthMm, heightMm, mask: { cols, rows, bits }, printableCells };
    },

    showMask(mask) {
      clearOverlay();
      if (!mask || !fitted) {
        viewer.invalidate();
        return;
      }
      const canvas = document.createElement('canvas');
      canvas.width = mask.cols;
      canvas.height = mask.rows;
      const context = canvas.getContext('2d')!;
      const image = context.createImageData(mask.cols, mask.rows);
      for (let j = 0; j < mask.rows; j++) {
        for (let i = 0; i < mask.cols; i++) {
          const bit = j * mask.cols + i;
          const set = (mask.bits[bit >> 3]! & (1 << (bit & 7))) !== 0;
          const p = (j * mask.cols + i) * 4;
          image.data[p] = set ? 82 : 178;
          image.data[p + 1] = set ? 160 : 74;
          image.data[p + 2] = set ? 74 : 53;
          image.data[p + 3] = set ? 150 : 60;
        }
      }
      context.putImageData(image, 0, 0);
      const texture = new THREE.CanvasTexture(canvas);
      texture.magFilter = THREE.NearestFilter;
      texture.minFilter = THREE.NearestFilter;
      texture.colorSpace = THREE.SRGBColorSpace;
      const box = fitted.box;
      overlay = new THREE.Mesh(
        new THREE.PlaneGeometry(mask.cols, mask.rows),
        new THREE.MeshBasicMaterial({ map: texture, transparent: true, depthTest: false, toneMapped: false }),
      );
      overlay.position.set(box.min.x + mask.cols / 2, box.max.y - mask.rows / 2, box.max.z + 0.3);
      overlay.renderOrder = 10;
      viewer.scene.add(overlay);
      viewer.invalidate();
    },

    dispose() {
      viewer.canvas.removeEventListener('pointerdown', onPointerDown, { capture: true });
      viewer.canvas.removeEventListener('pointermove', onPointerMove);
      viewer.canvas.removeEventListener('pointerup', onPointerUp);
      viewer.canvas.removeEventListener('pointercancel', onPointerUp);
      clearOverlay();
      viewer.dispose();
    },
  };
}
