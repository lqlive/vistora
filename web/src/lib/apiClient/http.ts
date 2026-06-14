import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

// Shared axios instance for the whole app. Feature-specific request functions
// live in their feature's `api.ts` and import this client.
export const api = axios.create({
  baseURL: API_BASE_URL,
});
