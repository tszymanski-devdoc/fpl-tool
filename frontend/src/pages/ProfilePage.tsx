import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { useAuthStore } from '../stores/authStore'
import { patchMe, getFplProfile } from '../api/endpoints'
import { Layout } from '../components/Layout'

export function ProfilePage() {
  const { user, updateUser } = useAuthStore()
  const [displayName, setDisplayName] = useState(user?.displayName ?? '')
  const [fplManagerId, setFplManagerId] = useState(user?.fplManagerId?.toString() ?? '')
  const [toast, setToast] = useState<string | null>(null)
  const [autoFillLoading, setAutoFillLoading] = useState(false)

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

  async function handleAutoFill() {
    const id = Number(fplManagerId)
    if (!id) return
    setAutoFillLoading(true)
    try {
      const profile = await getFplProfile(id)
      setDisplayName(profile.teamName)
      setToast(`Team name filled: ${profile.teamName}`)
      setTimeout(() => setToast(null), 3000)
    } catch {
      setToast('FPL Manager ID not found.')
      setTimeout(() => setToast(null), 3000)
    } finally {
      setAutoFillLoading(false)
    }
  }

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
              FPL Manager ID <span className="text-muted normal-case font-normal tracking-normal">(optional)</span>
            </label>
            <div className="flex gap-2">
              <input
                type="number"
                value={fplManagerId}
                onChange={(e) => setFplManagerId(e.target.value)}
                placeholder="e.g. 1234567"
                className="flex-1 bg-pitch border border-border rounded-lg px-4 py-2.5 text-white placeholder:text-muted text-sm focus:outline-none focus:border-green transition-colors font-mono"
              />
              <button
                type="button"
                onClick={handleAutoFill}
                disabled={!fplManagerId || autoFillLoading}
                className="px-4 py-2.5 rounded-lg border border-border text-muted text-sm font-medium hover:border-green hover:text-white transition-colors disabled:opacity-40 disabled:cursor-not-allowed whitespace-nowrap"
              >
                {autoFillLoading ? '…' : 'Auto-fill name'}
              </button>
            </div>
            <p className="text-muted text-xs mt-2">
              Optional — auto-fills your team name from FPL
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
                toast.includes('Failed') || toast.includes('not found') ? 'bg-red text-white' : 'bg-green text-pitch'
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
