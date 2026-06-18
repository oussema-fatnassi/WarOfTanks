import type { ReactNode } from 'react'

interface EmptyStateProps {
  message: string
  icon?: ReactNode
  className?: string
}

const EmptyState = ({ message, icon, className = '' }: EmptyStateProps) => (
  <div className={`flex flex-col items-center gap-3 py-16 text-center ${className}`}>
    {icon}
    <p className="font-mono text-sm text-muted">{message}</p>
  </div>
)

export default EmptyState
