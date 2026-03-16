/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        pitch: '#0A0F1E',
        card: '#1A2235',
        'card-hover': '#222D45',
        border: '#2A3550',
        green: { DEFAULT: '#00E676', dim: '#00B85A' },
        amber: '#FFB300',
        red: '#FF3D57',
        muted: '#8A9BBE',
        white: '#F0F4FF',
      },
      fontFamily: {
        display: ['"Barlow Condensed"', 'sans-serif'],
        body: ['Outfit', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'monospace'],
      },
      backgroundImage: {
        'pitch-radial': 'radial-gradient(ellipse at 20% 0%, #0F2040 0%, #0A0F1E 60%)',
      },
    },
  },
  plugins: [],
}
