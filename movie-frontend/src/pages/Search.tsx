
import { useEffect, useState } from "react";
import {
  Link,
  useSearchParams
} from "react-router-dom";

import { vsmovApi } from "../api/vsmovApi";
import type { VsmovMovie } from "../types/vsmov";

export default function Search() {
  const [params] = useSearchParams();

  // Hỗ trợ cả ?q=naruto và ?keyword=naruto
  const keyword = (
    params.get("q") ??
    params.get("keyword") ??
    ""
  ).trim();

  const [movies, setMovies] = useState<VsmovMovie[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function searchMovies() {
      if (!keyword) {
        setMovies([]);
        setError("");
        setLoading(false);
        return;
      }

      setLoading(true);
      setError("");
      setMovies([]);

      try {
        // Gọi API tìm kiếm VSMOV
        const result = await vsmovApi.search(keyword, 20);

        console.log("Kết quả tìm kiếm VSMOV:", result);

        if (active) {
          setMovies(result);
        }
      } catch (err) {
        console.error("Lỗi tìm kiếm VSMOV:", err);

        if (active) {
          setError("Không thể tìm kiếm phim từ VSMOV.");
          setMovies([]);
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    searchMovies();

    return () => {
      active = false;
    };
  }, [keyword]);

  return (
  <main className="main-content vsmov-search-page">
    <h1 className="vsmov-search-title">
      Kết quả tìm kiếm: {keyword}
    </h1>

    {loading && <p>Đang tìm kiếm phim...</p>}

    {!loading && error && (
      <p className="error">{error}</p>
    )}

    {!loading && !error && keyword && movies.length === 0 && (
      <p>Không tìm thấy phim phù hợp.</p>
    )}

    {!loading && !error && movies.length > 0 && (
      <p className="vsmov-search-count">
        Tìm thấy <strong>{movies.length}</strong> phim
      </p>
    )}

    <div className="vsmov-results-grid">
      {movies.map((movie) => (
        <Link
          key={movie.slug}
          to={`/vsmov/${encodeURIComponent(movie.slug)}`}
          className="vsmov-result-card"
        >
          <div className="vsmov-result-poster">
            <img
              src={movie.posterUrl || movie.thumbUrl || ""}
              alt={movie.name}
              loading="lazy"
            />
          </div>

          <div className="vsmov-result-info">
            <h3>{movie.name}</h3>
            <p>{movie.year ?? "Đang cập nhật"}</p>
          </div>
        </Link>
      ))}
    </div>
  </main>
);
}
