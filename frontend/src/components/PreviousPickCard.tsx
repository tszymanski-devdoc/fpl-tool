import { useState } from 'react'
import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import type { PreviousPickResult, PlayerStats } from '../api/endpoints'

const FPL_PHOTO_BASE = 'https://resources.premierleague.com/premierleague/photos/players/110x140'

interface StatPill {
  icon: string
  value: number
  label: string
  highlight: 'green' | 'amber' | 'red' | 'blue' | null
}

function buildStatPills(stats: PlayerStats, positionName: string): StatPill[] {
  const all: StatPill[] = [
    {
      icon: '⏱',
      value: stats.minutes,
      label: 'Mins',
      highlight: null,
    },
    {
      icon: '⚽',
      value: stats.goalsScored,
      label: 'Goals',
      highlight: stats.goalsScored > 0 ? 'green' : null,
    },
    {
      icon: '🅰',
      value: stats.assists,
      label: 'Assists',
      highlight: stats.assists > 0 ? 'green' : null,
    },
    {
      icon: '⭐',
      value: stats.bonus,
      label: 'Bonus',
      highlight: stats.bonus > 0 ? 'green' : null,
    },
    {
      icon: '🛡',
      value: stats.cleanSheets,
      label: 'CS',
      highlight: stats.cleanSheets > 0 ? 'blue' : null,
    },
    ...(positionName === 'GK'
      ? ([
          {
            icon: '🧤',
            value: stats.saves,
            label: 'Saves',
            highlight: stats.saves >= 3 ? 'green' : null,
          },
          {
            icon: '🚫',
            value: stats.penaltiesSaved,
            label: 'Pen Saved',
            highlight: stats.penaltiesSaved > 0 ? 'green' : null,
          },
        ] as StatPill[])
      : []),
    {
      icon: '🟡',
      value: stats.yellowCards,
      label: 'Yellow',
      highlight: stats.yellowCards > 0 ? 'amber' : null,
    },
    {
      icon: '🔴',
      value: stats.redCards,
      label: 'Red',
      highlight: stats.redCards > 0 ? 'red' : null,
    },
    {
      icon: '💀',
      value: stats.ownGoals,
      label: 'OG',
      highlight: stats.ownGoals > 0 ? 'red' : null,
    },
  ]

  return all.filter((p) => {
    if (['Mins', 'Goals', 'Assists'].includes(p.label)) return true
    return p.value > 0
  })
}

function pointsColor(pts: number | null): string {
  if (pts === null) return 'text-muted'
  if (pts >= 12) return 'text-green'
  if (pts >= 6) return 'text-white'
  return 'text-muted'
}

const HIGHLIGHT_BORDER: Record<string, string> = {
  green: 'border-green/40 shadow-[0_0_10px_rgba(0,230,118,0.15)]',
  amber: 'border-amber-400/40 shadow-[0_0_10px_rgba(245,158,11,0.15)]',
  red: 'border-red-500/40 shadow-[0_0_10px_rgba(239,68,68,0.15)]',
  blue: 'border-blue-400/40 shadow-[0_0_10px_rgba(96,165,250,0.15)]',
}

interface Props {
  result: PreviousPickResult
}

export function PreviousPickCard({ result }: Props) {
  const { pick } = result
  const [photoError, setPhotoError] = useState(false)

  if (!pick) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}
        className="rounded-xl border border-dashed border-border bg-card p-6"
      >
        <p className="text-muted text-xs uppercase tracking-widest mb-4">
          {result.gameweekName} Recap
        </p>
        <div className="flex flex-col items-center gap-3 py-4">
          <div className="w-14 h-14 rounded-full border-2 border-dashed border-border flex items-center justify-center opacity-40">
            <span className="font-display font-bold text-2xl text-muted">C</span>
          </div>
          <p className="font-display font-bold text-xl text-muted">
            No pick made for {result.gameweekName}
          </p>
          <p className="text-muted text-sm">Don't miss the next gameweek!</p>
          <Link
            to="/pick"
            className="mt-2 px-5 py-2 rounded-lg bg-green text-pitch font-display font-bold text-sm tracking-wide hover:bg-green-dim transition-colors"
          >
            Make your pick →
          </Link>
        </div>
      </motion.div>
    )
  }

  const pills = buildStatPills(pick.stats, pick.positionName)

  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.1 }}
      className="rounded-xl border border-border bg-card overflow-hidden"
    >
      {/* Header */}
      <div className="px-5 py-3 border-b border-border flex items-center gap-3">
        <span className="px-2.5 py-0.5 rounded-full bg-green/15 text-green text-xs font-display font-bold tracking-wide uppercase">
          {result.gameweekName} Recap
        </span>
      </div>

      {/* Player identity + points */}
      <div className="p-5 flex flex-col sm:flex-row sm:items-center gap-5">
        <div className="flex items-center gap-4">
          {/* Photo */}
          <div className="w-16 h-20 rounded-lg overflow-hidden bg-card-hover flex-shrink-0 flex items-center justify-center">
            {pick.photoCode && !photoError ? (
              <img
                src={`${FPL_PHOTO_BASE}/p${pick.photoCode}.png`}
                alt={pick.playerWebName}
                className="w-full h-full object-cover object-top"
                onError={() => setPhotoError(true)}
              />
            ) : (
              <span className="font-display font-bold text-2xl text-muted">
                {pick.playerWebName[0]}
              </span>
            )}
          </div>

          {/* Name / Team / Position */}
          <div>
            <p className="font-display font-bold text-xl text-white leading-tight">
              {pick.playerWebName}
            </p>
            <p className="text-muted text-sm mt-0.5">
              {pick.teamShortName}
              <span className="mx-1.5 text-border">·</span>
              {pick.positionName}
            </p>
          </div>
        </div>

        {/* Points */}
        <div className="flex items-baseline gap-1.5 sm:ml-auto">
          <span className={`font-mono font-bold text-6xl leading-none ${pointsColor(pick.pointsScored)}`}>
            {pick.pointsScored ?? '—'}
          </span>
          <span className="text-muted text-sm font-mono">pts</span>
        </div>
      </div>

      {/* Stat pills */}
      <div className="px-5 pb-5 grid grid-cols-3 sm:grid-cols-4 lg:grid-cols-6 gap-2">
        {pills.map((pill, i) => (
          <motion.div
            key={pill.label}
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.15 + i * 0.05 }}
            className={[
              'flex flex-col items-center gap-1 rounded-lg border p-3 bg-card-hover',
              pill.highlight ? HIGHLIGHT_BORDER[pill.highlight] : 'border-border',
            ].join(' ')}
          >
            <span className="text-xl leading-none">{pill.icon}</span>
            <span className="font-mono font-bold text-2xl text-white leading-tight">{pill.value}</span>
            <span className="text-muted text-xs uppercase tracking-wide">{pill.label}</span>
          </motion.div>
        ))}
      </div>
    </motion.div>
  )
}
