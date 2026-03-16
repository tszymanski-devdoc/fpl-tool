import { useQuery } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { getPicks } from '../api/endpoints'
import { Layout } from '../components/Layout'

function CountUp({ target }: { target: number }) {
  return <span className="font-mono font-bold text-4xl text-green">{target}</span>
}

export function HistoryPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['picks'],
    queryFn: () => getPicks(1, 38),
  })

  const picks = data?.items ?? []
  const totalPoints = picks.reduce((s, p) => s + (p.pointsScored ?? 0), 0)
  const completedPicks = picks.filter((p) => p.pointsScored !== null)

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div>
          <p className="text-muted text-xs uppercase tracking-widest">Season recap</p>
          <h1 className="font-display font-extrabold text-4xl text-white">My Captain History</h1>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-3 gap-3">
          <div className="rounded-xl border border-green/20 bg-green/5 p-4">
            <div className="text-muted text-xs uppercase tracking-widest mb-1">Total Points</div>
            <CountUp target={totalPoints} />
          </div>
          <div className="rounded-xl border border-border bg-card p-4">
            <div className="text-muted text-xs uppercase tracking-widest mb-1">Picks Made</div>
            <span className="font-mono font-bold text-4xl text-white">{picks.length}</span>
          </div>
          <div className="rounded-xl border border-border bg-card p-4">
            <div className="text-muted text-xs uppercase tracking-widest mb-1">Avg Points</div>
            <span className="font-mono font-bold text-4xl text-white">
              {completedPicks.length > 0 ? Math.round(totalPoints / completedPicks.length) : '—'}
            </span>
          </div>
        </div>

        {/* Table */}
        {isLoading ? (
          <div className="flex items-center justify-center min-h-32">
            <motion.div
              animate={{ rotate: 360 }}
              transition={{ repeat: Infinity, duration: 1, ease: 'linear' }}
              className="w-8 h-8 rounded-full border-2 border-green border-t-transparent"
            />
          </div>
        ) : picks.length === 0 ? (
          <div className="text-center py-16 text-muted">No captain picks yet this season.</div>
        ) : (
          <div className="rounded-xl border border-border bg-card overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left px-5 py-3 text-muted text-xs font-medium uppercase tracking-wider">GW</th>
                  <th className="text-left px-5 py-3 text-muted text-xs font-medium uppercase tracking-wider">Captain</th>
                  <th className="text-right px-5 py-3 text-muted text-xs font-medium uppercase tracking-wider">Points</th>
                  <th className="text-right px-5 py-3 text-muted text-xs font-medium uppercase tracking-wider hidden sm:table-cell">Picked</th>
                </tr>
              </thead>
              <tbody>
                {picks.map((pick, i) => (
                  <motion.tr
                    key={pick.id}
                    initial={{ opacity: 0, x: -8 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: i * 0.04 }}
                    className="border-b border-border/50 last:border-0 hover:bg-card-hover transition-colors"
                  >
                    <td className="px-5 py-3">
                      <span className="font-mono text-muted">GW{pick.gameweekId}</span>
                    </td>
                    <td className="px-5 py-3 font-medium text-white">{pick.playerWebName}</td>
                    <td className="px-5 py-3 text-right">
                      {pick.pointsScored !== null ? (
                        <span className={`font-mono font-bold ${pick.pointsScored >= 12 ? 'text-green' : pick.pointsScored >= 6 ? 'text-white' : 'text-muted'}`}>
                          {pick.pointsScored}
                        </span>
                      ) : (
                        <span className="text-muted text-xs font-mono">Pending</span>
                      )}
                    </td>
                    <td className="px-5 py-3 text-right text-muted text-xs hidden sm:table-cell">
                      {new Date(pick.pickedAtUtc).toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })}
                    </td>
                  </motion.tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </Layout>
  )
}
