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
  theme: {},
  plugins: [],
}