import { useEffect, useState } from 'react'
import { client } from '../api/client'
import type { Match } from '../types'
import ErrorBanner from '../components/ui/ErrorBanner'
import PageHeader from '../components/ui/PageHeader'
import SkeletonRows from '../components/ui/SkeletonRows'

const PAGE_SIZE = 10

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString('fr-FR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })

const formatDuration = (secs: number) => {
  const m = Math.floor(secs / 60)
  const s = Math.round(secs % 60)
  return `${m}:${s.toString().padStart(2, '0')}`
}

const TABLE_HEADERS = ['Result', 'Score', 'Duration', 'Date']

const HistoryPage = () => {
  const [matches, setMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [offset, setOffset] = useState(0)
  const [hasMore, setHasMore] = useState(true)

  const fetchMatches = async (currentOffset: number, reset: boolean) => {
    setLoading(true)
    setError(null)
    try {
      const { data } = await client.get<Match[]>('/api/v1/matches', {
        params: { limit: PAGE_SIZE, offset: currentOffset },
      })
      setMatches(prev => (reset ? data : [...prev, ...data]))
      setHasMore(data.length === PAGE_SIZE)
      setOffset(currentOffset + data.length)
    } catch {
      setError('Could not load match history. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchMatches(0, true)
  }, [])

  return (
    <div className="mx-auto max-w-5xl px-6 py-10">
      <PageHeader section="Personal" title="Match History" />

      {error && <ErrorBanner message={error} />}

      {!loading && matches.length === 0 ? (
        <p className="py-16 text-center font-mono text-sm text-[#98a1ad]">
          No matches played yet.
        </p>
      ) : (
        <>
          <div className="overflow-x-auto border border-[#2a313b]">
            <div className="min-w-[480px]">
            <div className="grid grid-cols-4 border-b border-[#2a313b] px-5 py-3">
              {TABLE_HEADERS.map(h => (
                <span
                  key={h}
                  className="font-mono text-[10.5px] tracking-[1.05px] uppercase text-[#98a1ad]"
                >
                  {h}
                </span>
              ))}
            </div>

            {matches.map((match, i) => {
              const won = match.winnerTeam === 1
              return (
                <div
                  key={match.id}
                  className={`grid grid-cols-4 items-center px-5 py-4 transition-colors hover:bg-[#11161d] ${
                    i < matches.length - 1 ? 'border-b border-[#2a313b]/60' : ''
                  }`}
                >
                  <span
                    className={`font-mono text-xs font-bold tracking-[1px] uppercase ${
                      won ? 'text-[#5ebc7b]' : 'text-[#ee6951]'
                    }`}
                  >
                    {won ? 'Victory' : 'Defeat'}
                  </span>
                  <span className="font-mono text-sm text-[#e7ecef]">
                    {match.playerScore}
                    <span className="mx-1.5 text-[#98a1ad]">—</span>
                    {match.aiScore}
                  </span>
                  <span className="font-mono text-sm text-[#98a1ad]">
                    {formatDuration(match.duration)}
                  </span>
                  <span className="font-mono text-xs text-[#98a1ad]">
                    {formatDate(match.createdAt)}
                  </span>
                </div>
              )
            })}

            {loading && <SkeletonRows count={3} height="h-[60px]" />}
            </div>
          </div>

          {!loading && hasMore && (
            <button
              onClick={() => fetchMatches(offset, false)}
              className="mt-4 w-full border border-[#2a313b] py-3 font-mono text-[11px] tracking-[1.1px] uppercase text-[#98a1ad] transition-colors hover:border-[#5dcbd1] hover:text-[#5dcbd1]"
            >
              Load more
            </button>
          )}
        </>
      )}
    </div>
  )
}

export default HistoryPage
