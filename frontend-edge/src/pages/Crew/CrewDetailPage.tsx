import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { 
  ArrowLeft, 
  Trash2,
  Upload,
  Download,
  CheckCircle,
  XCircle,
  AlertTriangle
} from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, differenceInDays, parseISO } from 'date-fns'

type TabType = 'basic-data' | 'documents'

export function CrewDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  
  const [crew, setCrew] = useState<CrewMember | null>(null)
  const [loading, setLoading] = useState(true)
  const [editedCrew, setEditedCrew] = useState<Partial<CrewMember>>({})
  const [saving, setSaving] = useState(false)
  const [activeTab, setActiveTab] = useState<TabType>('basic-data')
  const [certificates, setCertificates] = useState<any[]>([])
  const [loadingCertificates, setLoadingCertificates] = useState(false)
  const [isIdentityExpanded, setIsIdentityExpanded] = useState(true)
  const [isHealthExpanded, setIsHealthExpanded] = useState(true)
  const [isCertificatesExpanded, setIsCertificatesExpanded] = useState(true)

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
      
      // Load certificates
      setLoadingCertificates(true)
      try {
        const certs = await maritimeService.certificates.getCrewCertificatesByCrewId(id)
        setCertificates(certs)
      } catch (certError) {
        console.error('❌ Failed to load certificates:', certError)
        setCertificates([])
      } finally {
        setLoadingCertificates(false)
      }
    } catch (error: any) {
      console.error('❌ Failed to load crew details:', error)
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
      alert('✅ Crew member updated successfully!')
    } catch (error: any) {
      console.error('❌ Failed to save crew:', error)
      alert(`Error: ${error.message || 'Failed to update crew member'}`)
    } finally {
      setSaving(false)
    }
  }

  const splitFullName = (fullName: string) => {
    const parts = fullName.trim().split(' ')
    if (parts.length === 1) return { lastName: parts[0], firstName: '', middleName: '' }
    if (parts.length === 2) return { lastName: parts[0], firstName: parts[1], middleName: '' }
    return { lastName: parts[0], firstName: parts[1], middleName: parts.slice(2).join(' ') }
  }

  const calculateAge = (dateOfBirth: string | undefined) => {
    if (!dateOfBirth) return ''
    const today = new Date()
    const birthDate = new Date(dateOfBirth)
    let age = today.getFullYear() - birthDate.getFullYear()
    const monthDiff = today.getMonth() - birthDate.getMonth()
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--
    }
    return age.toString()
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
          <p className="font-semibold">Error loading crew details</p>
          <p className="text-sm">Crew member not found</p>
        </div>
      </div>
    )
  }

  const nameParts = splitFullName(crew.fullName || '')
  const age = calculateAge(editedCrew.dateOfBirth)

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-6 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <button
              onClick={() => navigate('/crew')}
              className="p-2 hover:bg-gray-100 rounded"
            >
              <ArrowLeft className="h-5 w-5" />
            </button>
            <h1 className="text-lg font-semibold text-gray-800">
              EDIT {crew.fullName.toUpperCase()} - {crew.position?.toUpperCase() || 'CREW'}
            </h1>
          </div>
          <div className="flex items-center gap-2">
            <button className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 flex items-center gap-2">
              <span>📄</span> Group documents
            </button>
            <button
              onClick={handleSave}
              disabled={saving}
              className="px-6 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 font-medium disabled:opacity-50"
            >
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white border-b border-gray-300">
        <div className="px-6">
          <div className="flex gap-1">
            <button
              onClick={() => setActiveTab('basic-data')}
              className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'basic-data'
                  ? 'border-blue-600 text-blue-600 bg-blue-50'
                  : 'border-transparent text-gray-600 hover:text-gray-800'
              }`}
            >
              Basic Data
            </button>
            <button
              onClick={() => setActiveTab('documents')}
              className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'documents'
                  ? 'border-blue-600 text-blue-600 bg-blue-50'
                  : 'border-transparent text-gray-600 hover:text-gray-800'
              }`}
            >
              Documents
            </button>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="p-3">
        {activeTab === 'basic-data' && (
          <div className="space-y-3">
            {/* Main Form */}
            <div className="bg-white rounded-lg shadow-sm p-6">
              <div className="grid grid-cols-12 gap-6">
                {/* Left Column - Name & Position */}
                <div className="col-span-3 space-y-2">
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Last Name
                    </label>
                    <input
                      type="text"
                      value={nameParts.lastName}
                      onChange={(e) => {
                        const newFullName = `${e.target.value} ${nameParts.firstName} ${nameParts.middleName}`.trim()
                        setEditedCrew({ ...editedCrew, fullName: newFullName })
                      }}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Position
                    </label>
                    <input
                      type="text"
                      value={editedCrew.position || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, position: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Department
                    </label>
                    <input
                      type="text"
                      value={editedCrew.department || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, department: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Date of Birth
                    </label>
                    <input
                      type="date"
                      value={editedCrew.dateOfBirth?.split('T')[0] || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, dateOfBirth: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Age
                    </label>
                    <input
                      type="text"
                      value={age}
                      readOnly
                      className="w-full px-3 py-2 border border-gray-300 rounded bg-gray-50"
                    />
                  </div>
                </div>

                {/* Middle-Left Column - Personal Info */}
                <div className="col-span-3 space-y-2">
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      First Name
                    </label>
                    <input
                      type="text"
                      value={nameParts.firstName}
                      onChange={(e) => {
                        const newFullName = `${nameParts.lastName} ${e.target.value} ${nameParts.middleName}`.trim()
                        setEditedCrew({ ...editedCrew, fullName: newFullName })
                      }}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Rank
                    </label>
                    <input
                      type="text"
                      value={editedCrew.rank || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, rank: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Nationality
                    </label>
                    <input
                      type="text"
                      value={editedCrew.nationality || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, nationality: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Phone Number
                    </label>
                    <input
                      type="text"
                      value={editedCrew.phoneNumber || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, phoneNumber: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Email
                    </label>
                    <input
                      type="email"
                      value={editedCrew.emailAddress || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, emailAddress: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                </div>

                {/* Middle-Right Column - Dates */}
                <div className="col-span-3 space-y-2">
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Middle Name
                    </label>
                    <input
                      type="text"
                      value={nameParts.middleName}
                      onChange={(e) => {
                        const newFullName = `${nameParts.lastName} ${nameParts.firstName} ${e.target.value}`.trim()
                        setEditedCrew({ ...editedCrew, fullName: newFullName })
                      }}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Join Date
                    </label>
                    <input
                      type="date"
                      value={editedCrew.joinDate?.split('T')[0] || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, joinDate: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Embark Date
                    </label>
                    <input
                      type="date"
                      value={editedCrew.embarkDate?.split('T')[0] || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, embarkDate: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Disembark Date
                    </label>
                    <input
                      type="date"
                      value={editedCrew.disembarkDate?.split('T')[0] || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, disembarkDate: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Contract End
                    </label>
                    <input
                      type="date"
                      value={editedCrew.contractEnd?.split('T')[0] || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, contractEnd: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                </div>

                {/* Right Column - Avatar */}
                <div className="col-span-3 flex flex-col items-center">
                  <div className="mb-2">
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1 text-center">
                      Company ID Number
                    </label>
                    <input
                      type="text"
                      value={editedCrew.crewId || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, crewId: e.target.value })}
                      className="w-32 px-3 py-2 border border-gray-300 rounded text-center focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div className="w-40 h-40 rounded-full overflow-hidden bg-gray-200 mb-3">
                    <img
                      src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'%3E%3Ccircle cx='100' cy='100' r='100' fill='%23e5e7eb'/%3E%3Ccircle cx='100' cy='80' r='35' fill='%239ca3af'/%3E%3Cellipse cx='100' cy='160' rx='60' ry='45' fill='%239ca3af'/%3E%3C/svg%3E"
                      alt="Avatar"
                      className="w-full h-full object-cover"
                    />
                  </div>
                  <div className="flex gap-2">
                    <button className="px-4 py-2 bg-blue-600 text-white text-sm rounded hover:bg-blue-700 flex items-center gap-1">
                      + Choose
                    </button>
                    <button className="px-4 py-2 bg-green-600 text-white text-sm rounded hover:bg-green-700 flex items-center gap-1">
                      <Upload className="w-4 h-4" /> Upload
                    </button>
                    <button className="px-4 py-2 bg-red-600 text-white text-sm rounded hover:bg-red-700">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                  <p className="text-xs text-gray-500 mt-2 text-center">
                    Select with the button or drag the photo here.
                  </p>
                  <div className="mt-4 flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={editedCrew.isOnboard}
                      onChange={(e) => setEditedCrew({ ...editedCrew, isOnboard: e.target.checked })}
                      className="w-4 h-4 text-blue-600"
                    />
                    <label className="text-sm font-medium text-gray-700">On Board</label>
                  </div>
                </div>
              </div>
            </div>

            {/* Travel Documents */}
            <div className="bg-white rounded-lg shadow-sm p-3">
              <h3 className="text-sm font-bold text-gray-700 uppercase mb-4">Travel Documents</h3>
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Passport Number
                  </label>
                  <input
                    type="text"
                    value={editedCrew.passportNumber || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, passportNumber: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Passport Expiry
                  </label>
                  <input
                    type="date"
                    value={editedCrew.passportExpiry?.split('T')[0] || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, passportExpiry: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Seaman Book Number
                  </label>
                  <input
                    type="text"
                    value={editedCrew.seamanBookNumber || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, seamanBookNumber: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
              </div>
              <div className="grid grid-cols-3 gap-4 mt-4">
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Visa Number
                  </label>
                  <input
                    type="text"
                    value={editedCrew.visaNumber || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, visaNumber: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Visa Expiry
                  </label>
                  <input
                    type="date"
                    value={editedCrew.visaExpiry?.split('T')[0] || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, visaExpiry: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
              </div>
            </div>

            {/* Contact Information */}
            <div className="bg-white rounded-lg shadow-sm p-3">
              <h3 className="text-sm font-bold text-gray-700 uppercase mb-4">Contact Information</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Address
                  </label>
                  <textarea
                    value={editedCrew.address || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, address: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Emergency Contact
                  </label>
                  <textarea
                    value={editedCrew.emergencyContact || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, emergencyContact: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
              </div>
              <div className="mt-4">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  Notes
                </label>
                <textarea
                  value={editedCrew.notes || ''}
                  onChange={(e) => setEditedCrew({ ...editedCrew, notes: e.target.value })}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                />
              </div>
            </div>
          </div>
        )}

        {activeTab === 'documents' && (
          <div className="space-y-2">
            {/* IDENTITY DOCUMENTS Section */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-3 flex items-center justify-between cursor-pointer" onClick={() => setIsIdentityExpanded(!isIdentityExpanded)}>
                <h3 className="text-sm font-semibold text-gray-700 uppercase">IDENTITY DOCUMENTS ({[
                  editedCrew.passportNumber,
                  editedCrew.visaNumber,
                  editedCrew.seamanBookNumber,
                  editedCrew.joinDate
                ].filter(Boolean).length})</h3>
                <button className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all">
                  <span className="text-white text-xs transition-transform" style={{ transform: isIdentityExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
                    ▼
                  </span>
                </button>
              </div>
              {isIdentityExpanded && (
                <div className="overflow-x-auto">
                  <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
                    <thead className="bg-white border-b-2 border-gray-300">
                      <tr>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '3%'}}></th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '20%'}}>Name / Type</th>
                        <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>Files</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Number</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Date of Issue</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Place</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Country</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '12%'}}>Exp. Date</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white">
                      {/* Passport */}
                      {editedCrew.passportNumber && (
                        <tr className="border-b border-gray-100 hover:bg-gray-50">
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                            <button className="text-gray-400 hover:text-gray-600">::</button>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                            <div className="flex items-center gap-2">
                              <span>🔒</span>
                              <span className="font-medium">Passport</span>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                            <button className="text-blue-600 hover:text-blue-800">
                              <Download className="w-4 h-4 inline" />
                            </button>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.passportNumber}</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.nationality || '-'}</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.passportExpiry ? format(new Date(editedCrew.passportExpiry), 'dd/MM/yyyy') : '-'}</div>
                          </td>
                        </tr>
                      )}
                      
                      {/* US Visa */}
                      {editedCrew.visaNumber && (
                        <tr className="border-b border-gray-100 hover:bg-gray-50">
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                            <button className="text-gray-400 hover:text-gray-600">::</button>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                            <div className="flex items-center gap-2">
                              <span>🔒</span>
                              <div>
                                <div className="font-medium">US Visa</div>
                                <div className="text-xs text-gray-500">Type: B-1/B-2</div>
                              </div>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                            <button className="text-blue-600 hover:text-blue-800">
                              <Download className="w-4 h-4 inline" />
                            </button>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.visaNumber}</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.visaExpiry ? format(new Date(editedCrew.visaExpiry), 'dd/MM/yyyy') : '-'}</div>
                          </td>
                        </tr>
                      )}
                      
                      {/* Seaman's Book - National */}
                      {editedCrew.seamanBookNumber && (
                        <tr className="border-b border-gray-100 hover:bg-gray-50">
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                            <button className="text-gray-400 hover:text-gray-600">::</button>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                            <div className="flex items-center gap-2">
                              <span>🔒</span>
                              <span className="font-medium">(SB) Seaman's Book - National</span>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                            <button className="text-blue-600 hover:text-blue-800">
                              <Download className="w-4 h-4 inline" />
                            </button>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.seamanBookNumber}</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.nationality || '-'}</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                        </tr>
                      )}
                      
                      {/* Contract of Employment */}
                      {editedCrew.joinDate && (
                        <tr className="border-b border-gray-100 hover:bg-gray-50">
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                            <button className="text-gray-400 hover:text-gray-600">::</button>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                            <div className="flex items-center gap-2">
                              <span>🔒</span>
                              <span className="font-medium">Contract of Employment (COE)</span>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">{format(new Date(editedCrew.joinDate), 'dd/MM/yyyy')}</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                            <div className="truncate">-</div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                            <div className="truncate">{editedCrew.contractEnd ? format(new Date(editedCrew.contractEnd), 'dd/MM/yyyy') : '-'}</div>
                          </td>
                        </tr>
                      )}
                      
                      {[editedCrew.passportNumber, editedCrew.visaNumber, editedCrew.seamanBookNumber, editedCrew.joinDate].filter(Boolean).length === 0 && (
                        <tr className="border-b border-gray-100">
                          <td colSpan={8} className="px-4 py-8 text-center text-gray-500">
                            No identity documents available
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* HEALTH DOCUMENTS Section */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-3 flex items-center justify-between cursor-pointer" onClick={() => setIsHealthExpanded(!isHealthExpanded)}>
                <h3 className="text-sm font-semibold text-gray-700 uppercase">HEALTH DOCUMENTS (0)</h3>
                <button className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all">
                  <span className="text-white text-xs transition-transform" style={{ transform: isHealthExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
                    ▼
                  </span>
                </button>
              </div>
              {isHealthExpanded && (
                <div className="overflow-x-auto">
                  <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
                    <thead className="bg-white border-b-2 border-gray-300">
                      <tr>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '3%'}}></th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '25%'}}>Name</th>
                        <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>Files</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Number</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Date of Issue</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Place</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Country</th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '12%'}}>Exp. Date</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white">
                      <tr className="border-b border-gray-100">
                        <td colSpan={8} className="px-4 py-8 text-center text-gray-500">
                          No health documents available
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* CERTIFICATES Section */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-3 flex items-center justify-between cursor-pointer" onClick={() => setIsCertificatesExpanded(!isCertificatesExpanded)}>
                <h3 className="text-sm font-semibold text-gray-700 uppercase">CERTIFICATES ({certificates.length})</h3>
                <div className="flex items-center gap-2">
                  <button 
                    onClick={(e) => {
                      e.stopPropagation()
                      navigate(`/crew/certificates/add?crewId=${id}`)
                    }}
                    className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold transition-colors"
                    title="Add certificate"
                  >
                    +
                  </button>
                  <button className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all">
                    <span className="text-white text-xs transition-transform" style={{ transform: isCertificatesExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
                      ▼
                    </span>
                  </button>
                </div>
              </div>
              {isCertificatesExpanded && (
                loadingCertificates ? (
                  <div className="flex items-center justify-center py-12">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                  </div>
                ) : certificates && certificates.length > 0 ? (
                  <div className="overflow-x-auto">
                    <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
                      <thead className="bg-white border-b-2 border-gray-300">
                        <tr>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '15%'}}>Certificate Name</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>CoC</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>Country</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Cert. Number</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>Issue Date</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>Expiry Date</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '15%'}}>Issuing Authority</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '10%'}}>Status</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white">
                        {certificates.map((cert) => {
                          const getCertStatus = (expiryDate: string) => {
                            if (!expiryDate) return { icon: AlertTriangle, status: 'N/A', color: 'text-gray-500', bgColor: 'bg-gray-100' }
                            const daysLeft = differenceInDays(parseISO(expiryDate), new Date())
                            if (daysLeft < 0) {
                              return { icon: XCircle, status: 'Expired', color: 'text-red-600', bgColor: 'bg-red-100' }
                            } else if (daysLeft < 90) {
                              return { icon: AlertTriangle, status: 'Expiring', color: 'text-yellow-600', bgColor: 'bg-yellow-100' }
                            } else {
                              return { icon: CheckCircle, status: 'Valid', color: 'text-green-600', bgColor: 'bg-green-100' }
                            }
                          }
                          
                          const status = getCertStatus(cert.expiryDate)
                          const StatusIcon = status.icon
                          const daysLeft = cert.expiryDate ? differenceInDays(parseISO(cert.expiryDate), new Date()) : null
                          
                          return (
                            <tr key={cert.id} className="border-b border-gray-100 hover:bg-gray-50">
                              <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '15%'}}>
                                <div className="font-medium text-gray-900 truncate">
                                  {cert.certificate?.certificateName || cert.certificateName || 'Unknown Certificate'}
                                </div>
                                <div className="text-xs text-gray-500 truncate">
                                  {cert.certificate?.certificateCode || cert.certificateCode || ''}
                                </div>
                              </td>
                              <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '8%'}}>
                                {cert.certificateOfCompetency ? (
                                  <span className={`px-2 py-0.5 text-xs font-medium rounded ${
                                    cert.certificateOfCompetency === 'National' 
                                      ? 'bg-blue-100 text-blue-800' 
                                      : 'bg-purple-100 text-purple-800'
                                  }`}>
                                    {cert.certificateOfCompetency}
                                  </span>
                                ) : '-'}
                              </td>
                              <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                                <div className="truncate">{cert.country?.countryName || cert.countryName || '-'}</div>
                              </td>
                              <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '12%'}}>
                                <code className="text-xs font-mono text-gray-900 truncate block">
                                  {cert.certificateNumber || '-'}
                                </code>
                              </td>
                              <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                                <div className="truncate">
                                  {cert.issueDate ? format(parseISO(cert.issueDate), 'dd MMM yyyy') : '-'}
                                </div>
                              </td>
                              <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '10%'}}>
                                <div className="text-gray-900 font-medium truncate">
                                  {cert.expiryDate ? format(parseISO(cert.expiryDate), 'dd MMM yyyy') : '-'}
                                </div>
                                {daysLeft !== null && (
                                  <div className={`text-xs ${status.color} truncate`}>
                                    {daysLeft} days left
                                  </div>
                                )}
                              </td>
                              <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '15%'}}>
                                <div className="truncate">
                                  {cert.issuingAuthority || '-'}
                                </div>
                              </td>
                              <td className="px-4 py-3 text-sm" style={{width: '10%'}}>
                                <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-semibold ${status.bgColor} ${status.color}`}>
                                  <StatusIcon className="w-3 h-3" />
                                  {status.status}
                                </span>
                              </td>
                            </tr>
                          )
                        })}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <div className="text-center py-12">
                    <p className="text-gray-500 mb-4">No certificates found for this crew member</p>
                  </div>
                )
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
     