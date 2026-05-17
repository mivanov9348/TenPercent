import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true, // ВАЖНО: Това забранява минаването на 5174. Ако 5173 е зает, направо ще хвърли грешка.
  }
})