import { useEffect, useState } from 'react'
import { client } from '../api/client'
import type { Player } from '../types'
import ErrorBanner from '../components/ui/ErrorBanner'
import PageHeader from '../components/ui/PageHeader'
import SkeletonRows from '../components/ui/SkeletonRows'
import StatCard from '../components/stats/StatCard'
import WinLossBar from '../components/stats/WinLossBar'

const StatsPage = () => {
  const [player, setPlayer] = useState<Player | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    client
      .get<Player>('/api/v1/players/me')
      .then(({ data }) => setPlayer(data))
      .catch(() => setError('Could not load your stats. Please try again.'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) {
    return (
      <div className="mx-auto max-w-5xl px-6 py-10">
        <div className="mb-8 h-7 w-48 animate-pulse bg-[#11161d]" />
        <SkeletonRows count={5} height="h-28" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="mx-auto max-w-5xl px-6 py-10">
        <ErrorBanner message={error} />
      </div>
    )
  }

  if (!player) return null

  const { stats } = player
  const winRate =
    stats.totalMatches > 0 ? Math.round((stats.wins / stats.totalMatches) * 100) : 0

  return (
    <div className="mx-auto max-w-5xl px-6 py-10">
      <PageHeader section="Personal" title={player.username} />

      <div className="mb-8 grid grid-cols-2 gap-px border border-[#2a313b] lg:grid-cols-3">
        <StatCard label="Total Score" value={stats.totalScore} accent />
        <StatCard label="Matches" value={stats.totalMatches} />
        <StatCard label="Win Rate" value={`${winRate}%`} />
        <StatCard label="Wins" value={stats.wins} />
        <StatCard label="Losses" value={stats.losses} />
      </div>

      {stats.totalMatches > 0 && <WinLossBar winRate={winRate} />}
    </div>
  )
}

export default StatsPage
