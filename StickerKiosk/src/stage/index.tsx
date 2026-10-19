import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react';
import type { BackPoint, StickerSize } from '../kiosk';
import { mountFitter, type BakeResult, type FitterStage } from './fitter';
import { mountProduct, type ProductStage, type StageView } from './product';

export type { StagePlacement, StageView } from './product';
export type { BakeResult, BrushMode, FitterStage } from './fitter';

export interface CaseStageHandle {
  pointToBack(clientX: number, clientY: number): BackPoint | null;
  preview(clientX: number, clientY: number, sticker: ({ readonly imageUrl: string } & StickerSize) | null): void;
  resetView(): void;
}

export interface CaseStageProps {
  readonly view: StageView | null;
  readonly onCommit?: (key: string, at: BackPoint) => void;
  readonly onNudge?: (dx: number, dy: number) => void;
  readonly onRotateTo?: (key: string, rotationDeg: number) => void;
  readonly onSelect?: (key: string) => void;
  readonly onReject?: () => void;
  readonly className?: string;
}

/** The phone case on the bench. The sticker only ever lands on the case. */
export const CaseStage = forwardRef<CaseStageHandle, CaseStageProps>(function CaseStage(props, ref) {
  const hostRef = useRef<HTMLDivElement>(null);
  const stageRef = useRef<ProductStage | null>(null);
  const hooks = useRef(props);
  hooks.current = props;
  const [status, setStatus] = useState<'loading' | 'ready' | 'error'>('loading');

  useEffect(() => {
    const host = hostRef.current;
    if (!host) return undefined;
    let stage: ProductStage;
    try {
      stage = mountProduct(host, {
        onCommit: (key, at) => hooks.current.onCommit?.(key, at),
        onNudge: (dx, dy) => hooks.current.onNudge?.(dx, dy),
        onRotateTo: (key, rotationDeg) => hooks.current.onRotateTo?.(key, rotationDeg),
        onSelect: (key) => hooks.current.onSelect?.(key),
        onReject: () => hooks.current.onReject?.(),
        onStatus: setStatus,
      });
    } catch {
      setStatus('error');
      return undefined;
    }
    stageRef.current = stage;
    return () => {
      stage.dispose();
      stageRef.current = null;
    };
  }, []);

  useEffect(() => {
    if (props.view) stageRef.current?.show(props.view);
    else stageRef.current?.clear();
  }, [props.view]);

  useImperativeHandle(ref, () => ({
    pointToBack: (x, y) => stageRef.current?.pointToBack(x, y) ?? null,
    preview: (x, y, sticker) => stageRef.current?.preview(x, y, sticker),
    resetView: () => stageRef.current?.resetView(),
  }));

  return (
    <div ref={hostRef} className={`stage ${props.className ?? ''}`} data-status={props.view ? status : 'empty'}>
      {!props.view ? <p className="stage-note">Chưa có ốp nào có model 3D.</p> : null}
      {props.view && status === 'loading' ? <p className="stage-note">Đang tải model…</p> : null}
      {status === 'error' ? <p className="stage-note">Không hiển thị được model 3D trên máy này.</p> : null}
    </div>
  );
});

export interface FitterHandle {
  readonly stage: FitterStage | null;
}

export const FitterView = forwardRef<FitterHandle, { readonly className?: string }>(function FitterView(props, ref) {
  const hostRef = useRef<HTMLDivElement>(null);
  const stageRef = useRef<FitterStage | null>(null);

  useEffect(() => {
    const host = hostRef.current;
    if (!host) return undefined;
    const stage = mountFitter(host);
    stageRef.current = stage;
    return () => {
      stage.dispose();
      stageRef.current = null;
    };
  }, []);

  useImperativeHandle(ref, () => ({
    get stage() {
      return stageRef.current;
    },
  }));

  return <div ref={hostRef} className={`stage ${props.className ?? ''}`} />;
});

export type { BakeResult as Bake };
