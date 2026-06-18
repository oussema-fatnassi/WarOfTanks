interface RankBadgeProps {
  rank: number
}

/** Mono rank number; top-3 highlighted in gold. */
const RankBadge = ({ rank }: RankBadgeProps) => (
  <span
    className={`font-mono text-[13px] ${rank <= 3 ? 'text-gold' : 'text-muted'}`}
  >
    {String(rank).padStart(2, '0')}
  </span>
)

export default RankBadge
