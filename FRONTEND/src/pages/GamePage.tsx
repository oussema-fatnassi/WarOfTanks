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

const GAME_CONTROLS = [
  { keys: ['Left click'], action: 'Select a tank' },
  { keys: ['Left drag'], action: 'Box select tanks' },
  { keys: ['Shift', 'Left click'], action: 'Add or remove a tank' },
  { keys: ['Right click ground'], action: 'Move selected tanks' },
  { keys: ['Right click enemy'], action: 'Attack an enemy' },
  { keys: ['A', 'Right click'], action: 'Attack a zone' },
  { keys: ['S'], action: 'Stop selected tanks' },
  { keys: ['Screen edge'], action: 'Move the camera' },
]

/** Hosts the Unity WebGL build inside the tactical viewport. */
const GameViewport = ({ accessToken }: { accessToken: string | null }) => {
  const iframeRef = useRef<HTMLIFrameElement>(null)
  const viewportRef = useRef<HTMLDivElement>(null)
  const [isFullscreen, setIsFullscreen] = useState(false)

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

  useEffect(() => {
    const handleFullscreenChange = () => {
      setIsFullscreen(document.fullscreenElement === viewportRef.current)
    }

    document.addEventListener('fullscreenchange', handleFullscreenChange)
    return () =>
      document.removeEventListener('fullscreenchange', handleFullscreenChange)
  }, [])

  const toggleFullscreen = async () => {
    if (document.fullscreenElement) {
      await document.exitFullscreen()
      return
    }

    await viewportRef.current?.requestFullscreen()
  }

  return (
    <div
      ref={viewportRef}
      className={`rounded-card border-line relative overflow-hidden border bg-[#0b0e13] ${
        isFullscreen ? 'h-screen w-screen' : 'aspect-video w-full'
      }`}
    >
      <button
        type="button"
        onClick={toggleFullscreen}
        className="border-line bg-panel/90 text-muted hover:border-win hover:text-fg absolute top-3 right-3 z-10 flex items-center gap-2 rounded border px-3 py-2 font-mono text-[11px] uppercase tracking-[0.08em] backdrop-blur transition-colors"
        aria-label={isFullscreen ? 'Exit fullscreen' : 'Enter fullscreen'}
      >
        {isFullscreen ? (
          <svg viewBox="0 0 24 24" className="h-4 w-4" aria-hidden="true">
            <path
              d="M9 4v5H4M15 4v5h5M9 20v-5H4M15 20v-5h5"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinecap="square"
            />
          </svg>
        ) : (
          <svg viewBox="0 0 24 24" className="h-4 w-4" aria-hidden="true">
            <path
              d="M9 4H4v5M15 4h5v5M9 20H4v-5M15 20h5v-5"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinecap="square"
            />
          </svg>
        )}
        {isFullscreen ? 'Exit fullscreen' : 'Fullscreen'}
      </button>
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
            <div className="flex flex-col">
              <p className="text-muted p-[18px] text-[13px] leading-relaxed">
                Hold the central <span className="text-win">ZONE</span> to gain
                points over time. First team to{' '}
                <span className="text-win">5</span> captures wins the round.
              </p>
              <div className="border-line border-t px-[18px] py-4">
                <h3 className="text-dim mb-3 font-mono text-[10px] uppercase tracking-[0.16em]">
                  Controls
                </h3>
                <dl className="flex flex-col gap-2.5">
                  {GAME_CONTROLS.map(({ keys, action }) => (
                    <div
                      key={action}
                      className="flex items-center justify-between gap-3"
                    >
                      <dt className="text-muted text-[12px]">{action}</dt>
                      <dd className="flex shrink-0 items-center gap-1">
                        {keys.map((key) => (
                          <kbd
                            key={key}
                            className="border-line bg-bg text-fg rounded border px-1.5 py-0.5 font-mono text-[9px] uppercase"
                          >
                            {key}
                          </kbd>
                        ))}
                      </dd>
                    </div>
                  ))}
                </dl>
              </div>
            </div>
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
