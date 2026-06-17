interface SkeletonRowsProps {
  count?: number
  height?: string
}

const SkeletonRows = ({ count = 6, height = 'h-14' }: SkeletonRowsProps) => (
  <div className="space-y-px">
    {Array.from({ length: count }).map((_, i) => (
      <div key={i} className={`${height} animate-pulse bg-[#11161d]`} />
    ))}
  </div>
)

export default SkeletonRows
