/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Views/**/*.cshtml',
    './Views/*.cshtml',
    './Pages/**/*.cshtml'
  ],
  theme: {
    extend: {
      colors: {
        'bg': '#d0ecd6',
        'primary': '#156630',
        'secondary': '#156630',
        'warning': '#ef4444',
        'error': '#ef4444',
        'custom-gray': '#2c2c2c',
      }
    },
  },
  plugins: [],
  safelist: [
    {
      pattern: /(bg|text|border|shadow|p|w|px|py|h|gap|min-h|justify)-.+/,
    },
    {
      pattern: /(absolute|fixed|relative|hidden|h-screen|group|group-hover|cursor-pointer|max-md:hidden|max-sm:hidden|max-lg:hidden)/,
    },
    {
      pattern: /(top|right)-(0|4|8|12|16)/,
    },
    {
      pattern: /(line-clamp)-(0|1|2|3)/,
    }
  ]
}
