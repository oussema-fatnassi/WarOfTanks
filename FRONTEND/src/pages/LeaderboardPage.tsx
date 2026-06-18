import { useEffect, useState } from 'react'
import { client } from '../api/client'
import { useAuth } from '../hooks/useAuth'
import type { Player } from '../types'
import ErrorBanner from '../components/ui/ErrorBanner'
import PageHeader from '../components/ui/PageHeader'
import SkeletonRows from '../components/ui/SkeletonRows'

const LeaderboardPage = () => {
  const { player: me } = useAuth()
  const [players, setPlayers] = useState<Player[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchPlayers = async () => {
    setLoading(true)
    setError(null)
    try {
      const { data } = await client.get<Player[]>('/api/v1/players')
      setPlayers(data)
    } catch {
      setError('Could not load leaderboard. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchPlayers()
  }, [])

  return (
    <div className="mx-auto max-w-5xl px-6 py-10">
      <PageHeader
        section="Rankings"
        title="Leaderboard"
        action={
          <button
            onClick={fetchPlayers}
            disabled={loading}
            className="font-mono text-[11px] tracking-[1.1px] uppercase text-[#98a1ad] transition-colors hover:text-[#5dcbd1] disabled:opacity-40"
          >
            {loading ? 'Loading…' : 'Refresh'}
          </button>
        }
      />

      {error && <ErrorBanner message={error} />}

      {loading ? (
        <SkeletonRows count={6} />
      ) : players.length === 0 ? (
        <p className="py-16 text-center font-mono text-sm text-[#98a1ad]">
          No players yet.
        </p>
      ) : (
        <div className="overflow-x-auto">
        <table className="w-full min-w-[500px] border-collapse">
          <thead>
            <tr className="border-b border-[#2a313b]">
              {['#', 'Player', 'Score', 'W', 'L', 'Win %'].map((h, i) => (
                <th
                  key={h}
                  className={`pb-3 font-mono text-[10.5px] tracking-[1.05px] uppercase text-[#98a1ad] ${
                    i === 0 ? 'w-12 text-left' : i === 1 ? 'text-left' : 'text-right'
                  }`}
                >
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {players.map((p, i) => {
              const isMe = p.id === me?.id
              const winRate =
                p.stats.totalMatches > 0
                  ? Math.round((p.stats.wins / p.stats.totalMatches) * 100)
                  : 0
              return (
                <tr
                  key={p.id}
                  className={`border-b border-[#2a313b]/60 transition-colors ${
                    isMe ? 'bg-[#5dcbd1]/[0.06]' : 'hover:bg-[#11161d]'
                  }`}
                >
                  <td className="py-4 font-mono text-xs text-[#98a1ad]">
                    {String(i + 1).padStart(2, '0')}
                  </td>
                  <td className="py-4 text-sm text-[#e7ecef]">
                    {p.username}
                    {isMe && (
                      <span className="ml-2 font-mono text-[10px] tracking-[0.5px] text-[#5dcbd1]">
                        you
                      </span>
                    )}
                  </td>
                  <td className="py-4 text-right font-mono text-sm text-[#e7ecef]">
                    {p.stats.totalScore}
                  </td>
                  <td className="py-4 text-right font-mono text-sm text-[#5ebc7b]">
                    {p.stats.wins}
                  </td>
                  <td className="py-4 text-right font-mono text-sm text-[#ee6951]">
                    {p.stats.losses}
                  </td>
                  <td className="py-4 text-right font-mono text-sm text-[#98a1ad]">
                    {winRate}%
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
        </div>
      )}
    </div>
  )
}

export default LeaderboardPage
