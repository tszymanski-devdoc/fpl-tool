import { apiClient } from './client'

// ── Types ──────────────────────────────────────────────────────────────────

export interface User {
  id: string
  email: string
  displayName: string
  fplManagerId: number | null
}

export interface AuthResponse {
  accessToken: string
  user: User
}

export interface SquadPlayer {
  fplPlayerId: number
  webName: string
  teamId: number
  elementType: number  // 1=GK 2=DEF 3=MID 4=FWD
}

export interface Squad {
  gameweekId: number
  gameweekName: string
  deadline: string  // ISO datetime
  players: SquadPlayer[]
}

export interface CaptainPick {
  id: string
  gameweekId: number
  fplPlayerId: number
  playerWebName: string
  pointsScored: number | null
  pickedAtUtc: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface LeaderboardEntry {
  rank: number
  userId: string
  teamName: string
  totalPoints: number
  picksCount: number
}

export interface GameweekBreakdown {
  gameweekId: number
  userId: string
  teamName: string
  fplPlayerId: number
  playerWebName: string
  pointsScored: number | null
}

// ── Auth ───────────────────────────────────────────────────────────────────

export const postGoogleAuth = (idToken: string) =>
  apiClient.post<AuthResponse>('/api/v1/auth/google', { idToken }).then((r) => r.data)

// ── Profile ────────────────────────────────────────────────────────────────

export const getMe = () =>
  apiClient.get<User>('/api/v1/users/me').then((r) => r.data)

export const patchMe = (data: { displayName?: string; fplManagerId?: number }) =>
  apiClient.patch<User>('/api/v1/users/me', data).then((r) => r.data)

// ── Picks ──────────────────────────────────────────────────────────────────

export const getSquad = () =>
  apiClient.get<Squad>('/api/v1/picks/squad').then((r) => r.data)

export const postPick = (gameweekId: number, fplPlayerId: number) =>
  apiClient.post<CaptainPick>('/api/v1/picks', { gameweekId, fplPlayerId }).then((r) => r.data)

export const getPicks = (page = 1, pageSize = 20) =>
  apiClient.get<PagedResult<CaptainPick>>('/api/v1/picks', { params: { page, pageSize } }).then((r) => r.data)

export const getCurrentPick = (gameweekId: number) =>
  apiClient.get<CaptainPick | null>(`/api/v1/picks/current`, { params: { gameweekId } }).then((r) => r.data)

// ── Leaderboard ────────────────────────────────────────────────────────────

export const getLeaderboard = (gameweekId?: number) =>
  apiClient
    .get<LeaderboardEntry[]>(gameweekId ? `/api/v1/leaderboard/${gameweekId}` : '/api/v1/leaderboard')
    .then((r) => r.data)
