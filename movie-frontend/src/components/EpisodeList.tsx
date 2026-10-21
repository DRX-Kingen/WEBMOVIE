
    import type { VsmovEpisodeServer } from "../types/vsmov";

    interface EpisodeListProps {
    servers: VsmovEpisodeServer[];
    selectedServer: number;
    selectedEpisode: number;
    onSelect: (serverIndex: number, episodeIndex: number) => void;
    }

    export default function EpisodeList({
    servers,
    selectedServer,
    selectedEpisode,
    onSelect,
    }: EpisodeListProps) {
    if (servers.length === 0) {
        return <p>Chưa có thông tin tập phim.</p>;
    }

    return (
        <section className="watch-episodes">
        <h2>Danh sách tập phim</h2>

        {servers.map((server, serverIndex) => (
            <div
            className="watch-server"
            key={`${server.serverName}-${serverIndex}`}
            >
            <h3>{server.serverName}</h3>

            <div className="watch-episode-grid">
                {server.items.map((episode, episodeIndex) => {
                const active =
                    selectedServer === serverIndex &&
                    selectedEpisode === episodeIndex;

                return (
                    <button
                    type="button"
                    key={`${episode.slug}-${episodeIndex}`}
                    className={
                        active
                        ? "watch-episode-button active"
                        : "watch-episode-button"
                    }
                    onClick={() =>
                        onSelect(serverIndex, episodeIndex)
                    }
                    >
                    {episode.name || `Tập ${episodeIndex + 1}`}
                    </button>
                );
                })}
            </div>
            </div>
        ))}
        </section>
    );
    }
