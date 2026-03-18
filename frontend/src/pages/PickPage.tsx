import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { motion } from 'framer-motion'
import { getPlayers, getCurrentPick, postPick } from '../api/endpoints'
import type { PlayerSummary } from '../api/endpoints'
import { DeadlineCountdown } from '../components/DeadlineCountdown'
import { PlayerCard } from '../components/PlayerCard'
import { Layout } from '../components/Layout'

type SortBy = 'totalPoints' | 'name' | 'position'

const POSITIONS = [
  { label: 'All', value: null },
  { label: 'GK', value: 1 },
  { label: 'DEF', value: 2 },
  { label: 'MID', value: 3 },
  { label: 'FWD', value: 4 },
]

export function PickPage() {
  const [selectedPlayerId, setSelectedPlayerId] = useState<number | null>(null)
  const [saved, setSaved] = useState(false)
  const [search, setSearch] = useState('')
  const [posFilter, setPosFilter] = useState<number | null>(null)
  const [sortBy, setSortBy] = useState<SortBy>('totalPoints')
  const queryClient = useQueryClient()

  const { data, isLoading, error } = useQuery({
    queryKey: ['allPlayers'],
    queryFn: () => getPlayers(),
    staleTime: 5 * 60 * 1000,
  })

  const { data: currentPick } = useQuery({
    queryKey: ['currentPick', data?.gameweekId],
    queryFn: () => getCurrentPick(data!.gameweekId),
    enabled: !!data?.gameweekId,
  })

  const effectiveSelectedPlayerId = selectedPlayerId ?? currentPick?.fplPlayerId ?? null

  const { mutate: submitPick, isPending } = useMutation({
    mutationFn: () => postPick(data!.gameweekId, effectiveSelectedPlayerId!),
    onSuccess: () => {
      setSaved(true)
      queryClient.invalidateQueries({ queryKey: ['currentPick'] })
      setTimeout(() => setSaved(false), 3000)
    },
  })

  const isDeadlinePassed = data ? new Date(data.deadline) < new Date() : false

  if (isLoading) {
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

  if (error || !data) {
    return (
      <Layout>
        <div className="flex flex-col items-center justify-center min-h-64 text-center gap-3">
          <p className="text-red font-medium">Failed to load players. Please try again.</p>
        </div>
      </Layout>
    )
  }

  // Client-side filter + sort
  let displayed = data.players
  if (posFilter !== null) displayed = displayed.filter((p) => p.position === posFilter)
  if (search.trim()) {
    const q = search.toLowerCase()
    displayed = displayed.filter(
      (p) =>
        p.webName.toLowerCase().includes(q) ||
        p.firstName.toLowerCase().includes(q) ||
        p.lastName.toLowerCase().includes(q)
    )
  }
  if (sortBy === 'name') displayed = [...displayed].sort((a, b) => a.webName.localeCompare(b.webName))
  else if (sortBy === 'position') displayed = [...displayed].sort((a, b) => a.position - b.position || b.totalPoints - a.totalPoints)
  else displayed = [...displayed].sort((a, b) => b.totalPoints - a.totalPoints)

  const selectedPlayer = data.players.find((p) => p.fplPlayerId === effectiveSelectedPlayerId)
  const hasChanged = effectiveSelectedPlayerId !== (currentPick?.fplPlayerId ?? null)

  return (
    <Layout>
      <div className="space-y-5">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div>
            <p className="text-muted text-xs uppercase tracking-widest">Captain Pick</p>
            <h1 className="font-display font-extrabold text-4xl text-white">{data.gameweekName}</h1>
          </div>
          <DeadlineCountdown deadline={data.deadline} />
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
              {selectedPlayer ? (
                <>
                  {selectedPlayer.webName}
                  <span className="text-muted text-base font-normal ml-2">{selectedPlayer.teamShortName}</span>
                </>
              ) : (
                <span className="text-muted">None selected</span>
              )}
            </span>
          </div>
          <button
            onClick={() => submitPick()}
            disabled={!effectiveSelectedPlayerId || isDeadlinePassed || isPending || !hasChanged}
            className={[
              'px-6 py-2.5 rounded-lg font-display font-bold text-sm tracking-wide transition-all',
              saved
                ? 'bg-green text-pitch'
                : effectiveSelectedPlayerId && !isDeadlinePassed && hasChanged
                ? 'bg-green text-pitch hover:bg-green-dim glow-green'
                : 'bg-card-hover text-muted cursor-not-allowed',
            ].join(' ')}
          >
            {isPending ? 'Saving…' : saved ? '✓ Saved!' : 'Confirm Pick'}
          </button>
        </div>

        {/* Controls */}
        <div className="flex flex-col sm:flex-row gap-3">
          {/* Position filter */}
          <div className="flex gap-1">
            {POSITIONS.map(({ label, value }) => (
              <button
                key={label}
                onClick={() => setPosFilter(value)}
                className={[
                  'px-3 py-1.5 rounded-lg text-xs font-mono font-medium transition-colors',
                  posFilter === value
                    ? 'bg-green text-pitch'
                    : 'bg-card border border-border text-muted hover:border-muted',
                ].join(' ')}
              >
                {label}
              </button>
            ))}
          </div>

          {/* Sort */}
          <div className="flex gap-1">
            {(['totalPoints', 'name', 'position'] as SortBy[]).map((s) => (
              <button
                key={s}
                onClick={() => setSortBy(s)}
                className={[
                  'px-3 py-1.5 rounded-lg text-xs font-mono font-medium transition-colors',
                  sortBy === s
                    ? 'bg-card-hover border border-green text-green'
                    : 'bg-card border border-border text-muted hover:border-muted',
                ].join(' ')}
              >
                {s === 'totalPoints' ? 'Points' : s === 'name' ? 'Name' : 'Position'}
              </button>
            ))}
          </div>

          {/* Search */}
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search players…"
            className="flex-1 bg-pitch border border-border rounded-lg px-3 py-1.5 text-white placeholder:text-muted text-sm focus:outline-none focus:border-green transition-colors"
          />
        </div>

        {/* Player grid */}
        {displayed.length === 0 ? (
          <p className="text-muted text-sm text-center py-8">No players match your search.</p>
        ) : (
          <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 lg:grid-cols-6 gap-2">
            {displayed.map((p: PlayerSummary) => (
              <PlayerCard
                key={p.fplPlayerId}
                player={p}
                selected={effectiveSelectedPlayerId === p.fplPlayerId}
                onSelect={(pl) => setSelectedPlayerId(pl.fplPlayerId)}
                disabled={isDeadlinePassed}
              />
            ))}
          </div>
        )}
      </div>
    </Layout>
  )
}
