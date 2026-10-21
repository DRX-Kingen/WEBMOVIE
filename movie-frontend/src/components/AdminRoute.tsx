
import { Navigate, Outlet } from "react-router-dom";
import { getAuth } from "../utils/authStorage";

export default function AdminRoute() {
  const auth = getAuth();

  if (!auth) {
    return <Navigate to="/login" replace />;
  }

  if (!auth.roles.includes("Admin")) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
