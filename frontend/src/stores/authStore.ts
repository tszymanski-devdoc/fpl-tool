import { create } from 'zustand'
import type { User } from '../api/endpoints'

interface AuthState {
  user: User | null
  accessToken: string | null
  isAuthenticated: boolean
  login: (user: User, accessToken: string) => void
  logout: () => void
  updateUser: (user: User) => void
}

const storedUser = localStorage.getItem('fpl_user')
const storedToken = localStorage.getItem('fpl_access_token')

export const useAuthStore = create<AuthState>((set) => ({
  user: storedUser ? (JSON.parse(storedUser) as User) : null,
  accessToken: storedToken,
  isAuthenticated: !!storedToken,

  login(user, accessToken) {
    localStorage.setItem('fpl_access_token', accessToken)
    localStorage.setItem('fpl_user', JSON.stringify(user))
    set({ user, accessToken, isAuthenticated: true })
  },

  logout() {
    localStorage.removeItem('fpl_access_token')
    localStorage.removeItem('fpl_user')
    set({ user: null, accessToken: null, isAuthenticated: false })
  },

  updateUser(user) {
    localStorage.setItem('fpl_user', JSON.stringify(user))
    set({ user })
  },
}))
