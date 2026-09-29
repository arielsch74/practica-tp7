import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    // En dev, las llamadas a /api se proxean al backend local (dotnet run → :8080).
    // En producción (contenedor) el mismo rol lo cumple el proxy_pass de nginx.
    proxy: {
      '/api': 'http://localhost:8080',
    },
  },
  test: {
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov', 'json-summary'],
      include: ['src/lib/**'],                  // ← QUÉ se mide: acá vive la lógica
      thresholds: { lines: 80, branches: 80 },  // ← TU número, el que puedas defender
    },
  },
})
