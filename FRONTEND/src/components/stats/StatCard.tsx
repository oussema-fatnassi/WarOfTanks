interface StatCardProps {
  label: string
  value: string | number
  accent?: boolean
}

const StatCard = ({ label, value, accent = false }: StatCardProps) => (
  <div className="border border-[#2a313b] bg-[#11161d] px-5 py-5">
    <p className="mb-3 font-mono text-[10.5px] tracking-[1.05px] uppercase text-[#98a1ad]">
      {label}
    </p>
    <p
      className={`font-mono text-[28px] font-bold leading-none tracking-tight ${
        accent ? 'text-[#5dcbd1]' : 'text-[#e7ecef]'
      }`}
    >
      {value}
    </p>
  </div>
)

export default StatCard
