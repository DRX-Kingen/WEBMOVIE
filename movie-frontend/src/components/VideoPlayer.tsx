
import { useEffect, useRef, useState } from "react";
import Hls from "hls.js";

interface VideoPlayerProps {
  src: string | null;
  title?: string;
  type?: "auto" | "mp4" | "hls" | "embed";
}

export default function VideoPlayer({
  src,
  title = "Video",
  type = "auto",
}: VideoPlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [error, setError] = useState("");

  // Nhận diện loại nguồn video
  const sourceType = (() => {
    if (!src) return null;

    if (type !== "auto") return type;

    try {
      const pathname = new URL(
        src,
        window.location.href
      ).pathname.toLowerCase();

      if (pathname.endsWith(".m3u8")) return "hls";
      if (pathname.endsWith(".mp4")) return "mp4";

      // Không tự động coi URL bất kỳ là nguồn nhúng.
      return null;
    } catch {
      return null;
    }
  })();

  useEffect(() => {
    const video = videoRef.current;

    setError("");

    if (!video || !src) return;

    let hls: Hls | null = null;

    const onVideoError = () => {
      setError("Không thể phát video. Hãy kiểm tra nguồn phát.");
    };

    video.addEventListener("error", onVideoError);

    if (sourceType === "hls") {
      if (Hls.isSupported()) {
        hls = new Hls();

        hls.loadSource(src);
        hls.attachMedia(video);

        hls.on(Hls.Events.ERROR, (_, data) => {
          if (data.fatal) {
            setError("Lỗi phát HLS: " + data.details);
          }
        });
      } else if (
        video.canPlayType("application/vnd.apple.mpegurl")
      ) {
        video.src = src;
      } else {
        setError("Trình duyệt không hỗ trợ HLS.");
      }
    } else if (sourceType === "mp4") {
      video.src = src;
    }

    return () => {
      video.pause();
      video.removeEventListener("error", onVideoError);
      hls?.destroy();
      video.removeAttribute("src");
      video.load();
    };
  }, [src, sourceType]);

  if (!src) {
    return (
      <div className="video-player-container">
        <div className="video-placeholder">
          Chưa có nguồn phát cho tập này.
        </div>
      </div>
    );
  }

  if (sourceType === "embed") {
    return (
      <div className="video-player-container">
        <iframe
          key={src}
          src={src}
          title={title}
          className="video-player"
          allow="autoplay; fullscreen; picture-in-picture"
          allowFullScreen
          referrerPolicy="strict-origin-when-cross-origin"
        />
      </div>
    );
  }

  if (sourceType === null) {
    return (
      <div className="video-player-container">
        <div className="video-placeholder">
          Định dạng nguồn phát chưa được hỗ trợ.
        </div>
      </div>
    );
  }

  return (
    <div className="video-player-container">
      <video
        ref={videoRef}
        controls
        playsInline
        preload="metadata"
        aria-label={title}
        className="video-player"
      />

      {error && (
        <p className="video-error">{error}</p>
      )}
    </div>
  );
}
