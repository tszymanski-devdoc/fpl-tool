import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { useAuthStore } from '../stores/authStore'
import { getSquad, getCurrentPick, postPick } from '../api/endpoints'
import type { SquadPlayer } from '../api/endpoints'
import { DeadlineCountdown } from '../components/DeadlineCountdown'
import { PlayerCard } from '../components/PlayerCard'
import { Layout } from '../components/Layout'

const POSITION_ORDER = [1, 4, 3, 2]

export function PickPage() {
  const user = useAuthStore((s) => s.user)
  const [selectedPlayerId, setSelectedPlayerId] = useState<number | null>(null)
  const [saved, setSaved] = useState(false)
  const queryClient = useQueryClient()

  const { data: squad, isLoading: squadLoading, error: squadError } = useQuery({
    queryKey: ['squad'],
    queryFn: getSquad,
    enabled: !!user?.fplManagerId,
  })

  const { data: currentPick } = useQuery({
    queryKey: ['currentPick', squad?.gameweekId],
    queryFn: () => getCurrentPick(squad!.gameweekId),
    enabled: !!squad?.gameweekId,
  })

  useEffect(() => {
    if (currentPick && selectedPlayerId === null) {
      setSelectedPlayerId(currentPick.fplPlayerId)
    }
  }, [currentPick])

  const { mutate: submitPick, isPending } = useMutation({
    mutationFn: () => postPick(squad!.gameweekId, selectedPlayerId!),
    onSuccess: () => {
      setSaved(true)
      queryClient.invalidateQueries({ queryKey: ['currentPick'] })
      setTimeout(() => setSaved(false), 3000)
    },
  })

  const isDeadlinePassed = squad ? new Date(squad.deadline) < new Date() : false

  if (!user?.fplManagerId) {
    return (
      <Layout>
        <div className="flex flex-col items-center justify-center min-h-64 text-center gap-4">
          <div className="text-5xl">⚡</div>
          <h2 className="font-display font-bold text-2xl text-white">Set your FPL Manager ID first</h2>
          <p className="text-muted">We need your FPL Manager ID to fetch your squad.</p>
          <Link to="/profile" className="px-5 py-2 rounded-lg bg-green text-pitch font-bold text-sm">
            Go to Profile →
          </Link>
        </div>
      </Layout>
    )
  }

  if (squadLoading) {
    return (
      <Layout>
        <div className="flex items-center justify-center min-h-64">
          <motion.div
            animate={{ rotate: 360 }}
            transition={{ repeat: Infinity, duration: 1, ease: 'linear' }}
            className="w-8 h-8 rounded-full border-2 border-green border-t-transparent"
          />
        </div>
      </Layout>
    )
  }

  if (squadError || !squad) {
    return (
      <Layout>
        <div className="flex flex-col items-center justify-center min-h-64 text-center gap-3">
          <p className="text-red font-medium">Failed to load your squad. Check your FPL Manager ID.</p>
          <Link to="/profile" className="text-green text-sm hover:underline">Edit profile →</Link>
        </div>
      </Layout>
    )
  }

  const playersByPosition = POSITION_ORDER.map((pos) => ({
    pos,
    players: squad.players.filter((p) => p.elementType === pos),
  })).filter((g) => g.players.length > 0)

  const selectedPlayer = squad.players.find((p) => p.fplPlayerId === selectedPlayerId)
  const hasChanged = selectedPlayerId !== (currentPick?.fplPlayerId ?? null)

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div>
            <p className="text-muted text-xs uppercase tracking-widest">Captain Pick</p>
            <h1 className="font-display font-extrabold text-4xl text-white">{squad.gameweekName}</h1>
          </div>
          <DeadlineCountdown deadline={squad.deadline} />
        </div>

        {isDeadlinePassed && (
          <div className="rounded-xl border border-red/30 bg-red/5 px-5 py-3 text-red text-sm font-medium">
            The deadline for this gameweek has passed. Picks are locked.
          </div>
        )}

        {/* Selected summary + submit */}
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 rounded-xl border border-border bg-card p-4">
          <div>
            <span className="text-muted text-xs uppercase tracking-widest block mb-1">Selected Captain</span>
            <span className="font-display font-bold text-2xl text-white">
              {selectedPlayer ? selectedPlayer.webName : <span className="text-muted">None selected</span>}
            </span>
          </div>
          <button
            onClick={() => submitPick()}
            disabled={!selectedPlayerId || isDeadlinePassed || isPending || !hasChanged}
            className={[
              'px-6 py-2.5 rounded-lg font-display font-bold text-sm tracking-wide transition-all',
              saved
                ? 'bg-green text-pitch'
                : selectedPlayerId && !isDeadlinePassed && hasChanged
                ? 'bg-green text-pitch hover:bg-green-dim glow-green'
                : 'bg-card-hover text-muted cursor-not-allowed',
            ].join(' ')}
          >
            {isPending ? 'Saving…' : saved ? '✓ Saved!' : 'Confirm Pick'}
          </button>
        </div>

        {/* Squad grid by position */}
        {playersByPosition.map(({ pos, players }, i) => {
          const posLabel = { 1: 'Goalkeeper', 2: 'Defenders', 3: 'Midfielders', 4: 'Forwards' }[pos]
          return (
            <motion.div
              key={pos}
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.07 }}
            >
              <h3 className="font-display font-bold text-sm text-muted uppercase tracking-widest mb-2">{posLabel}</h3>
              <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-2">
                {players.map((p: SquadPlayer) => (
                  <PlayerCard
                    key={p.fplPlayerId}
                    player={p}
                    selected={selectedPlayerId === p.fplPlayerId}
                    onSelect={(pl) => setSelectedPlayerId(pl.fplPlayerId)}
                    disabled={isDeadlinePassed}
                  />
                ))}
              </div>
            </motion.div>
          )
        })}
      </div>
    </Layout>
  )
}
