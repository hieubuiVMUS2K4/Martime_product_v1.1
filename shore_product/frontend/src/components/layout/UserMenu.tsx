import { useState, useRef, useEffect, useCallback } from 'react'
import { createPortal } from 'react-dom'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'
import { User, LogOut, Shield, ChevronDown, Anchor } from 'lucide-react'

export function UserMenu() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [isOpen, setIsOpen] = useState(false)
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const btnRef = useRef<HTMLButtonElement>(null)
  const dropRef = useRef<HTMLDivElement>(null)
  const [dropPos, setDropPos] = useState({ top: 0, right: 0 })

  // Calculate dropdown position from button
  useEffect(() => {
    if (isOpen && btnRef.current) {
      const rect = btnRef.current.getBoundingClientRect()
      setDropPos({
        top: rect.bottom + 8,
        right: window.innerWidth - rect.right,
      })
    }
  }, [isOpen])

  // Close on click outside
  useEffect(() => {
    if (!isOpen) return
    function handler(e: MouseEvent) {
      const t = e.target as Node
      if (btnRef.current?.contains(t)) return
      if (dropRef.current?.contains(t)) return
      setIsOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [isOpen])

  // Close on Escape
  useEffect(() => {
    if (!isOpen) return
    function handler(e: KeyboardEvent) {
      if (e.key === 'Escape') setIsOpen(false)
    }
    document.addEventListener('keydown', handler)
    return () => document.removeEventListener('keydown', handler)
  }, [isOpen])

  const handleLogout = useCallback(() => {
    setIsLoggingOut(true)
    logout()
    navigate('/login')
  }, [logout, navigate])

  if (!user) return null

  const displayName = user.username
  const roleLabel = user.role

  return (
    <>
      <button
        ref={btnRef}
        onClick={() => setIsOpen(v => !v)}
        style={{
          display: 'flex', alignItems: 'center', gap: 10,
          padding: '6px 12px', borderRadius: 8,
          // Nut nam tren thanh navy nen hover phai la trang mo, khong phai nen sang:
          // nen sang se nuot mat chu trang ben trong.
          border: 'none', background: isOpen ? 'rgba(255,255,255,0.12)' : 'transparent',
          cursor: 'pointer', transition: 'background 0.15s',
        }}
        onMouseEnter={(e) => e.currentTarget.style.background = 'rgba(255,255,255,0.12)'}
        onMouseLeave={(e) => e.currentTarget.style.background = isOpen ? 'rgba(255,255,255,0.12)' : 'transparent'}
      >
        <div style={{
          width: 32, height: 32, borderRadius: '50%',
          background: '#1b4c7e', color: '#fff',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 13, fontWeight: 700,
        }}>
          {displayName.charAt(0).toUpperCase()}
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start' }}>
          <span style={{ fontSize: 13, fontWeight: 600, color: '#ffffff', lineHeight: 1.2 }}>
            {displayName}
          </span>
          {roleLabel && (
            <span style={{ fontSize: 10, color: '#a8bcd4', textTransform: 'uppercase', letterSpacing: '0.05em', lineHeight: 1.2 }}>
              {roleLabel}
            </span>
          )}
        </div>
        <ChevronDown size={14} style={{
          color: '#a8bcd4', transition: 'transform 0.2s',
          transform: isOpen ? 'rotate(180deg)' : 'rotate(0deg)',
        }} />
      </button>

      {isOpen && createPortal(
        <div
          ref={dropRef}
          style={{
            position: 'fixed',
            top: dropPos.top,
            right: dropPos.right,
            width: 260,
            background: '#fff',
            border: '1px solid #e5e7eb',
            borderRadius: 12,
            boxShadow: '0 12px 32px rgba(0,0,0,0.15)',
            zIndex: 99999,
            overflow: 'hidden',
          }}
        >
          {/* User Info Header */}
          <div style={{ padding: '14px 16px', borderBottom: '1px solid #f1f5f9' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
              <div style={{
                width: 40, height: 40, borderRadius: '50%',
                background: 'linear-gradient(135deg, #0d7377, #14b8a6)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontWeight: 700, fontSize: 16,
              }}>
                {displayName.charAt(0).toUpperCase()}
              </div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: 14, fontWeight: 600, color: '#0f172a' }}>{displayName}</div>
                <div style={{ fontSize: 12, color: '#64748b' }}>@{user.username}</div>
              </div>
            </div>
            {roleLabel && (
              <div style={{ marginTop: 10 }}>
                <span style={{
                  display: 'inline-flex', alignItems: 'center', gap: 4,
                  padding: '2px 8px', borderRadius: 4,
                  fontSize: 10, fontWeight: 700, textTransform: 'uppercase',
                  letterSpacing: '0.06em', color: '#0d7377',
                  background: 'rgba(13,115,119,0.08)', border: '1px solid rgba(13,115,119,0.15)',
                }}>
                  <Shield size={10} />
                  {roleLabel}
                </span>
              </div>
            )}
          </div>

          {/* Profile */}
          <div style={{ padding: '6px 0' }}>
            <button
              style={{
                width: '100%', display: 'flex', alignItems: 'center', gap: 12,
                padding: '10px 16px', border: 'none', background: 'none',
                cursor: 'pointer', fontSize: 13, color: '#475569', textAlign: 'left',
              }}
              onMouseEnter={(e) => e.currentTarget.style.background = '#f8fafc'}
              onMouseLeave={(e) => e.currentTarget.style.background = 'none'}
              onClick={() => setIsOpen(false)}
            >
              <User size={16} style={{ color: '#94a3b8' }} />
              <span>Hồ sơ</span>
            </button>
          </div>

          {/* Logout */}
          <div style={{ borderTop: '1px solid #f1f5f9', padding: '6px 0' }}>
            <button
              onClick={handleLogout}
              disabled={isLoggingOut}
              style={{
                width: '100%', display: 'flex', alignItems: 'center', gap: 12,
                padding: '10px 16px', border: 'none', background: 'none',
                cursor: isLoggingOut ? 'not-allowed' : 'pointer',
                fontSize: 13, color: '#ef4444', textAlign: 'left',
                opacity: isLoggingOut ? 0.5 : 1,
              }}
              onMouseEnter={(e) => { if (!isLoggingOut) e.currentTarget.style.background = '#fef2f2' }}
              onMouseLeave={(e) => e.currentTarget.style.background = 'none'}
            >
              <LogOut size={16} />
              <span>{isLoggingOut ? 'Đang đăng xuất...' : 'Đăng xuất'}</span>
            </button>
          </div>

          {/* Footer */}
          <div style={{
            padding: '8px 16px', borderTop: '1px solid #f1f5f9',
            background: '#f8fafc', textAlign: 'center',
          }}>
            <span style={{ fontSize: 9, color: '#94a3b8', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
              <Anchor size={9} style={{ display: 'inline', verticalAlign: 'middle', marginRight: 4 }} />
              Shore Fleet Management
            </span>
          </div>
        </div>,
        document.body
      )}
    </>
  )
}
