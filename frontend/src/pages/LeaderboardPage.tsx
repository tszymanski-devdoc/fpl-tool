import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { useAuthStore } from '../stores/authStore'
import { getLeaderboard } from '../api/endpoints'
import { Layout } from '../components/Layout'

const GW_OPTIONS = Array.from({ length: 38 }, (_, i) => i + 1)

export function LeaderboardPage() {
  const user = useAuthStore((s) => s.user)
  const [gwFilter, setGwFilter] = useState<number | undefined>(undefined)

  const { data: entries, isLoading } = useQuery({
    queryKey: ['leaderboard', gwFilter],
    queryFn: () => getLeaderboard(gwFilter),
  })

  const myIndex = entries?.findIndex((e) => e.userId === user?.id)

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4">
          <div>
            <p className="text-muted text-xs uppercase tracking-widest">Rankings</p>
            <h1 className="font-display font-extrabold text-4xl text-white">Leaderboard</h1>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setGwFilter(undefined)}
              className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${!gwFilter ? 'bg-green text-pitch' : 'bg-card text-muted hover:text-white border border-border'}`}
            >
              Overall
            </button>
            <select
              value={gwFilter ?? ''}
              onChange={(e) => setGwFilter(e.target.value ? Number(e.target.value) : undefined)}
              className="px-3 py-1.5 rounded-lg text-sm bg-card border border-border text-muted hover:text-white transition-colors cursor-pointer appearance-none"
            >
              <option value="">By Gameweek…</option>
              {GW_OPTIONS.map((gw) => (
                <option key={gw} value={gw}>GW {gw}</option>
              ))}
            </select>
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center min-h-64">
            <motion.div
              animate={{ rotate: 360 }}
              transition={{ repeat: Infinity, duration: 1, ease: 'linear' }}
              className="w-8 h-8 rounded-full border-2 border-green border-t-transparent"
            />
          </div>
        ) : !entries?.length ? (
          <div className="text-center py-16 text-muted">No picks recorded yet.</div>
        ) : (
          <div className="rounded-xl border border-border bg-card overflow-hidden">
            {/* Podium — top 3 */}
            <div className="grid grid-cols-3 gap-px bg-border p-px">
              {entries.slice(0, 3).map((entry, i) => (
                <motion.div
                  key={entry.userId}
                  initial={{ opacity: 0, y: 12 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.1 }}
                  className={`flex flex-col items-center py-6 px-3 text-center ${i === 0 ? 'bg-green/10' : 'bg-card'}`}
                >
                  <span className={`font-display font-extrabold text-4xl ${i === 0 ? 'text-green' : 'text-muted'}`}>
                    #{entry.rank}
                  </span>
                  <span className={`font-display font-bold text-lg mt-1 ${entry.userId === user?.id ? 'text-green' : 'text-white'}`}>
                    {entry.teamName}
                  </span>
                  <span className="font-mono text-2xl font-bold text-white mt-2">{entry.totalPoints}</span>
                  <span className="text-muted text-xs mt-0.5">pts</span>
                </motion.div>
              ))}
            </div>

            {/* Rest of table */}
            {entries.length > 3 && (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border">
                    <th className="text-left px-5 py-2 text-muted text-xs font-medium">#</th>
                    <th className="text-left px-5 py-2 text-muted text-xs font-medium">Team</th>
                    <th className="text-right px-5 py-2 text-muted text-xs font-medium">Points</th>
                    <th className="text-right px-5 py-2 text-muted text-xs font-medium hidden sm:table-cell">Picks</th>
                  </tr>
                </thead>
                <tbody>
                  {entries.slice(3).map((entry, i) => (
                    <motion.tr
                      key={entry.userId}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      transition={{ delay: 0.3 + i * 0.03 }}
                      className={`border-b border-border/50 last:border-0 transition-colors ${
                        entry.userId === user?.id ? 'bg-green/5' : 'hover:bg-card-hover'
                      } ${i + 3 === myIndex ? 'sticky bottom-0' : ''}`}
                    >
                      <td className="px-5 py-3 font-mono text-muted">{entry.rank}</td>
                      <td className={`px-5 py-3 font-medium ${entry.userId === user?.id ? 'text-green' : 'text-white'}`}>
                        {entry.teamName}
                        {entry.userId === user?.id && <span className="ml-2 text-xs text-muted">(you)</span>}
                      </td>
                      <td className="px-5 py-3 text-right font-mono font-bold text-white">{entry.totalPoints}</td>
                      <td className="px-5 py-3 text-right font-mono text-muted hidden sm:table-cell">{entry.picksCount}</td>
                    </motion.tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>
    </Layout>
  )
}
