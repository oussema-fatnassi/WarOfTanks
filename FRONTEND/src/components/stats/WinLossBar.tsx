interface WinLossBarProps {
  winRate: number
}

const WinLossBar = ({ winRate }: WinLossBarProps) => (
  <div className="border border-[#2a313b] bg-[#11161d] px-5 py-5">
    <p className="mb-4 font-mono text-[10.5px] tracking-[1.05px] uppercase text-[#98a1ad]">
      Win / Loss ratio
    </p>
    <div className="h-2 overflow-hidden rounded-full bg-[#2a313b]">
      {winRate > 0 && (
        <div
          className="h-full bg-[#5ebc7b] transition-all duration-500"
          style={{ width: `${winRate}%` }}
        />
      )}
    </div>
    <div className="mt-2.5 flex justify-between font-mono text-[10.5px]">
      <span className="text-[#5ebc7b]">{winRate}% wins</span>
      <span className="text-[#ee6951]">{100 - winRate}% losses</span>
    </div>
  </div>
)

export default WinLossBar
