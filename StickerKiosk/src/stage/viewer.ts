import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { RoomEnvironment } from 'three/addons/environments/RoomEnvironment.js';

/** Renderer, camera, lights, and an on-demand render loop shared by the studio and the admin fitter. */
export interface Viewer {
  readonly renderer: THREE.WebGLRenderer;
  readonly scene: THREE.Scene;
  readonly camera: THREE.PerspectiveCamera;
  readonly controls: OrbitControls;
  readonly canvas: HTMLCanvasElement;
  invalidate(): void;
  frame(box: THREE.Box3, view: 'angled' | 'straight'): void;
  /** Quarter-turns the camera around the model. The fitted phone and case stay put. */
  orbit(axis: 'x' | 'y' | 'z'): void;
  ray(clientX: number, clientY: number): THREE.Raycaster;
  onFrame(callback: (dt: number) => boolean): () => void;
  dispose(): void;
}

export function createViewer(host: HTMLElement, pixelRatioCap = 1.5): Viewer {
  const canvas = document.createElement('canvas');
  canvas.className = 'stage-canvas';
  canvas.tabIndex = 0;
  host.appendChild(canvas);

  const renderer = new THREE.WebGLRenderer({
    canvas,
    antialias: true,
    alpha: true,
    powerPreference: 'high-performance',
  });
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, pixelRatioCap));
  renderer.setClearColor(0x000000, 0);
  renderer.outputColorSpace = THREE.SRGBColorSpace;
  renderer.toneMapping = THREE.NeutralToneMapping;
  renderer.toneMappingExposure = 1.05;

  const scene = new THREE.Scene();
  const pmrem = new THREE.PMREMGenerator(renderer);
  const room = new RoomEnvironment();
  scene.environment = pmrem.fromScene(room, 0.04).texture;
  scene.environmentIntensity = 0.75;

  const key = new THREE.DirectionalLight(0xf3f5fa, 1.55);
  key.position.set(120, 260, 320);
  scene.add(key);
  const fill = new THREE.DirectionalLight(0xd5dce8, 0.55);
  fill.position.set(-260, 40, 120);
  scene.add(fill);
  scene.add(new THREE.AmbientLight(0xffffff, 0.35));

  const camera = new THREE.PerspectiveCamera(30, 1, 1, 4000);
  camera.position.set(0, 0, 500);

  const controls = new OrbitControls(camera, canvas);
  controls.enableDamping = true;
  controls.dampingFactor = 0.08;
  controls.enablePan = false;
  controls.minPolarAngle = 0.02;
  controls.maxPolarAngle = Math.PI - 0.02;

  let dirty = true;
  let coasting = false;
  const held = new THREE.Vector3();
  const heldTarget = new THREE.Vector3();
  const heldQuat = new THREE.Quaternion();
  let heldReady = false;
  const callbacks = new Set<(dt: number) => boolean>();
  controls.addEventListener('start', () => {
    coasting = true;
    dirty = true;
  });
  controls.addEventListener('end', () => {
    coasting = true;
  });

  const raycaster = new THREE.Raycaster();
  const pointer = new THREE.Vector2();

  function resize() {
    const width = host.clientWidth || 1;
    const height = host.clientHeight || 1;
    renderer.setSize(width, height, false);
    camera.aspect = width / height;
    camera.updateProjectionMatrix();
    dirty = true;
  }
  const observer = new ResizeObserver(resize);
  observer.observe(host);
  resize();

  let last = performance.now();
  let frameId = 0;
  let disposed = false;
  function loop(now: number) {
    if (disposed) return;
    frameId = requestAnimationFrame(loop);
    const dt = Math.min(0.05, (now - last) / 1000);
    last = now;
    if (document.hidden) return;
    if (coasting || controls.autoRotate) {
      controls.update();
      if (cameraMoved()) dirty = true;
      else coasting = false;
    }
    for (const callback of callbacks) {
      if (callback(dt)) dirty = true;
    }
    if (!dirty) return;
    dirty = false;
    renderer.render(scene, camera);
    held.copy(camera.position);
    heldTarget.copy(controls.target);
    heldQuat.copy(camera.quaternion);
    heldReady = true;
  }

  /** 0.05 mm at the camera. Smaller motion is the lookAt jitter that never reaches OrbitControls' own epsilon. */
  function cameraMoved(): boolean {
    if (!heldReady) return true;
    if (held.distanceToSquared(camera.position) > 0.0025) return true;
    if (heldTarget.distanceToSquared(controls.target) > 0.0025) return true;
    return 1 - Math.abs(heldQuat.dot(camera.quaternion)) > 1e-5;
  }
  frameId = requestAnimationFrame(loop);

  return {
    renderer,
    scene,
    camera,
    controls,
    canvas,
    invalidate() {
      dirty = true;
    },
    frame(box, view) {
      const size = box.getSize(new THREE.Vector3());
      const center = box.getCenter(new THREE.Vector3());
      const vFov = THREE.MathUtils.degToRad(camera.fov);
      const hFov = 2 * Math.atan(Math.tan(vFov / 2) * camera.aspect);
      const distance = Math.max(size.y / 2 / Math.tan(vFov / 2), size.x / 2 / Math.tan(hFov / 2)) * 1.3 + size.z / 2;
      const direction = view === 'angled' ? new THREE.Vector3(0.32, 0.12, 1).normalize() : new THREE.Vector3(0, 0, 1);
      controls.target.copy(center);
      camera.position.copy(center).addScaledVector(direction, distance);
      camera.near = Math.max(0.5, distance / 50);
      camera.far = distance * 10;
      camera.updateProjectionMatrix();
      controls.minDistance = distance * 0.45;
      controls.maxDistance = distance * 2.2;
      controls.update();
      dirty = true;
    },
    orbit(axis) {
      const offset = camera.position.clone().sub(controls.target);
      const world = new THREE.Vector3(axis === 'x' ? 1 : 0, axis === 'y' ? 1 : 0, axis === 'z' ? 1 : 0);
      offset.applyAxisAngle(world, Math.PI / 2);
      if (axis === 'z') camera.up.applyAxisAngle(world, Math.PI / 2);
      else camera.up.set(0, 1, 0);
      camera.position.copy(controls.target).add(offset);
      camera.lookAt(controls.target);
      controls.update();
      heldReady = false;
      dirty = true;
    },
    ray(clientX, clientY) {
      const rect = canvas.getBoundingClientRect();
      pointer.set(((clientX - rect.left) / rect.width) * 2 - 1, -((clientY - rect.top) / rect.height) * 2 + 1);
      raycaster.setFromCamera(pointer, camera);
      return raycaster;
    },
    onFrame(callback) {
      callbacks.add(callback);
      return () => callbacks.delete(callback);
    },
    dispose() {
      disposed = true;
      cancelAnimationFrame(frameId);
      observer.disconnect();
      controls.dispose();
      pmrem.dispose();
      room.dispose();
      renderer.dispose();
      canvas.remove();
    },
  };
}
