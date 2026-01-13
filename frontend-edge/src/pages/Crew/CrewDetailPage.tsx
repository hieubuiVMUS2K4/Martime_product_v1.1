import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { 
  ArrowLeft, 
  Calendar,
  Phone,
  Mail,
  Award,
  AlertCircle,
  CheckCircle,
  Clock,
  Edit2,
  User
} from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO } from 'date-fns'
import { useTranslationSafe } from '@/contexts/I18nContext'

export function CrewDetailPage() {
  const { t } = useTranslationSafe()
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  
  const [crew, setCrew] = useState<CrewMember | null>(null)
  const [loading, setLoading] = useState(true)
  const [isEditing, setIsEditing] = useState(false)
  const [editedCrew, setEditedCrew] = useState<Partial<CrewMember>>({})
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [resettingPassword, setResettingPassword] = useState(false)
  const [certificates, setCertificates] = useState<any[]>([])
  const [loadingCertificates, setLoadingCertificates] = useState(false)

  useEffect(() => {
    loadCrewDetails()
  }, [id])

  const loadCrewDetails = async () => {
    if (!id) return
    
    try {
      setLoading(true)
      const crewData = await maritimeService.crew.getById(id)
      setCrew(crewData)
      setEditedCrew(crewData)
      
      // Load certificates for this crew member
      loadCertificates(id)
    } catch (error: any) {
      console.error('❌ Failed to load crew details:', error)
      const errorMessage = error.message || 'Failed to load crew member details'
      alert(`Error: ${errorMessage}`)
      setCrew(null)
    } finally {
      setLoading(false)
    }
  }

  const loadCertificates = async (crewId: string) => {
    try {
      setLoadingCertificates(true)
      const certs = await maritimeService.certificates.getCrewCertificatesByCrewId(crewId)
      setCertificates(certs)
    } catch (error: any) {
      console.error('❌ Failed to load certificates:', error)
    } finally {
      setLoadingCertificates(false)
    }
  }

  const handleSave = async () => {
    if (!crew) return
    
    try {
      setSaving(true)
      const updated = await maritimeService.crew.update(crew.id, editedCrew)
      setCrew(updated)
      setEditedCrew(updated)
      setIsEditing(false)
      
      // Show success notification
      console.log('✅ Crew member updated successfully')
      alert('✅ Crew member updated successfully!')
    } catch (error: any) {
      console.error('❌ Failed to save crew:', error)
      const errorMessage = error.message || 'Failed to update crew member'
      const errorDetails = error.details || ''
      alert(`Error: ${errorMessage}${errorDetails ? '\n\nDetails: ' + errorDetails : ''}`)
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async () => {
    if (!crew) return
    
    const confirmed = window.confirm(
      `⚠️ Are you sure you want to delete this crew member?\n\n` +
      `Crew ID: ${crew.crewId}\n` +
      `Name: ${crew.fullName}\n` +
      `Position: ${crew.position}\n\n` +
      `This action cannot be undone!`
    )
    
    if (!confirmed) return
    
    try {
      setDeleting(true)
      await maritimeService.crew.delete(crew.id)
      
      console.log('✅ Crew member deleted successfully')
      alert('✅ Crew member deleted successfully!')
      
      // Navigate back to crew list
      navigate('/crew')
    } catch (error: any) {
      console.error('❌ Failed to delete crew:', error)
      const errorMessage = error.message || 'Failed to delete crew member'
      const errorDetails = error.details || ''
      alert(`Error: ${errorMessage}${errorDetails ? '\n\nDetails: ' + errorDetails : ''}`)
    } finally {
      setDeleting(false)
    }
  }

  const handleCancel = () => {
    setEditedCrew(crew || {})
    setIsEditing(false)
  }

  const handleResetPassword = async () => {
    if (!crew) return
    
    if (!crew.dateOfBirth) {
      alert('⚠️ Cannot reset password: Date of birth is not set for this crew member.\n\nPlease update the date of birth first.')
      return
    }
    
    const confirmed = window.confirm(
      `🔐 Reset password for ${crew.fullName}?\n\n` +
      `Crew ID: ${crew.crewId}\n` +
      `New password will be: ${format(parseISO(crew.dateOfBirth), 'ddMMyyyy')}\n\n` +
      `(Format: DDMMYYYY based on date of birth)`
    )
    
    if (!confirmed) return
    
    try {
      setResettingPassword(true)
      
      const response = await fetch(`${import.meta.env.VITE_API_URL || 'http://localhost:3002'}/api/auth/reset-password`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: crew.crewId })
      })
      
      const result = await response.json()
      
      if (result.success) {
        alert(`✅ Password reset successfully!\n\nNew password: ${result.defaultPassword}\n\n(Based on date of birth: DDMMYYYY)`)
      } else {
        alert(`❌ Failed to reset password:\n${result.message}`)
      }
    } catch (error: any) {
      console.error('❌ Failed to reset password:', error)
      alert(`Error: ${error.message || 'Failed to reset password'}`)
    } finally {
      setResettingPassword(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (!crew) {
    return (
      <div className="p-8">
        <div className="bg-red-50 text-red-700 px-4 py-3 rounded">
          <p className="font-semibold">{t('crew.detail.error')}</p>
          <p className="text-sm">{t('crew.detail.crewNotFound')}</p>
        </div>
      </div>
    )
  }

  // Certificate data now managed separately via crew_certificates table

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-5xl mx-auto p-6">
        {/* Back Button */}
        <button
          onClick={() => navigate('/crew')}
          className="flex items-center gap-2 text-gray-600 hover:text-blue-600 transition-colors mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          <span className="text-sm">{t('crew.detail.backToList')}</span>
        </button>

        {/* Title and Action Buttons */}
        <div className="mb-4">
          <div className="flex items-center justify-between">
            <h1 className="text-xl font-semibold text-gray-700">Sory Dr Improment hisatory</h1>
            <div className="flex items-center gap-3">
              {isEditing ? (
                <>
                  <button onClick={handleCancel} disabled={saving} className="px-4 py-2 bg-white text-gray-700 text-sm rounded border border-gray-300 hover:bg-gray-50">
                    {t('crew.detail.cancel')}
                  </button>
                  <button onClick={handleSave} disabled={saving} className="px-4 py-2 bg-white text-blue-600 text-sm rounded border border-blue-600 hover:bg-blue-50">
                    {saving ? t('crew.detail.saving') : t('crew.detail.save')}
                  </button>
                </>
              ) : (
                <>
                  <button onClick={() => { setEditedCrew({ ...crew }); setIsEditing(true) }} className="px-4 py-2.5 bg-white text-blue-600 text-sm rounded border-2 border-blue-600 hover:bg-blue-50 flex items-center gap-2 font-medium">
                    <Edit2 className="w-4 h-4" />
                    {t('crew.detail.edit')}
                  </button>
                  <button onClick={handleResetPassword} disabled={resettingPassword} className="px-4 py-2.5 bg-white text-blue-600 text-sm rounded border-2 border-blue-600 hover:bg-blue-50 font-medium">
                    {t('crew.detail.resetPassword')}
                  </button>
                  <button onClick={handleDelete} disabled={deleting} className="px-4 py-2.5 bg-red-600 text-white text-sm rounded hover:bg-red-700 font-medium">
                    {t('crew.detail.delete')}
                  </button>
                </>
              )}
            </div>
          </div>
          <p className="text-sm text-gray-500 mt-1">{crew.fullName}</p>
        </div>

        {/* Header Card */}
        <div className="bg-gradient-to-r from-teal-700 to-teal-600 rounded-lg p-8 mb-4">
          <div className="flex items-center gap-6">
            {/* Avatar */}
            <div className="h-24 w-24 rounded-full bg-white flex items-center justify-center shadow-xl">
              <span className="text-teal-600 font-bold text-4xl">
                {crew.fullName.split(' ').map(n => n[0]).join('').slice(0, 2)}
              </span>
            </div>
            
            {/* Name & Title */}
            <div className="text-white">
              <h2 className="text-3xl font-bold mb-2">{crew.position} {crew.fullName}</h2>
              <p className="text-teal-100 text-lg mb-3">
                {crew.rank || 'N/A'}
              </p>
              <div className="flex items-center gap-3 text-sm">
                <span className="px-3 py-1.5 bg-teal-600/50 rounded-full border border-teal-400/30">{t('crew.detail.crewId')}: {crew.crewId || 'N/A'}</span>
                {crew.isOnboard && (
                  <span className="px-3 py-1.5 bg-teal-600/50 rounded-full flex items-center gap-1 border border-teal-400/30">
                    <CheckCircle className="w-4 h-4" />
                    {t('crew.status.onBoard')}
                  </span>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Contact Info Bar */}
        <div className={`shadow-sm p-5 mb-4 ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-white'}`}>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-8">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center">
                  <Phone className="w-5 h-5 text-blue-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase">{t('crew.detail.phone')}</p>
                  {isEditing ? (
                    <input 
                      type="tel" 
                      value={editedCrew.phoneNumber || ''} 
                      onChange={(e) => setEditedCrew({...editedCrew, phoneNumber: e.target.value})} 
                      className="w-full px-3 py-1 border-2 border-blue-400 rounded font-semibold focus:outline-none focus:border-blue-600"
                      placeholder="Phone number"
                    />
                  ) : (
                    <p className="font-semibold text-gray-900">{crew.phoneNumber || 'N/A'}</p>
                  )}
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center">
                  <Mail className="w-5 h-5 text-blue-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase">{t('crew.detail.email')}</p>
                  {isEditing ? (
                    <input 
                      type="email" 
                      value={editedCrew.emailAddress || ''} 
                      onChange={(e) => setEditedCrew({...editedCrew, emailAddress: e.target.value})} 
                      className="w-full px-3 py-1 border-2 border-blue-400 rounded font-semibold text-sm focus:outline-none focus:border-blue-600"
                      placeholder="Email address"
                    />
                  ) : (
                    <p className="font-semibold text-gray-900 text-sm">{crew.emailAddress || 'N/A'}</p>
                  )}
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center">
                  <Calendar className="w-5 h-5 text-blue-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase">{t('crew.detail.dateOfBirth')}</p>
                  <p className="font-semibold text-gray-900">{crew.dateOfBirth ? format(parseISO(crew.dateOfBirth), 'dd MMM yyyy') : 'N/A'}</p>
                </div>
              </div>
            </div>
            <button className="px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium flex items-center gap-2">
              <Calendar className="w-4 h-4" />
              {t('crew.detail.downloadCV')}
            </button>
          </div>
        </div>

        <div className="grid grid-cols-3 gap-4">
          {/* Left Column - Main Content */}
          <div className="col-span-2 space-y-4">
            {/* Personal Information */}
            <div className="bg-white shadow-sm rounded-lg p-6">
              <h2 className="text-lg font-bold mb-5 flex items-center gap-2 text-gray-700">
                <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center">
                  <User className="w-4 h-4 text-blue-600" />
                </div>
                {t('crew.section.personalInfo').toUpperCase()}
              </h2>
              <div className="space-y-4">
                <div className={`flex items-center gap-4 p-4 rounded-lg ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-gray-50'}`}>
                  <div className="w-10 h-10 rounded-full bg-teal-100 flex items-center justify-center">
                    <User className="w-5 h-5 text-teal-600" />
                  </div>
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.detail.fullName')}</p>
                    {isEditing ? (
                      <input 
                        type="text" 
                        value={editedCrew.fullName || ''} 
                        onChange={(e) => setEditedCrew({...editedCrew, fullName: e.target.value})} 
                        className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold focus:outline-none focus:border-blue-600"
                        placeholder="Enter full name"
                      />
                    ) : (
                      <p className="font-semibold text-gray-900">{crew.fullName}</p>
                    )}
                  </div>
                  <div className="text-right">
                    <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.detail.crewId')}</p>
                    {isEditing ? (
                      <input 
                        type="text" 
                        value={editedCrew.crewId || ''} 
                        onChange={(e) => setEditedCrew({...editedCrew, crewId: e.target.value})} 
                        className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold text-right focus:outline-none focus:border-blue-600"
                        placeholder="Crew ID"
                      />
                    ) : (
                      <p className="font-semibold text-gray-900">{crew.crewId}</p>
                    )}
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className={`p-4 rounded-lg ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-gray-50'}`}>
                    <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.detail.dateOfBirth')}</p>
                    {isEditing ? (
                      <input 
                        type="date" 
                        value={editedCrew.dateOfBirth?.split('T')[0] || ''} 
                        onChange={(e) => setEditedCrew({...editedCrew, dateOfBirth: e.target.value})} 
                        className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold focus:outline-none focus:border-blue-600"
                      />
                    ) : (
                      <p className="font-semibold text-gray-900">{crew.dateOfBirth ? format(parseISO(crew.dateOfBirth), 'dd MMM yyyy') : 'N/A'}</p>
                    )}
                  </div>
                  <div className={`p-4 rounded-lg ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-gray-50'}`}>
                    <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.detail.nationality')}</p>
                    {isEditing ? (
                      <input 
                        type="text" 
                        value={editedCrew.nationality || ''} 
                        onChange={(e) => setEditedCrew({...editedCrew, nationality: e.target.value})} 
                        className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold focus:outline-none focus:border-blue-600"
                        placeholder="Nationality"
                      />
                    ) : (
                      <p className="font-semibold text-gray-900">{crew.nationality || 'N/A'}</p>
                    )}
                  </div>
                  <div className={`p-4 rounded-lg ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-gray-50'}`}>
                    <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.detail.position')}</p>
                    {isEditing ? (
                      <input 
                        type="text" 
                        value={editedCrew.position || ''} 
                        onChange={(e) => setEditedCrew({...editedCrew, position: e.target.value})} 
                        className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold focus:outline-none focus:border-blue-600"
                        placeholder="Position"
                      />
                    ) : (
                      <p className="font-semibold text-gray-900">{crew.position}</p>
                    )}
                  </div>
                  <div className={`p-4 rounded-lg ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-gray-50'}`}>
                    <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.detail.rank')}</p>
                    {isEditing ? (
                      <input 
                        type="text" 
                        value={editedCrew.rank || ''} 
                        onChange={(e) => setEditedCrew({...editedCrew, rank: e.target.value})} 
                        className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold focus:outline-none focus:border-blue-600"
                        placeholder="Rank"
                      />
                    ) : (
                      <p className="font-semibold text-gray-900">{crew.rank || 'Master'}</p>
                    )}
                  </div>
                  <div className={`col-span-2 p-4 rounded-lg ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-gray-50'}`}>
                    <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.detail.address')}</p>
                    {isEditing ? (
                      <input 
                        type="text" 
                        value={editedCrew.address || ''} 
                        onChange={(e) => setEditedCrew({...editedCrew, address: e.target.value})} 
                        className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold focus:outline-none focus:border-blue-600"
                        placeholder="Address"
                      />
                    ) : (
                      <p className="font-semibold text-gray-900">{crew.address || 'N/A'}</p>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Employment History */}
            <div className="bg-white shadow-sm rounded-lg p-6">
              <h2 className="text-lg font-bold mb-5 flex items-center gap-2 text-gray-700">
                <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center">
                  <Clock className="w-4 h-4 text-blue-600" />
                </div>
                {t('crew.section.employmentHistory').toUpperCase()}
              </h2>
              
              {/* Timeline */}
              <div className="space-y-6 mb-6">
                {/* Current Position */}
                <div className="relative pl-8">
                  <div className="absolute left-0 top-1 w-4 h-4 rounded-full border-2 border-teal-600 bg-white"></div>
                  <div className="mb-1">
                    <span className="inline-block text-sm font-semibold text-teal-700 mb-1">{t('crew.detail.currentPosition')}</span>
                    {crew.joinDate && (
                      <p className="text-xs text-gray-500 mb-2">{t('crew.detail.joined')}: {format(parseISO(crew.joinDate), 'dd MMM yyyy')}</p>
                    )}
                  </div>
                  <div className="flex items-start gap-2 mb-3">
                    <div className="w-2 h-2 rounded-full bg-teal-600 mt-1.5"></div>
                    <div>
                      <p className="font-semibold text-gray-900">{crew.position}{crew.rank ? ` - ${crew.rank}` : ''}</p>
                      <p className="text-sm text-gray-600">{crew.joinDate ? format(parseISO(crew.joinDate), 'MMM yyyy') : 'N/A'} - {t('crew.detail.present')}</p>
                      {crew.isOnboard && (
                        <span className="inline-block mt-1 px-2 py-0.5 bg-green-100 text-green-700 rounded text-xs font-medium">{t('crew.status.currentlyOnBoard')}</span>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Employment History Card */}
            <div className="bg-white shadow-sm rounded-lg p-6">
              <h2 className="text-lg font-bold mb-5 flex items-center gap-2 text-gray-700">
                <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center">
                  <Clock className="w-4 h-4 text-blue-600" />
                </div>
                {t('crew.section.employmentHistory').toUpperCase()}
              </h2>

              {/* Captain Card */}
              <div className="bg-slate-700 rounded-lg p-4 mb-4 flex items-center gap-4">
                <div className="w-16 h-16 rounded-full bg-white flex items-center justify-center flex-shrink-0">
                  <span className="text-slate-700 font-bold text-2xl">
                    {crew.fullName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                  </span>
                </div>
                <div className="flex-1 text-white">
                  <h3 className="font-bold text-lg">{crew.position} {crew.fullName}</h3>
                  <p className="text-sm text-slate-300">{crew.rank || 'N/A'}</p>
                  <p className="text-xs text-slate-400 mt-1">{crew.nationality || 'N/A'}</p>
                  <div className="flex gap-2 mt-2">
                    <span className="px-2 py-0.5 bg-slate-600 rounded text-xs">{t('crew.detail.crewId')}: {crew.crewId}</span>
                    {crew.isOnboard && (
                      <span className="px-2 py-0.5 bg-green-600 rounded text-xs">{t('crew.status.onBoard')}</span>
                    )}
                  </div>
                </div>
                <div className="text-right text-white">
                  <p className="text-xs text-slate-400 mb-1">{t('crew.detail.joinDate').toUpperCase()}</p>
                  <p className="font-semibold mb-2">{crew.joinDate ? format(parseISO(crew.joinDate), 'dd MMM yyyy') : 'N/A'}</p>
                  {crew.phoneNumber && (
                    <p className="text-xs text-slate-300">{crew.phoneNumber}</p>
                  )}
                </div>
              </div>

              {/* Documents Grid */}
              <div className="grid grid-cols-2 gap-4">
                {/* Contact Info Card */}
                <div className="bg-slate-700 rounded-lg p-4 text-white">
                  <p className="text-xs text-slate-400 mb-2">{t('crew.detail.contact').toUpperCase()}</p>
                  <div className="space-y-2">
                    {crew.phoneNumber && (
                      <div className="flex items-center gap-2">
                        <Phone className="w-4 h-4" />
                        <span className="text-sm">{crew.phoneNumber}</span>
                      </div>
                    )}
                    {crew.emailAddress && (
                      <div className="flex items-center gap-2">
                        <Mail className="w-4 h-4" />
                        <span className="text-sm">{crew.emailAddress}</span>
                      </div>
                    )}
                    {crew.address && (
                      <p className="text-xs text-slate-300 mt-2">{crew.address}</p>
                    )}
                  </div>
                </div>

                {/* Certificate Info Card - Now managed via crew_certificates table */}
                <div className="bg-slate-700 rounded-lg p-4 text-white">
                  <p className="text-xs text-slate-400 mb-2">{t('crew.detail.certificates').toUpperCase()}</p>
                  <div className="space-y-2">
                    {crew.seamanBookNumber && (
                      <div>
                        <p className="text-xs text-slate-400">{t('crew.detail.seamanBook')}</p>
                        <p className="text-sm">{crew.seamanBookNumber}</p>
                      </div>
                    )}
                    <div className="mt-2">
                      <button
                        onClick={() => navigate('/crew', { state: { activeTab: 'certificates' } })}
                        className="text-xs text-blue-300 hover:text-blue-200 underline"
                      >
                        {t('crew.detail.viewAllCertificates')} →
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Seaman's Book */}
            <div className={`shadow-sm p-5 ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-white'}`}>
              <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <Award className="w-5 h-5 text-blue-600" />
                {t('crew.detail.seamanBook')}
              </h2>
              <div className="text-sm">
                <label className="text-gray-500 uppercase text-xs">{t('crew.detail.bookNumber')}</label>
                {isEditing ? (
                  <input type="text" value={editedCrew.seamanBookNumber || ''} onChange={(e) => setEditedCrew({...editedCrew, seamanBookNumber: e.target.value})} className="w-full px-3 py-2 border-2 border-blue-400 rounded font-semibold mt-1 focus:outline-none focus:border-blue-600" placeholder={t('crew.detail.bookNumber')} />
                ) : (
                  <p className="font-medium">{crew.seamanBookNumber || 'N/A'}</p>
                )}
              </div>
            </div>

            {/* Additional Notes */}
            {(crew.notes || isEditing) && (
              <div className={`shadow-sm p-5 ${isEditing ? 'bg-blue-50 border-2 border-blue-300' : 'bg-white'}`}>
                <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                  <AlertCircle className="w-5 h-5 text-blue-600" />
                  {t('crew.detail.additionalNotes')}
                </h2>
                {isEditing ? (
                  <textarea value={editedCrew.notes || ''} onChange={(e) => setEditedCrew({...editedCrew, notes: e.target.value})} rows={4} className="w-full px-3 py-2 border-2 border-blue-400 rounded focus:outline-none focus:border-blue-600" placeholder={t('crew.detail.additionalNotes')} />
                ) : (
                  <p className="text-sm text-gray-700">{crew.notes}</p>
                )}
              </div>
            )}
          </div>

          {/* Right Column - Certificates */}
          <div className="space-y-4">
            {/* STCW Certificates */}
            <div className="bg-white shadow-sm rounded-lg p-5">
              <div className="flex items-start gap-3 mb-4">
                <div className="w-12 h-12 rounded-lg bg-blue-100 flex items-center justify-center">
                  <Award className="w-6 h-6 text-blue-600" />
                </div>
                <div className="flex-1">
                  <h3 className="font-bold text-gray-900">{t('crew.detail.certificates')} ({certificates.length})</h3>
                  <p className="text-xs text-gray-500">{t('crew.detail.stcwCertificates')}</p>
                </div>
              </div>
              
              {loadingCertificates ? (
                <div className="text-center py-4">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
                  <p className="text-xs text-gray-500 mt-2">{t('crew.detail.loadingCertificates')}</p>
                </div>
              ) : certificates.length === 0 ? (
                <div className="p-3 bg-gray-50 rounded-lg text-center">
                  <p className="text-sm text-gray-600 mb-2">{t('crew.detail.noCertificatesFound')}</p>
                  <button
                    onClick={() => navigate('/crew', { state: { activeTab: 'certificates' } })}
                    className="text-sm text-blue-600 hover:text-blue-800 font-semibold underline"
                  >
                    {t('crew.detail.viewCertificateManagement')} →
                  </button>
                </div>
              ) : (
                <div className="space-y-3">
                  {certificates.map((cert: any) => {
                    const daysLeft = Math.floor((new Date(cert.expiryDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24))
                    const isExpired = daysLeft < 0
                    const isExpiring = daysLeft > 0 && daysLeft <= 90
                    
                    return (
                      <div key={cert.id} className="p-3 bg-gray-50 rounded-lg border border-gray-200">
                        <div className="flex items-start justify-between mb-2">
                          <div className="flex-1">
                            <p className="font-semibold text-sm text-gray-900">
                              {cert.certificate?.certificateName || 'Unknown Certificate'}
                            </p>
                            <p className="text-xs text-gray-500 mt-1">
                              {cert.certificate?.certificateCode || 'N/A'}
                            </p>
                          </div>
                          {isExpired ? (
                            <span className="px-2 py-1 bg-red-100 text-red-700 rounded text-xs font-semibold flex items-center gap-1">
                              <AlertCircle className="w-3 h-3" />
                              {t('crew.detail.expired')}
                            </span>
                          ) : isExpiring ? (
                            <span className="px-2 py-1 bg-yellow-100 text-yellow-700 rounded text-xs font-semibold flex items-center gap-1">
                              <Clock className="w-3 h-3" />
                              {t('crew.detail.expiring')}
                            </span>
                          ) : (
                            <span className="px-2 py-1 bg-green-100 text-green-700 rounded text-xs font-semibold flex items-center gap-1">
                              <CheckCircle className="w-3 h-3" />
                              {t('crew.detail.valid')}
                            </span>
                          )}
                        </div>
                        
                        <div className="space-y-1 text-xs">
                          <div className="flex justify-between">
                            <span className="text-gray-500">{t('crew.detail.number')}:</span>
                            <span className="font-medium text-gray-900">{cert.certificateNumber}</span>
                          </div>
                          <div className="flex justify-between">
                            <span className="text-gray-500">{t('crew.detail.issueDate')}:</span>
                            <span className="font-medium text-gray-900">{format(parseISO(cert.issueDate), 'dd MMM yyyy')}</span>
                          </div>
                          <div className="flex justify-between">
                            <span className="text-gray-500">{t('crew.detail.expiryDate')}:</span>
                            <span className={`font-medium ${isExpired ? 'text-red-600' : isExpiring ? 'text-yellow-600' : 'text-gray-900'}`}>
                              {format(parseISO(cert.expiryDate), 'dd MMM yyyy')}
                            </span>
                          </div>
                          {cert.issuingAuthority && (
                            <div className="flex justify-between mt-2 pt-2 border-t border-gray-200">
                              <span className="text-gray-500">{t('crew.detail.issuedBy')}:</span>
                              <span className="font-medium text-gray-900 text-right max-w-[150px] truncate">{cert.issuingAuthority}</span>
                            </div>
                          )}
                        </div>
                        
                        {isExpired && (
                          <div className="mt-2 pt-2 border-t border-gray-200">
                            <p className="text-xs text-red-600 font-medium">
                              ⚠️ {t('crew.detail.expiredDaysAgo', { days: Math.abs(daysLeft) })}
                            </p>
                          </div>
                        )}
                        {isExpiring && (
                          <div className="mt-2 pt-2 border-t border-gray-200">
                            <p className="text-xs text-yellow-600 font-medium">
                              ⏰ {t('crew.detail.expiresInDays', { days: daysLeft })}
                            </p>
                          </div>
                        )}
                      </div>
                    )
                  })}
                  
                  <button
                    onClick={() => navigate('/crew', { state: { activeTab: 'certificates' } })}
                    className="w-full py-2 text-sm text-blue-600 hover:text-blue-800 font-semibold border border-blue-200 rounded-lg hover:bg-blue-50 transition-colors"
                  >
                    {t('crew.detail.viewAllCertificates')} →
                  </button>
                </div>
              )}
            </div>

            {/* Medical Certificate - now managed in Certificate Management page */}

            {/* Seaman's Book */}
            <div className="bg-white shadow-sm rounded-lg p-5">
              <div className="flex items-start gap-3 mb-3">
                <div className="w-12 h-12 rounded-lg bg-blue-100 flex items-center justify-center">
                  <Award className="w-6 h-6 text-blue-600" />
                </div>
                <div className="flex-1">
                  <h3 className="font-bold text-gray-900">{t('crew.detail.seamanBook')}</h3>
                </div>
              </div>
              <p className="text-lg font-semibold text-gray-900">{crew.seamanBookNumber || 'N/A'}</p>
            </div>

            {/* Action Buttons */}
            <div className="space-y-3">
              <button className="w-full py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-semibold">
                {t('crew.detail.renewStcw')}
              </button>
              <button className="w-full py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-semibold">
                {t('crew.detail.requestShoreLeave')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
