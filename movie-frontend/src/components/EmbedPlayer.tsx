interface EmbedPlayerProps {
  url: string;
  title: string;
}

export default function EmbedPlayer({
  url,
  title,
}: EmbedPlayerProps) {
  return (
    <div className="video-player-container">
      <iframe
        key={url}
        src={url}
        title={title}
        className="video-player"
        allow="autoplay; fullscreen; picture-in-picture"
        allowFullScreen
        referrerPolicy="strict-origin-when-cross-origin"
      />
    </div>
  );
}