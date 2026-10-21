import { useEffect, useState } from "react";
import { vsmovApi } from "../api/vsmovApi";
import VsmovMovieCard from "../components/VsmovMovieCard";
import type { VsmovMovie } from "../types/vsmov";

export default function Home() {
  const [movies, setMovies] = useState<VsmovMovie[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [page, setPage] = useState(1);

  useEffect(() => {
    let active = true;

    async function loadMovies() {
      setLoading(true);
      setError("");

      try {
        const data = await vsmovApi.getLatest(page);

        if (active) {
          setMovies(data);
        }
      } catch (err) {
        console.error(err);

        if (active) {
          setError("Không thể tải danh sách phim từ VSMOV.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    loadMovies();

    return () => {
      active = false;
    };
  }, [page]);

  return (
    <>
      <section className="hero">
        <div className="hero-content">
          <span className="hero-label">
            MOVIE STREAMING
          </span>

          <h1>Khám phá thế giới điện ảnh</h1>

          <p>
            Những bộ phim mới được cập nhật liên tục.
          </p>

          <a href="#movies" className="primary-btn">
            Khám phá ngay
          </a>
        </div>
      </section>

      <main className="main-content" id="movies">
        <h2>🎬 Phim mới cập nhật</h2>

        {loading && <p>Đang tải phim...</p>}

        {error && <p className="error">{error}</p>}

        {!loading && !error && (
          <>
            <div className="movie-grid">
              {movies.map((movie) => (
                <VsmovMovieCard
                  key={movie.slug}
                  movie={movie}
                />
              ))}
            </div>

            {movies.length === 0 && (
              <p>Chưa có dữ liệu phim.</p>
            )}

            <div className="pagination">
              <button
                disabled={page === 1}
                onClick={() => setPage(p => Math.max(1, p - 1))}
              >
                Trang trước
              </button>

              <span>Trang {page}</span>

              <button
                disabled={movies.length === 0}
                onClick={() => setPage(p => p + 1)}
              >
                Trang sau
              </button>
            </div>
          </>
        )}
      </main>
    </>
  );
}