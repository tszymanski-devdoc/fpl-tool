import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { useAuthStore } from '../stores/authStore'
import { getPlayers, getCurrentPick, getLeaderboard } from '../api/endpoints'
import { DeadlineCountdown } from '../components/DeadlineCountdown'
import { Layout } from '../components/Layout'

function StatCard({ label, value, accent = false }: { label: string; value: React.ReactNode; accent?: boolean }) {
  return (
    <div className={`rounded-xl border p-4 ${accent ? 'border-green/30 bg-green/5' : 'border-border bg-card'}`}>
      <div className="text-muted text-xs uppercase tracking-widest mb-1">{label}</div>
      <div className={`font-display font-bold text-3xl ${accent ? 'text-green' : 'text-white'}`}>{value}</div>
    </div>
  )
}

export function DashboardPage() {
  const user = useAuthStore((s) => s.user)

  const { data } = useQuery({
    queryKey: ['allPlayers'],
    queryFn: () => getPlayers(),
    staleTime: 5 * 60 * 1000,
    retry: false,
  })

  const { data: currentPick } = useQuery({
    queryKey: ['currentPick', data?.gameweekId],
    queryFn: () => getCurrentPick(data!.gameweekId),
    enabled: !!data?.gameweekId,
  })

  const { data: leaderboard } = useQuery({
    queryKey: ['leaderboard'],
    queryFn: () => getLeaderboard(),
  })

  const myRank = leaderboard?.findIndex((e) => e.userId === user?.id)
  const myEntry = myRank !== undefined && myRank >= 0 ? leaderboard![myRank] : null
  const top5 = leaderboard?.slice(0, 5) ?? []
  const isDeadlinePassed = data ? new Date(data.deadline) < new Date() : false

  return (
    <Layout>
      <div className="space-y-6">
        {/* Hero row */}
        <motion.div
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          className="rounded-2xl border border-border bg-pitch-radial relative overflow-hidden p-6 noise"
        >
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <p className="text-muted text-xs uppercase tracking-widest mb-1">
                {data ? data.gameweekName : 'Current Gameweek'}
              </p>
              <h1 className="font-display font-extrabold text-5xl text-white leading-none">
                {currentPick ? (
                  <>
                    <span className="text-green">{currentPick.playerWebName}</span>
                    <span className="text-muted text-2xl ml-3 font-medium">your captain</span>
                  </>
                ) : (
                  <span className="text-muted">No pick yet</span>
                )}
              </h1>
            </div>

            <div className="flex flex-col items-start sm:items-end gap-3">
              {data && <DeadlineCountdown deadline={data.deadline} />}
              {!isDeadlinePassed && (
                <Link
                  to="/pick"
                  className="px-5 py-2 rounded-lg bg-green text-pitch font-display font-bold text-sm tracking-wide hover:bg-green-dim transition-colors"
                >
                  {currentPick ? 'Change Pick →' : 'Pick Captain →'}
                </Link>
              )}
            </div>
          </div>
        </motion.div>

        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          <StatCard label="Season Points" value={myEntry?.totalPoints ?? '—'} accent />
          <StatCard label="Leaderboard Rank" value={myRank !== undefined && myRank >= 0 ? `#${myRank + 1}` : '—'} />
          <StatCard label="Picks Made" value={myEntry?.picksCount ?? '—'} />
          <StatCard label="GW" value={data?.gameweekId ?? '—'} />
        </div>

        {/* Mini leaderboard */}
        {top5.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 }}
            className="rounded-xl border border-border bg-card overflow-hidden"
          >
            <div className="flex items-center justify-between px-5 py-3 border-b border-border">
              <h2 className="font-display font-bold text-lg text-white">Top Managers</h2>
              <Link to="/leaderboard" className="text-green text-sm hover:underline">
                Full table →
              </Link>
            </div>
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left px-5 py-2 text-muted text-xs font-medium">#</th>
                  <th className="text-left px-5 py-2 text-muted text-xs font-medium">Team</th>
                  <th className="text-right px-5 py-2 text-muted text-xs font-medium">Pts</th>
                </tr>
              </thead>
              <tbody>
                {top5.map((entry) => (
                  <tr
                    key={entry.userId}
                    className={`border-b border-border/50 last:border-0 ${entry.userId === user?.id ? 'bg-green/5' : ''}`}
                  >
                    <td className="px-5 py-3 font-mono text-muted">{entry.rank}</td>
                    <td className={`px-5 py-3 font-medium ${entry.userId === user?.id ? 'text-green' : 'text-white'}`}>
                      {entry.teamName}
                    </td>
                    <td className="px-5 py-3 text-right font-mono font-bold text-white">{entry.totalPoints}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </motion.div>
        )}
      </div>
    </Layout>
  )
}
