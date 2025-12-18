import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3002,
    host: '0.0.0.0', // Allow access from other devices on LAN
    proxy: {
      '/api': {
        target: 'http://localhost:5001', // Edge Backend API
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
})
