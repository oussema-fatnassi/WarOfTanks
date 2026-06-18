import type { ReactNode } from 'react'

interface EyebrowProps {
  children: ReactNode
  className?: string
}

/** Mono uppercase micro-label used for breadcrumbs, card labels and panel headers. */
const Eyebrow = ({ children, className = '' }: EyebrowProps) => (
  <span
    className={`font-mono text-[10.5px] tracking-[1.26px] text-dim uppercase ${className}`}
  >
    {children}
  </span>
)

export default Eyebrow
