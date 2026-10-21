
import type { AuthResponse } from "../types/auth";

const SESSION_KEY = "movie_auth";

// Lưu thông tin đăng nhập
export function saveAuth(data: AuthResponse): void {
  sessionStorage.setItem(
    SESSION_KEY,
    JSON.stringify(data)
  );

  window.dispatchEvent(
    new Event("auth-changed")
  );
}

// Lấy thông tin đăng nhập
export function getAuth(): AuthResponse | null {
  const raw = sessionStorage.getItem(SESSION_KEY);

  if (!raw) {
    return null;
  }

  try {
    const data = JSON.parse(raw) as AuthResponse;

    // Kiểm tra token và thời gian hết hạn
    if (
      !data.token ||
      !data.expiresAt ||
      !Number.isFinite(Date.parse(data.expiresAt)) ||
      Date.parse(data.expiresAt) <= Date.now()
    ) {
      sessionStorage.removeItem(SESSION_KEY);
      return null;
    }

    return {
      ...data,
      roles: Array.isArray(data.roles)
        ? data.roles
        : []
    };
  } catch {
    sessionStorage.removeItem(SESSION_KEY);
    return null;
  }
}

// Kiểm tra đã đăng nhập
export function isAuthenticated(): boolean {
  return getAuth() !== null;
}

// Kiểm tra quyền Admin
export function isAdmin(): boolean {
  const auth = getAuth();

  return auth?.roles?.includes("Admin") ?? false;
}

// Đăng xuất
export function logout(): void {
  sessionStorage.removeItem(SESSION_KEY);

  window.dispatchEvent(
    new Event("auth-changed")
  );
}
