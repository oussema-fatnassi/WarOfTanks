import { useEffect, useRef, useState } from 'react'
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

const UNITY_BUILD_URL = '/UnityBuild/index.html'

/** Hosts the Unity WebGL build inside the tactical viewport. */
const GameViewport = ({ accessToken }: { accessToken: string | null }) => {
  const iframeRef = useRef<HTMLIFrameElement>(null)

  useEffect(() => {
    const sendConfiguration = () => {
      iframeRef.current?.contentWindow?.postMessage(
        {
          type: 'wot:web-client-config',
          apiBaseUrl: import.meta.env.VITE_API_URL ?? '',
          accessToken: accessToken ?? '',
        },
        window.location.origin,
      )
    }

    const handleMessage = (event: MessageEvent) => {
      if (
        event.origin !== window.location.origin ||
        event.source !== iframeRef.current?.contentWindow ||
        event.data?.type !== 'wot:unity-ready'
      ) {
        return
      }

      sendConfiguration()
    }

    window.addEventListener('message', handleMessage)
    sendConfiguration()
    return () => window.removeEventListener('message', handleMessage)
  }, [accessToken])

  return (
    <div className="rounded-card border-line relative min-h-[460px] overflow-hidden border bg-[#0b0e13] lg:min-h-[640px]">
      <iframe
        ref={iframeRef}
        title="War of Tanks"
        src={UNITY_BUILD_URL}
        className="absolute inset-0 h-full w-full border-0"
        allow="fullscreen; autoplay; gamepad"
      />
    </div>
  )
}

const GamePage = () => {
  const { player, accessToken } = useAuth()
  const navigate = useNavigate()
  const [lastMatch, setLastMatch] = useState<Match | null>(null)

  useEffect(() => {
    client
      .get<Match[]>('/api/v1/matches', { params: { limit: 1 } })
      .then((res) => setLastMatch(res.data?.[0] ?? null))
      .catch(() => setLastMatch(null))
  }, [])

  const won = lastMatch?.winnerTeam === 1

  return (
    <PageContainer>
      <div className="grid gap-6 lg:grid-cols-[1fr_340px]">
        <GameViewport accessToken={accessToken} />

        <aside className="flex flex-col gap-4">
          <Panel header="Pilot">
            <div className="flex items-center gap-3 p-[18px]">
              {player && <Avatar name={player.username} size="lg" />}
              <div className="flex flex-col">
                <span className="text-fg text-sm font-medium">
                  {player?.username}
                </span>
                <span className="text-dim font-mono text-[11px]">pilot</span>
              </div>
            </div>
          </Panel>

          {lastMatch && (
            <Panel header="Last match">
              <div className="flex flex-col gap-3 p-[18px]">
                <div className="flex items-center justify-between">
                  <span className="font-mono text-[22px] font-bold">
                    <span className="text-win">{lastMatch.playerScore}</span>
                    <span className="text-dim mx-1">:</span>
                    <span className="text-loss">{lastMatch.aiScore}</span>
                  </span>
                  <ResultBadge won={won} />
                </div>
                <div className="text-dim flex items-center justify-between font-mono text-[11px]">
                  <span>Duration · {formatDuration(lastMatch.duration)}</span>
                  <span>{formatDateTime(lastMatch.createdAt)}</span>
                </div>
              </div>
            </Panel>
          )}

          <Panel header="Tips">
            <p className="text-muted p-[18px] text-[13px] leading-relaxed">
              Hold the central <span className="text-win">ZONE</span> to gain
              points over time. First team to{' '}
              <span className="text-win">5</span> captures wins the round.
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
