import { motion } from 'framer-motion'
import { CaptainBadge } from './CaptainBadge'
import type { PlayerSummary } from '../api/endpoints'

const POSITION_COLORS: Record<number, string> = {
  1: 'text-amber',
  2: 'text-blue-400',
  3: 'text-green',
  4: 'text-red',
}

interface Props {
  player: PlayerSummary
  selected: boolean
  onSelect: (player: PlayerSummary) => void
  disabled?: boolean
}

export function PlayerCard({ player, selected, onSelect, disabled }: Props) {
  const initials = player.webName
    .split(' ')
    .map((w) => w[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()

  return (
    <motion.button
      onClick={() => !disabled && onSelect(player)}
      whileHover={disabled ? {} : { y: -4, scale: 1.02 }}
      whileTap={disabled ? {} : { scale: 0.97 }}
      className={[
        'relative flex flex-col items-center gap-1.5 p-3 rounded-xl border transition-colors text-left w-full',
        selected
          ? 'bg-card-hover border-green glow-green'
          : 'bg-card border-border hover:border-muted',
        disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
      ].join(' ')}
    >
      {selected && <CaptainBadge />}

      {/* Avatar */}
      <div
        className={[
          'w-10 h-10 rounded-full flex items-center justify-center font-display font-bold text-base',
          selected ? 'bg-green text-pitch' : 'bg-card-hover text-muted',
        ].join(' ')}
      >
        {initials}
      </div>

      {/* Name */}
      <span className={`font-display font-bold text-sm leading-tight text-center ${selected ? 'text-white' : 'text-white/80'}`}>
        {player.webName}
      </span>

      {/* Team + Position */}
      <span className="text-muted text-xs">{player.teamShortName}</span>
      <span className={`font-mono text-xs font-medium ${POSITION_COLORS[player.position] ?? 'text-muted'}`}>
        {player.positionName}
      </span>

      {/* Points */}
      <span className="font-mono text-xs text-white/50">{player.totalPoints}pts</span>
    </motion.button>
  )
}
