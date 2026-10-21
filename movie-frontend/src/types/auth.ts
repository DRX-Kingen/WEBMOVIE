
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface AuthUser {
  userId: string;
  fullName: string;
  email: string;
}

export interface AuthResponse extends AuthUser {
  token: string;
  expiresAt: string;
  roles: string[];
}
