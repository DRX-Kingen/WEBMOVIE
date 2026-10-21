
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";

import { authApi } from "../api/authApi";
import { saveAuth } from "../utils/authStorage";

export default function Login() {
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (loading) return;

    setError("");
    setLoading(true);

    try {
      // 1. Gọi API đăng nhập
      const response = await authApi.login({
        email: email.trim(),
        password
      });

      // 2. Lưu JWT và thông tin người dùng
      saveAuth(response);

      // 3. Điều hướng theo quyền
      if (response.roles?.includes("Admin")) {
        navigate("/admin/dashboard", {
          replace: true
        });
      } else {
        navigate("/", {
          replace: true
        });
      }
    } catch (err) {
      console.error("LOGIN ERROR:", err);

      if (axios.isAxiosError(err)) {
        const message = err.response?.data?.message;

        setError(
          typeof message === "string"
            ? message
            : "Email hoặc mật khẩu không chính xác."
        );
      } else {
        setError("Đã xảy ra lỗi. Vui lòng thử lại.");
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="auth-page">
      <div className="auth-card">
        <h1>Đăng nhập</h1>

        <p>Chào mừng trở lại MovieStreaming</p>

        <form onSubmit={handleSubmit}>
          <label htmlFor="login-email">
            Email
          </label>

          <input
            id="login-email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Nhập email"
            autoComplete="email"
            required
          />

          <label htmlFor="login-password">
            Mật khẩu
          </label>

          <input
            id="login-password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Nhập mật khẩu"
            autoComplete="current-password"
            required
          />

          {error && (
            <p className="auth-error" role="alert">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loading}
          >
            {loading
              ? "Đang đăng nhập..."
              : "Đăng nhập"}
          </button>
        </form>

        <p className="auth-footer">
          Chưa có tài khoản?{" "}
          <Link to="/register">
            Đăng ký ngay
          </Link>
        </p>
      </div>
    </main>
  );
}
