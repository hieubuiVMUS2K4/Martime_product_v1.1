import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { 
  ArrowLeft, 
  Save, 
  X, 
  User, 
  FileText, 
  Calendar,
  Globe,
  Phone,
  Mail,
  MapPin,
  Shield,
  Award,
  AlertCircle,
  CheckCircle,
  Clock,
  Edit2,
  Trash2
} from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, differenceInDays, parseISO } from 'date-fns'

export function CrewDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  
  const [crew, setCrew] = useState<CrewMember | null>(null)
  const [loading, setLoading] = useState(true)
  const [isEditing, setIsEditing] = useState(false)
  const [editedCrew, setEditedCrew] = useState<Partial<CrewMember>>({})
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState(false)

  useEffect(() => {
    loadCrewDetails()
  }, [id])

  const loadCrewDetails = async () => {
    if (!id) return
    
    try {
      setLoading(true)
      const crewData = await maritimeService.crew.getById(Number(id))
      setCrew(crewData)
      setEditedCrew(crewData)
    } catch (error: any) {
      console.error('❌ Failed to load crew details:', error)
      const errorMessage = error.message || 'Failed to load crew member details'
      alert(`Error: ${errorMessage}`)
      setCrew(null)
    } finally {
      setLoading(false)
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

  const getCertificateStatus = (expiryDate?: string) => {
    if (!expiryDate) return { 
      status: 'Unknown', 
      color: 'text-gray-500', 
      bgColor: 'bg-gray-100',
      icon: AlertCircle,
      daysLeft: null 
    }
    
    const daysLeft = differenceInDays(parseISO(expiryDate), new Date())
    
    if (daysLeft < 0) return { 
      status: 'EXPIRED', 
      color: 'text-red-700', 
      bgColor: 'bg-red-100',
      icon: AlertCircle,
      daysLeft 
    }
    if (daysLeft <= 30) return { 
      status: 'CRITICAL', 
      color: 'text-red-600', 
      bgColor: 'bg-red-50',
      icon: AlertCircle,
      daysLeft 
    }
    if (daysLeft <= 90) return { 
      status: 'WARNING', 
      color: 'text-yellow-600', 
      bgColor: 'bg-yellow-50',
      icon: Clock,
      daysLeft 
    }
    return { 
      status: 'VALID', 
      color: 'text-green-700', 
      bgColor: 'bg-green-100',
      icon: CheckCircle,
      daysLeft 
    }
  }

  if (loading) {
    return (
      <div className="fixed inset-0 bg-gray-50 dark:bg-gray-900 flex items-center justify-center z-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading crew details...</p>
        </div>
      </div>
    )
  }

  if (!crew) {
    return (
      <div className="fixed inset-0 bg-gray-50 dark:bg-gray-900 flex items-center justify-center z-50">
        <div className="text-center">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">Crew Member Not Found</h2>
          <p className="text-gray-600 dark:text-gray-400 mb-6">The requested crew member could not be found.</p>
          <button
            onClick={() => navigate('/crew')}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Back to Crew List
          </button>
        </div>
      </div>
    )
  }

  const certStatus = getCertificateStatus(crew.certificateExpiry)
  const medicalStatus = getCertificateStatus(crew.medicalExpiry)
  const passportStatus = getCertificateStatus(crew.passportExpiry)
  const visaStatus = getCertificateStatus(crew.visaExpiry)

  const StatusIcon = certStatus.icon

  return (
    <div className="fixed inset-0 bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 dark:bg-gray-900 overflow-y-auto z-50">
      {/* Header Bar */}
      <div className="bg-white dark:bg-gray-800 shadow-sm sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <button
                onClick={() => navigate('/crew')}
                className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <ArrowLeft className="w-5 h-5 text-gray-600 dark:text-gray-400" />
              </button>
              <div>
                <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
                  <div className="h-12 w-12 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
                    <span className="text-blue-600 dark:text-blue-300 font-semibold text-lg">
                      {crew.fullName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                    </span>
                  </div>
                  {crew.fullName}
                </h1>
                <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                  {crew.position} • {crew.rank} • Crew ID: {crew.crewId}
                </p>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              {isEditing ? (
                <>
                  <button
                    onClick={handleCancel}
                    disabled={saving}
                    className="flex items-center gap-2 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50"
                  >
                    <X className="w-4 h-4" />
                    Cancel
                  </button>
                  <button
                    onClick={handleSave}
                    disabled={saving}
                    className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  >
                    {saving ? (
                      <>
                        <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                        Saving...
                      </>
                    ) : (
                      <>
                        <Save className="w-4 h-4" />
                        Save Changes
                      </>
                    )}
                  </button>
                </>
              ) : (
                <>
                  <button
                    onClick={() => {
                      setEditedCrew({ ...crew })
                      setIsEditing(true)
                    }}
                    className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    <Edit2 className="w-4 h-4" />
                    Edit Information
                  </button>
                  <button
                    onClick={handleDelete}
                    disabled={deleting}
                    className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {deleting ? (
                      <>
                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                        Deleting...
                      </>
                    ) : (
                      <>
                        <Trash2 className="w-4 h-4" />
                        Delete Crew
                      </>
                    )}
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-6 py-8">
        {/* Status Banner */}
        <div className="mb-6">
          <div className={`${certStatus.bgColor} border-l-4 ${certStatus.color.replace('text-', 'border-')} p-4 rounded-r-lg`}>
            <div className="flex items-start gap-3">
              <StatusIcon className={`w-6 h-6 ${certStatus.color} flex-shrink-0 mt-0.5`} />
              <div className="flex-1">
                <h3 className={`font-semibold ${certStatus.color}`}>
                  Certificate Status: {certStatus.status}
                </h3>
                {certStatus.daysLeft !== null && (
                  <p className={`text-sm ${certStatus.color} mt-1`}>
                    {certStatus.daysLeft < 0 
                      ? `Expired ${Math.abs(certStatus.daysLeft)} days ago`
                      : `${certStatus.daysLeft} days remaining until expiry`
                    }
                  </p>
                )}
                {crew.certificateExpiry && (
                  <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                    Expiry Date: {format(parseISO(crew.certificateExpiry), 'dd MMMM yyyy')}
                  </p>
                )}
              </div>
              {crew.isOnboard && (
                <span className="px-3 py-1 bg-green-100 text-green-700 text-sm font-semibold rounded-full">
                  ON BOARD
                </span>
              )}
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Left Column - Main Info */}
          <div className="lg:col-span-2 space-y-6">
            {/* Personal Information */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <User className="w-5 h-5 text-blue-600" />
                Personal Information
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <InfoField
                  label="Full Name"
                  value={isEditing ? (editedCrew.fullName || crew.fullName) : crew.fullName}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, fullName: value })}
                />
                <InfoField
                  label="Crew ID"
                  value={isEditing ? (editedCrew.crewId || crew.crewId) : crew.crewId}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, crewId: value })}
                />
                <InfoField
                  label="Date of Birth"
                  value={isEditing 
                    ? (editedCrew.dateOfBirth || crew.dateOfBirth || '') 
                    : (crew.dateOfBirth ? format(parseISO(crew.dateOfBirth), 'dd MMM yyyy') : 'N/A')}
                  type="date"
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, dateOfBirth: value })}
                />
                <InfoField
                  label="Nationality"
                  value={isEditing ? (editedCrew.nationality || crew.nationality || '') : (crew.nationality || 'N/A')}
                  icon={<Globe className="w-4 h-4" />}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, nationality: value })}
                />
                <InfoField
                  label="Phone Number"
                  value={isEditing ? (editedCrew.phoneNumber || crew.phoneNumber || '') : (crew.phoneNumber || 'N/A')}
                  icon={<Phone className="w-4 h-4" />}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, phoneNumber: value })}
                />
                <InfoField
                  label="Email Address"
                  value={isEditing ? (editedCrew.emailAddress || crew.emailAddress || '') : (crew.emailAddress || 'N/A')}
                  icon={<Mail className="w-4 h-4" />}
                  type="email"
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, emailAddress: value })}
                />
                <InfoField
                  label="Home Address"
                  value={isEditing ? (editedCrew.address || crew.address || '') : (crew.address || 'N/A')}
                  icon={<MapPin className="w-4 h-4" />}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, address: value })}
                  fullWidth
                />
              </div>
            </div>

            {/* Position & Rank */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <Shield className="w-5 h-5 text-blue-600" />
                Position & Rank
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <InfoField
                  label="Position"
                  value={isEditing ? (editedCrew.position || crew.position) : crew.position}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, position: value })}
                />
                <InfoField
                  label="Rank"
                  value={isEditing ? (editedCrew.rank || crew.rank || '') : (crew.rank || 'N/A')}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, rank: value })}
                />
                <InfoField
                  label="Department"
                  value={isEditing ? (editedCrew.department || crew.department || '') : (crew.department || 'N/A')}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, department: value })}
                />
                <InfoField
                  label="Join Date"
                  value={isEditing 
                    ? (editedCrew.joinDate || crew.joinDate || '') 
                    : (crew.joinDate ? format(parseISO(crew.joinDate), 'dd MMM yyyy') : 'N/A')}
                  type="date"
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, joinDate: value })}
                />
                {crew.embarkDate && (
                  <InfoField
                    label="Embark Date (Current Voyage)"
                    value={isEditing 
                      ? (editedCrew.embarkDate || crew.embarkDate) 
                      : format(parseISO(crew.embarkDate), 'dd MMM yyyy')}
                    type="date"
                    isEditing={isEditing}
                    onChange={(value) => setEditedCrew({ ...editedCrew, embarkDate: value })}
                  />
                )}
                {crew.contractEnd && (
                  <InfoField
                    label="Contract End Date"
                    value={isEditing 
                      ? (editedCrew.contractEnd || crew.contractEnd) 
                      : format(parseISO(crew.contractEnd), 'dd MMM yyyy')}
                    type="date"
                    isEditing={isEditing}
                    onChange={(value) => setEditedCrew({ ...editedCrew, contractEnd: value })}
                  />
                )}
              </div>
            </div>

            {/* Travel Documents */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <FileText className="w-5 h-5 text-blue-600" />
                Travel Documents
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <InfoField
                  label="Passport Number"
                  value={isEditing ? (editedCrew.passportNumber || crew.passportNumber || '') : (crew.passportNumber || 'N/A')}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, passportNumber: value })}
                />
                <div>
                  <InfoField
                    label="Passport Expiry"
                    value={isEditing 
                      ? (editedCrew.passportExpiry || crew.passportExpiry || '') 
                      : (crew.passportExpiry ? format(parseISO(crew.passportExpiry), 'dd MMM yyyy') : 'N/A')}
                    type="date"
                    isEditing={isEditing}
                    onChange={(value) => setEditedCrew({ ...editedCrew, passportExpiry: value })}
                  />
                  {passportStatus.daysLeft !== null && (
                    <div className={`mt-2 flex items-center gap-2 text-sm ${passportStatus.color}`}>
                      <passportStatus.icon className="w-4 h-4" />
                      <span>{passportStatus.daysLeft} days remaining</span>
                    </div>
                  )}
                </div>
                <InfoField
                  label="Visa Number"
                  value={isEditing ? (editedCrew.visaNumber || crew.visaNumber || '') : (crew.visaNumber || 'N/A')}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, visaNumber: value })}
                />
                <div>
                  <InfoField
                    label="Visa Expiry"
                    value={isEditing 
                      ? (editedCrew.visaExpiry || crew.visaExpiry || '') 
                      : (crew.visaExpiry ? format(parseISO(crew.visaExpiry), 'dd MMM yyyy') : 'N/A')}
                    type="date"
                    isEditing={isEditing}
                    onChange={(value) => setEditedCrew({ ...editedCrew, visaExpiry: value })}
                  />
                  {visaStatus.daysLeft !== null && (
                    <div className={`mt-2 flex items-center gap-2 text-sm ${visaStatus.color}`}>
                      <visaStatus.icon className="w-4 h-4" />
                      <span>{visaStatus.daysLeft} days remaining</span>
                    </div>
                  )}
                </div>
                <InfoField
                  label="Seaman's Book Number"
                  value={isEditing ? (editedCrew.seamanBookNumber || crew.seamanBookNumber || '') : (crew.seamanBookNumber || 'N/A')}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, seamanBookNumber: value })}
                />
              </div>
            </div>
          </div>

          {/* Right Column - Certificates */}
          <div className="space-y-6">
            {/* STCW Certificate */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <Award className="w-5 h-5 text-blue-600" />
                STCW Certificate
              </h2>
              <div className="space-y-4">
                <div className={`${certStatus.bgColor} p-4 rounded-lg border ${certStatus.color.replace('text-', 'border-')}`}>
                  <div className="flex items-center gap-2 mb-2">
                    <certStatus.icon className={`w-5 h-5 ${certStatus.color}`} />
                    <span className={`font-semibold ${certStatus.color}`}>{certStatus.status}</span>
                  </div>
                  {certStatus.daysLeft !== null && (
                    <p className={`text-sm ${certStatus.color}`}>
                      {certStatus.daysLeft < 0 
                        ? `Expired ${Math.abs(certStatus.daysLeft)} days ago`
                        : `${certStatus.daysLeft} days remaining`
                      }
                    </p>
                  )}
                </div>
                <InfoField
                  label="Certificate Number"
                  value={isEditing ? (editedCrew.certificateNumber || crew.certificateNumber || '') : (crew.certificateNumber || 'N/A')}
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, certificateNumber: value })}
                />
                <InfoField
                  label="Issue Date"
                  value={isEditing 
                    ? (editedCrew.certificateIssue || crew.certificateIssue || '') 
                    : (crew.certificateIssue ? format(parseISO(crew.certificateIssue), 'dd MMM yyyy') : 'N/A')}
                  type="date"
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, certificateIssue: value })}
                />
                <InfoField
                  label="Expiry Date"
                  value={isEditing 
                    ? (editedCrew.certificateExpiry || crew.certificateExpiry || '') 
                    : (crew.certificateExpiry ? format(parseISO(crew.certificateExpiry), 'dd MMM yyyy') : 'N/A')}
                  type="date"
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, certificateExpiry: value })}
                />
              </div>
            </div>

            {/* Medical Certificate */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <Calendar className="w-5 h-5 text-blue-600" />
                Medical Certificate
              </h2>
              <div className="space-y-4">
                <div className={`${medicalStatus.bgColor} p-4 rounded-lg border ${medicalStatus.color.replace('text-', 'border-')}`}>
                  <div className="flex items-center gap-2 mb-2">
                    <medicalStatus.icon className={`w-5 h-5 ${medicalStatus.color}`} />
                    <span className={`font-semibold ${medicalStatus.color}`}>{medicalStatus.status}</span>
                  </div>
                  {medicalStatus.daysLeft !== null && (
                    <p className={`text-sm ${medicalStatus.color}`}>
                      {medicalStatus.daysLeft < 0 
                        ? `Expired ${Math.abs(medicalStatus.daysLeft)} days ago`
                        : `${medicalStatus.daysLeft} days remaining`
                      }
                    </p>
                  )}
                </div>
                <InfoField
                  label="Issue Date"
                  value={isEditing 
                    ? (editedCrew.medicalIssue || crew.medicalIssue || '') 
                    : (crew.medicalIssue ? format(parseISO(crew.medicalIssue), 'dd MMM yyyy') : 'N/A')}
                  type="date"
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, medicalIssue: value })}
                />
                <InfoField
                  label="Expiry Date"
                  value={isEditing 
                    ? (editedCrew.medicalExpiry || crew.medicalExpiry || '') 
                    : (crew.medicalExpiry ? format(parseISO(crew.medicalExpiry), 'dd MMM yyyy') : 'N/A')}
                  type="date"
                  isEditing={isEditing}
                  onChange={(value) => setEditedCrew({ ...editedCrew, medicalExpiry: value })}
                />
              </div>
            </div>

            {/* Additional Information */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Notes</h2>
              {isEditing ? (
                <textarea
                  value={editedCrew.notes || crew.notes || ''}
                  onChange={(e) => setEditedCrew({ ...editedCrew, notes: e.target.value })}
                  rows={6}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                  placeholder="Additional notes about this crew member..."
                />
              ) : (
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  {crew.notes || 'No additional notes'}
                </p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

// InfoField Component
interface InfoFieldProps {
  label: string
  value: string
  icon?: React.ReactNode
  type?: 'text' | 'date' | 'email'
  isEditing: boolean
  onChange: (value: string) => void
  fullWidth?: boolean
}

function InfoField({ label, value, icon, type = 'text', isEditing, onChange, fullWidth }: InfoFieldProps) {
  return (
    <div className={fullWidth ? 'md:col-span-2' : ''}>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
        {label}
      </label>
      {isEditing ? (
        <input
          type={type}
          value={value === 'N/A' ? '' : value}
          onChange={(e) => onChange(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        />
      ) : (
        <div className="flex items-center gap-2 text-gray-900 dark:text-white">
          {icon && <span className="text-gray-400">{icon}</span>}
          <span>{value}</span>
        </div>
      )}
    </div>
  )
}
