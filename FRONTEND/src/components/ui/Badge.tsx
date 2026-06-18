import type { ReactNode } from 'react'

export type BadgeTone = 'win' | 'loss' | 'neutral'

interface BadgeProps {
  children: ReactNode
  tone?: BadgeTone
  dot?: boolean
  className?: string
}

const tones: Record<BadgeTone, string> = {
  win: 'border-win bg-win/10 text-win',
  loss: 'border-loss bg-loss/10 text-loss',
  neutral: 'border-line bg-raised text-muted',
}

/** Outlined pill with optional leading dot. */
const Badge = ({ children, tone = 'neutral', dot = false, className = '' }: BadgeProps) => (
  <span
    className={`inline-flex items-center gap-1.5 rounded-pill border px-[9px] py-[5px] font-mono text-[10.5px] font-bold tracking-[1.05px] uppercase ${tones[tone]} ${className}`}
  >
    {dot && <span className="size-1.5 rounded-[2px] bg-current" />}
    {children}
  </span>
)

export default Badge
