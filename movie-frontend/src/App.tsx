import {
  BrowserRouter,
  Routes,
  Route
} from "react-router-dom";

import Header from "./components/Header";
import Home from "./pages/Home";
import Search from "./pages/Search";
import MovieDetail from "./pages/MovieDetail";
import Login from "./pages/Login";
import Register from "./pages/Register";
import VsmovMovieDetail from "./pages/VsmovMovieDetail";
import WatchMovie from "./pages/WatchMovie";
import Dashboard from "./pages/admin/Dashboard";
import AdminRoute from "./components/AdminRoute";

export default function App() {
  return (
    <BrowserRouter>
      <Header />

      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/search" element={<Search />} />
        <Route path="/movies/:id" element={<MovieDetail />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/vsmov/:slug" element={<VsmovMovieDetail />}/>
        <Route path="/watch/:slug" element={<WatchMovie />} />
        <Route path="/admin/dashboard" element={<Dashboard />} />
        <Route element={<AdminRoute />}>
        <Route path="/admin/dashboard" element={<Dashboard />}/></Route>
      </Routes>
    </BrowserRouter>
  );
}