
import axiosClient from "./axiosClient";

import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse
} from "../types/auth";

export const authApi = {
  // Đăng ký tài khoản
  register: async (data: RegisterRequest) => {
    const response = await axiosClient.post(
      "/api/auth/register",
      data
    );

    return response.data;
  },

  // Đăng nhập và nhận JWT
  login: async (
    data: LoginRequest
  ): Promise<AuthResponse> => {
    const response = await axiosClient.post<AuthResponse>(
      "/api/auth/login",
      data
    );

    return response.data;
  },

  // Lấy thông tin người dùng hiện tại
  getMe: async (token: string) => {
    const response = await axiosClient.get(
      "/api/auth/me",
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );

    return response.data;
  }
};
