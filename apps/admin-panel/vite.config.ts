/// <reference types="vitest/config" />
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react-swc'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    include: ['src/tests/useInformes.test.ts'],
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7096',
        changeOrigin: true,
        secure: false,
        // Rewrite Set-Cookie headers for dev environment
        // Backend sends Secure cookies, but we're on HTTP in dev
        cookieDomainRewrite: 'localhost',
        cookiePathRewrite: '/',
        headers: {
          // Tell backend the original request was HTTPS
          'X-Forwarded-Proto': 'https',
        },
      },
    },
  },
})
