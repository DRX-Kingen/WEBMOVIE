
import { useEffect, useState } from "react";
import axiosClient from "../../api/axiosClient";
import { getAuth } from "../../utils/authStorage";

interface DashboardData {
  totalMovies: number;
  totalEpisodes: number;
  totalComments: number;
  totalUsers: number;
}

export default function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const auth = getAuth();

    if (!auth) {
      setError("Bạn chưa đăng nhập.");
      return;
    }

    axiosClient.get<DashboardData>("/api/admin/dashboard", {
      headers: {
        Authorization: `Bearer ${auth.token}`
      }
    })
    .then(res => setData(res.data))
    .catch(() => setError("Không có quyền hoặc không tải được dữ liệu."));
  }, []);

  if (error) {
    return <main className="main-content"><p>{error}</p></main>;
  }

  if (!data) {
    return <main className="main-content">Đang tải Dashboard...</main>;
  }

  return (
    <main className="main-content">
      <h1>Dashboard quản trị</h1>

      <div className="admin-stats">
        <div className="admin-stat">
          <h3>Tổng số phim</h3>
          <strong>{data.totalMovies}</strong>
        </div>

        <div className="admin-stat">
          <h3>Người dùng</h3>
          <strong>{data.totalUsers}</strong>
        </div>

        <div className="admin-stat">
          <h3>Tập phim</h3>
          <strong>{data.totalEpisodes}</strong>
        </div>

        <div className="admin-stat">
          <h3>Bình luận</h3>
          <strong>{data.totalComments}</strong>
        </div>
      </div>
    </main>
  );
}
