import axios from "axios";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { AuthGoResponse } from "@/interface/DTO/AuthGoResponse";

export const api = axios.create({
  baseURL: "http://localhost:5107",
});

console.log("🚀 API INSTANCE CREATED");

// ================= REQUEST =================
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");

  console.log("➡️ REQUEST:", {
    url: config.url,
    method: config.method,
    token,
    headers: config.headers,
  });

  if (token && token !== "undefined" && token !== "null") {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// ================= RESPONSE =================
api.interceptors.response.use(
  (res) => {
    console.log("⬅️ RESPONSE SUCCESS:", {
      url: res.config.url,
      status: res.status,
      data: res.data,
    });

    return res;
  },
  async (error) => {
    const originalRequest = error.config;

    console.log("❌ RESPONSE ERROR:", {
      url: originalRequest?.url,
      status: error.response?.status,
      data: error.response?.data,
    });

    // если нет config — вообще не можем retry
    if (!originalRequest) {
      return Promise.reject(error);
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      console.log("🔄 REFRESH START");

      try {
        console.log("STORAGE SNAPSHOT:", {
  access: localStorage.getItem("accessToken"),
  refresh: localStorage.getItem("refreshToken"),
});
        const refreshToken = localStorage.getItem("refreshToken");

        console.log("📦 refreshToken:", refreshToken);

        if (!refreshToken || refreshToken === "undefined") {
          console.log("❌ NO REFRESH TOKEN → logout");

          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");

          return Promise.reject(error);
        }

        const refreshResponse = await axios.post<ApiResponse<AuthGoResponse>>(
          "http://localhost:5107/auth/refresh",
          { refreshToken }
        );

        console.log("🔁 REFRESH RESPONSE:", refreshResponse.data);

        const data = refreshResponse.data;

        if (!data.success || !data.data) {
          console.log("❌ REFRESH INVALID");

          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");

          return Promise.reject(error);
        }

        const newAccessToken = data.data.accessToken;
        const newRefreshToken = data.data.refreshToken;

        console.log("✅ NEW TOKENS:", {
          access: newAccessToken,
          refresh: newRefreshToken,
        });

        localStorage.setItem("accessToken", newAccessToken);
        localStorage.setItem("refreshToken", newRefreshToken);

        originalRequest.headers = originalRequest.headers ?? {};
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;

        return api.request(originalRequest);
      } catch (e) {
        console.log("💥 REFRESH FAILED:", e);

        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");

        return Promise.reject(e);
      }
    }

    return Promise.reject(error);
  }
);
