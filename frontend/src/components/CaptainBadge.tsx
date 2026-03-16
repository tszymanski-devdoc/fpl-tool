import { motion } from 'framer-motion'

export function CaptainBadge() {
  return (
    <motion.div
      initial={{ scale: 0, rotate: -20 }}
      animate={{ scale: 1, rotate: 0 }}
      transition={{ type: 'spring', stiffness: 400, damping: 15 }}
      className="absolute -top-2 -right-2 w-7 h-7 rounded-full bg-green flex items-center justify-center shadow-lg glow-green z-10"
    >
      <span className="font-display font-bold text-pitch text-sm leading-none">C</span>
    </motion.div>
  )
}
