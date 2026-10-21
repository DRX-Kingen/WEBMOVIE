
import axiosClient from "./axiosClient";

export interface MovieComment {
  id: number;
  userId: string;
  userName: string;
  content: string;
  createdAt: string;
}

export interface CommentPage {
  items: MovieComment[];
  total: number;
  page: number;
  pageSize: number;
}

export const commentApi = {
  async getComments(
    slug: string,
    page = 1
  ): Promise<CommentPage> {
    const response = await axiosClient.get<CommentPage>(
      `/api/movies/${encodeURIComponent(slug)}/comments`,
      { params: { page } }
    );

    return response.data;
  },

  async createComment(
    slug: string,
    content: string
  ): Promise<MovieComment> {
    const response = await axiosClient.post<MovieComment>(
      `/api/movies/${encodeURIComponent(slug)}/comments`,
      { content }
    );

    return response.data;
  },

  async deleteComment(
    slug: string,
    id: number
  ): Promise<void> {
    await axiosClient.delete(
      `/api/movies/${encodeURIComponent(slug)}/comments/${id}`
    );
  }
};
