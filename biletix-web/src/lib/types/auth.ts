export interface User {
  userId: string
  email: string
  firstName: string
  lastName: string
  role: "Admin" | "Organizer" | "Customer"
  fullName: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiry: string
  userId: string
  email: string
  fullName: string
  role: string
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RefreshTokenRequest {
  accessToken: string
  refreshToken: string
}

