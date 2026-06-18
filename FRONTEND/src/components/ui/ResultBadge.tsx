import Badge from './Badge'

interface ResultBadgeProps {
  won: boolean
}

/** Victory / Defeat pill. */
const ResultBadge = ({ won }: ResultBadgeProps) => (
  <Badge tone={won ? 'win' : 'loss'} dot>
    {won ? 'Victory' : 'Defeat'}
  </Badge>
)

export default ResultBadge
