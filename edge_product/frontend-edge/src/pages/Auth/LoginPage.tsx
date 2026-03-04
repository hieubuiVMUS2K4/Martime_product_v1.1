import { useState, useCallback, useEffect, useRef } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'
import { Anchor, Eye, EyeOff, AlertTriangle, Loader2, Shield, Ship, User, Lock } from 'lucide-react'
import { VESSEL_CONFIG } from '@/config/app.config'

// ============================================================
// LOGIN PAGE - Maritime ISPS/ISM Compliant Professional UI v2
// ============================================================

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { login, isLoggingIn, error, clearError, isAuthenticated } = useAuthStore()

  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/dashboard'

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [mounted, setMounted] = useState(false)
  const usernameRef = useRef<HTMLInputElement>(null)

  // Entrance animation trigger
  useEffect(() => {
    const timer = setTimeout(() => setMounted(true), 50)
    return () => clearTimeout(timer)
  }, [])

  useEffect(() => {
    if (isAuthenticated) {
      navigate(from, { replace: true })
    }
  }, [isAuthenticated, navigate, from])

  useEffect(() => {
    usernameRef.current?.focus()
  }, [])

  useEffect(() => {
    if (error) clearError()
  }, [username, password]) // eslint-disable-line react-hooks/exhaustive-deps

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault()
      if (!username.trim() || !password.trim() || isLoggingIn) return

      const success = await login(username.trim(), password)
      if (success) {
        navigate(from, { replace: true })
      }
    },
    [username, password, isLoggingIn, login, navigate, from]
  )

  const [currentTime, setCurrentTime] = useState(new Date())
  useEffect(() => {
    const timer = setInterval(() => setCurrentTime(new Date()), 1000)
    return () => clearInterval(timer)
  }, [])

  const formatTime = (date: Date) =>
    date.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false })
  const formatDate = (date: Date) =>
    date.toLocaleDateString('en-GB', { weekday: 'short', day: '2-digit', month: 'short', year: 'numeric' })

  return (
    <div className="login-page min-h-screen relative flex items-center justify-center overflow-hidden">
      {/* ===== BACKGROUND LAYERS ===== */}

      {/* Base gradient - richer ocean colors */}
      <div
        className="absolute inset-0"
        style={{
          background: 'linear-gradient(135deg, #0a0e27 0%, #0c1445 25%, #0d1b4a 50%, #0a1628 75%, #060d1f 100%)',
        }}
      />

      {/* Animated gradient orbs for depth */}
      <div
        className="absolute w-[600px] h-[600px] rounded-full opacity-[0.07]"
        style={{
          background: 'radial-gradient(circle, #1e40af 0%, transparent 70%)',
          top: '-15%',
          right: '-10%',
          animation: 'float-slow 20s ease-in-out infinite',
        }}
      />
      <div
        className="absolute w-[500px] h-[500px] rounded-full opacity-[0.05]"
        style={{
          background: 'radial-gradient(circle, #0ea5e9 0%, transparent 70%)',
          bottom: '-10%',
          left: '-10%',
          animation: 'float-slow 25s ease-in-out infinite reverse',
        }}
      />
      <div
        className="absolute w-[300px] h-[300px] rounded-full opacity-[0.04]"
        style={{
          background: 'radial-gradient(circle, #6366f1 0%, transparent 70%)',
          top: '50%',
          left: '60%',
          animation: 'float-slow 18s ease-in-out infinite',
        }}
      />

      {/* Grid pattern for texture */}
      <div
        className="absolute inset-0 opacity-[0.03]"
        style={{
          backgroundImage:
            'linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.05) 1px, transparent 1px)',
          backgroundSize: '60px 60px',
        }}
      />

      {/* Wave pattern - more visible */}
      <div
        className="absolute inset-0 opacity-[0.06]"
        style={{
          backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1440 320'%3E%3Cpath fill='%233b82f6' d='M0,192L48,186.7C96,181,192,171,288,186.7C384,203,480,245,576,245.3C672,245,768,203,864,181.3C960,160,1056,160,1152,170.7C1248,181,1344,203,1392,213.3L1440,224L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z'%3E%3C/path%3E%3C/svg%3E")`,
          backgroundSize: '100% 250px',
          backgroundRepeat: 'repeat-y',
          animation: 'wave-drift 30s linear infinite',
        }}
      />

      {/* Compass rose - more visible */}
      <div className="absolute -top-10 -right-10 w-[500px] h-[500px] opacity-[0.06]">
        <svg viewBox="0 0 200 200" fill="none" xmlns="http://www.w3.org/2000/svg">
          <circle cx="100" cy="100" r="95" stroke="#60a5fa" strokeWidth="0.8" />
          <circle cx="100" cy="100" r="75" stroke="#60a5fa" strokeWidth="0.4" />
          <circle cx="100" cy="100" r="55" stroke="#60a5fa" strokeWidth="0.2" />
          <line x1="100" y1="5" x2="100" y2="195" stroke="#60a5fa" strokeWidth="0.4" />
          <line x1="5" y1="100" x2="195" y2="100" stroke="#60a5fa" strokeWidth="0.4" />
          <line x1="30" y1="30" x2="170" y2="170" stroke="#60a5fa" strokeWidth="0.2" />
          <line x1="170" y1="30" x2="30" y2="170" stroke="#60a5fa" strokeWidth="0.2" />
          <polygon points="100,10 106,50 100,38 94,50" fill="#60a5fa" />
          <polygon points="100,190 106,150 100,162 94,150" fill="#60a5fa" />
          <polygon points="10,100 50,94 38,100 50,106" fill="#60a5fa" />
          <polygon points="190,100 150,94 162,100 150,106" fill="#60a5fa" />
          <text x="100" y="7" textAnchor="middle" fill="#60a5fa" fontSize="6" fontWeight="bold">N</text>
          <text x="100" y="199" textAnchor="middle" fill="#60a5fa" fontSize="6" fontWeight="bold">S</text>
          <text x="197" y="102" textAnchor="end" fill="#60a5fa" fontSize="6" fontWeight="bold">E</text>
          <text x="5" y="102" textAnchor="start" fill="#60a5fa" fontSize="6" fontWeight="bold">W</text>
        </svg>
      </div>

      {/* Compass rose - bottom left (subtle) */}
      <div className="absolute -bottom-20 -left-20 w-[350px] h-[350px] opacity-[0.03] rotate-45">
        <svg viewBox="0 0 200 200" fill="none" xmlns="http://www.w3.org/2000/svg">
          <circle cx="100" cy="100" r="95" stroke="#60a5fa" strokeWidth="0.8" />
          <circle cx="100" cy="100" r="75" stroke="#60a5fa" strokeWidth="0.4" />
          <line x1="100" y1="5" x2="100" y2="195" stroke="#60a5fa" strokeWidth="0.4" />
          <line x1="5" y1="100" x2="195" y2="100" stroke="#60a5fa" strokeWidth="0.4" />
        </svg>
      </div>

      {/* ===== MAIN CONTENT ===== */}
      <div
        className={`relative z-10 w-full max-w-[420px] px-5 sm:px-0 transition-all duration-700 ease-out ${
          mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-6'
        }`}
      >
        {/* Vessel Info Strip - Top */}
        <div className="flex items-center justify-between text-xs mb-5 px-1">
          <span className="font-mono tracking-wider uppercase text-blue-300/70 bg-white/[0.04] px-3 py-1 rounded-full border border-white/[0.06]">
            {VESSEL_CONFIG.IMO_NUMBER ? `IMO ${VESSEL_CONFIG.IMO_NUMBER}` : 'MARITIME EDGE'}
          </span>
          <span className="font-mono text-blue-300/50 tabular-nums">
            {formatDate(currentTime)} — {formatTime(currentTime)} UTC
          </span>
        </div>

        {/* Login Card */}
        <div
          className="relative rounded-2xl overflow-hidden"
          style={{
            background: 'linear-gradient(135deg, rgba(255,255,255,0.08) 0%, rgba(255,255,255,0.03) 100%)',
            backdropFilter: 'blur(24px) saturate(1.4)',
            WebkitBackdropFilter: 'blur(24px) saturate(1.4)',
            boxShadow: '0 25px 60px -12px rgba(0, 0, 0, 0.5), 0 0 0 1px rgba(255,255,255,0.08), inset 0 1px 0 0 rgba(255,255,255,0.1)',
          }}
        >
          {/* Subtle top accent line */}
          <div
            className="h-[2px] w-full"
            style={{
              background: 'linear-gradient(90deg, transparent 0%, #3b82f6 30%, #06b6d4 50%, #3b82f6 70%, transparent 100%)',
              opacity: 0.6,
            }}
          />

          {/* Card Header */}
          <div className="px-8 pt-8 pb-5 text-center">
            {/* Anchor Icon with glow ring */}
            <div className="relative inline-flex items-center justify-center mb-6">
              {/* Outer glow ring */}
              <div
                className="absolute w-24 h-24 rounded-2xl opacity-20"
                style={{
                  background: 'radial-gradient(circle, #3b82f6 0%, transparent 70%)',
                  animation: 'pulse-glow 3s ease-in-out infinite',
                }}
              />
              {/* Icon container */}
              <div
                className="relative w-[68px] h-[68px] rounded-2xl flex items-center justify-center"
                style={{
                  background: 'linear-gradient(135deg, #2563eb 0%, #0891b2 100%)',
                  boxShadow: '0 8px 32px -4px rgba(37, 99, 235, 0.4), 0 0 0 1px rgba(255,255,255,0.1)',
                }}
              >
                <Anchor className="w-8 h-8 text-white drop-shadow-sm" />
              </div>
            </div>

            <h1 className="text-[26px] font-bold text-white tracking-tight leading-tight">
              {VESSEL_CONFIG.VESSEL_NAME || 'Maritime Edge'}
            </h1>
            <p className="text-[13px] text-blue-200/60 mt-2 font-semibold tracking-[0.15em] uppercase">
              Electronic Logbook System
            </p>
          </div>

          {/* ISPS Security Notice - more visible */}
          <div
            className="mx-6 mb-5 px-4 py-3 rounded-xl flex items-start gap-3"
            style={{
              background: 'linear-gradient(135deg, rgba(245, 158, 11, 0.08) 0%, rgba(245, 158, 11, 0.04) 100%)',
              border: '1px solid rgba(245, 158, 11, 0.15)',
            }}
          >
            <Shield className="w-4 h-4 text-amber-400/90 mt-0.5 flex-shrink-0" />
            <p className="text-[11.5px] leading-[1.6] text-amber-100/70">
              ISPS Code compliant system. All access is logged and monitored per IMO MSC.428(98).
              Unauthorized access is prohibited.
            </p>
          </div>

          {/* Login Form */}
          <form onSubmit={handleSubmit} className="px-8 pb-8">
            {/* Error Alert */}
            {error && (
              <div
                className="mb-5 px-4 py-3 rounded-xl flex items-start gap-3"
                style={{
                  background: 'linear-gradient(135deg, rgba(239, 68, 68, 0.12) 0%, rgba(239, 68, 68, 0.06) 100%)',
                  border: '1px solid rgba(239, 68, 68, 0.2)',
                  animation: 'shake 0.4s ease-out, fadeSlideIn 0.3s ease-out',
                }}
              >
                <AlertTriangle className="w-4 h-4 text-red-400 mt-0.5 flex-shrink-0" />
                <p className="text-sm text-red-300/90">{error}</p>
              </div>
            )}

            {/* Username Field */}
            <div className="mb-4">
              <label
                htmlFor="username"
                className="block text-[11px] font-bold text-blue-200/60 uppercase tracking-[0.12em] mb-2.5"
              >
                Username / Officer ID
              </label>
              <div className="relative group">
                <div className="absolute left-4 top-1/2 -translate-y-1/2 pointer-events-none">
                  <User className="w-[18px] h-[18px] text-blue-300/30 group-focus-within:text-blue-400/60 transition-colors duration-200" />
                </div>
                <input
                  ref={usernameRef}
                  id="username"
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  placeholder="Enter your username"
                  disabled={isLoggingIn}
                  autoComplete="off"
                  spellCheck={false}
                  className="w-full h-[50px] pl-11 pr-4 rounded-xl text-white text-sm placeholder-white/25
                           border border-white/[0.08] 
                           focus:outline-none focus:border-blue-500/50 focus:ring-2 focus:ring-blue-500/20
                           hover:border-white/[0.15] hover:bg-white/[0.06]
                           disabled:opacity-50 disabled:cursor-not-allowed
                           transition-all duration-200"
                  style={{ background: 'rgba(255,255,255,0.04)' }}
                />
              </div>
            </div>

            {/* Password Field */}
            <div className="mb-7">
              <label
                htmlFor="password"
                className="block text-[11px] font-bold text-blue-200/60 uppercase tracking-[0.12em] mb-2.5"
              >
                Password
              </label>
              <div className="relative group">
                <div className="absolute left-4 top-1/2 -translate-y-1/2 pointer-events-none">
                  <Lock className="w-[18px] h-[18px] text-blue-300/30 group-focus-within:text-blue-400/60 transition-colors duration-200" />
                </div>
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter your password"
                  disabled={isLoggingIn}
                  autoComplete="off"
                  className="w-full h-[50px] pl-11 pr-12 rounded-xl text-white text-sm placeholder-white/25
                           border border-white/[0.08]
                           focus:outline-none focus:border-blue-500/50 focus:ring-2 focus:ring-blue-500/20
                           hover:border-white/[0.15] hover:bg-white/[0.06]
                           disabled:opacity-50 disabled:cursor-not-allowed
                           transition-all duration-200"
                  style={{ background: 'rgba(255,255,255,0.04)' }}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  tabIndex={-1}
                  className="absolute right-3.5 top-1/2 -translate-y-1/2 p-1.5 rounded-lg
                           text-white/30 hover:text-white/70 hover:bg-white/[0.06]
                           transition-all duration-200"
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? (
                    <EyeOff className="w-[18px] h-[18px]" />
                  ) : (
                    <Eye className="w-[18px] h-[18px]" />
                  )}
                </button>
              </div>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoggingIn || !username.trim() || !password.trim()}
              className="group relative w-full h-[50px] text-white font-semibold text-sm rounded-xl
                       disabled:opacity-35 disabled:cursor-not-allowed disabled:hover:shadow-none
                       focus:outline-none focus:ring-2 focus:ring-blue-400/50 focus:ring-offset-2 focus:ring-offset-transparent
                       transition-all duration-300 
                       active:scale-[0.98]
                       overflow-hidden"
              style={{
                background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 50%, #1e40af 100%)',
                boxShadow: '0 8px 24px -4px rgba(37, 99, 235, 0.35), 0 0 0 1px rgba(255,255,255,0.05)',
              }}
              onMouseEnter={(e) => {
                if (!isLoggingIn) {
                  e.currentTarget.style.boxShadow =
                    '0 12px 32px -4px rgba(37, 99, 235, 0.5), 0 0 0 1px rgba(255,255,255,0.1)'
                  e.currentTarget.style.background =
                    'linear-gradient(135deg, #3b82f6 0%, #2563eb 50%, #1d4ed8 100%)'
                }
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.boxShadow =
                  '0 8px 24px -4px rgba(37, 99, 235, 0.35), 0 0 0 1px rgba(255,255,255,0.05)'
                e.currentTarget.style.background =
                  'linear-gradient(135deg, #2563eb 0%, #1d4ed8 50%, #1e40af 100%)'
              }}
            >
              {/* Hover shine effect */}
              <div
                className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500"
                style={{
                  background: 'linear-gradient(105deg, transparent 40%, rgba(255,255,255,0.08) 45%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.08) 55%, transparent 60%)',
                }}
              />
              <span className="relative z-10">
                {isLoggingIn ? (
                  <span className="inline-flex items-center gap-2.5">
                    <Loader2 className="w-4 h-4 animate-spin" />
                    Authenticating...
                  </span>
                ) : (
                  'Sign In'
                )}
              </span>
            </button>
          </form>
        </div>

        {/* Footer - Compliance Info */}
        <div className="mt-7 text-center space-y-2">
          <div className="flex items-center justify-center gap-2 text-[11px] text-blue-300/45">
            <Ship className="w-3.5 h-3.5" />
            <span className="tracking-wide">ISPS Code &middot; ISM Code &middot; IMO MSC.428(98)</span>
          </div>
          <p className="text-[10.5px] text-blue-300/30 tracking-wide">
            Maritime Edge E-Logbook &copy; {new Date().getFullYear()}. All sessions are recorded.
          </p>
        </div>
      </div>

      {/* ===== INLINE STYLES FOR ANIMATIONS ===== */}
      <style>{`
        .login-page * {
          /* Prevent dark mode overrides on login page */
        }
        
        @keyframes float-slow {
          0%, 100% { transform: translate(0, 0) scale(1); }
          33% { transform: translate(15px, -20px) scale(1.05); }
          66% { transform: translate(-10px, 15px) scale(0.95); }
        }

        @keyframes wave-drift {
          0% { background-position-y: 0; }
          100% { background-position-y: 250px; }
        }

        @keyframes pulse-glow {
          0%, 100% { opacity: 0.15; transform: scale(1); }
          50% { opacity: 0.3; transform: scale(1.08); }
        }

        @keyframes shake {
          0%, 100% { transform: translateX(0); }
          20% { transform: translateX(-6px); }
          40% { transform: translateX(5px); }
          60% { transform: translateX(-3px); }
          80% { transform: translateX(2px); }
        }

        @keyframes fadeSlideIn {
          from { opacity: 0; transform: translateY(-8px); }
          to { opacity: 1; transform: translateY(0); }
        }
      `}</style>
    </div>
  )
}
