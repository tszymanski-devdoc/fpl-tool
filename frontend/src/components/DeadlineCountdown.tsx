import { useEffect, useState } from 'react'
import { motion } from 'framer-motion'
import dayjs from 'dayjs'
import duration from 'dayjs/plugin/duration'

dayjs.extend(duration)

interface Props {
  deadline: string  // ISO datetime
  className?: string
}

function getTimeLeft(deadline: string) {
  const diff = dayjs(deadline).diff(dayjs())
  if (diff <= 0) return null
  const d = dayjs.duration(diff)
  return {
    hours: Math.floor(d.asHours()),
    minutes: d.minutes(),
    seconds: d.seconds(),
    totalMs: diff,
  }
}

function urgencyColor(totalMs: number) {
  const hours = totalMs / 1000 / 3600
  if (hours < 1) return 'text-red'
  if (hours < 6) return 'text-amber'
  return 'text-green'
}

export function DeadlineCountdown({ deadline, className = '' }: Props) {
  const [timeLeft, setTimeLeft] = useState(getTimeLeft(deadline))

  useEffect(() => {
    const id = setInterval(() => setTimeLeft(getTimeLeft(deadline)), 1000)
    return () => clearInterval(id)
  }, [deadline])

  if (!timeLeft) {
    return (
      <span className={`font-mono text-sm text-red ${className}`}>
        DEADLINE PASSED
      </span>
    )
  }

  const color = urgencyColor(timeLeft.totalMs)
  const isUrgent = timeLeft.totalMs < 3600 * 1000

  const pad = (n: number) => String(n).padStart(2, '0')

  return (
    <motion.div
      className={`inline-flex items-center gap-1 font-mono ${color} ${className}`}
      animate={isUrgent ? { opacity: [1, 0.5, 1] } : {}}
      transition={isUrgent ? { repeat: Infinity, duration: 1.5 } : {}}
    >
      <span className="text-muted text-xs uppercase tracking-widest mr-1">Deadline</span>
      <span className="text-lg font-medium">
        {timeLeft.hours > 0 && `${timeLeft.hours}h `}
        {pad(timeLeft.minutes)}m {pad(timeLeft.seconds)}s
      </span>
    </motion.div>
  )
}
