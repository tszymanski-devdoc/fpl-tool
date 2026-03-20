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

export interface PlayerSummary {
  fplPlayerId: number
  webName: string
  firstName: string
  lastName: string
  teamId: number
  teamName: string
  teamShortName: string
  position: number      // 1=GK 2=DEF 3=MID 4=FWD
  positionName: string  // "GK" | "DEF" | "MID" | "FWD"
  totalPoints: number
  nowCost: number
  photoCode?: string
  opponentTeamShortName?: string
  isHome?: boolean
}

export interface AllPlayers {
  gameweekId: number
  gameweekName: string
  deadline: string  // ISO datetime
  players: PlayerSummary[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  captainPickCounts: Record<number, number>
}

export interface FplProfile {
  teamName: string
  playerName: string
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

export interface PlayerStats {
  minutes: number
  goalsScored: number
  assists: number
  cleanSheets: number
  bonus: number
  yellowCards: number
  redCards: number
  saves: number
  penaltiesSaved: number
  penaltiesMissed: number
  ownGoals: number
}

export interface PreviousPickStats {
  fplPlayerId: number
  playerWebName: string
  photoCode: string | null
  teamShortName: string
  positionName: string
  pointsScored: number | null
  stats: PlayerStats
}

export interface PreviousPickResult {
  gameweekId: number
  gameweekName: string
  pick: PreviousPickStats | null
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

// ── Players ────────────────────────────────────────────────────────────────

export const getPlayers = (params?: { position?: number; sortBy?: string; search?: string; page?: number; pageSize?: number }) =>
  apiClient.get<AllPlayers>('/api/v1/players', { params }).then((r) => r.data)

export const getFplProfile = (managerId: number) =>
  apiClient.get<FplProfile>('/api/v1/users/fpl-profile', { params: { managerId } }).then((r) => r.data)

// ── Picks ──────────────────────────────────────────────────────────────────

export const postPick = (gameweekId: number, fplPlayerId: number) =>
  apiClient.post<CaptainPick>('/api/v1/picks', { gameweekId, fplPlayerId }).then((r) => r.data)

export const getPicks = (page = 1, pageSize = 20) =>
  apiClient.get<PagedResult<CaptainPick>>('/api/v1/picks', { params: { page, pageSize } }).then((r) => r.data)

export const getCurrentPick = (gameweekId: number) =>
  apiClient.get<CaptainPick | null>(`/api/v1/picks/current`, { params: { gameweekId } }).then((r) => r.data)

export const getPreviousPick = (): Promise<PreviousPickResult | null> =>
  apiClient
    .get<PreviousPickResult>('/api/v1/picks/previous')
    .then((r) => r.data ?? null)
    .catch((err) => {
      if (err.response?.status === 204 || err.response?.status === 404) return null
      throw err
    })

// ── Leaderboard ────────────────────────────────────────────────────────────

export const getLeaderboard = (gameweekId?: number) =>
  apiClient
    .get<LeaderboardEntry[]>(gameweekId ? `/api/v1/leaderboard/${gameweekId}` : '/api/v1/leaderboard')
    .then((r) => r.data)
