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

const POSITION_GRADIENT: Record<number, string> = {
  1: 'from-amber-900/60 to-amber-600/20',
  2: 'from-blue-900/60 to-blue-600/20',
  3: 'from-emerald-900/60 to-emerald-600/20',
  4: 'from-red-900/60 to-red-600/20',
}

const FPL_PHOTO_BASE = 'https://resources.premierleague.com/premierleague/photos/players/110x140'

export function PlayerCard({ player, selected, onSelect, disabled }: Props) {
  const initials = player.webName
    .split(' ')
    .map((w) => w[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()

  const photoUrl = player.photoCode
    ? `${FPL_PHOTO_BASE}/p${player.photoCode}.png`
    : null

  const gradient = POSITION_GRADIENT[player.position] ?? 'from-zinc-800 to-zinc-700/20'

  return (
    <motion.button
      onClick={() => !disabled && onSelect(player)}
      whileHover={disabled ? {} : { y: -4, scale: 1.02 }}
      whileTap={disabled ? {} : { scale: 0.97 }}
      className={[
        'relative flex flex-col items-center rounded-xl border transition-colors text-left w-full overflow-hidden',
        selected
          ? 'bg-card-hover border-green glow-green'
          : 'bg-card border-border hover:border-muted',
        disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
      ].join(' ')}
    >
      {selected && <CaptainBadge />}

      {/* Player photo banner */}
      <div className={`w-full h-24 bg-gradient-to-b ${gradient} flex items-end justify-center relative`}>
        {photoUrl ? (
          <img
            src={photoUrl}
            alt={player.webName}
            className="h-full w-auto object-contain object-bottom"
            style={{ filter: 'drop-shadow(0 -2px 8px rgba(0,0,0,0.6))' }}
            onError={(e) => {
              const target = e.currentTarget
              target.style.display = 'none'
              const fallback = target.nextElementSibling as HTMLElement | null
              if (fallback) fallback.style.removeProperty('display')
            }}
          />
        ) : null}
        <span
          className="font-display font-bold text-xl text-muted absolute inset-0 flex items-center justify-center"
          style={{ display: photoUrl ? 'none' : undefined }}
        >
          {initials}
        </span>
      </div>

      {/* Text content */}
      <div className="flex flex-col items-center gap-1 px-3 pb-3 w-full">

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
      </div>
    </motion.button>
  )
}
