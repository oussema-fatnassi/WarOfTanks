export interface SegmentOption<T extends string> {
  value: T
  label: string
  tone?: 'default' | 'win' | 'loss'
}

interface SegmentedFilterProps<T extends string> {
  options: SegmentOption<T>[]
  value: T
  onChange: (value: T) => void
}

const activeTone = {
  default: 'border-line-strong bg-raised text-fg',
  win: 'border-win bg-win/10 text-win',
  loss: 'border-loss bg-loss/10 text-loss',
} as const

/** Row of mutually-exclusive outline filter buttons. */
function SegmentedFilter<T extends string>({
  options,
  value,
  onChange,
}: SegmentedFilterProps<T>) {
  return (
    <div className="flex items-center gap-2">
      {options.map(opt => {
        const active = opt.value === value
        return (
          <button
            key={opt.value}
            type="button"
            onClick={() => onChange(opt.value)}
            className={`rounded-card border px-4 py-2 text-[13px] font-medium transition-colors ${
              active
                ? activeTone[opt.tone ?? 'default']
                : 'border-line text-muted hover:border-line-strong hover:text-fg'
            }`}
          >
            {opt.label}
          </button>
        )
      })}
    </div>
  )
}

export default SegmentedFilter
