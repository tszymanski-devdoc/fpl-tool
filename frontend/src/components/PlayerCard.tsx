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
  captainCount?: number
}

const POSITION_BG: Record<number, string> = {
  1: 'bg-amber-500/20',
  2: 'bg-blue-500/20',
  3: 'bg-green-500/20',
  4: 'bg-red-500/20',
}

const FPL_PHOTO_BASE = 'https://resources.premierleague.com/premierleague/photos/players/110x140'

export function PlayerCard({ player, selected, onSelect, disabled, captainCount }: Props) {
  const initials = player.webName
    .split(' ')
    .map((w) => w[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()

  const photoUrl = player.photoCode
    ? `${FPL_PHOTO_BASE}/p${player.photoCode}.png`
    : null

  const fixtureLabel = player.opponentTeamShortName
    ? `${player.isHome ? 'vs' : '@'} ${player.opponentTeamShortName}`
    : null

  return (
    <motion.button
      onClick={() => !disabled && onSelect(player)}
      whileHover={disabled ? {} : { y: -4, scale: 1.02 }}
      whileTap={disabled ? {} : { scale: 0.97 }}
      className={[
        'relative flex flex-col items-center gap-1 p-3 rounded-xl border transition-colors text-left w-full',
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
          'w-14 h-14 rounded-full overflow-hidden flex items-center justify-center font-display font-bold text-base',
          selected ? 'bg-green/30' : (POSITION_BG[player.position] ?? 'bg-card-hover'),
        ].join(' ')}
      >
        {photoUrl ? (
          <img
            src={photoUrl}
            alt={player.webName}
            className="w-full h-full object-cover object-top"
            style={{
              filter: 'saturate(2.5) contrast(1.4) brightness(1.05) drop-shadow(2px 4px 0 rgba(0,0,0,0.8))',
            }}
            onError={(e) => {
              const target = e.currentTarget
              target.style.display = 'none'
              const fallback = target.nextElementSibling as HTMLElement | null
              if (fallback) fallback.style.display = 'flex'
            }}
          />
        ) : null}
        <span
          className={selected ? 'text-white' : 'text-muted'}
          style={{ display: photoUrl ? 'none' : 'flex' }}
        >
          {initials}
        </span>
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

      {/* Fixture — always rendered to keep uniform height */}
      <span className={`font-mono text-xs font-semibold ${fixtureLabel ? (player.isHome ? 'text-green/80' : 'text-muted') : 'invisible'}`}>
        {fixtureLabel ?? 'vs ???'}
      </span>

      {/* Points */}
      <span className="font-mono text-xs text-white/50">{player.totalPoints}pts</span>

      {/* Captain count — always rendered to keep uniform height */}
      <span className={`font-mono text-xs text-amber/80 ${captainCount ? '' : 'invisible'}`}>
        C ×{captainCount ?? 0}
      </span>
    </motion.button>
  )
}
