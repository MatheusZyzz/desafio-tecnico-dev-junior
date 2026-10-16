import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Em desenvolvimento, /api é encaminhado para a API .NET (http://localhost:5080),
// então o front e a API funcionam na mesma origem e não há CORS a configurar no navegador.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL ?? 'http://localhost:5080',
        changeOrigin: true,
      },
    },
  },
})
