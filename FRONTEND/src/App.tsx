import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import LeaderboardPage from './pages/LeaderboardPage'
import StatsPage from './pages/StatsPage'
import HistoryPage from './pages/HistoryPage'
import GamePage from './pages/GamePage'
import { useAuth } from './hooks/useAuth'
import Navbar from './components/Navbar'

const RootRedirect = () => {
  const { accessToken } = useAuth()
  return accessToken ? (
    <Navigate to="/leaderboard" replace />
  ) : (
    <Navigate to="/login" replace />
  )
}

const ProtectedLayout = () => {
  const { accessToken } = useAuth()
  if (!accessToken) return <Navigate to="/login" replace />
  return (
    <div className="min-h-screen bg-[#0e1116] text-[#e7ecef] text-left">
      <Navbar />
      <main className="py-6">
        <Outlet />
      </main>
    </div>
  )
}

const App = () => {
  return (
    <BrowserRouter>
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
    </BrowserRouter>
  )
}

export default App
