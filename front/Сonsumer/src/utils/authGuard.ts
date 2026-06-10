const PUBLIC_PATHS = new Set(["/auth", "/reg", "/res"]);
const AUTH_ONLY_PATHS = new Set(["/auth", "/reg", "/res"]);

export function getRefreshToken(): string | null {
  const token = localStorage.getItem("refreshToken");
  if (!token || token === "undefined" || token === "null") return null;
  return token;
}

export function hasRefreshToken(): boolean {
  return !!getRefreshToken();
}

export function clearAuthTokens(): void {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
}

export function isPublicRoute(path: string): boolean {
  return PUBLIC_PATHS.has(path);
}

export function isAuthOnlyRoute(path: string): boolean {
  return AUTH_ONLY_PATHS.has(path);
}
