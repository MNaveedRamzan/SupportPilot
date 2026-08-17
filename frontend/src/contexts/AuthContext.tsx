import { createContext, useState, type ReactNode } from "react";
import { login as loginApi, register as registerApi } from "../api/auth";
import type { LoginRequest, RegisterRequest, UserRole } from "../api/auth";

interface AuthUser {
  email: string;
  role: UserRole;
}

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
  error: string | null;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function readStoredUser(): AuthUser | null {
  const email = localStorage.getItem("userEmail");
  const role = localStorage.getItem("userRole") as UserRole | null;
  if (!email || !role) return null;
  return { email, role };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(readStoredUser());
  const [token, setToken] = useState<string | null>(localStorage.getItem("token"));
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function persistSession(authToken: string, email: string, role: UserRole) {
    localStorage.setItem("token", authToken);
    localStorage.setItem("userEmail", email);
    localStorage.setItem("userRole", role);
    setToken(authToken);
    setUser({ email, role });
  }

  async function login(request: LoginRequest) {
    setIsLoading(true);
    setError(null);
    try {
      const response = await loginApi(request);
      persistSession(response.token, response.email, response.role);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
      throw err;
    } finally {
      setIsLoading(false);
    }
  }

  async function register(request: RegisterRequest) {
    setIsLoading(true);
    setError(null);
    try {
      const response = await registerApi(request);
      persistSession(response.token, response.email, response.role);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed");
      throw err;
    } finally {
      setIsLoading(false);
    }
  }

  function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("userEmail");
    localStorage.removeItem("userRole");
    setToken(null);
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, token, isLoading, error, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}