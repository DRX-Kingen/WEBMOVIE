
import { useEffect, useState } from "react";
import { commentApi } from "../api/commentApi";
import type { MovieComment } from "../api/commentApi";

interface Props {
  movieSlug: string;
  isLoggedIn: boolean;
  currentUserId?: string;
  isAdmin?: boolean;
}

export default function MovieComments({
  movieSlug,
  isLoggedIn,
  currentUserId,
  isAdmin = false
}: Props) {
  const [comments, setComments] = useState<MovieComment[]>([]);
  const [content, setContent] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function loadComments() {
    try {
      const result = await commentApi.getComments(movieSlug);
      setComments(result.items);
    } catch {
      setError("Không thể tải bình luận.");
    }
  }

  useEffect(() => {
    void loadComments();
  }, [movieSlug]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (!content.trim() || loading) return;

    setLoading(true);
    setError("");

    try {
      await commentApi.createComment(
        movieSlug,
        content.trim()
      );

      setContent("");
      await loadComments();
    } catch {
      setError("Không thể gửi bình luận.");
    } finally {
      setLoading(false);
    }
  }

  async function handleDelete(id: number) {
    if (!window.confirm("Bạn muốn xóa bình luận này?"))
      return;

    try {
      await commentApi.deleteComment(movieSlug, id);
      await loadComments();
    } catch {
      setError("Không thể xóa bình luận.");
    }
  }

  return (
    <section className="movie-comments">
      <h2>Bình luận phim</h2>

      {isLoggedIn ? (
        <form onSubmit={handleSubmit}>
          <textarea
            value={content}
            onChange={e => setContent(e.target.value)}
            placeholder="Viết bình luận..."
            maxLength={1000}
            rows={4}
            required
          />

          <button
            type="submit"
            disabled={loading || !content.trim()}
          >
            {loading ? "Đang gửi..." : "Gửi bình luận"}
          </button>
        </form>
      ) : (
        <p>Vui lòng đăng nhập để bình luận.</p>
      )}

      {error && <p role="alert">{error}</p>}

      <div className="comment-list">
        {comments.length === 0 && (
          <p>Chưa có bình luận nào.</p>
        )}

        {comments.map(comment => (
          <div key={comment.id} className="comment-item">
            <strong>{comment.userName}</strong>

            <small>
              {" "}
              {new Date(comment.createdAt)
                .toLocaleString("vi-VN")}
            </small>

            <p>{comment.content}</p>

            {(currentUserId === comment.userId ||
              isAdmin) && (
              <button
                type="button"
                onClick={() => handleDelete(comment.id)}
              >
                Xóa
              </button>
            )}
          </div>
        ))}
      </div>
    </section>
  );
}
