import type { ReactNode } from 'react'

interface PageHeaderProps {
  /** Mono breadcrumb eyebrow, e.g. "/LEADERBOARD". */
  eyebrow: string
  title: string
  subtitle?: string
  action?: ReactNode
}

const PageHeader = ({ eyebrow, title, subtitle, action }: PageHeaderProps) => (
  <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
    <div className="flex flex-col gap-1">
      <span className="font-mono text-[11px] tracking-[1.1px] text-dim uppercase">
        {eyebrow}
      </span>
      <h1 className="text-[28px] leading-tight font-semibold tracking-[-0.28px] text-fg">
        {title}
      </h1>
      {subtitle && <p className="text-[13px] text-muted">{subtitle}</p>}
    </div>
    {action && <div className="flex flex-wrap items-center gap-2">{action}</div>}
  </div>
)

export default PageHeader
