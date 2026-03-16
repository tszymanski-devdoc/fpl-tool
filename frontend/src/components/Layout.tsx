import { NavLink, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { useAuthStore } from '../stores/authStore'

const navItems = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/pick', label: 'Pick Captain' },
  { to: '/history', label: 'My History' },
  { to: '/leaderboard', label: 'Leaderboard' },
  { to: '/profile', label: 'Profile' },
]

export function Layout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, user, logout } = useAuthStore()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <div className="min-h-screen flex flex-col">
      {/* Top nav */}
      <header className="sticky top-0 z-50 border-b border-border bg-pitch/90 backdrop-blur-md">
        <div className="max-w-6xl mx-auto px-4 h-14 flex items-center justify-between gap-6">
          {/* Wordmark */}
          <NavLink to="/dashboard" className="font-display font-bold text-xl tracking-wide text-green shrink-0">
            FPL <span className="text-white">CAPTAIN</span>
          </NavLink>

          {/* Nav links */}
          {isAuthenticated && (
            <nav className="hidden md:flex items-center gap-1">
              {navItems.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  className={({ isActive }) =>
                    [
                      'px-3 py-1.5 rounded-lg text-sm font-medium transition-colors',
                      isActive
                        ? 'bg-card text-green'
                        : 'text-muted hover:text-white hover:bg-card/50',
                    ].join(' ')
                  }
                >
                  {item.label}
                </NavLink>
              ))}
            </nav>
          )}

          {/* Right side */}
          <div className="flex items-center gap-3 shrink-0">
            {isAuthenticated ? (
              <>
                <span className="hidden sm:block text-muted text-sm truncate max-w-32">
                  {user?.displayName}
                </span>
                <button
                  onClick={handleLogout}
                  className="text-muted hover:text-white text-sm transition-colors"
                >
                  Sign out
                </button>
              </>
            ) : (
              <NavLink to="/leaderboard" className="text-muted hover:text-white text-sm">
                Leaderboard
              </NavLink>
            )}
          </div>
        </div>
      </header>

      {/* Page content */}
      <motion.main
        key={location.pathname}
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.2 }}
        className="flex-1 max-w-6xl mx-auto w-full px-4 py-6"
      >
        {children}
      </motion.main>
    </div>
  )
}
