/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
        // Maritime specific colors
        maritime: {
          critical: '#dc2626',
          warning: '#f59e0b',
          info: '#3b82f6',
          success: '#10b981',
          ocean: '#0369a1',
          sky: '#0284c7',
        },
        // Industrial Theme (Maritime E-Logbook)
        industrial: {
          bg: {
            primary: '#1a1a1a',
            secondary: '#262626',
            tertiary: '#333333',
          },
          text: {
            amber: '#ffb000',
            green: '#00ff00',
            white: '#ffffff',
            gray: '#a3a3a3',
          },
          border: {
            DEFAULT: '#333333',
            light: '#404040',
            heavy: '#525252',
          },
          surface: {
            DEFAULT: '#262626',
            raised: '#2d2d2d',
          },
          status: {
            running: '#10b981',
            stopped: '#ef4444',
            standby: '#f59e0b',
            alarm: '#dc2626',
          }
        }
      },
      fontFamily: {
        mono: ['Consolas', 'Roboto Mono', 'Courier New', 'monospace'],
        sans: ['Inter', 'Roboto', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        // Large touch-friendly sizes
        'touch-sm': ['1.125rem', { lineHeight: '1.75rem' }],
        'touch-base': ['1.25rem', { lineHeight: '2rem' }],
        'touch-lg': ['1.5rem', { lineHeight: '2.25rem' }],
        'touch-xl': ['1.875rem', { lineHeight: '2.5rem' }],
      },
      spacing: {
        // Touch target minimum 44px
        'touch': '2.75rem', // 44px
        'touch-lg': '3.5rem', // 56px
      },
      borderRadius: {
        none: '0',
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
      keyframes: {
        marquee: {
          '0%': { transform: 'translateX(0%)' },
          '100%': { transform: 'translateX(-50%)' },
        },
      },
      animation: {
        marquee: 'marquee 8s linear infinite',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
}
