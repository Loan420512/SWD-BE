import type { Icon, IconWeight } from '@phosphor-icons/react';

/** One weight and one size for every control icon. */
export function Glyph({ icon: IconMark, weight = 'bold' }: { icon: Icon; weight?: IconWeight }) {
  return <IconMark size={18} weight={weight} aria-hidden focusable="false" />;
}
