import { Link } from "react-router-dom";
import type { Movie } from "../types/movie";

interface Props {
  movie: Movie;
}

export default function MovieCard({ movie }: Props) {
  return (
    <Link to={`/movies/${movie.id}`} className="movie-card">
      <div className="poster-container">
        <img
          src={movie.posterUrl || "/placeholder.jpg"}
          alt={movie.title}
          loading="lazy"
        />
        <span className="play-overlay">▶</span>
      </div>

      <h3>{movie.title}</h3>
      <p>{movie.releaseYear ?? "Đang cập nhật"}</p>
    </Link>
  );
}