
import { useEffect, useState } from "react";

import {
  Link,
  useNavigate,
  useSearchParams
} from "react-router-dom";

import {
  Search,
  Film,
  UserRound,
  LogOut,
  LayoutDashboard
} from "lucide-react";

import {
  getAuth,
  logout
} from "../utils/authStorage";

import type {
  AuthResponse
} from "../utils/authStorage";

export default function Header() {
  const [keyword, setKeyword] = useState("");

  const [auth, setAuth] =
    useState<AuthResponse | null>(
      () => getAuth()
    );

  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  // Đồng bộ trạng thái đăng nhập
  useEffect(() => {
    function syncAuth() {
      setAuth(getAuth());
    }

    window.addEventListener(
      "auth-changed",
      syncAuth
    );

    window.addEventListener(
      "storage",
      syncAuth
    );

    return () => {
      window.removeEventListener(
        "auth-changed",
        syncAuth
      );

      window.removeEventListener(
        "storage",
        syncAuth
      );
    };
  }, []);

  // Đồng bộ ô tìm kiếm với URL
  useEffect(() => {
    setKeyword(
      searchParams.get("q") ?? ""
    );
  }, [searchParams]);

  function handleSearch(
    e: React.FormEvent
  ) {
    e.preventDefault();

    const q = keyword.trim();

    if (q) {
      navigate(
        `/search?q=${encodeURIComponent(q)}`
      );
    }
  }

  function handleLogout() {
    logout();
    setAuth(null);
    navigate("/");
  }

  // Kiểm tra quyền Admin
  const isAdmin =
    auth?.roles?.includes("Admin") ?? false;

  return (
    <header className="header">

      {/* Logo */}
      <Link to="/" className="logo">
        <Film size={30} />
        <span>NewMovie</span>
      </Link>

      {/* Menu */}
      <nav className="nav-links">
        <Link to="/">Trang chủ</Link>
        <Link to="/search">Tìm kiếm</Link>

        {isAdmin && (
          <Link
            to="/admin/dashboard"
            className="admin-nav-link"
          >
            <LayoutDashboard size={18} />
            Quản trị
          </Link>
        )}
      </nav>

      {/* Tìm kiếm phim */}
      <form
        className="search-box"
        onSubmit={handleSearch}
      >
        <input
          type="text"
          placeholder="Tìm kiếm phim..."
          value={keyword}
          onChange={(e) =>
            setKeyword(e.target.value)
          }
        />

        <button
          type="submit"
          aria-label="Tìm kiếm"
        >
          <Search size={20} />
        </button>
      </form>

      {/* Tài khoản */}
      {auth ? (
        <div className="header-account">

          <div className="header-user">
            <UserRound size={19} />

            <span>
              {auth.fullName || auth.email}
            </span>
          </div>

          <button
            type="button"
            className="logout-btn"
            onClick={handleLogout}
          >
            <LogOut size={17} />
            Đăng xuất
          </button>
        </div>
      ) : (
        <Link
          to="/login"
          className="login-btn"
        >
          <UserRound size={18} />
          Đăng nhập
        </Link>
      )}
    </header>
  );
}
