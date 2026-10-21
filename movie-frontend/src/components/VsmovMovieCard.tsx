import { Link } from "react-router-dom";
import type { VsmovMovie } from "../types/vsmov";

interface Props {
  movie: VsmovMovie;
}

export default function VsmovMovieCard({ movie }: Props) {
  const imageUrl = movie.posterUrl || movie.thumbUrl;

  return (
    <Link
      to={`/vsmov/${encodeURIComponent(movie.slug)}`}
      className="movie-card"
    >
      <div className="poster-container">
        {imageUrl ? (
          <img
            src={imageUrl}
            alt={movie.name}
            loading="lazy"
            onError={(e) => {
              e.currentTarget.style.display = "none";
            }}
          />
        ) : (
          <div className="poster-placeholder">
            Chưa có poster
          </div>
        )}

        <span className="play-overlay">▶</span>
      </div>

      <h3>{movie.name}</h3>
      <p>{movie.year ?? "Đang cập nhật"}</p>
    </Link>
  );
}