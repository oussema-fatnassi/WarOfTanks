import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import Avatar from './ui/Avatar'

const LINKS = [
  { to: '/play', label: 'Play' },
  { to: '/leaderboard', label: 'Leaderboard' },
  { to: '/stats', label: 'Stats' },
  { to: '/history', label: 'Match History' },
]

const Brand = () => (
  <div className="flex items-center gap-2.5 font-mono text-[13px] font-bold tracking-[2.34px] text-fg">
    <span className="relative grid size-[22px] shrink-0 place-items-center border border-fg">
      <span className="absolute inset-1 border border-fg" />
      <span className="size-1 bg-win" />
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

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `relative flex items-center gap-2 rounded-card px-3.5 py-2 text-[13px] font-medium transition-colors ${
    isActive
      ? 'bg-raised text-fg shadow-[inset_0_-2px_0_0_var(--color-win)]'
      : 'text-muted hover:text-fg'
  }`

const NavDot = ({ active }: { active: boolean }) => (
  <span className={`size-1.5 rounded-[2px] ${active ? 'bg-win' : 'bg-dim'}`} />
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

  return (
    <header className="border-b border-line bg-bg">
      <div className="mx-auto flex h-16 max-w-[1440px] items-center justify-between px-4 sm:px-8">
        <div className="flex items-center gap-8">
          <Brand />
          <nav className="hidden items-center gap-1.5 md:flex">
            {LINKS.map(l => (
              <NavLink key={l.to} to={l.to} className={linkClass}>
                {({ isActive }) => (
                  <>
                    <NavDot active={isActive} />
                    {l.label}
                  </>
                )}
              </NavLink>
            ))}
          </nav>
        </div>

        <div className="hidden items-center gap-3.5 md:flex">
          {player && <Avatar name={player.username} size="md" />}
          <span className="text-[13px] font-medium text-fg">{player?.username}</span>
          <button
            onClick={handleLogout}
            className="rounded-card px-2.5 py-1.5 text-[12px] text-muted transition-colors hover:text-loss"
          >
            Logout
          </button>
        </div>

        <button
          onClick={() => setOpen(prev => !prev)}
          className="text-muted transition-colors hover:text-fg md:hidden"
          aria-label="Toggle menu"
        >
          {open ? <CloseIcon /> : <HamburgerIcon />}
        </button>
      </div>

      {open && (
        <div className="border-t border-line px-4 pt-4 pb-5 sm:px-8 md:hidden">
          <nav className="flex flex-col gap-1.5">
            {LINKS.map(l => (
              <NavLink
                key={l.to}
                to={l.to}
                className={linkClass}
                onClick={() => setOpen(false)}
              >
                {({ isActive }) => (
                  <>
                    <NavDot active={isActive} />
                    {l.label}
                  </>
                )}
              </NavLink>
            ))}
          </nav>
          <div className="mt-4 flex items-center justify-between border-t border-line pt-4">
            <div className="flex items-center gap-2.5">
              {player && <Avatar name={player.username} size="sm" />}
              <span className="text-[13px] font-medium text-fg">{player?.username}</span>
            </div>
            <button
              onClick={handleLogout}
              className="text-[12px] text-muted transition-colors hover:text-loss"
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
