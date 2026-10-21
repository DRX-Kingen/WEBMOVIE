
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import axiosClient from "../api/axiosClient";
import type { Movie } from "../types/movie";

import MovieComments from "../components/MovieComment";
import { getAuth } from "../utils/authStorage";

export default function MovieDetail() {
  const { id } = useParams<{ id: string }>();

  const [movie, setMovie] = useState<Movie | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Thông tin người dùng đang đăng nhập
  const [auth, setAuth] = useState(() => getAuth());

  // Theo dõi đăng nhập và đăng xuất
  useEffect(() => {
    const updateAuth = () => {
      setAuth(getAuth());
    };

    window.addEventListener(
      "auth-changed",
      updateAuth
    );

    return () => {
      window.removeEventListener(
        "auth-changed",
        updateAuth
      );
    };
  }, []);

  // Lấy thông tin chi tiết phim
  useEffect(() => {
    if (!id) {
      setError("ID phim không hợp lệ");
      setLoading(false);
      return;
    }

    let cancelled = false;

    setLoading(true);
    setError("");
    setMovie(null);

    axiosClient
      .get<Movie>(
        `/api/movies/${encodeURIComponent(id)}`
      )
      .then((res) => {
        if (!cancelled) {
          setMovie(res.data);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setError("Không tìm thấy phim");
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  if (loading) {
    return (
      <main className="main-content">
        Đang tải thông tin phim...
      </main>
    );
  }

  if (error || !movie) {
    return (
      <main className="main-content">
        {error || "Không tìm thấy phim"}
      </main>
    );
  }

  return (
    <main className="main-content">
      {/* Thông tin chi tiết phim */}
      <div className="movie-detail">
        <img
          src={movie.posterUrl || "/placeholder.jpg"}
          alt={movie.title}
          className="detail-poster"
        />

        <div>
          <h1>{movie.title}</h1>

          <p>
            Năm phát hành: {movie.releaseYear}
          </p>

          <p>
            Quốc gia: {movie.country}
          </p>

          <p>{movie.description}</p>

          <Link
            to={`/watch/${movie.id}`}
            className="primary-btn"
          >
            ▶ Xem phim
          </Link>
        </div>
      </div>

      {/* Khu vực bình luận */}
      <section
        className="movie-comments-section"
        style={{ marginTop: "40px" }}
      >
        <MovieComments
          movieSlug={String(movie.id)}
          isLoggedIn={Boolean(auth?.token)}
          currentUserId={auth?.userId}
          isAdmin={
            auth?.roles?.includes("Admin") ?? false
          }
        />
      </section>
    </main>
  );
}
