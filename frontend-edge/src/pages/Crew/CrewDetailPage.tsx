import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { 
  ArrowLeft, 
  Trash2,
  Upload
} from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'

type TabType = 'basic-data' | 'certificates'

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
              onClick={() => setActiveTab('certificates')}
              className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'certificates'
                  ? 'border-blue-600 text-blue-600 bg-blue-50'
                  : 'border-transparent text-gray-600 hover:text-gray-800'
              }`}
            >
              Certificates
            </button>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="p-6">
        {activeTab === 'basic-data' && (
          <div className="space-y-6">
            {/* Main Form */}
            <div className="bg-white rounded-lg shadow-sm p-6">
              <div className="grid grid-cols-12 gap-6">
                {/* Left Column - Name & Position */}
                <div className="col-span-3 space-y-4">
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
                <div className="col-span-3 space-y-4">
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
                <div className="col-span-3 space-y-4">
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
            <div className="bg-white rounded-lg shadow-sm p-6">
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
            <div className="bg-white rounded-lg shadow-sm p-6">
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

        {activeTab === 'certificates' && (
          <div className="bg-white rounded-lg shadow-sm p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-800">Certificates</h3>
              <button className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
                + Add Certificate
              </button>
            </div>
            
            {loadingCertificates ? (
              <div className="flex items-center justify-center py-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              </div>
            ) : certificates && certificates.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200 bg-gray-50">
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Certificate</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Number</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Issue Date</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Expiry Date</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Issuing Authority</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {certificates.map((cert) => {
                      const isExpired = cert.expiryDate && new Date(cert.expiryDate) < new Date()
                      const isExpiringSoon = cert.expiryDate && !isExpired && 
                        (new Date(cert.expiryDate).getTime() - new Date().getTime()) < (30 * 24 * 60 * 60 * 1000)
                      
                      return (
                        <tr key={cert.id} className="border-b border-gray-200 hover:bg-gray-50">
                          <td className="px-4 py-3">
                            <div>
                              <div className="font-medium text-gray-900">
                                {cert.certificate?.certificateName || 'Unknown Certificate'}
                              </div>
                              <div className="text-xs text-gray-500">
                                {cert.certificate?.certificateCode}
                              </div>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700">{cert.certificateNumber}</td>
                          <td className="px-4 py-3 text-sm text-gray-700">
                            {cert.issueDate ? new Date(cert.issueDate).toLocaleDateString('en-GB') : '-'}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700">
                            {cert.expiryDate ? new Date(cert.expiryDate).toLocaleDateString('en-GB') : '-'}
                          </td>
                          <td className="px-4 py-3">
                            <span className={`px-2 py-1 text-xs font-medium rounded ${
                              cert.status === 'VALID' 
                                ? 'bg-green-100 text-green-800'
                                : cert.status === 'EXPIRED'
                                ? 'bg-red-100 text-red-800'
                                : 'bg-gray-100 text-gray-800'
                            }`}>
                              {cert.status}
                            </span>
                            {isExpiringSoon && (
                              <span className="ml-2 px-2 py-1 text-xs font-medium rounded bg-yellow-100 text-yellow-800">
                                Expiring Soon
                              </span>
                            )}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700">{cert.issuingAuthority || '-'}</td>
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-2">
                              <button className="text-blue-600 hover:text-blue-800 text-sm">
                                Edit
                              </button>
                              <button className="text-red-600 hover:text-red-800 text-sm">
                                Delete
                              </button>
                            </div>
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
                <button className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
                  + Add First Certificate
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
     