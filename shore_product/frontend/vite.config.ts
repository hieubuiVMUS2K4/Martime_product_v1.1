import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

const backendUrl = process.env.VITE_BACKEND_URL || 'http://localhost:5000'
const internalApiKey = process.env.INTERNAL_API_KEY || ''

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: backendUrl,
        changeOrigin: true,
        headers: internalApiKey ? { 'X-Internal-Api-Key': internalApiKey } : undefined,
      },
      '/uploads': {
        target: backendUrl,
        changeOrigin: true,
        headers: internalApiKey ? { 'X-Internal-Api-Key': internalApiKey } : undefined,
      }
    }
  }
})
