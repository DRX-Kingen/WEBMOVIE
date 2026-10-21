
import { useEffect, useMemo, useState } from "react";
import {
  Link,
  useParams,
  useSearchParams
} from "react-router-dom";

import VideoPlayer from "../components/VideoPlayer";
import EpisodeList from "../components/EpisodeList";
import { vsmovApi } from "../api/vsmovApi";
import type { VsmovMovieDetail } from "../types/vsmov";

export default function WatchMovie() {
  const { slug } = useParams<{ slug: string }>();
  const [searchParams, setSearchParams] = useSearchParams();

  const [movie, setMovie] = useState<VsmovMovieDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const serverIndex = Number(searchParams.get("server") ?? 0);
  const episodeIndex = Number(searchParams.get("episode") ?? 0);

  // 1. Lấy thông tin phim và danh sách tập từ Backend
  useEffect(() => {
    let active = true;

    async function loadMovie() {
      if (!slug) {
        setError("Thiếu mã phim.");
        setLoading(false);
        return;
      }

      setLoading(true);
      setError("");
      setMovie(null);

      try {
        const data = await vsmovApi.getDetail(slug);

        if (active) {
          setMovie(data);
        }
      } catch (err) {
        console.error("Lỗi tải phim:", err);

        if (active) {
          setError("Không thể tải thông tin phim.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    loadMovie();

    return () => {
      active = false;
    };
  }, [slug]);

  // 2. Xác định tập đang được chọn
  const selection = useMemo(() => {
    if (!movie) return null;

    if (
      !Number.isInteger(serverIndex) ||
      !Number.isInteger(episodeIndex) ||
      serverIndex < 0 ||
      episodeIndex < 0
    ) {
      return null;
    }

    const server = movie.episodes[serverIndex];
    const episode = server?.items[episodeIndex];

    if (!server || !episode) return null;

    return { server, episode };
  }, [movie, serverIndex, episodeIndex]);

  // 3. Lấy URL nhúng của tập đang chọn
  const embedUrl = selection?.episode.linkEmbed ?? null;

  // 4. Chuyển tập phim
  function selectEpisode(server: number, episode: number) {
    setSearchParams({
      server: String(server),
      episode: String(episode)
    });
  }

  if (loading) {
    return (
      <main className="watch-page">
        <p>Đang tải thông tin phim...</p>
      </main>
    );
  }

  if (error || !movie) {
    return (
      <main className="watch-page">
        <p className="video-error">
          {error || "Không tìm thấy phim."}
        </p>

        <Link to="/">Quay lại trang chủ</Link>
      </main>
    );
  }

  return (
    <main className="watch-page">
      <Link to={`/vsmov/${encodeURIComponent(movie.slug)}`}>
        ← Quay lại chi tiết phim
      </Link>

      <h1>{movie.name}</h1>

      <p className="watch-current-episode">
        {selection
          ? `${selection.server.serverName.trim()} — Tập ${selection.episode.name}`
          : "Vui lòng chọn một tập hợp lệ"}
      </p>

      {/* 5. Hiển thị nguồn nhúng được cấp phép */}
      {selection && embedUrl ? (
        <VideoPlayer
          key={`${serverIndex}-${episodeIndex}`}
          src={embedUrl}
          title={`${movie.name} - Tập ${selection.episode.name}`}
          type="embed"
        />
      ) : (
        <div className="video-placeholder">
          Chưa có nguồn phát cho tập này.
        </div>
      )}

      {/* 6. Danh sách tập */}
      <EpisodeList
        servers={movie.episodes}
        selectedServer={serverIndex}
        selectedEpisode={episodeIndex}
        onSelect={selectEpisode}
      />
    </main>
  );
}
