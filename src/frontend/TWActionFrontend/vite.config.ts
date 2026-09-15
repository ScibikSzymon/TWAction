import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // PORT is injected by Aspire (AppHost.cs) so it stays the single source of truth for the port.
    port: Number(process.env.PORT) || 3000,
    strictPort: true,
    proxy:{
      '/api' : {
        target: 'http://localhost:8000',
        changeOrigin: true,
        rewrite: (path => path.replace(/^\/api/,''))
      }
    }
  },
})
