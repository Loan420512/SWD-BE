import { useEffect, useRef, useState } from 'react';
import { ArrowClockwise, ArrowsClockwise, ArrowsHorizontal, ArrowsVertical } from '@phosphor-icons/react';
import { DEFAULT_NEST, turnPose, type AssetPose, type PhoneNest } from '../../kiosk';
import { FitterView, type FitterHandle } from '../../stage';
import { Glyph } from '../../app/glyph';

export interface PoseEditorProps {
  readonly url: string;
  readonly pose: AssetPose;
  readonly heightMm: number;
  readonly phone?: { readonly url: string; readonly pose: AssetPose } | null;
  readonly nest?: PhoneNest;
  readonly onPose: (pose: AssetPose) => void;
  readonly onLoaded?: (size: { widthMm: number; heightMm: number; depthMm: number } | null) => void;
  readonly fitter?: React.RefObject<FitterHandle | null>;
}

/** Live preview of an imported GLB, seen from behind. Quarter turns change the saved pose. */
export function PoseEditor(props: PoseEditorProps) {
  const own = useRef<FitterHandle>(null);
  const ref = props.fitter ?? own;
  const [error, setError] = useState<string | null>(null);
  const onLoaded = useRef(props.onLoaded);
  onLoaded.current = props.onLoaded;
  const nestRef = useRef(props.nest ?? DEFAULT_NEST);
  nestRef.current = props.nest ?? DEFAULT_NEST;

  const phoneUrl = props.phone?.url ?? '';
  const phoneBack = props.phone?.pose.back ?? '';
  const phoneUp = props.phone?.pose.up ?? '';

  useEffect(() => {
    let live = true;
    const stage = ref.current?.stage;
    if (!stage || !props.url || !(props.heightMm > 0)) return undefined;
    setError(null);
    stage.setNest(nestRef.current);
    stage
      .show(props.url, props.pose, props.heightMm, props.phone ?? null)
      .then((loaded) => {
        if (live) onLoaded.current?.(loaded);
      })
      .catch(() => {
        if (!live) return;
        setError('Không đọc được file GLB này.');
        onLoaded.current?.(null);
      });
    return () => {
      live = false;
    };
  }, [props.url, props.pose, props.heightMm, phoneUrl, phoneBack, phoneUp, ref]);

  return (
    <div className="pose-editor">
      <FitterView ref={ref} className="stage-fitter" />
      <div className="pose-tools">
        <div className="row">
          <button type="button" className="button small ghost" onClick={() => ref.current?.stage?.orbit('y')}>
            <Glyph icon={ArrowsHorizontal} />
            Xoay ngang 90°
          </button>
          <button
            type="button"
            className="button small ghost"
            onClick={() => {
              ref.current?.stage?.orbit('y');
              ref.current?.stage?.orbit('y');
            }}
          >
            <Glyph icon={ArrowsClockwise} />
            Lật mặt trước, sau
          </button>
          <button type="button" className="button small ghost" onClick={() => ref.current?.stage?.orbit('x')}>
            <Glyph icon={ArrowsVertical} />
            Lật dọc 90°
          </button>
          <button type="button" className="button small ghost" onClick={() => ref.current?.stage?.orbit('z')}>
            <Glyph icon={ArrowClockwise} />
            Xoay trong mặt phẳng 90°
          </button>
        </div>
        <p className="meta">
          Mặt lưng {props.pose.back}, cạnh trên {props.pose.up}
        </p>
        <div className="row">
          <button type="button" className="button small ghost" onClick={() => props.onPose(turnPose(props.pose, 'y'))}>
            <Glyph icon={ArrowsHorizontal} />
            Trục ốp ngang 90°
          </button>
          <button type="button" className="button small ghost" onClick={() => props.onPose(turnPose(props.pose, 'x'))}>
            <Glyph icon={ArrowsVertical} />
            Trục ốp dọc 90°
          </button>
          <button type="button" className="button small ghost" onClick={() => props.onPose(turnPose(props.pose, 'z'))}>
            <Glyph icon={ArrowClockwise} />
            Trục ốp trong mặt 90°
          </button>
        </div>
        {error ? <p className="error">{error}</p> : null}
      </div>
    </div>
  );
}
