import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { GoogleLogin } from '@react-oauth/google'
import { motion } from 'framer-motion'
import { useAuthStore } from '../stores/authStore'
import { postGoogleAuth } from '../api/endpoints'

export function LoginPage() {
  const { isAuthenticated, login } = useAuthStore()
  const navigate = useNavigate()

  useEffect(() => {
    if (isAuthenticated) navigate('/dashboard', { replace: true })
  }, [isAuthenticated, navigate])

  async function handleSuccess(credentialResponse: { credential?: string }) {
    if (!credentialResponse.credential) return
    try {
      const data = await postGoogleAuth(credentialResponse.credential)
      login(data.user, data.accessToken)
      navigate('/dashboard')
    } catch (err) {
      console.error('Auth failed', err)
    }
  }

  return (
    <div className="min-h-screen bg-pitch-radial relative overflow-hidden flex items-center justify-center">
      {/* Background decorative lines */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 left-1/4 w-px h-full bg-gradient-to-b from-transparent via-green/10 to-transparent" />
        <div className="absolute top-0 left-3/4 w-px h-full bg-gradient-to-b from-transparent via-border to-transparent" />
        <div className="absolute top-1/3 left-0 w-full h-px bg-gradient-to-r from-transparent via-border to-transparent" />
        <div className="absolute top-2/3 left-0 w-full h-px bg-gradient-to-r from-transparent via-green/10 to-transparent" />
      </div>

      <div className="relative z-10 flex flex-col items-center text-center px-6 max-w-lg">
        {/* Badge */}
        <motion.div
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.4 }}
          className="mb-6 px-3 py-1 rounded-full border border-green/30 bg-green/10 text-green text-xs font-mono tracking-widest uppercase"
        >
          2025/26 Season
        </motion.div>

        {/* Wordmark */}
        <motion.h1
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.1 }}
          className="font-display font-extrabold text-7xl sm:text-8xl leading-none tracking-tight mb-2"
        >
          <span className="text-gradient-green">FPL</span>
          <br />
          <span className="text-white">CAPTAIN</span>
        </motion.h1>

        {/* Tagline */}
        <motion.p
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.2 }}
          className="text-muted text-lg mb-10"
        >
          Track your captain picks. Own the leaderboard.
        </motion.p>

        {/* Google login */}
        <motion.div
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.4, delay: 0.35 }}
          className="flex flex-col items-center gap-3"
        >
          <GoogleLogin
            onSuccess={handleSuccess}
            onError={() => console.error('Google login error')}
            theme="filled_black"
            size="large"
            text="continue_with"
            shape="rectangular"
          />
          <p className="text-muted text-xs">
            Free to use. No FPL account sharing required.
          </p>
        </motion.div>

        {/* Stat strip */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          className="mt-16 flex gap-8 text-center"
        >
          {[
            { value: '38', label: 'Gameweeks' },
            { value: '∞', label: 'Bragging rights' },
            { value: '1', label: 'Best captain pick' },
          ].map((stat) => (
            <div key={stat.label}>
              <div className="font-display font-bold text-3xl text-green">{stat.value}</div>
              <div className="text-muted text-xs mt-0.5">{stat.label}</div>
            </div>
          ))}
        </motion.div>
      </div>
    </div>
  )
}
