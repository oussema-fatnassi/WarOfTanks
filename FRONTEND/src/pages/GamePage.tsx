import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { client } from '../api/client'
import { useAuth } from '../hooks/useAuth'
import type { Match } from '../types'
import { formatDateTime, formatDuration } from '../utils/format'
import PageContainer from '../components/ui/PageContainer'
import Panel from '../components/ui/Panel'
import Button from '../components/ui/Button'
import Avatar from '../components/ui/Avatar'
import ResultBadge from '../components/ui/ResultBadge'

const UNITY_BUILD_URL = '/UnityBuild/UnityBuild/index.html'

/** Hosts the Unity WebGL build inside the tactical viewport. */
const GameViewport = () => (
  <div className="relative min-h-[460px] overflow-hidden rounded-card border border-line bg-[#0b0e13] lg:min-h-[640px]">
    <iframe
      title="War of Tanks"
      src={UNITY_BUILD_URL}
      className="absolute inset-0 h-full w-full border-0"
      allow="fullscreen; autoplay; gamepad"
      allowFullScreen
    />
  </div>
)

const GamePage = () => {
  const { player } = useAuth()
  const navigate = useNavigate()
  const [lastMatch, setLastMatch] = useState<Match | null>(null)

  useEffect(() => {
    client
      .get<Match[]>('/api/v1/matches', { params: { limit: 1 } })
      .then(res => setLastMatch(res.data?.[0] ?? null))
      .catch(() => setLastMatch(null))
  }, [])

  const won = lastMatch?.winnerTeam === 1

  return (
    <PageContainer>
      <div className="grid gap-6 lg:grid-cols-[1fr_340px]">
        <GameViewport />

        <aside className="flex flex-col gap-4">
          <Panel header="Pilot">
            <div className="flex items-center gap-3 p-[18px]">
              {player && <Avatar name={player.username} size="lg" />}
              <div className="flex flex-col">
                <span className="text-sm font-medium text-fg">
                  {player?.username}
                </span>
                <span className="font-mono text-[11px] text-dim">pilot</span>
              </div>
            </div>
          </Panel>

          {lastMatch && (
            <Panel header="Last match">
              <div className="flex flex-col gap-3 p-[18px]">
                <div className="flex items-center justify-between">
                  <span className="font-mono text-[22px] font-bold">
                    <span className="text-win">{lastMatch.playerScore}</span>
                    <span className="mx-1 text-dim">:</span>
                    <span className="text-loss">{lastMatch.aiScore}</span>
                  </span>
                  <ResultBadge won={won} />
                </div>
                <div className="flex items-center justify-between font-mono text-[11px] text-dim">
                  <span>Duration · {formatDuration(lastMatch.duration)}</span>
                  <span>{formatDateTime(lastMatch.createdAt)}</span>
                </div>
              </div>
            </Panel>
          )}

          <Panel header="Tips">
            <p className="p-[18px] text-[13px] leading-relaxed text-muted">
              Hold the central <span className="text-win">ZONE</span> to gain
              points over time. First team to <span className="text-win">5</span>{' '}
              captures wins the round.
            </p>
          </Panel>

          <div className="flex flex-col gap-2">
            <Button variant="outline" onClick={() => navigate('/stats')}>
              ← Back to Stats
            </Button>
            <Button variant="danger" onClick={() => navigate('/stats')}>
              Surrender match
            </Button>
          </div>
        </aside>
      </div>
    </PageContainer>
  )
}

export default GamePage
