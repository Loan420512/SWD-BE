import { useMemo } from 'react';
import type { CaseOffer, PhoneModel } from '../kiosk';
import type { StagePlacement, StageView } from '../stage';

/** Plain data for the 3D stage. Null when the case has no imported model. */
export function useStageView(
  model: PhoneModel | null,
  offer: CaseOffer | null,
  placements: readonly StagePlacement[],
  editable: boolean,
): StageView | null {
  const signature = placements.map((item) => `${item.key}:${item.imageUrl}:${item.at.x},${item.at.y}:${item.rotationDeg}:${item.active}`).join('|');
  return useMemo(() => {
    const fit = offer?.fit;
    if (!fit) return null;
    return {
      caseModel: { url: fit.modelUrl, pose: fit.pose, sheet: fit.sheet, nest: fit.nest },
      phone: model?.phone ? { url: model.phone.modelUrl, pose: model.phone.pose } : null,
      placements,
      editable,
    };
  }, [model, offer, signature, editable]);
}
