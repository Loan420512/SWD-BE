import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { poseRows, type AssetPose, type PhoneNest, DEFAULT_NEST } from '../kiosk/sheet';

const loader = new GLTFLoader();
const scenes = new Map<string, Promise<THREE.Object3D>>();
const textures = new Map<string, Promise<THREE.Texture>>();

export function loadScene(url: string): Promise<THREE.Object3D> {
  let pending = scenes.get(url);
  if (!pending) {
    pending = loader.loadAsync(url).then((gltf) => gltf.scene);
    pending.catch(() => scenes.delete(url));
    scenes.set(url, pending);
  }
  return pending;
}

export function loadTexture(url: string): Promise<THREE.Texture> {
  let pending = textures.get(url);
  if (!pending) {
    pending = new THREE.TextureLoader().loadAsync(url).then((texture) => {
      texture.colorSpace = THREE.SRGBColorSpace;
      texture.anisotropy = 8;
      return texture;
    });
    pending.catch(() => textures.delete(url));
    textures.set(url, pending);
  }
  return pending;
}

export interface Fitted {
  /** Units are millimetres. +Z leaves the back toward the camera, +Y is up, origin is the AABB center. */
  readonly object: THREE.Group;
  readonly box: THREE.Box3;
  readonly meshes: readonly THREE.Mesh[];
  /** Uniform scale that makes the fitted height equal the requested millimetres, before a nest multiplier. */
  readonly heightScale: number;
}

/**
 * The only place a GLB is rotated or scaled. Rotation comes from the saved pose.
 * `height` makes the fitted height equal `heightMm`. `longest` keeps that same scale
 * while the admin turns the case, so a thin edge is never stretched up to the phone height.
 */
export function canonicalize(source: THREE.Object3D, pose: AssetPose, heightMm: number, fit: 'height' | 'longest' = 'height'): Fitted {
  const model = source.clone(true);
  ownMaterials(model);
  const turn = new THREE.Group();
  turn.add(model);
  const [right, up, back] = poseRows(pose);
  const rotation = new THREE.Matrix4().set(
    right[0], right[1], right[2], 0,
    up[0], up[1], up[2], 0,
    back[0], back[1], back[2], 0,
    0, 0, 0, 1,
  );
  turn.quaternion.setFromRotationMatrix(rotation);

  const outer = new THREE.Group();
  outer.add(turn);
  outer.updateMatrixWorld(true);
  const raw = new THREE.Box3().setFromObject(outer, true);
  const size = raw.getSize(new THREE.Vector3());
  const basis = fit === 'longest' ? Math.max(size.x, size.y, size.z) : size.y;
  if (basis <= 0) throw new Error('Model has no height');
  turn.position.copy(raw.getCenter(new THREE.Vector3())).negate();
  const heightScale = heightMm / basis;
  outer.scale.setScalar(heightScale);
  outer.updateMatrixWorld(true);

  const meshes: THREE.Mesh[] = [];
  outer.traverse((child) => {
    const mesh = child as THREE.Mesh;
    if (!mesh.isMesh) return;
    meshes.push(mesh);
    const materials = Array.isArray(mesh.material) ? mesh.material : [mesh.material];
    for (const material of materials) {
      if (material) material.side = THREE.DoubleSide;
    }
  });

  return { object: outer, box: new THREE.Box3().setFromObject(outer, true), meshes, heightScale };
}

/** GLTF clone shares materials with the cached scene. Copy them before any preview edit. */
function ownMaterials(root: THREE.Object3D) {
  root.traverse((child) => {
    const mesh = child as THREE.Mesh;
    if (!mesh.isMesh || !mesh.material) return;
    mesh.material = Array.isArray(mesh.material) ? mesh.material.map((material) => material.clone()) : mesh.material.clone();
  });
}

/**
 * The clear case stores a black base color and relies on transmission. Turning transmission
 * off without recoloring draws that black shell over the phone. This keeps a pale shell.
 */
export function cheapenPreview(root: THREE.Object3D) {
  root.traverse((child) => {
    const mesh = child as THREE.Mesh;
    if (!mesh.isMesh) return;
    const materials = Array.isArray(mesh.material) ? mesh.material : [mesh.material];
    for (const material of materials) {
      const physical = material as THREE.MeshPhysicalMaterial;
      if (!physical) continue;
      let changed = false;
      if (physical.transmission) {
        physical.transmission = 0;
        physical.color.set(0xd5dbd4);
        physical.metalness = 0;
        physical.roughness = 0.35;
        physical.transparent = true;
        physical.opacity = 0.28;
        physical.depthWrite = false;
        changed = true;
      }
      if (physical.clearcoat) {
        physical.clearcoat = 0;
        changed = true;
      }
      if (changed) physical.needsUpdate = true;
    }
  });
}

/**
 * Sits a fitted phone in a fitted case. `nest.scale` multiplies the height match.
 * `nest.insetMm` pushes the phone back from the case's outer back face. Calling it again
 * replaces the previous seat instead of stacking.
 */
export function seatPhone(phone: Fitted, caseBox: THREE.Box3, nest: PhoneNest = DEFAULT_NEST) {
  const scale = nest.scale > 0 ? nest.scale : 1;
  phone.object.scale.setScalar(phone.heightScale * scale);
  phone.object.position.set(0, 0, 0);
  phone.object.updateMatrixWorld(true);
  const box = new THREE.Box3().setFromObject(phone.object, true);
  const caseCenter = caseBox.getCenter(new THREE.Vector3());
  const phoneCenter = box.getCenter(new THREE.Vector3());
  phone.object.position.set(caseCenter.x - phoneCenter.x, caseCenter.y - phoneCenter.y, caseBox.max.z - nest.insetMm - box.max.z);
  phone.object.updateMatrixWorld(true);
}
