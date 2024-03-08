/** @type {import('tailwindcss').Config} */
module.exports = {
  content:
  {
    relative: true,
    files: [
      "./**/*.{razor,cshtml,html}",
      "../Modules/**/*.{razor,cshtml,html}",
      "../Pulse.Shared.UI/**/*.{razor,cshtml,html}"
    ],
  },
  theme: {
    extend: {
      colors: {
        'pulse': {
          DEFAULT: '#080E24',
          50: '#202C8E',
          100: '#1E2B88',
          200: '#1B287C',
          300: '#19256F',
          400: '#162263',
          500: '#7FCAFF',
          600: '#101B4A',
          700: '#0E173D',
          800: '#0B1231',
          900: '#080E24',
          950: '#050815'
        },
        'gray': {
          50: '#F9FAFB',
          100: '#F3F4F6',
          200: '#E5E7EB',
          300: '#D1D5DB',
          400: '#9CA3AF',
          500: '#6B7280',
          600: '#888888',
          700: '#374151',
          800: '#1F2937',
          900: '#111827',
        },
      }
    }
  },
  plugins: [],
}