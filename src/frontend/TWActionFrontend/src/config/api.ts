import axios from "axios";
import { API_BASE_URL } from "./env";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  timeout: 10000, // 10 second timeout
  headers: {
    "Content-Type": "application/json",
  },
});
