import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
  Outlet,
} from 'react-router-dom'
import { lazy, Suspense } from 'react'
import { useAuth } from './hooks/useAuth'
import Navbar from './components/Navbar'

// Code-split per route so a visitor only downloads the page they land on —
// GamePage in particular pulls in the WebGL iframe-loading logic, which
// shouldn't ship to someone who only ever visits /login.
const LoginPage = lazy(() => import('./pages/LoginPage'))
const RegisterPage = lazy(() => import('./pages/RegisterPage'))
const LeaderboardPage = lazy(() => import('./pages/LeaderboardPage'))
const StatsPage = lazy(() => import('./pages/StatsPage'))
const HistoryPage = lazy(() => import('./pages/HistoryPage'))
const GamePage = lazy(() => import('./pages/GamePage'))

const AppLoader = () => (
  <div className="bg-bg grid min-h-svh place-items-center">
    <span className="text-dim font-mono text-[11px] tracking-[2px] uppercase">
      Loading…
    </span>
  </div>
)

const PageLoader = () => (
  <div className="grid min-h-[60vh] place-items-center">
    <span className="text-dim font-mono text-[11px] tracking-[2px] uppercase">
      Loading…
    </span>
  </div>
)

const RootRedirect = () => {
  const { accessToken, initializing } = useAuth()
  if (initializing) return <AppLoader />
  return accessToken ? (
    <Navigate to="/leaderboard" replace />
  ) : (
    <Navigate to="/login" replace />
  )
}

const ProtectedLayout = () => {
  const { accessToken, initializing } = useAuth()
  if (initializing) return <AppLoader />
  if (!accessToken) return <Navigate to="/login" replace />
  return (
    <div className="bg-bg text-fg min-h-screen text-left">
      <Navbar />
      <main>
        <Suspense fallback={<PageLoader />}>
          <Outlet />
        </Suspense>
      </main>
    </div>
  )
}

const App = () => {
  return (
    <BrowserRouter>
      <Suspense fallback={<AppLoader />}>
        <Routes>
          <Route path="/" element={<RootRedirect />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route element={<ProtectedLayout />}>
            <Route path="/leaderboard" element={<LeaderboardPage />} />
            <Route path="/stats" element={<StatsPage />} />
            <Route path="/history" element={<HistoryPage />} />
            <Route path="/play" element={<GamePage />} />
          </Route>
        </Routes>
      </Suspense>
    </BrowserRouter>
  )
}

export default App
