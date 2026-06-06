export interface LoginRequest {
  UserId: string
  Password: string
}

export interface LoginUserInfo {
  UserId: string
  UserName: string
  RoleId: string
  RoleName: string
}

export interface LoginResponse {
  Token: string
  ExpiresAt: string
  User: LoginUserInfo
}

export interface StoredAuth {
  token: string
  expiresAt: string
  user: LoginUserInfo
}
