import { createContext, useContext, useSyncExternalStore, type ReactElement, type ReactNode } from 'react';
import type { Kiosk } from './kiosk';
import type { Session } from './session';

const KioskContext = createContext<Kiosk | null>(null);

export function KioskProvider(props: { readonly kiosk: Kiosk; readonly children: ReactNode }): ReactElement {
  return <KioskContext.Provider value={props.kiosk}>{props.children}</KioskContext.Provider>;
}

export function useKiosk(): Kiosk {
  const kiosk = useContext(KioskContext);
  if (!kiosk) throw new Error('useKiosk needs KioskProvider');
  return kiosk;
}

export function useSession(): Session {
  const kiosk = useKiosk();
  return useSyncExternalStore(kiosk.subscribe, kiosk.getState, kiosk.getState);
}
