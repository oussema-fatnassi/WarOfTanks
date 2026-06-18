interface ProgressBarProps {
  /** 0–100 */
  value: number
  className?: string
  tone?: 'win' | 'loss'
}

/** Thin progress bar on a dark track. */
const ProgressBar = ({ value, className = '', tone = 'win' }: ProgressBarProps) => {
  const pct = Math.max(0, Math.min(100, value))
  return (
    <div className={`h-1.5 overflow-hidden rounded-pill bg-line ${className}`}>
      <div
        className={`h-full rounded-pill transition-all duration-500 ${tone === 'win' ? 'bg-win' : 'bg-loss'}`}
        style={{ width: `${pct}%` }}
      />
    </div>
  )
}

export default ProgressBar
