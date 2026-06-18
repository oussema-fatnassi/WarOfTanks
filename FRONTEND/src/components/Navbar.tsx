import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

const Brand = () => (
  <div className="flex items-center gap-3 font-mono text-[13px] font-bold tracking-[2.34px] text-[#e7ecef]">
    <span className="relative grid size-[22px] shrink-0 place-items-center border border-[#e7ecef]">
      <span className="size-2.5 border border-[#e7ecef]" />
      <span className="absolute size-[5px] rounded-full bg-[#5ebc7b]" />
    </span>
    <span>WAR OF TANKS</span>
  </div>
)

const HamburgerIcon = () => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5">
    <line x1="3" y1="5" x2="17" y2="5" />
    <line x1="3" y1="10" x2="17" y2="10" />
    <line x1="3" y1="15" x2="17" y2="15" />
  </svg>
)

const CloseIcon = () => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5">
    <line x1="4" y1="4" x2="16" y2="16" />
    <line x1="16" y1="4" x2="4" y2="16" />
  </svg>
)

const Navbar = () => {
  const { player, logout } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)

  const handleLogout = async () => {
    setOpen(false)
    await logout()
    navigate('/login')
  }

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `font-mono text-[11px] tracking-[1.1px] uppercase transition-colors ${
      isActive ? 'text-[#e7ecef]' : 'text-[#98a1ad] hover:text-[#e7ecef]'
    }`

  return (
    <header className="border-b border-[#2a313b] bg-[#0e1116]">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-4">
        <Brand />

        <nav className="hidden items-center gap-7 md:flex">
          <NavLink to="/leaderboard" className={linkClass}>Leaderboard</NavLink>
          <NavLink to="/stats" className={linkClass}>Stats</NavLink>
          <NavLink to="/history" className={linkClass}>History</NavLink>
        </nav>

        <div className="hidden items-center gap-5 md:flex">
          <span className="font-mono text-[11px] tracking-[1px] text-[#98a1ad]">
            {player?.username}
          </span>
          <button
            onClick={handleLogout}
            className="font-mono text-[11px] tracking-[1.1px] uppercase text-[#98a1ad] transition-colors hover:text-[#ee6951]"
          >
            Logout
          </button>
        </div>

        <button
          onClick={() => setOpen(prev => !prev)}
          className="text-[#98a1ad] transition-colors hover:text-[#e7ecef] md:hidden"
          aria-label="Toggle menu"
        >
          {open ? <CloseIcon /> : <HamburgerIcon />}
        </button>
      </div>

      {open && (
        <div className="border-t border-[#2a313b] px-6 pb-5 pt-4 md:hidden">
          <nav className="flex flex-col gap-4">
            <NavLink to="/leaderboard" className={linkClass} onClick={() => setOpen(false)}>Leaderboard</NavLink>
            <NavLink to="/stats" className={linkClass} onClick={() => setOpen(false)}>Stats</NavLink>
            <NavLink to="/history" className={linkClass} onClick={() => setOpen(false)}>History</NavLink>
          </nav>
          <div className="mt-5 flex items-center justify-between border-t border-[#2a313b] pt-4">
            <span className="font-mono text-[11px] tracking-[1px] text-[#98a1ad]">
              {player?.username}
            </span>
            <button
              onClick={handleLogout}
              className="font-mono text-[11px] tracking-[1.1px] uppercase text-[#98a1ad] transition-colors hover:text-[#ee6951]"
            >
              Logout
            </button>
          </div>
        </div>
      )}
    </header>
  )
}

export default Navbar
