
    import { useEffect, useState } from "react";
    import { Link, useParams } from "react-router-dom";
    import { vsmovApi } from "../api/vsmovApi";
    import type { VsmovMovieDetail as MovieDetailType } from "../types/vsmov";

    export default function VsmovMovieDetail() {
    const { slug } = useParams<{ slug: string }>();

    const [movie, setMovie] = useState<MovieDetailType | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let active = true;

        async function loadMovie() {
        setLoading(true);
        setMovie(null);
        setError("");

        if (!slug) {
            setError("Không tìm thấy mã phim.");
            setLoading(false);
            return;
        }

        try {
            const data = await vsmovApi.getDetail(slug);

            if (active) {
            setMovie(data);
            }
        } catch (err) {
            console.error("Lỗi tải chi tiết phim:", err);

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

    if (loading) {
        return (
        <main className="detail-page">
            <p>Đang tải thông tin phim...</p>
        </main>
        );
    }

    if (error || !movie) {
        return (
        <main className="detail-page">
            <p className="detail-error">
            {error || "Không tìm thấy phim."}
            </p>
            <Link to="/">← Quay lại trang chủ</Link>
        </main>
        );
    }

    const poster = movie.posterUrl || movie.thumbUrl;

    // API có thể trả content dưới dạng HTML.
    // Hiển thị như văn bản thuần để tránh chèn HTML không tin cậy.
    const description = (movie.content || "")
        .replace(/<[^>]*>/g, " ")
        .replace(/&nbsp;/g, " ")
        .replace(/&amp;/g, "&")
        .trim();

    return (
        <main className="detail-page">
        <Link to="/" className="detail-back">
            ← Quay lại trang chủ
        </Link>

        <section className="vsmov-detail">
            <div className="vsmov-detail-poster">
            {poster ? (
                <img src={poster} alt={movie.name} />
            ) : (
                <div className="detail-no-poster">
                Chưa có poster
                </div>
            )}
            </div>

            <div className="vsmov-detail-info">
            <h1>{movie.name}</h1>

            {movie.originName && (
                <h3>{movie.originName}</h3>
            )}

            <div className="detail-meta">
                {movie.year && <span>{movie.year}</span>}
                {movie.time && <span>{movie.time}</span>}
                {movie.episodeCurrent && (
                <span>{movie.episodeCurrent}</span>
                )}
            </div>

            <p>
                <strong>Thể loại: </strong>
                {movie.categories.length > 0
                ? movie.categories.join(", ")
                : "Đang cập nhật"}
            </p>

            <p>
                <strong>Quốc gia: </strong>
                {movie.countries.length > 0
                ? movie.countries.join(", ")
                : "Đang cập nhật"}
            </p>

            <h2>Nội dung phim</h2>
            <p className="detail-description">
                {description || "Chưa có mô tả phim."}
            </p>
            <Link
                to={`/watch/${encodeURIComponent(movie.slug)}?server=0&episode=0`}
                className="watch-now-button"
                >
                ▶ Xem phim
            </Link>
            </div>
        </section>

        <section className="episode-section">
            <h2>Danh sách tập phim</h2>

            {movie.episodes.length === 0 ? (
            <p>Chưa có thông tin tập phim.</p>
            ) : (
            movie.episodes.map((server, index) => (
                <div key={`${server.serverName}-${index}`}
                    className="episode-server">
                <h3>{server.serverName}</h3>

                <div className="episode-list">
                    {server.items.map((episode, i) => (
                    <span
                        key={`${episode.slug}-${i}`}
                        className="episode-item"
                    >
                        {episode.name}
                    </span>
                    ))}
                </div>
                </div>
            ))
            )}
        </section>
        </main>
    );
    }
