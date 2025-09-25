
module.exports = {
  darkMode: 'class',
  content: [
    "./Views/**/*.cshtml",
    "./Areas/**/*.cshtml",
    "./Pages/**/*.cshtml",
    "./wwwroot/**/*.js",
    "./wwwroot/**/*.css",
    "./node_modules/tailadmin/src/**/*.{js,jsx,ts,tsx}"
  ],
  theme: {
    extend: {
      zIndex: {
        '99999': '99999',
      },
      boxShadow: {
        'theme-xs': 'var(--shadow-theme-xs)',
        'theme-sm': 'var(--shadow-theme-sm)',
        'theme-md': 'var(--shadow-theme-md)',
        'theme-lg': 'var(--shadow-theme-lg)',
        'theme-xl': 'var(--shadow-theme-xl)',
        'focus-ring': 'var(--shadow-focus-ring)',
      },
      ringColor: {
        'brand-500/10': 'rgb(var(--color-brand-500) / 0.1)',
      },
      colors: {
        // Base colors
        white: 'var(--white)',
        black: 'var(--black)',
        
        // Brand colors - Simplified
        brand: {
          light: 'var(--brand-light)',
          DEFAULT: 'var(--brand)',
          hover: 'var(--brand-hover)',        // Use this for hover states
          dark: 'var(--brand-dark)',          // DEPRECATED: Use brand.hover instead
        },
        
        // Status colors
        success: {
          light: 'var(--success-light)',
          DEFAULT: 'var(--success)',
          dark: 'var(--success-dark)',
        },
        error: {
          light: 'var(--error-light)',
          DEFAULT: 'var(--error)',
          dark: 'var(--error-dark)',
        },
        warning: {
          light: 'var(--warning-light)',
          DEFAULT: 'var(--warning)',
          dark: 'var(--warning-dark)',
        },
        
        // Gray scale - Essential shades
        gray: {
          50: 'var(--gray-50)',
          100: 'var(--gray-100)',
          200: 'var(--gray-200)',
          400: 'var(--gray-400)',
          500: 'var(--gray-500)',
          700: 'var(--gray-700)',
          800: 'var(--gray-800)',
          900: 'var(--gray-900)',
        },
        
        // Surface colors for subtle backgrounds
        surface: {
          light: 'var(--surface-light)',
          subtle: 'var(--surface-subtle)',
        },
      },
      backgroundImage: {
        'gradient-brand': 'var(--gradient-brand)',
        'gradient-dark': 'var(--gradient-dark)',
        'gradient-light': 'var(--gradient-light)',
      },
    },
  },
  plugins: [],
}
