import type { ReactNode } from 'react'

interface PageHeaderProps {
  section: string
  title: string
  action?: ReactNode
}

const PageHeader = ({ section, title, action }: PageHeaderProps) => (
  <div className="mb-8 flex items-end justify-between border-b border-[#2a313b] pb-5">
    <div>
      <p className="mb-1 font-mono text-[10.5px] tracking-[1.05px] uppercase text-[#98a1ad]">
        {section}
      </p>
      <p className="text-[22px] font-semibold leading-tight tracking-tight text-[#e7ecef]">
        {title}
      </p>
    </div>
    {action && <div>{action}</div>}
  </div>
)

export default PageHeader
