import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
    plugins: [vue()],
    server: {
        // Forward API calls to the .NET app so the browser only talks to one origin (no CORS needed).
        proxy: {
            '/api': 'http://localhost:5219'
        }
    }
})