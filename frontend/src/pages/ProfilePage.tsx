import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { useAuthStore } from '../stores/authStore'
import { patchMe } from '../api/endpoints'
import { Layout } from '../components/Layout'

export function ProfilePage() {
  const { user, updateUser } = useAuthStore()
  const [displayName, setDisplayName] = useState(user?.displayName ?? '')
  const [fplManagerId, setFplManagerId] = useState(user?.fplManagerId?.toString() ?? '')
  const [toast, setToast] = useState<string | null>(null)

  const { mutate, isPending } = useMutation({
    mutationFn: () =>
      patchMe({
        displayName: displayName || undefined,
        fplManagerId: fplManagerId ? Number(fplManagerId) : undefined,
      }),
    onSuccess: (updatedUser) => {
      updateUser(updatedUser)
      setToast('Profile saved!')
      setTimeout(() => setToast(null), 3000)
    },
    onError: () => {
      setToast('Failed to save. Try again.')
      setTimeout(() => setToast(null), 3000)
    },
  })

  return (
    <Layout>
      <div className="max-w-lg space-y-6">
        <div>
          <p className="text-muted text-xs uppercase tracking-widest">Settings</p>
          <h1 className="font-display font-extrabold text-4xl text-white">Profile</h1>
        </div>

        {/* Account info */}
        <div className="rounded-xl border border-border bg-card p-5 space-y-4">
          <div>
            <label className="text-muted text-xs uppercase tracking-widest block mb-1">Email</label>
            <p className="text-white/70 text-sm">{user?.email}</p>
          </div>
        </div>

        {/* Editable fields */}
        <div className="rounded-xl border border-border bg-card p-5 space-y-5">
          <div>
            <label className="text-muted text-xs uppercase tracking-widest block mb-2">
              Team Name (Display Name)
            </label>
            <input
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="e.g. Salah's Disciples FC"
              className="w-full bg-pitch border border-border rounded-lg px-4 py-2.5 text-white placeholder:text-muted text-sm focus:outline-none focus:border-green transition-colors"
            />
          </div>

          <div>
            <label className="text-muted text-xs uppercase tracking-widest block mb-2">
              FPL Manager ID
            </label>
            <input
              type="number"
              value={fplManagerId}
              onChange={(e) => setFplManagerId(e.target.value)}
              placeholder="e.g. 1234567"
              className="w-full bg-pitch border border-border rounded-lg px-4 py-2.5 text-white placeholder:text-muted text-sm focus:outline-none focus:border-green transition-colors font-mono"
            />
            <p className="text-muted text-xs mt-2">
              Find this in the FPL app: <span className="text-white/60">Points → View gameweek history → check the URL</span>
            </p>
          </div>

          <button
            onClick={() => mutate()}
            disabled={isPending}
            className="w-full py-2.5 rounded-lg bg-green text-pitch font-display font-bold text-sm tracking-wide hover:bg-green-dim transition-colors disabled:opacity-60"
          >
            {isPending ? 'Saving…' : 'Save Profile'}
          </button>
        </div>

        {/* Toast */}
        <AnimatePresence>
          {toast && (
            <motion.div
              initial={{ opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              className={`fixed bottom-6 left-1/2 -translate-x-1/2 px-5 py-2.5 rounded-xl text-sm font-medium shadow-xl ${
                toast.includes('Failed') ? 'bg-red text-white' : 'bg-green text-pitch'
              }`}
            >
              {toast}
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </Layout>
  )
}
