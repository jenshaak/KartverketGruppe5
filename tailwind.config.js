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
        'custom-blue': '#1d4ed8',
        'warning': '#ef4444',
        'error': '#ef4444',
        'custom-gray': '#2c2c2c',
      }
    },
  },
  plugins: [],
  safelist: [
    {
      pattern: /(bg|text|border|shadow|p|w|px|py|h|gap|min-h|z|justify|items|mt|rounded-t|backdrop-blur)-.+/,
    },
    {
      pattern: /(absolute|fixed|sticky|relative|hidden|h-screen|group|group-hover|cursor-pointer|max-md:hidden|max-sm:hidden|max-lg:hidden|divider)/,
    },
    {
      pattern: /(rounded-l-lg|rounded-r-lg)/,
    },
    {
      pattern: /(top|right)-(0|4|8|12|16)/,
    },
    {
      pattern: /(space-y)-(0|2|4)/,
    },
    {
      pattern: /(line-clamp)-(0|1|2|3)/,
    },
    'group',
    'group-hover:hidden',
    'group-hover:block',
    'group-hover:w-56',
    'group-hover:justify-start',
    'group-hover:p-2'
  ]
}
