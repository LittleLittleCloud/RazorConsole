import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  base: process.env.VITE_BASE ?? '/',
  plugins: [
    react(), 
    tailwindcss(),
    // Custom plugin to replace %VITE_BASE% in HTML files
    {
      name: 'html-transform',
      transformIndexHtml(html) {
        const base = process.env.VITE_BASE ?? '/'
        return html.replace('{VITE_BASE}', base)
      }
    }
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  }
})
