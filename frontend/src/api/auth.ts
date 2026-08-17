import { API_BASE_URL } from "./client";

// --- Types matching backend DTOs ---

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

// --- API calls ---
// Note: login/register don't use authFetch — no token exists yet at this point.

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE_URL}/Auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });
  if (!res.ok) throw new Error("Invalid email or password");
  return res.json();
}

export async function register(request: RegisterRequest): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE_URL}/Auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });
  if (!res.ok) throw new Error("Email is already registered");
  return res.json();
}