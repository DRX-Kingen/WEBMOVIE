export interface Movie {
  id: number;
  title: string;
  slug: string;
  description?: string | null;
  posterUrl?: string | null;
  trailerUrl?: string | null;
  releaseYear?: number | null;
  country?: string | null;
}