import type { ReactNode } from 'react'

interface KeycapHintProps {
  keys: string
  children: ReactNode
}

/** A keycap chip followed by a label, e.g. [WASD] move. */
const KeycapHint = ({ keys, children }: KeycapHintProps) => (
  <span className="inline-flex items-center gap-2 font-mono text-[11px] text-dim">
    <kbd className="rounded-[3px] border border-line bg-raised px-2 py-1 text-muted">
      {keys}
    </kbd>
    {children}
  </span>
)

export default KeycapHint
