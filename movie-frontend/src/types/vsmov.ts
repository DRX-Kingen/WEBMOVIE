export interface VsmovMovie {
  name: string;
  slug: string;
  posterUrl: string | null;
  thumbUrl: string | null;
  year: number | null;
}

export interface VsmovEpisode {
  name: string;
  slug: string;
  linkEmbed: string | null;
}

export interface VsmovEpisodeServer {
  serverName: string;
  items: VsmovEpisode[];
}

export interface VsmovMovieDetail {
  name: string;
  slug: string;
  originName: string | null;
  content: string | null;
  posterUrl: string | null;
  thumbUrl: string | null;
  year: number | null;
  time: string | null;
  episodeCurrent: string | null;
  categories: string[];
  countries: string[];
  episodes: VsmovEpisodeServer[];
}
