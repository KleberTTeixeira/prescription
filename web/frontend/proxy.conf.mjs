// Forwards the API calls made by the browser to the Web API (no CORS needed).
// The target can be changed with the API_URL environment variable (e.g. in Docker).
const target = process.env.API_URL || 'http://localhost:5000';

export default {
  '/api': { target, changeOrigin: true },
  '/odata': { target, changeOrigin: true },
  '/health': { target, changeOrigin: true },
};
