import axiosClient from "./axiosClient";
import type { VsmovMovie, VsmovMovieDetail } from "../types/vsmov";

export const vsmovApi = {
  getLatest: async (page: number = 1) => {
    const response = await axiosClient.get<VsmovMovie[]>(
      "/api/vsmov/latest",
      { params: { page } }
    );

    return response.data;
  },

  getDetail: async (slug: string) => {
    const response = await axiosClient.get<VsmovMovieDetail>(
      `/api/vsmov/detail/${encodeURIComponent(slug)}`
    );

    return response.data;
  },
  search: async (keyword: string, limit = 20) => {
  const response = await axiosClient.get<VsmovMovie[]>(
    "/api/vsmov/search",
    {
      params: { keyword, limit }
    }
  );

  return response.data;
},
};