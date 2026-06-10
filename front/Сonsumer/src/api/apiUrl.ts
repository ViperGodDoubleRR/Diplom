import axios from "axios";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { AuthGoResponse } from "@/interface/DTO/AuthGoResponse";
import { getApiData, isApiSuccess, readAuthTokens } from "@/utils/apiHelpers";

export function resolveApiBaseUrl(): string {
  const fromEnv = import.meta.env.VITE_API_URL as string | undefined;

  if (fromEnv !== undefined && fromEnv.trim() !== "") {
    return fromEnv;
  }

  if (import.meta.env.PROD) {
    return "";
  }

  return "http://localhost:5107";
}

export const api = axios.create({
  baseURL: resolveApiBaseUrl(),
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");

  if (token && token !== "undefined" && token !== "null") {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const originalRequest = error.config;

    if (!originalRequest) {
      return Promise.reject(error);
    }

    const isMultipart =
      typeof FormData !== "undefined" && originalRequest.data instanceof FormData;

    if (error.response?.status === 401 && !originalRequest._retry && !isMultipart) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem("refreshToken");

        if (!refreshToken || refreshToken === "undefined" || refreshToken === "null") {
          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");
          return Promise.reject(error);
        }

        const refreshBase = resolveApiBaseUrl();
        const refreshResponse = await axios.post<ApiResponse<AuthGoResponse>>(
          `${refreshBase}/auth/refresh`,
          { refreshToken }
        );

        const data = refreshResponse.data;
        const tokens = readAuthTokens(getApiData(data));

        if (!isApiSuccess(data) || !tokens) {
          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");
          return Promise.reject(error);
        }

        localStorage.setItem("accessToken", tokens.accessToken);
        localStorage.setItem("refreshToken", tokens.refreshToken);

        originalRequest.headers = originalRequest.headers ?? {};
        originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`;

        return api.request(originalRequest);
      } catch {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);
