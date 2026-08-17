export type UserRole = "Customer" | "Agent" | "Admin";

export interface AuthResponse {
  token: string;
  email: string;
  role: UserRole;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}