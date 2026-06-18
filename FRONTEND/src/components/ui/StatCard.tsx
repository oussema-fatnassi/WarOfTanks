import type { ReactNode } from 'react'
import Eyebrow from './Eyebrow'

interface StatCardProps {
  label: string
  value: ReactNode
  /** Color of the big value. */
  tone?: 'default' | 'win' | 'loss'
  /** Optional mono footnote under the value. */
  delta?: ReactNode
}

const tones = {
  default: 'text-fg',
  win: 'text-win',
  loss: 'text-loss',
} as const

const StatCard = ({ label, value, tone = 'default', delta }: StatCardProps) => (
  <div className="flex flex-col gap-1 rounded-card border border-line bg-panel p-[21px]">
    <Eyebrow>{label}</Eyebrow>
    <p
      className={`pt-0.5 text-[40px] leading-none font-semibold tracking-[-0.8px] ${tones[tone]}`}
    >
      {value}
    </p>
    {delta && <p className="font-mono text-[11px] text-muted">{delta}</p>}
  </div>
)

export default StatCard
