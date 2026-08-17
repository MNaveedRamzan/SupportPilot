const API_BASE_URL = "https://localhost:7020/api";

/**
 * Wraps fetch with the Authorization header automatically attached from
 * localStorage, so individual API modules (dashboard.ts, auth.ts) don't
 * need to repeat token-handling logic on every call.
 */
export async function authFetch(
  path: string,
  options: RequestInit = {}
): Promise<Response> {
  const token = localStorage.getItem("token");

  const headers = new Headers(options.headers);
  headers.set("Content-Type", "application/json");
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  return fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
  });
}

export { API_BASE_URL };