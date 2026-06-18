interface Side {
  label: string
  score: number
  sub?: string
}

interface ScoreboardProps {
  ally: Side
  enemy: Side
}

const Column = ({ side, tone }: { side: Side; tone: 'win' | 'loss' }) => (
  <div className="flex flex-col items-center gap-1 text-center">
    <span
      className={`font-mono text-[11px] tracking-[1.54px] uppercase ${tone === 'win' ? 'text-win' : 'text-loss'}`}
    >
      {side.label}
    </span>
    <span
      className={`text-[64px] leading-none font-bold tracking-[-2.5px] ${tone === 'win' ? 'text-win' : 'text-loss'}`}
    >
      {side.score}
    </span>
    {side.sub && <span className="font-mono text-[11px] text-dim">{side.sub}</span>}
  </div>
)

/** ALLY vs ENEMY big-number scoreboard. */
const Scoreboard = ({ ally, enemy }: ScoreboardProps) => (
  <div className="flex items-center justify-center gap-6 sm:gap-10">
    <Column side={ally} tone="win" />
    <span className="font-mono text-[28px] text-dim">vs</span>
    <Column side={enemy} tone="loss" />
  </div>
)

export default Scoreboard
