import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { client } from '../api/client'
import { useAuth } from '../hooks/useAuth'
import type { Player } from '../types'
import PageContainer from '../components/ui/PageContainer'
import PageHeader from '../components/ui/PageHeader'
import Panel from '../components/ui/Panel'
import Button from '../components/ui/Button'
import Avatar from '../components/ui/Avatar'
import Badge from '../components/ui/Badge'
import RankBadge from '../components/ui/RankBadge'
import ProgressBar from '../components/ui/ProgressBar'
import DataTable, { type Column } from '../components/ui/DataTable'
import SkeletonRows from '../components/ui/SkeletonRows'
import EmptyState from '../components/ui/EmptyState'
import ErrorBanner from '../components/ui/ErrorBanner'

const winRateOf = (p: Player) =>
  p.stats.totalMatches > 0
    ? Math.round((p.stats.wins / p.stats.totalMatches) * 100)
    : 0

const LeaderboardPage = () => {
  const { player: me } = useAuth()
  const navigate = useNavigate()
  const [players, setPlayers] = useState<Player[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchPlayers = async () => {
    setLoading(true)
    setError(null)
    try {
      const { data } = await client.get<Player[]>('/api/v1/players')
      setPlayers(data ?? [])
    } catch {
      setError('Could not load leaderboard. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchPlayers()
  }, [])

  const columns: Column<Player>[] = [
    {
      key: 'rank',
      header: '#',
      headerClassName: 'w-16',
      render: (_p, i) => <RankBadge rank={i + 1} />,
    },
    {
      key: 'player',
      header: 'Player',
      render: p => {
        const isMe = p.id === me?.id
        return (
          <div className="flex items-center gap-3">
            <Avatar name={p.username} size="sm" />
            <span className="text-sm text-fg">{p.username}</span>
            {isMe && <Badge tone="win">You</Badge>}
          </div>
        )
      },
    },
    {
      key: 'score',
      header: 'Score',
      align: 'right',
      render: p => (
        <span className="font-mono text-sm font-bold text-fg">{p.stats.totalScore}</span>
      ),
    },
    {
      key: 'wins',
      header: 'Wins',
      align: 'right',
      render: p => <span className="font-mono text-sm text-win">{p.stats.wins}</span>,
    },
    {
      key: 'losses',
      header: 'Losses',
      align: 'right',
      render: p => <span className="font-mono text-sm text-loss">{p.stats.losses}</span>,
    },
    {
      key: 'matches',
      header: 'Matches',
      align: 'right',
      render: p => (
        <span className="font-mono text-sm text-muted">{p.stats.totalMatches}</span>
      ),
    },
    {
      key: 'winrate',
      header: 'Win rate',
      align: 'right',
      render: p => {
        const rate = winRateOf(p)
        return (
          <div className="flex items-center justify-end gap-3">
            <ProgressBar value={rate} className="w-24" />
            <span className="w-9 text-right font-mono text-sm text-muted">{rate}%</span>
          </div>
        )
      },
    },
  ]

  return (
    <PageContainer>
      <PageHeader
        eyebrow="/LEADERBOARD"
        title="Leaderboard"
        subtitle="Top players ranked by total score. You are highlighted in green."
        action={
          <>
            <Button variant="ghost" onClick={fetchPlayers} disabled={loading}>
              {loading ? 'Loading…' : 'Refresh'}
            </Button>
            <Button variant="primary" onClick={() => navigate('/play')}>
              Start game
            </Button>
          </>
        }
      />

      {error && <ErrorBanner message={error} />}

      <Panel
        header="Global ranking"
        meta={loading ? '—' : `${players.length} players`}
      >
        {loading ? (
          <div className="p-4">
            <SkeletonRows count={8} />
          </div>
        ) : players.length === 0 ? (
          <EmptyState message="No players yet." />
        ) : (
          <DataTable
            columns={columns}
            rows={players}
            rowKey={p => p.id}
            rowClassName={p =>
              p.id === me?.id
                ? 'bg-win/[0.06] [&>td:first-child]:border-l-2 [&>td:first-child]:border-win'
                : 'hover:bg-raised/40'
            }
            minWidth="min-w-[720px]"
          />
        )}
      </Panel>
    </PageContainer>
  )
}

export default LeaderboardPage
