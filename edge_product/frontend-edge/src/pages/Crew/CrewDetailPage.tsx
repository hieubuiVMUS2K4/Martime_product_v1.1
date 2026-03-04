import { useEffect, useState } from 'react'
import { toast } from 'react-toastify'
import { useParams, useNavigate } from 'react-router-dom'
import { 
  ArrowLeft, 
  Trash2,
  Upload,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Eye,
  Ship,
  MapPin,
  Calendar,
  FileDown
} from 'lucide-react'
import { CrewMember, ServiceRecord } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { voyageMgmtService } from '../../services/voyage.service'
import type { VoyageCrewAssignment } from '../../types/voyage.types'
import { format, differenceInDays, parseISO } from 'date-fns'
import AddDocumentModal from '../../components/crew/AddDocumentModal'
import AddHealthDocumentModal from '../../components/crew/AddHealthDocumentModal'
import ImageViewerModal from '../../components/crew/ImageViewerModal'
import { AddCrewCertificateModal } from './AddCrewCertificateModal'
import jsPDF from 'jspdf'
import 'jspdf-autotable'

type TabType = 'basic-data' | 'documents' | 'voyage-history'

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
  const [travelDocuments, setTravelDocuments] = useState<any[]>([])
  const [seafarerDocuments, setSeafarerDocuments] = useState<any[]>([])
  const [employmentDocuments, setEmploymentDocuments] = useState<any[]>([])
  const [healthDocuments, setHealthDocuments] = useState<any[]>([])
  const [loadingDocuments, setLoadingDocuments] = useState(false)
  const [isIdentityExpanded, setIsIdentityExpanded] = useState(true)
  const [isHealthExpanded, setIsHealthExpanded] = useState(true)
  const [isCertificatesExpanded, setIsCertificatesExpanded] = useState(true)
  const [isAddDocumentModalOpen, setIsAddDocumentModalOpen] = useState(false)
  const [isAddHealthDocumentModalOpen, setIsAddHealthDocumentModalOpen] = useState(false)
  const [isImageViewerOpen, setIsImageViewerOpen] = useState(false)
  const [imageViewerUrl, setImageViewerUrl] = useState<string | null>(null)
  const [imageViewerDocId, setImageViewerDocId] = useState<string | null>(null)
  const [imageViewerTargetTable, setImageViewerTargetTable] = useState<string | null>(null)
  const [uploadingDocId, setUploadingDocId] = useState<string | null>(null)
  const [uploadingCertId, setUploadingCertId] = useState<number | null>(null)
  const [uploadingAvatar, setUploadingAvatar] = useState(false)
  const [pendingAvatarFile, setPendingAvatarFile] = useState<File | null>(null)
  const [pendingAvatarPreview, setPendingAvatarPreview] = useState<string | null>(null)
  const [ranks, setRanks] = useState<any[]>([])
  const [voyageHistory, setVoyageHistory] = useState<VoyageCrewAssignment[]>([])
  const [loadingVoyageHistory, setLoadingVoyageHistory] = useState(false)
  const [serviceRecords, setServiceRecords] = useState<ServiceRecord[]>([])
  const [loadingServiceRecords, setLoadingServiceRecords] = useState(false)
  const [showAddCertModal, setShowAddCertModal] = useState(false)

  useEffect(() => {
    loadCrewDetails()
    loadRanks()
  }, [id])
  
  const loadRanks = async () => {
    try {
      const data = await maritimeService.ranks.getAll()
      setRanks(data)
    } catch (error) {
      console.error('Failed to load ranks:', error)
    }
  }

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
      
      await loadDocuments(id)
    } catch (error: any) {
      console.error('❌ Failed to load crew details:', error)
      setCrew(null)
    } finally {
      setLoading(false)
    }
  }

  const loadDocuments = async (crewMemberId: string) => {
    setLoadingDocuments(true)
    try {
      const [travel, seafarer, employment, health] = await Promise.all([
        maritimeService.crew.getTravelDocuments(crewMemberId).catch(() => []),
        maritimeService.crew.getSeafarerDocuments(crewMemberId).catch(() => []),
        maritimeService.crew.getEmploymentDocuments(crewMemberId).catch(() => []),
        maritimeService.crew.getHealthDocuments(crewMemberId).catch(() => [])
      ])
      setTravelDocuments(travel)
      setSeafarerDocuments(seafarer)
      setEmploymentDocuments(employment)
      setHealthDocuments(health)
    } catch (docError) {
      console.error('❌ Failed to load documents:', docError)
    } finally {
      setLoadingDocuments(false)
    }
  }

  const handleViewImage = (fileUrl: string, documentId: string, targetTable: string) => {
    setImageViewerUrl(fileUrl)
    setImageViewerDocId(documentId)
    setImageViewerTargetTable(targetTable)
    setIsImageViewerOpen(true)
  }

  // State to track if the image viewer is showing a certificate (for custom upload handler)
  const [viewingCertificateId, setViewingCertificateId] = useState<number | null>(null)

  const handleViewCertificateImage = (fileUrl: string, certId: number) => {
    setImageViewerUrl(fileUrl)
    setImageViewerDocId(String(certId))
    setViewingCertificateId(certId)
    setImageViewerTargetTable(null)
    setIsImageViewerOpen(true)
  }

  const handleCertificateUploadHandler = async (documentId: string, formData: FormData) => {
    const certId = parseInt(documentId)
    const result = await maritimeService.certificates.uploadCertificateFile(certId, formData)
    return result
  }

  const handleCertificateFileUpload = async (certId: number) => {
    const input = document.createElement('input')
    input.type = 'file'
    input.accept = '.jpg,.jpeg,.png,.gif,.pdf'
    
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0]
      if (!file) return

      try {
        setUploadingCertId(certId)
        const formData = new FormData()
        formData.append('file', file)

        await maritimeService.certificates.uploadCertificateFile(certId, formData)
        
        // Reload certificates
        if (id) {
          const certs = await maritimeService.certificates.getCrewCertificatesByCrewId(id)
          setCertificates(certs)
        }
        
        toast.success('File uploaded successfully!')
      } catch (error: any) {
        console.error('❌ Failed to upload certificate file:', error)
        toast.error(error.message || 'Failed to upload file')
      } finally {
        setUploadingCertId(null)
      }
    }

    input.click()
  }

  const handleDocumentFileUpload = async (documentId: string, targetTable: string) => {
    // Create hidden file input
    const input = document.createElement('input')
    input.type = 'file'
    input.accept = 'image/*'
    
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0]
      if (!file) return

      try {
        setUploadingDocId(documentId)
        const formData = new FormData()
        formData.append('targetTable', targetTable)
        formData.append('file', file)

        await maritimeService.crew.updateDocumentFile(documentId, formData)
        
        // Reload documents
        if (id) {
          await loadDocuments(id)
        }
        
        toast.success('File uploaded successfully!')
      } catch (error: any) {
        console.error('❌ Failed to upload file:', error)
        toast.error(error.message || 'Failed to upload file')
      } finally {
        setUploadingDocId(null)
      }
    }

    input.click()
  }

  const handleAvatarUpload = async () => {
    if (!id) return

    const input = document.createElement('input')
    input.type = 'file'
    input.accept = 'image/*'
    
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0]
      if (!file) return

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        toast.warning('File size must not exceed 5MB')
        return
      }

      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif']
      if (!allowedTypes.includes(file.type)) {
        toast.warning('Only image files (JPG, PNG, GIF) are allowed')
        return
      }

      // Store file for later upload, generate preview
      setPendingAvatarFile(file)
      const reader = new FileReader()
      reader.onloadend = () => {
        setPendingAvatarPreview(reader.result as string)
      }
      reader.readAsDataURL(file)
    }

    input.click()
  }

  const handleAvatarSave = async () => {
    if (!id || !pendingAvatarFile) return

    try {
      setUploadingAvatar(true)
      const formData = new FormData()
      formData.append('file', pendingAvatarFile)

      const response = await maritimeService.crew.uploadAvatar(id, formData)
      
      // Update crew member with new photo URL
      if (response.crewMember) {
        setCrew(response.crewMember)
        setEditedCrew(response.crewMember)
      }
      
      // Clear pending state
      setPendingAvatarFile(null)
      setPendingAvatarPreview(null)
      
      toast.success('Avatar uploaded successfully!')
    } catch (error: any) {
      console.error('❌ Failed to upload avatar:', error)
      toast.error(error.message || 'Failed to upload avatar')
    } finally {
      setUploadingAvatar(false)
    }
  }

  const handleCancelAvatarChange = () => {
    setPendingAvatarFile(null)
    setPendingAvatarPreview(null)
  }

  const handleDeleteAvatar = async () => {
    if (!id || !crew?.photoUrl) return

    if (!confirm('Are you sure you want to delete the avatar?')) {
      return
    }

    try {
      setUploadingAvatar(true)
      
      // Update crew member with null photo URL
      const updated = await maritimeService.crew.update(id, { ...editedCrew, photoUrl: undefined })
      setCrew(updated)
      setEditedCrew(updated)
      
      toast.success('Avatar deleted successfully!')
    } catch (error: any) {
      console.error('❌ Failed to delete avatar:', error)
      toast.error(error.message || 'Failed to delete avatar')
    } finally {
      setUploadingAvatar(false)
    }
  }

  const exportToPDF = async () => {
    if (!crew) return
    
    try {
      const doc = new jsPDF('landscape', 'mm', 'a4') as any
      const pageWidth = doc.internal.pageSize.getWidth()
      const pageHeight = doc.internal.pageSize.getHeight()
      const ml = 8 // margin left
      const mr = 8 // margin right
      const tw = pageWidth - ml - mr // table width
      
      // Common table styles matching Excel template
      const headerStyle = { fillColor: [173, 216, 230] as [number, number, number], textColor: 0 as number, fontStyle: 'bold' as const, fontSize: 6, cellPadding: 1.2 }
      const bodyStyle = { fontSize: 6, cellPadding: 1.2, lineWidth: 0.1, lineColor: [0, 0, 0] as [number, number, number] }
      const certColumns = ['No.', 'Name', 'Issued by', 'Number', 'Date of issue', 'Date of expiry', 'Remark']
      const certColWidths = {
        0: { cellWidth: tw * 0.03 },  // No.
        1: { cellWidth: tw * 0.27 }, // Name
        2: { cellWidth: tw * 0.14 }, // Issued by
        3: { cellWidth: tw * 0.16 }, // Number
        4: { cellWidth: tw * 0.13 }, // Date of issue
        5: { cellWidth: tw * 0.13 }, // Date of expiry
        6: { cellWidth: tw * 0.14 }  // Remark
      }

      // ========== ROW 1-5: HEADER ==========
      // Logo placeholder (A1:D5)
      doc.setDrawColor(180, 180, 180)
      doc.setLineWidth(0.3)
      doc.rect(ml, 6, 30, 16, 'S')
      doc.setFontSize(6)
      doc.setTextColor(150, 150, 150)
      doc.setFont('helvetica', 'italic')
      doc.text('LOGO', ml + 15, 15, { align: 'center' })
      
      // Title: BIO - DATA (F2:AQ2)
      doc.setFontSize(16)
      doc.setTextColor(0, 0, 0)
      doc.setFont('helvetica', 'bold')
      doc.text('BIO - DATA', pageWidth / 2, 14, { align: 'center' })
      
      // Row 4: Crew code | Present Rank | Prepared by | Date Prepared
      const row4Y = 26
      doc.setFontSize(7)
      doc.setFont('helvetica', 'normal')
      doc.text('Crew code', ml, row4Y)
      doc.setFont('helvetica', 'bold')
      doc.text(crew.crewId || '', ml + 25, row4Y)
      
      doc.setFont('helvetica', 'normal')
      doc.text('Present Rank', ml + 60, row4Y)
      doc.setFont('helvetica', 'bold')
      doc.text(crew.rank?.rankName || '', ml + 85, row4Y)
      
      doc.setFont('helvetica', 'normal')
      doc.text('Prepared by', ml + 140, row4Y)
      doc.setFont('helvetica', 'bold')
      doc.text('', ml + 165, row4Y)
      
      doc.setFont('helvetica', 'normal')
      doc.text('Date Prepared', ml + 200, row4Y)
      doc.setFont('helvetica', 'bold')
      doc.text(format(new Date(), 'dd/MM/yyyy'), ml + 230, row4Y)
      
      let yPos = 31

      // ========== 1. Personal Particular ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFontSize(8)
      doc.setFont('helvetica', 'bold')
      doc.setTextColor(0, 0, 0)
      doc.text('1. Personal Particular', ml + 2, yPos + 3.5)
      yPos += 6
      
      // Photo on the left side (matching Excel A8:G14)
      const photoX = ml
      const photoY = yPos
      const photoW = 28
      const photoH = 38
      
      if (crew.photoUrl && crew.photoUrl.trim() !== '') {
        try {
          let imageUrl = crew.photoUrl
          if (imageUrl.startsWith('data:')) {
            doc.addImage(imageUrl, 'JPEG', photoX + 1, photoY + 1, photoW - 2, photoH - 2)
          } else {
            if (!imageUrl.startsWith('http')) {
              imageUrl = `http://localhost:5001${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`
            }
            doc.addImage(imageUrl, 'JPEG', photoX + 1, photoY + 1, photoW - 2, photoH - 2)
          }
        } catch (imgError) {
          console.warn('PDF: Could not add image:', imgError)
          doc.setDrawColor(180, 180, 180)
          doc.rect(photoX, photoY, photoW, photoH, 'S')
          doc.setFontSize(6)
          doc.setTextColor(150, 150, 150)
          doc.text('PHOTO', photoX + photoW / 2, photoY + photoH / 2, { align: 'center' })
        }
      } else {
        doc.setDrawColor(180, 180, 180)
        doc.rect(photoX, photoY, photoW, photoH, 'S')
        doc.setFontSize(6)
        doc.setTextColor(150, 150, 150)
        doc.text('PHOTO', photoX + photoW / 2, photoY + photoH / 2, { align: 'center' })
      }
      doc.setTextColor(0, 0, 0)
      
      // Personal data table (to the right of photo, matching Excel H8:AR14)
      const pdLeft = ml + photoW + 2
      const pdWidth = tw - photoW - 2
      
      // Cell style helpers: Label (bold + gray bg) vs Value (normal)
      const Lb = (text: string) => ({ content: text, styles: { fontStyle: 'bold' as const, fillColor: [245, 245, 245] as [number, number, number] } })
      const Va = (text: string) => ({ content: text, styles: {} as any })

      // Build autoTable data for personal info — cell-level styling to match Excel exactly
      doc.autoTable({
        startY: yPos,
        head: [['', 'Full name', '', 'Date of Birth', '', 'Place of Birth', '', 'Nationality', '']],
        body: [
          [Lb('Name'), Va(crew.fullName || ''), Va(''), Va(crew.dateOfBirth ? format(new Date(crew.dateOfBirth), 'dd/MM/yyyy') : ''), Va(''), Va(crew.placeOfBirth || ''), Va(''), Va(crew.nationality || ''), Va('')],
          [Lb('ID No.'), Va(crew.idCardNumber || ''), Va(''), Lb('Address'), Va(crew.address || ''), Va(''), Va(''), Va(''), Va('')],
          [Lb('Home Tel'), Va(''), Lb('Hand phone'), Va(crew.phoneNumber || ''), Lb('Email'), Va(crew.emailAddress || ''), Va(''), Lb('Marital status'), Va(crew.maritalStatus || '')],
          [Lb('Height'), Va(crew.height ? `${crew.height} cm` : ''), Lb('Weight'), Va(crew.weight ? `${crew.weight} kg` : ''), Lb('Overall size'), Va(crew.clothingSize || ''), Lb("Shoe's size"), Va(crew.shoeSize || ''), Va('')],
          [Lb('Catering size'), Va(crew.cateringSize || ''), Lb('Blood Group'), Va(crew.bloodGroup || ''), Lb('Covid-19 Vaccinated'), Va(crew.isCovidVaccinated ? 'Yes' : 'No'), Lb('Smoker'), Va(crew.isSmoker ? 'Yes' : 'No'), Va('')],
          [Lb('Contact person/\nNext of Kin'), Lb('Name'), Va(crew.nextOfKinName || ''), Lb('Phone No.'), Va(crew.nextOfKinPhone || ''), Lb('Covid-19 Vaccinated'), Va(crew.isCovidVaccinated ? 'Yes' : 'No'), Lb('Smoker'), Va(crew.isSmoker ? 'Yes' : 'No')],
          [Va(''), Lb('Relation'), Va(crew.nextOfKinRelation || ''), Lb('Address'), Va(crew.nextOfKinAddress || ''), Va(''), Va(''), Va(''), Va('')]
        ],
        theme: 'grid',
        styles: { fontSize: 5.5, cellPadding: 1, lineWidth: 0.1, lineColor: [0, 0, 0], overflow: 'linebreak', valign: 'middle' },
        headStyles: { fillColor: [230, 240, 250], textColor: 0, fontStyle: 'bold', fontSize: 5.5 },
        margin: { left: pdLeft, right: mr },
        columnStyles: {
          0: { cellWidth: pdWidth * 0.08 },
          1: { cellWidth: pdWidth * 0.15 },
          2: { cellWidth: pdWidth * 0.09 },
          3: { cellWidth: pdWidth * 0.13 },
          4: { cellWidth: pdWidth * 0.10 },
          5: { cellWidth: pdWidth * 0.14 },
          6: { cellWidth: pdWidth * 0.08 },
          7: { cellWidth: pdWidth * 0.08 },
          8: { cellWidth: pdWidth * 0.15 }
        }
      })
      
      yPos = Math.max(doc.lastAutoTable.finalY, photoY + photoH) + 4
      
      // ========== 2. Education ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFontSize(8)
      doc.setFont('helvetica', 'bold')
      doc.text('2. Education', ml + 2, yPos + 3.5)
      yPos += 6
      
      doc.autoTable({
        startY: yPos,
        head: [['University/College/School name', 'Course', 'Period', 'Year of graduation']],
        body: [[crew.educationInstitution || '', crew.educationCourse || '', crew.educationPeriodYears ? `${crew.educationPeriodYears} years` : '', crew.educationGraduationYear || '']],
        theme: 'grid',
        styles: bodyStyle,
        headStyles: headerStyle,
        margin: { left: ml, right: mr },
        columnStyles: {
          0: { cellWidth: tw * 0.4 },
          1: { cellWidth: tw * 0.25 },
          2: { cellWidth: tw * 0.15 },
          3: { cellWidth: tw * 0.2 }
        }
      })
      
      yPos = doc.lastAutoTable.finalY + 4
      
      // ========== 3. Immigration Documents — from travel_documents ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(8)
      doc.text('3. Immigration Documents', ml + 2, yPos + 3.5)
      yPos += 6
      
      const immigDocsPdf = travelDocuments.map((d: any) => [
        d.documentType || '', d.country?.countryName || 'Vietnam', d.documentNumber || '',
        d.issueDate ? format(new Date(d.issueDate), 'dd/MM/yyyy') : '',
        d.expiryDate ? format(new Date(d.expiryDate), 'dd/MM/yyyy') : '', d.notes || ''
      ])
      if (immigDocsPdf.length === 0) immigDocsPdf.push(['', '', '', '', '', ''])
      
      doc.autoTable({
        startY: yPos,
        head: [['Name of Document', 'Issued by', 'Number', 'Date of Issue', 'Date of expiry', 'Remark']],
        body: immigDocsPdf,
        theme: 'grid',
        styles: bodyStyle,
        headStyles: headerStyle,
        margin: { left: ml, right: mr },
        columnStyles: {
          0: { cellWidth: tw * 0.28 },
          1: { cellWidth: tw * 0.13 },
          2: { cellWidth: tw * 0.17 },
          3: { cellWidth: tw * 0.13 },
          4: { cellWidth: tw * 0.13 },
          5: { cellWidth: tw * 0.16 }
        }
      })
      
      yPos = doc.lastAutoTable.finalY + 4
      if (yPos > pageHeight - 30) { doc.addPage(); yPos = 10 }
      
      // ========== 4. Licenses ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(8)
      doc.text('4. Licenses', ml + 2, yPos + 3.5)
      yPos += 6
      
      // 4.1. National Licenses (Vietnam) — from seafarer_documents (coc)
      doc.setFontSize(7)
      doc.setFont('helvetica', 'bold')
      doc.text('4.1. National Licenses (Vietnam)', ml + 2, yPos + 3)
      yPos += 5
      
      const cocPdf = (seafarerDocuments || []).filter((d: any) =>
        d.documentType?.toLowerCase() === 'coc' || d.documentType?.toLowerCase() === 'certificate of competency'
      ).map((d: any, i: number) => [
        `${i + 1}`, d.documentType || 'CoC', d.country?.countryName || 'Vietnam', d.documentNumber || '',
        d.issueDate ? format(new Date(d.issueDate), 'dd/MM/yyyy') : '',
        d.expiryDate ? format(new Date(d.expiryDate), 'dd/MM/yyyy') : '', d.notes || ''
      ])
      if (cocPdf.length === 0) cocPdf.push(['1', '', '', '', '', '', ''])
      
      doc.autoTable({
        startY: yPos,
        head: [certColumns],
        body: cocPdf,
        theme: 'grid',
        styles: bodyStyle,
        headStyles: headerStyle,
        margin: { left: ml, right: mr },
        columnStyles: certColWidths
      })
      
      yPos = doc.lastAutoTable.finalY + 4
      if (yPos > pageHeight - 30) { doc.addPage(); yPos = 10 }
      
      // ========== 5. Training Certificate ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(8)
      doc.text('5. Training Certificate', ml + 2, yPos + 3.5)
      yPos += 6
      
      // 5.1. Training Certificate (required by STCW) — from crew_certificates
      doc.setFontSize(7)
      doc.setFont('helvetica', 'bold')
      doc.text('5.1. Training Certificate (required by STCW)', ml + 2, yPos + 3)
      yPos += 5
      
      const stcwPdf = (certificates || []).map((c: any, i: number) => [
        `${i + 1}`,
        c.certificate?.certificateName || c.certificateName || '',
        c.issuingAuthority || c.country?.countryName || '',
        c.certificateNumber || '',
        c.issueDate ? format(new Date(c.issueDate), 'dd/MM/yyyy') : '',
        c.expiryDate ? format(new Date(c.expiryDate), 'dd/MM/yyyy') : '',
        c.notes || ''
      ])
      if (stcwPdf.length === 0) stcwPdf.push(['1', '', '', '', '', '', ''])
      
      doc.autoTable({
        startY: yPos,
        head: [certColumns],
        body: stcwPdf,
        theme: 'grid',
        styles: bodyStyle,
        headStyles: headerStyle,
        margin: { left: ml, right: mr },
        columnStyles: certColWidths
      })
      
      yPos = doc.lastAutoTable.finalY + 3
      if (yPos > pageHeight - 30) { doc.addPage(); yPos = 10 }
      
      // 5.2. Training Certificate (required by Owner)
      doc.setFontSize(7)
      doc.setFont('helvetica', 'bold')
      doc.text('5.2. Training Certificate (required by Owner)', ml + 2, yPos + 3)
      yPos += 5
      
      doc.autoTable({
        startY: yPos,
        head: [certColumns],
        body: [['1', '', '', '', '', '', '']],
        theme: 'grid',
        styles: bodyStyle,
        headStyles: headerStyle,
        margin: { left: ml, right: mr },
        columnStyles: certColWidths
      })
      
      yPos = doc.lastAutoTable.finalY + 3
      if (yPos > pageHeight - 30) { doc.addPage(); yPos = 10 }
      
      // 5.3. In house training course — from employment_documents
      doc.setFontSize(7)
      doc.setFont('helvetica', 'bold')
      doc.text('5.3. In house training course', ml + 2, yPos + 3)
      yPos += 5
      
      const inHousePdf = (employmentDocuments || []).map((d: any, i: number) => [
        `${i + 1}`, d.documentType || '', d.country?.countryName || 'Vietnam', d.documentNumber || '',
        d.issueDate ? format(new Date(d.issueDate), 'dd/MM/yyyy') : '',
        d.expiryDate ? format(new Date(d.expiryDate), 'dd/MM/yyyy') : '', d.notes || ''
      ])
      if (inHousePdf.length === 0) inHousePdf.push(['1', '', '', '', '', '', ''])
      
      doc.autoTable({
        startY: yPos,
        head: [certColumns],
        body: inHousePdf,
        theme: 'grid',
        styles: bodyStyle,
        headStyles: headerStyle,
        margin: { left: ml, right: mr },
        columnStyles: certColWidths
      })
      
      yPos = doc.lastAutoTable.finalY + 4
      if (yPos > pageHeight - 30) { doc.addPage(); yPos = 10 }
      
      // ========== 6. Other certificate — from health_documents ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(8)
      doc.text('6. Other certificate', ml + 2, yPos + 3.5)
      yPos += 6
      
      const otherPdf = (healthDocuments || []).map((d: any, i: number) => [
        `${i + 1}`, d.documentType || '', d.country?.countryName || 'Vietnam', d.documentNumber || '',
        d.issueDate ? format(new Date(d.issueDate), 'dd/MM/yyyy') : '',
        d.expiryDate ? format(new Date(d.expiryDate), 'dd/MM/yyyy') : '', d.notes || ''
      ])
      if (otherPdf.length === 0) otherPdf.push(['1', '', '', '', '', '', ''])
      
      doc.autoTable({
        startY: yPos,
        head: [certColumns],
        body: otherPdf,
        theme: 'grid',
        styles: bodyStyle,
        headStyles: headerStyle,
        margin: { left: ml, right: mr },
        columnStyles: certColWidths
      })
      
      yPos = doc.lastAutoTable.finalY + 4
      if (yPos > pageHeight - 30) { doc.addPage(); yPos = 10 }
      
      // ========== 7. Service records ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(8)
      doc.text('7. Service records', ml + 2, yPos + 3.5)
      yPos += 6
      
      // Load service records if not available
      let svcRecords = serviceRecords
      if (svcRecords.length === 0 && id) {
        try { svcRecords = await maritimeService.crew.getServiceRecords(id) } catch { /* ignore */ }
      }
      
      const svcHeaders = [
        'Rank', 'Vessel', 'Flag', 'Type', 'GRT', 'Trade Area',
        'Year Built', 'Maker', 'Type/Model', 'K.W',
        'ECDIS', 'Embark Date', 'Disembark Date', 'Duration'
      ]
      
      const svcData = svcRecords.length > 0 ? svcRecords.map((s: ServiceRecord) => [
        s.rankAtTime || '',
        s.vesselName || '',
        s.vesselFlag || '',
        s.vesselType || '',
        s.vesselGrt ? `${s.vesselGrt}` : '',
        s.tradeArea || '',
        s.vesselYearBuilt ? `${s.vesselYearBuilt}` : '',
        s.mainEngineMaker || '',
        s.mainEngineType || '',
        s.mainEnginePowerKw ? `${s.mainEnginePowerKw}` : '',
        s.ecdis || '',
        s.boardingDate ? format(new Date(s.boardingDate), 'dd/MM/yyyy') : '',
        s.disembarkDate ? format(new Date(s.disembarkDate), 'dd/MM/yyyy') : '',
        s.totalServiceDays ? `${s.totalServiceDays}` : ''
      ]) : [['', '', '', '', '', '', '', '', '', '', '', '', '', '']]
      
      doc.autoTable({
        startY: yPos,
        head: [svcHeaders],
        body: svcData,
        theme: 'grid',
        styles: { fontSize: 5, cellPadding: 0.8, lineWidth: 0.1, lineColor: [0, 0, 0], overflow: 'linebreak' },
        headStyles: { ...headerStyle, fontSize: 5 },
        margin: { left: ml, right: mr },
        columnStyles: {
          0: { cellWidth: tw * 0.06 },   // Rank
          1: { cellWidth: tw * 0.115 },  // Vessel
          2: { cellWidth: tw * 0.05 },   // Flag
          3: { cellWidth: tw * 0.07 },   // Type
          4: { cellWidth: tw * 0.05 },   // GRT
          5: { cellWidth: tw * 0.085 },  // Trade Area
          6: { cellWidth: tw * 0.055 },  // Year Built
          7: { cellWidth: tw * 0.085 },  // Maker
          8: { cellWidth: tw * 0.085 },  // Type/Model
          9: { cellWidth: tw * 0.05 },   // K.W
          10: { cellWidth: tw * 0.07 },  // ECDIS
          11: { cellWidth: tw * 0.08 },  // Embark
          12: { cellWidth: tw * 0.08 },  // Disembark
          13: { cellWidth: tw * 0.065 }  // Duration
        }
      })
      
      yPos = doc.lastAutoTable.finalY + 4
      if (yPos > pageHeight - 20) { doc.addPage(); yPos = 10 }
      
      // ========== 8. Remark ==========
      doc.setFillColor(173, 216, 230)
      doc.rect(ml, yPos, tw, 5, 'F')
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(8)
      doc.text('8. Remark', ml + 2, yPos + 3.5)
      yPos += 7
      
      doc.setFont('helvetica', 'normal')
      doc.setFontSize(7)
      doc.text(crew.notes || '', ml + 2, yPos, { maxWidth: tw - 4 })
      
      // Save PDF
      doc.save(`BIO-DATA_${crew.crewId || crew.fullName}_${format(new Date(), 'yyyyMMdd')}.pdf`)
      
      toast.success('PDF exported successfully!')
    } catch (error: any) {
      console.error('Failed to export PDF:', error)
      toast.error(error.message || 'Failed to export PDF')
    }
  }

  const exportToExcel = async () => {
    if (!crew) return
    
    try {
      // Dynamic import ExcelJS
      const ExcelJS = await import('exceljs')
      
      // Load user's template XLSX (with merged cells already set up)
      const templateResponse = await fetch('/template1.xlsx')
      if (!templateResponse.ok) {
        throw new Error('Failed to load template1.xlsx')
      }
      const arrayBuffer = await templateResponse.arrayBuffer()
      
      // Load workbook from user's template - preserving all formatting, merged cells, borders
      const workbook = new ExcelJS.Workbook()
      await workbook.xlsx.load(arrayBuffer)
      
      // Get the worksheet (first sheet)
      const worksheet = workbook.worksheets[0]
      if (!worksheet) {
        throw new Error('Template worksheet not found')
      }
      
      // ===== TEMPLATE STRUCTURE (from analysis): =====
      // Row 4: G4:J4="Crew code" | K4:M4=DATA | Q4:S4="Present Rank" | T4:X4="WPR" 
      //         Z4:AC4="Prepared by" | AD4:AI4=DATA | AK4:AM4="Date Prepared" | AN4:AQ4=DATA
      // Row 8: H8:K9="Name" | L8:Z8="Full name"(label) | AA8:AD8="Date of Birth"(label) | AE8:AL8="Place of Birth"(label) | AM8:AR8="Nationality"(label)
      // Row 9: L9:Z9=DATA(name) | AA9:AD9=DATA(dob) | AE9:AR9=DATA(pob) | AM9:AR9=DATA(nationality)
      // Row 10: H10:K10="ID No." | L10:U10=DATA | V10:W10="Address" | X10:AR10=DATA
      // Row 11: H11:K11="Home Tel" | L11:O11=DATA | P11:R11="Hand phone" | S11:U11=DATA | V11:W11="Email" | X11:AG11=DATA | AH11:AL11="Marital status" | AM11:AR11=DATA
      // Row 12: H12:K12="Height" | L12:O12=DATA | P12:R12="Weight" | S12:U12=DATA | V12:W12="Overall size" | X12:Z12=DATA | AA12:AD12="Shoe's size" | AE12:AF12=DATA | AG12:AJ12="Catering size" | AK12:AL12=DATA | AM12:AO12="Blood Group" | AP12:AR12=DATA
      // Row 13: H13:K14="Contact person/Next of Kin" | L13:O13="Name"(label) | P13:U13=DATA(name) | V13:W13="Phone No."(label) | X13:AD13=DATA(phone) | AE13:AJ13="Covid-19 Vaccinated" | AK13:AL13=DATA | AM13:AO13="Smoker" | AP13:AS13=DATA
      // Row 14: L14:O14="Relation"(label) | P14:U14=DATA | V14:W14="Address"(label) | X14:AR14=DATA
      
      // ===== FILL DATA =====
      
      // Row 4: Header info
      worksheet.getCell('K4').value = crew.crewId || ''
      worksheet.getCell('T4').value = crew.rank?.rankName || ''  // Overwrite "WPR" with actual rank
      worksheet.getCell('AD4').value = ''  // Prepared by
      worksheet.getCell('AN4').value = new Date().toLocaleDateString('en-GB')
      
      // Row 9: Personal data (Row 8 has labels, Row 9 has data cells)
      worksheet.getCell('L9').value = crew.fullName || ''  // Full name data
      worksheet.getCell('AA9').value = crew.dateOfBirth ? new Date(crew.dateOfBirth).toLocaleDateString('en-GB') : ''
      worksheet.getCell('AE9').value = crew.placeOfBirth || ''
      worksheet.getCell('AM9').value = crew.nationality || ''
      
      // Row 10: ID No. and Address
      worksheet.getCell('L10').value = crew.idCardNumber || ''
      worksheet.getCell('X10').value = crew.address || ''
      
      // Row 11: Phones, Email, Marital status
      worksheet.getCell('L11').value = ''  // Home Tel (not in DB)
      worksheet.getCell('S11').value = crew.phoneNumber || ''  // Hand phone
      worksheet.getCell('X11').value = crew.emailAddress || ''  // Email
      worksheet.getCell('AM11').value = crew.maritalStatus || ''  // Marital status
      
      // Row 12: Physical details
      worksheet.getCell('L12').value = crew.height ? `${crew.height}` : ''
      worksheet.getCell('S12').value = crew.weight ? `${crew.weight}` : ''
      worksheet.getCell('X12').value = crew.clothingSize || ''  // Overall size
      worksheet.getCell('AE12').value = crew.shoeSize || ''  // Shoe's size
      worksheet.getCell('AK12').value = crew.cateringSize || ''  // Catering size
      worksheet.getCell('AP12').value = crew.bloodGroup || ''  // Blood Group
      
      // Row 13: Next of Kin
      worksheet.getCell('P13').value = crew.nextOfKinName || ''
      worksheet.getCell('X13').value = crew.nextOfKinPhone || ''
      worksheet.getCell('AK13').value = crew.isCovidVaccinated ? 'Yes' : 'No'
      worksheet.getCell('AP13').value = crew.isSmoker ? 'Yes' : 'No'
      
      // Row 14: Next of Kin continued
      worksheet.getCell('P14').value = crew.nextOfKinRelation || ''
      worksheet.getCell('X14').value = crew.nextOfKinAddress || ''
      
      // ===== SECTION 2: EDUCATION (Row 16 is header, data goes in next available row) =====
      // Row 16: A16="University..." | V16="Course" | AE16="Period" | AK16="Year of graduation"
      // Data should go in a row below headers - but no empty data row exists, so we use same row
      // Actually looking at template, row 16 IS the header labels. Need to check if there's a data row after.
      // From merge analysis: A16:K16, L16:U16, V16:W16, AE16:AF16, AG16:AJ16, AK16:AO16, AP16:AR16
      // So the labels are in row 16 merged cells. We don't have a data row for education after.
      // We'll skip education for now as template doesn't have clear data rows.
      
      // ===== SECTION 3: IMMIGRATION DOCUMENTS (Rows 19-24) — from travel_documents =====
      // Row 18: headers | Rows 19-24 are data rows
      const immigrationDocsExcel = travelDocuments.map((d: any) => ({
        name: d.documentType || 'Document',
        issuedBy: d.country?.countryName || 'Vietnam',
        number: d.documentNumber || '',
        issueDate: d.issueDate ? new Date(d.issueDate).toLocaleDateString('en-GB') : '',
        expiryDate: d.expiryDate ? new Date(d.expiryDate).toLocaleDateString('en-GB') : '',
        remark: d.notes || ''
      }))
      
      for (let i = 0; i < Math.min(immigrationDocsExcel.length, 6); i++) {
        const row = 19 + i
        const doc = immigrationDocsExcel[i]
        worksheet.getCell(`A${row}`).value = doc.name
        worksheet.getCell(`P${row}`).value = doc.issuedBy
        worksheet.getCell(`V${row}`).value = doc.number
        worksheet.getCell(`AA${row}`).value = doc.issueDate
        worksheet.getCell(`AE${row}`).value = doc.expiryDate
        worksheet.getCell(`AH${row}`).value = doc.remark
      }
      
      // ===== SECTION 4: LICENSES (Row 28) — from seafarer_documents where documentType='coc' =====
      const cocDocs = (seafarerDocuments || []).filter((d: any) => 
        d.documentType?.toLowerCase() === 'coc' || d.documentType?.toLowerCase() === 'certificate of competency'
      )
      
      for (let i = 0; i < Math.min(cocDocs.length, 1); i++) {
        const doc = cocDocs[i] as any
        worksheet.getCell('A28').value = `${i + 1}`
        worksheet.getCell('D28').value = doc.documentType || 'CoC'
        worksheet.getCell('P28').value = doc.country?.countryName || 'Vietnam'
        worksheet.getCell('V28').value = doc.documentNumber || ''
        worksheet.getCell('AA28').value = doc.issueDate ? new Date(doc.issueDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell('AE28').value = doc.expiryDate ? new Date(doc.expiryDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell('AH28').value = doc.notes || ''
      }
      
      // ===== SECTION 5.1: TRAINING CERTIFICATES STCW (Rows 31-34) — from crew_certificates =====
      const crewCerts = certificates || []
      
      for (let i = 0; i < Math.min(crewCerts.length, 4); i++) {
        const row = 31 + i  // Rows 31, 32, 33, 34
        const cert = crewCerts[i] as any
        worksheet.getCell(`A${row}`).value = `${i + 1}`
        worksheet.getCell(`D${row}`).value = cert.certificate?.certificateName || cert.certificateName || ''
        worksheet.getCell(`P${row}`).value = cert.issuingAuthority || cert.country?.countryName || ''
        worksheet.getCell(`V${row}`).value = cert.certificateNumber || ''
        worksheet.getCell(`AA${row}`).value = cert.issueDate ? new Date(cert.issueDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell(`AE${row}`).value = cert.expiryDate ? new Date(cert.expiryDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell(`AH${row}`).value = cert.notes || ''
      }
      
      // ===== SECTION 5.3: IN-HOUSE TRAINING (Rows 38-39) — from employment_documents =====
      for (let i = 0; i < Math.min(employmentDocuments.length, 2); i++) {
        const row = 38 + i  // Rows 38, 39
        const doc = employmentDocuments[i] as any
        worksheet.getCell(`A${row}`).value = `${i + 1}`
        worksheet.getCell(`D${row}`).value = doc.documentType || ''
        worksheet.getCell(`P${row}`).value = doc.country?.countryName || 'Vietnam'
        worksheet.getCell(`V${row}`).value = doc.documentNumber || ''
        worksheet.getCell(`AA${row}`).value = doc.issueDate ? new Date(doc.issueDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell(`AE${row}`).value = doc.expiryDate ? new Date(doc.expiryDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell(`AH${row}`).value = doc.notes || ''
      }
      
      // ===== SECTION 6: OTHER CERTIFICATES (Rows 41-43) — from health_documents =====
      for (let i = 0; i < Math.min(healthDocuments.length, 3); i++) {
        const row = 41 + i  // Rows 41, 42, 43
        const doc = healthDocuments[i] as any
        worksheet.getCell(`A${row}`).value = `${i + 1}`
        worksheet.getCell(`D${row}`).value = doc.documentType || ''
        worksheet.getCell(`P${row}`).value = doc.country?.countryName || 'Vietnam'
        worksheet.getCell(`V${row}`).value = doc.documentNumber || ''
        worksheet.getCell(`AA${row}`).value = doc.issueDate ? new Date(doc.issueDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell(`AE${row}`).value = doc.expiryDate ? new Date(doc.expiryDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell(`AH${row}`).value = doc.notes || ''
      }
      
      // ===== SECTION 7: SERVICE RECORDS (Row 44="7. Service records", Row 45-46 headers, Row 47 data) =====
      // Row 47 merge structure (0-indexed cols → letters):
      // A47:B47=Rank, C47:H47=Vessel, I47=Flag, J47:L47=Type, M47:N47=GRT, 
      // O47:Q47=Trade Area, R47=Year Built, S47:T47=Engine Maker, U47:V47=Engine Type,
      // W47=KW, X47:AB47=BWTS Maker, AD47:AE47=Scrubber Maker, AF47:AH47=Scrubber Type,
      // AI47:AK47=ECDIS Maker, AM47:AN47=Embark Date, AO47:AP47=Disembark, AQ47:AR47=Duration
      
      // Load service records if not loaded yet
      let records = serviceRecords
      if (records.length === 0 && id) {
        try {
          records = await maritimeService.crew.getServiceRecords(id)
        } catch { /* ignore */ }
      }
      
      // Only row 47 is available as a data row (between row 44 header and row 48 remark)
      if (records.length > 0) {
        const rec = records[0]
        worksheet.getCell('A47').value = rec.rankAtTime || ''
        worksheet.getCell('C47').value = rec.vesselName || ''
        worksheet.getCell('I47').value = rec.vesselFlag || ''
        worksheet.getCell('J47').value = rec.vesselType || ''
        worksheet.getCell('M47').value = rec.vesselGrt ? `${rec.vesselGrt}` : ''
        worksheet.getCell('O47').value = rec.tradeArea || ''
        worksheet.getCell('R47').value = rec.vesselYearBuilt ? `${rec.vesselYearBuilt}` : ''
        worksheet.getCell('S47').value = rec.mainEngineMaker || ''
        worksheet.getCell('U47').value = rec.mainEngineType || ''
        worksheet.getCell('W47').value = rec.mainEnginePowerKw ? `${rec.mainEnginePowerKw}` : ''
        // ECDIS
        worksheet.getCell('AI47').value = rec.ecdis || ''
        // Boarding records
        worksheet.getCell('AM47').value = rec.boardingDate ? new Date(rec.boardingDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell('AO47').value = rec.disembarkDate ? new Date(rec.disembarkDate).toLocaleDateString('en-GB') : ''
        worksheet.getCell('AQ47').value = rec.totalServiceDays ? `${rec.totalServiceDays}` : ''
      }
      
      // ===== ADD CREW PHOTO =====
      // Template has A1:D5 merged (company logo area) and A8:G14 merged (crew photo area)
      // The photo should go into the A8:G14 area (left side of Personal Particulars section)
      if (crew.photoUrl && crew.photoUrl.trim() !== '') {
        try {
          let imageUrl = crew.photoUrl
          let base64Data = ''
          
          if (imageUrl.startsWith('data:')) {
            base64Data = imageUrl.split(',')[1]
          } else {
            if (!imageUrl.startsWith('http')) {
              imageUrl = `http://localhost:5001${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`
            }
            
            const response = await fetch(imageUrl)
            const blob = await response.blob()
            const reader = new FileReader()
            
            await new Promise((resolve) => {
              reader.onloadend = () => resolve(null)
              reader.readAsDataURL(blob)
            })
            
            if (reader.result) {
              base64Data = reader.result.toString().split(',')[1]
            }
          }
          
          if (base64Data) {
            const imageId = workbook.addImage({
              base64: base64Data,
              extension: 'jpeg'
            })
            
            // Place crew photo in A8:G14 merged area (0-indexed: col 0, row 7 to col 7, row 14)
            worksheet.addImage(imageId, {
              tl: { col: 0.2, row: 7.2 },
              br: { col: 6.8, row: 13.8 }
            } as any)
          }
        } catch (imgError) {
          console.error('Excel: Could not add image:', imgError)
        }
      }
      
      // Export file
      const buffer = await workbook.xlsx.writeBuffer()
      const blob = new Blob([buffer], { 
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
      })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `BIO-DATA_${crew.crewId || crew.fullName}_${new Date().toISOString().split('T')[0]}.xlsx`
      link.click()
      URL.revokeObjectURL(url)
      
      toast.success('Excel exported successfully!')
    } catch (error: any) {
      console.error('❌ Failed to export Excel:', error)
      toast.error(error.message || 'Failed to export Excel')
    }
  }

  const handleSave = async () => {
    if (!crew) return
    
    try {
      setSaving(true)
      const updated = await maritimeService.crew.update(crew.id, editedCrew)
      setCrew(updated)
      setEditedCrew(updated)
      toast.success('Crew member updated successfully!')
    } catch (error: any) {
      console.error('❌ Failed to save crew:', error)
      toast.error(error.message || 'Failed to update crew member')
    } finally {
      setSaving(false)
    }
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
              EDIT {crew.fullName.toUpperCase()} - {crew.rank?.rankName?.toUpperCase() || 'CREW'}
            </h1>
          </div>
          <div className="flex items-center gap-2">
            <button 
              onClick={exportToPDF}
              className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 flex items-center gap-2 transition-colors"
              title="Export BIO-DATA to PDF"
            >
              <FileDown className="w-4 h-4" />
              <span>PDF</span>
            </button>
            <button 
              onClick={exportToExcel}
              className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 flex items-center gap-2 transition-colors"
              title="Export BIO-DATA to Excel"
            >
              <FileDown className="w-4 h-4" />
              <span>Excel</span>
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
            <button
              onClick={() => {
                setActiveTab('voyage-history')
                if (id) {
                  if (voyageHistory.length === 0) {
                    setLoadingVoyageHistory(true)
                    voyageMgmtService.crewAssignments.getCrewHistory(id)
                      .then(setVoyageHistory)
                      .catch(() => {})
                      .finally(() => setLoadingVoyageHistory(false))
                  }
                  if (serviceRecords.length === 0) {
                    setLoadingServiceRecords(true)
                    maritimeService.crew.getServiceRecords(id)
                      .then(setServiceRecords)
                      .catch(() => {})
                      .finally(() => setLoadingServiceRecords(false))
                  }
                }
              }}
              className={`px-6 py-3 text-sm font-medium border-b-2 transition-colors flex items-center gap-1.5 ${
                activeTab === 'voyage-history'
                  ? 'border-blue-600 text-blue-600 bg-blue-50'
                  : 'border-transparent text-gray-600 hover:text-gray-800'
              }`}
            >
              <Ship className="w-4 h-4" />
              Voyage History
              {voyageHistory.length > 0 && (
                <span className="ml-1 px-1.5 py-0.5 rounded-full text-xs bg-gray-200 text-gray-600">{voyageHistory.length}</span>
              )}
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
                      Full Name
                    </label>
                    <input
                      type="text"
                      value={editedCrew.fullName || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, fullName: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Rank
                    </label>
                    <select
                      value={editedCrew.rankId || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, rankId: e.target.value ? Number(e.target.value) : undefined })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    >
                      <option value="">Select rank</option>
                      {ranks.map(rank => (
                        <option key={rank.id} value={rank.id}>
                          {rank.rankName} ({rank.rankCode})
                        </option>
                      ))}
                    </select>
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
                </div>

                {/* Middle-Left Column - Personal Info */}
                <div className="col-span-3 space-y-2">
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
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Place of Birth
                    </label>
                    <input
                      type="text"
                      value={editedCrew.placeOfBirth || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, placeOfBirth: e.target.value })}
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
                      ID Card Number
                    </label>
                    <input
                      type="text"
                      value={editedCrew.idCardNumber || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, idCardNumber: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    />
                  </div>
                </div>

                {/* Middle-Right Column - Contact & Dates */}
                <div className="col-span-3 space-y-2">
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
                  <div>
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Marital Status
                    </label>
                    <select
                      value={editedCrew.maritalStatus || ''}
                      onChange={(e) => setEditedCrew({ ...editedCrew, maritalStatus: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                    >
                      <option value="">Select status</option>
                      <option value="Single">Single</option>
                      <option value="Married">Married</option>
                      <option value="Divorced">Divorced</option>
                      <option value="Widowed">Widowed</option>
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                        Height (cm)
                      </label>
                      <input
                        type="number"
                        value={editedCrew.height || ''}
                        onChange={(e) => setEditedCrew({ ...editedCrew, height: e.target.value ? Number(e.target.value) : undefined })}
                        className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                        Weight (kg)
                      </label>
                      <input
                        type="number"
                        step="0.1"
                        value={editedCrew.weight || ''}
                        onChange={(e) => setEditedCrew({ ...editedCrew, weight: e.target.value ? Number(e.target.value) : undefined })}
                        className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                      />
                    </div>
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
                  <div 
                    className="w-40 h-52 rounded-lg overflow-hidden bg-gray-200 mb-3 relative cursor-pointer hover:opacity-90 transition-opacity shadow-md"
                    onClick={() => {
                      if (!pendingAvatarPreview && editedCrew.photoUrl && editedCrew.photoUrl !== "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'%3E%3Ccircle cx='100' cy='100' r='100' fill='%23e5e7eb'/%3E%3Ccircle cx='100' cy='80' r='35' fill='%239ca3af'/%3E%3Cellipse cx='100' cy='160' rx='60' ry='45' fill='%239ca3af'/%3E%3C/svg%3E") {
                        handleViewImage(editedCrew.photoUrl, '', 'avatar')
                      }
                    }}
                    title={pendingAvatarPreview ? "New avatar (click Save to confirm)" : editedCrew.photoUrl ? "Click to view full size" : "Upload avatar"}
                  >
                    <img
                      src={pendingAvatarPreview || editedCrew.photoUrl || "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 260'%3E%3Crect width='200' height='260' fill='%23e5e7eb'/%3E%3Ccircle cx='100' cy='70' r='35' fill='%239ca3af'/%3E%3Cellipse cx='100' cy='180' rx='65' ry='50' fill='%239ca3af'/%3E%3C/svg%3E"}
                      alt="Avatar"
                      className="w-full h-full object-cover"
                    />
                    {uploadingAvatar && (
                      <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center">
                        <div className="animate-spin rounded-full h-8 w-8 border-4 border-white border-t-transparent"></div>
                      </div>
                    )}
                    {pendingAvatarPreview && !uploadingAvatar && (
                      <div className="absolute top-1 right-1 bg-yellow-500 text-white text-xs px-1.5 py-0.5 rounded font-medium">
                        NEW
                      </div>
                    )}
                  </div>
                  <div className="flex gap-2">
                    {pendingAvatarFile ? (
                      <>
                        <button 
                          onClick={handleAvatarSave}
                          disabled={uploadingAvatar}
                          className={`px-4 py-2 text-white text-sm rounded flex items-center gap-1 ${
                            uploadingAvatar ? 'bg-gray-400 cursor-not-allowed' : 'bg-green-600 hover:bg-green-700'
                          }`}
                        >
                          <Upload className="w-4 h-4" /> Save
                        </button>
                        <button 
                          onClick={handleCancelAvatarChange}
                          disabled={uploadingAvatar}
                          className="px-4 py-2 text-white text-sm rounded bg-gray-500 hover:bg-gray-600 flex items-center gap-1"
                        >
                          Cancel
                        </button>
                      </>
                    ) : (
                      <>
                        <button 
                          onClick={handleAvatarUpload}
                          disabled={uploadingAvatar}
                          className={`px-4 py-2 text-white text-sm rounded flex items-center gap-1 ${
                            uploadingAvatar ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
                          }`}
                        >
                          <Upload className="w-4 h-4" /> Choose
                        </button>
                        <button 
                          onClick={handleDeleteAvatar}
                          disabled={uploadingAvatar || !editedCrew.photoUrl}
                          className={`px-4 py-2 text-white text-sm rounded ${
                            uploadingAvatar || !editedCrew.photoUrl 
                              ? 'bg-gray-400 cursor-not-allowed' 
                              : 'bg-red-600 hover:bg-red-700'
                          }`}
                          title="Delete avatar"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </>
                    )}
                  </div>
                  <p className="text-xs text-gray-500 mt-2 text-center">
                    {uploadingAvatar ? 'Uploading...' : pendingAvatarFile ? 'Click Save to upload avatar' : 'Click Choose to select avatar'}
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

            {/* Physical Details & Preferences */}
            <div className="bg-white rounded-lg shadow-sm p-4">
              <h3 className="text-sm font-bold text-gray-700 uppercase mb-4">Physical Details & Preferences</h3>
              <div className="grid grid-cols-4 gap-4">
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Blood Group
                  </label>
                  <select
                    value={editedCrew.bloodGroup || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, bloodGroup: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  >
                    <option value="">Select</option>
                    <option value="A+">A+</option>
                    <option value="A-">A-</option>
                    <option value="B+">B+</option>
                    <option value="B-">B-</option>
                    <option value="AB+">AB+</option>
                    <option value="AB-">AB-</option>
                    <option value="O+">O+</option>
                    <option value="O-">O-</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Clothing Size
                  </label>
                  <input
                    type="text"
                    value={editedCrew.clothingSize || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, clothingSize: e.target.value })}
                    placeholder="e.g., L, XL"
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Shoe Size
                  </label>
                  <input
                    type="text"
                    value={editedCrew.shoeSize || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, shoeSize: e.target.value })}
                    placeholder="e.g., 42"
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Catering Size
                  </label>
                  <input
                    type="text"
                    value={editedCrew.cateringSize || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, cateringSize: e.target.value })}
                    placeholder="e.g., M"
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4 mt-4">
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    checked={editedCrew.isSmoker || false}
                    onChange={(e) => setEditedCrew({ ...editedCrew, isSmoker: e.target.checked })}
                    className="w-4 h-4 text-blue-600"
                  />
                  <label className="text-sm font-medium text-gray-700">Smoker</label>
                </div>
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    checked={editedCrew.isCovidVaccinated || false}
                    onChange={(e) => setEditedCrew({ ...editedCrew, isCovidVaccinated: e.target.checked })}
                    className="w-4 h-4 text-blue-600"
                  />
                  <label className="text-sm font-medium text-gray-700">COVID-19 Vaccinated</label>
                </div>
              </div>
            </div>

            {/* Employment Dates */}
            <div className="bg-white rounded-lg shadow-sm p-4">
              <h3 className="text-sm font-bold text-gray-700 uppercase mb-4">Employment Dates</h3>
              <div className="grid grid-cols-4 gap-4">
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
            </div>

            {/* Next of Kin */}
            <div className="bg-white rounded-lg shadow-sm p-4">
              <h3 className="text-sm font-bold text-gray-700 uppercase mb-4">Next of Kin / Emergency Contact</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Full Name
                  </label>
                  <input
                    type="text"
                    value={editedCrew.nextOfKinName || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, nextOfKinName: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Relationship
                  </label>
                  <select
                    value={editedCrew.nextOfKinRelation || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, nextOfKinRelation: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  >
                    <option value="">Select</option>
                    <option value="Father">Father</option>
                    <option value="Mother">Mother</option>
                    <option value="Spouse">Spouse</option>
                    <option value="Sibling">Sibling</option>
                    <option value="Child">Child</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Phone Number
                  </label>
                  <input
                    type="text"
                    value={editedCrew.nextOfKinPhone || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, nextOfKinPhone: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Address
                  </label>
                  <input
                    type="text"
                    value={editedCrew.nextOfKinAddress || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, nextOfKinAddress: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
              </div>
            </div>

            {/* Education */}
            <div className="bg-white rounded-lg shadow-sm p-4">
              <h3 className="text-sm font-bold text-gray-700 uppercase mb-4">Education Background</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Institution / University
                  </label>
                  <input
                    type="text"
                    value={editedCrew.educationInstitution || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, educationInstitution: e.target.value })}
                    placeholder="e.g., Vietnam Maritime University"
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Course / Major
                  </label>
                  <input
                    type="text"
                    value={editedCrew.educationCourse || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, educationCourse: e.target.value })}
                    placeholder="e.g., Marine Engineering, Nautical Science"
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Period (Years)
                  </label>
                  <input
                    type="number"
                    value={editedCrew.educationPeriodYears || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, educationPeriodYears: e.target.value ? Number(e.target.value) : undefined })}
                    placeholder="e.g., 4"
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                    Graduation Year
                  </label>
                  <input
                    type="number"
                    value={editedCrew.educationGraduationYear || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, educationGraduationYear: e.target.value ? Number(e.target.value) : undefined })}
                    placeholder="e.g., 2020"
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  />
                </div>
              </div>
            </div>

            {/* Contact Information */}
            <div className="bg-white rounded-lg shadow-sm p-4">
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
                    Emergency Contact (Legacy)
                  </label>
                  <textarea
                    value={editedCrew.emergencyContact || ''}
                    onChange={(e) => setEditedCrew({ ...editedCrew, emergencyContact: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500 bg-gray-50"
                    placeholder="Use Next of Kin section above"
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
              <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
                <h3 className="text-sm font-semibold text-gray-700 uppercase">
                  IDENTITY DOCUMENTS ({travelDocuments.length + seafarerDocuments.length + employmentDocuments.length})
                </h3>
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => setIsAddDocumentModalOpen(true)}
                    className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold transition-colors"
                    title="Add identity document"
                  >
                    +
                  </button>
                  <button onClick={() => setIsIdentityExpanded(!isIdentityExpanded)} className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all">
                    <span className="text-white text-xs transition-transform" style={{ transform: isIdentityExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
                      ▼
                    </span>
                  </button>
                </div>
              </div>
              {isIdentityExpanded && (
                loadingDocuments ? (
                  <div className="flex items-center justify-center py-12">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                  </div>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
                      <thead className="bg-white border-b-2 border-gray-300">
                        <tr>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '3%'}}></th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '20%'}}>Name</th>
                          <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>Files</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Number</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Date of Issue</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Place</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Country</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '12%'}}>Exp. Date</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white">
                        {/* Travel Documents */}
                        {travelDocuments.map((doc) => (
                          <tr key={`travel-${doc.id}`} className="border-b border-gray-100 hover:bg-gray-50">
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                              <button className="text-gray-400 hover:text-gray-600">::</button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                              <div className="flex items-center gap-2">
                                <span>🔒</span>
                                <span className="font-medium">{doc.documentType}</span>
                              </div>
                            </td>
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                              <button 
                                onClick={() => doc.fileUrl ? handleViewImage(doc.fileUrl, doc.id, 'travel_documents') : handleDocumentFileUpload(doc.id, 'travel_documents')}
                                disabled={uploadingDocId === doc.id}
                                className={`inline-flex items-center justify-center w-8 h-8 rounded text-white transition-colors ${
                                  uploadingDocId === doc.id 
                                    ? 'bg-gray-400 cursor-not-allowed' 
                                    : doc.fileUrl 
                                      ? 'bg-blue-500 hover:bg-blue-600' 
                                      : 'bg-green-500 hover:bg-green-600'
                                }`}
                                title={doc.fileUrl ? 'View image' : 'Upload image'}
                              >
                                {uploadingDocId === doc.id ? (
                                  <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                                ) : doc.fileUrl ? (
                                  <Eye className="w-4 h-4" />
                                ) : (
                                  <Upload className="w-4 h-4" />
                                )}
                              </button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.documentNumber}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.issueDate ? format(new Date(doc.issueDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">-</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.country?.name || doc.countryId === 1 ? 'Vietnam' : '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                              <div className="truncate">{doc.expiryDate ? format(new Date(doc.expiryDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                          </tr>
                        ))}
                        
                        {/* Seafarer Documents */}
                        {seafarerDocuments.map((doc) => (
                          <tr key={`seafarer-${doc.id}`} className="border-b border-gray-100 hover:bg-gray-50">
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                              <button className="text-gray-400 hover:text-gray-600">::</button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                              <div className="flex items-center gap-2">
                                <span>🔒</span>
                                <span className="font-medium">{doc.documentType}</span>
                              </div>
                            </td>
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                              <button 
                                onClick={() => doc.fileUrl ? handleViewImage(doc.fileUrl, doc.id, 'seafarer_documents') : handleDocumentFileUpload(doc.id, 'seafarer_documents')}
                                disabled={uploadingDocId === doc.id}
                                className={`inline-flex items-center justify-center w-8 h-8 rounded text-white transition-colors ${
                                  uploadingDocId === doc.id 
                                    ? 'bg-gray-400 cursor-not-allowed' 
                                    : doc.fileUrl 
                                      ? 'bg-blue-500 hover:bg-blue-600' 
                                      : 'bg-green-500 hover:bg-green-600'
                                }`}
                                title={doc.fileUrl ? 'View image' : 'Upload image'}
                              >
                                {uploadingDocId === doc.id ? (
                                  <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                                ) : doc.fileUrl ? (
                                  <Eye className="w-4 h-4" />
                                ) : (
                                  <Upload className="w-4 h-4" />
                                )}
                              </button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.documentNumber}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.issueDate ? format(new Date(doc.issueDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">-</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.country?.name || doc.countryId === 1 ? 'Vietnam' : '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                              <div className="truncate">{doc.expiryDate ? format(new Date(doc.expiryDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                          </tr>
                        ))}
                        
                        {/* Employment Documents */}
                        {employmentDocuments.map((doc) => (
                          <tr key={`employment-${doc.id}`} className="border-b border-gray-100 hover:bg-gray-50">
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                              <button className="text-gray-400 hover:text-gray-600">::</button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                              <div className="flex items-center gap-2">
                                <span>🔒</span>
                                <span className="font-medium">{doc.documentType}</span>
                              </div>
                            </td>
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                              <button 
                                onClick={() => doc.fileUrl ? handleViewImage(doc.fileUrl, doc.id, 'employment_documents') : handleDocumentFileUpload(doc.id, 'employment_documents')}
                                disabled={uploadingDocId === doc.id}
                                className={`inline-flex items-center justify-center w-8 h-8 rounded text-white transition-colors ${
                                  uploadingDocId === doc.id 
                                    ? 'bg-gray-400 cursor-not-allowed' 
                                    : doc.fileUrl 
                                      ? 'bg-blue-500 hover:bg-blue-600' 
                                      : 'bg-green-500 hover:bg-green-600'
                                }`}
                                title={doc.fileUrl ? 'View image' : 'Upload image'}
                              >
                                {uploadingDocId === doc.id ? (
                                  <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                                ) : doc.fileUrl ? (
                                  <Eye className="w-4 h-4" />
                                ) : (
                                  <Upload className="w-4 h-4" />
                                )}
                              </button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.documentNumber}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.issueDate ? format(new Date(doc.issueDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">-</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.country?.name || doc.countryId === 1 ? 'Vietnam' : '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                              <div className="truncate">{doc.expiryDate ? format(new Date(doc.expiryDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                          </tr>
                        ))}
                        
                        {(travelDocuments.length + seafarerDocuments.length + employmentDocuments.length) === 0 && (
                          <tr className="border-b border-gray-100">
                            <td colSpan={8} className="px-4 py-8 text-center text-gray-500">
                              No identity documents available
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                )
              )}
            </div>

            {/* HEALTH DOCUMENTS Section */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
                <h3 className="text-sm font-semibold text-gray-700 uppercase">HEALTH DOCUMENTS ({healthDocuments.length})</h3>
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => setIsAddHealthDocumentModalOpen(true)}
                    className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold transition-colors"
                    title="Add health document"
                  >
                    +
                  </button>
                  <button onClick={() => setIsHealthExpanded(!isHealthExpanded)} className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all">
                    <span className="text-white text-xs transition-transform" style={{ transform: isHealthExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
                      ▼
                    </span>
                  </button>
                </div>
              </div>
              {isHealthExpanded && (
                loadingDocuments ? (
                  <div className="flex items-center justify-center py-12">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                  </div>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
                      <thead className="bg-white border-b-2 border-gray-300">
                        <tr>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '3%'}}></th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '20%'}}>Name</th>
                          <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>Files</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Number</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Date of Issue</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Place</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>Country</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '12%'}}>Exp. Date</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white">
                        {healthDocuments.map((doc) => (
                          <tr key={`health-${doc.id}`} className="border-b border-gray-100 hover:bg-gray-50">
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '3%'}}>
                              <button className="text-gray-400 hover:text-gray-600">::</button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                              <div className="flex items-center gap-2">
                                <span>🏥</span>
                                <span className="font-medium">{doc.documentType}</span>
                              </div>
                            </td>
                            <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                              <button 
                                onClick={() => doc.fileUrl ? handleViewImage(doc.fileUrl, doc.id, 'health_documents') : handleDocumentFileUpload(doc.id, 'health_documents')}
                                disabled={uploadingDocId === doc.id}
                                className={`inline-flex items-center justify-center w-8 h-8 rounded text-white transition-colors ${
                                  uploadingDocId === doc.id 
                                    ? 'bg-gray-400 cursor-not-allowed' 
                                    : doc.fileUrl 
                                      ? 'bg-blue-500 hover:bg-blue-600' 
                                      : 'bg-green-500 hover:bg-green-600'
                                }`}
                                title={doc.fileUrl ? 'View image' : 'Upload image'}
                              >
                                {uploadingDocId === doc.id ? (
                                  <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                                ) : doc.fileUrl ? (
                                  <Eye className="w-4 h-4" />
                                ) : (
                                  <Upload className="w-4 h-4" />
                                )}
                              </button>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.documentNumber}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">{doc.issueDate ? format(new Date(doc.issueDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">-</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">-</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700" style={{width: '12%'}}>
                              <div className="truncate">{doc.expiryDate ? format(new Date(doc.expiryDate), 'dd/MM/yyyy') : '-'}</div>
                            </td>
                          </tr>
                        ))}
                        
                        {healthDocuments.length === 0 && (
                          <tr className="border-b border-gray-100">
                            <td colSpan={8} className="px-4 py-8 text-center text-gray-500">
                              No health documents available
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                )
              )}
            </div>

            {/* CERTIFICATES Section */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
                <h3 className="text-sm font-semibold text-gray-700 uppercase">CERTIFICATES ({certificates.length})</h3>
                <div className="flex items-center gap-2">
                  <button 
                    onClick={() => setShowAddCertModal(true)}
                    className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold transition-colors"
                    title="Add certificate"
                  >
                    +
                  </button>
                  <button onClick={() => setIsCertificatesExpanded(!isCertificatesExpanded)} className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all">
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
                          <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '6%'}}>Files</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '7%'}}>CoC</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '9%'}}>Country</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '11%'}}>Cert. Number</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '9%'}}>Issue Date</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '9%'}}>Expiry Date</th>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '14%'}}>Issuing Authority</th>
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
                              <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '6%'}}>
                                <button 
                                  onClick={() => cert.documentFilePath ? handleViewCertificateImage(cert.documentFilePath, cert.id) : handleCertificateFileUpload(cert.id)}
                                  disabled={uploadingCertId === cert.id}
                                  className={`inline-flex items-center justify-center w-8 h-8 rounded text-white transition-colors ${
                                    uploadingCertId === cert.id 
                                      ? 'bg-gray-400 cursor-not-allowed' 
                                      : cert.documentFilePath 
                                        ? 'bg-blue-500 hover:bg-blue-600' 
                                        : 'bg-green-500 hover:bg-green-600'
                                  }`}
                                  title={cert.documentFilePath ? 'View file' : 'Upload file'}
                                >
                                  {uploadingCertId === cert.id ? (
                                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                                  ) : cert.documentFilePath ? (
                                    <Eye className="w-4 h-4" />
                                  ) : (
                                    <Upload className="w-4 h-4" />
                                  )}
                                </button>
                              </td>
                              <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '7%'}}>
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
                              <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '9%'}}>
                                <div className="truncate">{cert.country?.countryName || cert.countryName || '-'}</div>
                              </td>
                              <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '11%'}}>
                                <code className="text-xs font-mono text-gray-900 truncate block">
                                  {cert.certificateNumber || '-'}
                                </code>
                              </td>
                              <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '9%'}}>
                                <div className="truncate">
                                  {cert.issueDate ? format(parseISO(cert.issueDate), 'dd MMM yyyy') : '-'}
                                </div>
                              </td>
                              <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '9%'}}>
                                <div className="text-gray-900 font-medium truncate">
                                  {cert.expiryDate ? format(parseISO(cert.expiryDate), 'dd MMM yyyy') : '-'}
                                </div>
                                {daysLeft !== null && (
                                  <div className={`text-xs ${status.color} truncate`}>
                                    {daysLeft} days left
                                  </div>
                                )}
                              </td>
                              <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '14%'}}>
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

        {activeTab === 'voyage-history' && (
          <div className="space-y-4">
            {/* Voyage Assignments */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
              <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Ship className="w-5 h-5 text-blue-600" />
                  <h3 className="text-lg font-semibold text-gray-800">Current Voyage Assignments</h3>
                </div>
                <span className="text-sm text-gray-500">{voyageHistory.length} assignment(s)</span>
              </div>

            {loadingVoyageHistory ? (
              <div className="flex items-center justify-center py-16">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <span className="ml-3 text-gray-500">Loading voyage history...</span>
              </div>
            ) : voyageHistory.length === 0 ? (
              <div className="text-center py-16">
                <Ship className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <p className="text-gray-500 font-medium">No voyage assignments found</p>
                <p className="text-gray-400 text-sm mt-1">This crew member has not been assigned to any voyages yet.</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="bg-gray-50 border-b border-gray-200">
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Voyage</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Role / Rank</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Embarkation</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Disembarkation</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Duration</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {voyageHistory.map((assignment) => {
                      const statusColors: Record<string, string> = {
                        'ASSIGNED': 'bg-yellow-100 text-yellow-700',
                        'ONBOARD': 'bg-green-100 text-green-700',
                        'DISEMBARKED': 'bg-gray-100 text-gray-700',
                        'CANCELLED': 'bg-red-100 text-red-700',
                      }
                      const days = assignment.embarkDate && assignment.disembarkDate
                        ? differenceInDays(parseISO(assignment.disembarkDate), parseISO(assignment.embarkDate))
                        : assignment.embarkDate
                          ? differenceInDays(new Date(), parseISO(assignment.embarkDate))
                          : null
                      return (
                        <tr key={assignment.id} className="hover:bg-gray-50 transition-colors">
                          <td className="px-4 py-3">
                            <div className="font-medium text-gray-800">{assignment.voyageNumber || assignment.remarks || '-'}</div>
                          </td>
                          <td className="px-4 py-3">
                            <div className="text-sm text-gray-800">{assignment.role || '-'}</div>
                            <div className="text-xs text-gray-500">{assignment.rankName || '-'}</div>
                          </td>
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-1">
                              <MapPin className="w-3.5 h-3.5 text-green-500" />
                              <span className="text-sm text-gray-800">{assignment.embarkPortName || assignment.embarkPortCode || '-'}</span>
                            </div>
                            {assignment.embarkDate && (
                              <div className="flex items-center gap-1 mt-0.5">
                                <Calendar className="w-3.5 h-3.5 text-gray-400" />
                                <span className="text-xs text-gray-500">
                                  {format(parseISO(assignment.embarkDate), 'dd MMM yyyy')}
                                </span>
                              </div>
                            )}
                          </td>
                          <td className="px-4 py-3">
                            {assignment.disembarkPortName || assignment.disembarkPortCode ? (
                              <>
                                <div className="flex items-center gap-1">
                                  <MapPin className="w-3.5 h-3.5 text-red-500" />
                                  <span className="text-sm text-gray-800">{assignment.disembarkPortName || assignment.disembarkPortCode}</span>
                                </div>
                                {assignment.disembarkDate && (
                                  <div className="flex items-center gap-1 mt-0.5">
                                    <Calendar className="w-3.5 h-3.5 text-gray-400" />
                                    <span className="text-xs text-gray-500">
                                      {format(parseISO(assignment.disembarkDate), 'dd MMM yyyy')}
                                    </span>
                                  </div>
                                )}
                              </>
                            ) : (
                              <span className="text-sm text-gray-400">—</span>
                            )}
                          </td>
                          <td className="px-4 py-3">
                            {days !== null ? (
                              <span className="text-sm text-gray-700 font-medium">{days} day{days !== 1 ? 's' : ''}</span>
                            ) : (
                              <span className="text-sm text-gray-400">—</span>
                            )}
                          </td>
                          <td className="px-4 py-3">
                            <span className={`inline-flex px-2 py-1 rounded-full text-xs font-semibold ${statusColors[assignment.status] || 'bg-gray-100 text-gray-600'}`}>
                              {assignment.status}
                            </span>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* Service Records (Sea Service History) */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Ship className="w-5 h-5 text-purple-600" />
                <h3 className="text-lg font-semibold text-gray-800">Service Records (BIO-DATA)</h3>
              </div>
              <span className="text-sm text-gray-500">{serviceRecords.length} record(s)</span>
            </div>

            {loadingServiceRecords ? (
              <div className="flex items-center justify-center py-16">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <span className="ml-3 text-gray-500">Loading service records...</span>
              </div>
            ) : serviceRecords.length === 0 ? (
              <div className="text-center py-16">
                <Ship className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <p className="text-gray-500 font-medium">No service records found</p>
                <p className="text-gray-400 text-sm mt-1">This crew member's sea service history will appear here.</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="bg-gray-50 border-b border-gray-200">
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Vessel</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Type / Flag</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Rank</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Boarding</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Disembark</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Service Days</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">GRT / DWT</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {serviceRecords.map((record) => (
                      <tr key={record.id} className="hover:bg-gray-50 transition-colors">
                        <td className="px-4 py-3">
                          <div className="font-medium text-gray-800">{record.vesselName}</div>
                          {record.tradeArea && (
                            <div className="text-xs text-gray-500">{record.tradeArea}</div>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <div className="text-sm text-gray-700">{record.vesselType || '-'}</div>
                          <div className="text-xs text-gray-500">{record.vesselFlag || '-'}</div>
                        </td>
                        <td className="px-4 py-3">
                          <span className="inline-flex px-2 py-1 rounded text-xs font-medium bg-blue-100 text-blue-700">
                            {record.rankAtTime || 'N/A'}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          {record.boardingDate ? (
                            <>
                              <div className="text-sm text-gray-800">
                                {format(parseISO(record.boardingDate), 'dd MMM yyyy')}
                              </div>
                              {record.boardingPort && (
                                <div className="text-xs text-gray-500">{record.boardingPort}</div>
                              )}
                            </>
                          ) : (
                            <span className="text-sm text-gray-400">—</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          {record.disembarkDate ? (
                            <>
                              <div className="text-sm text-gray-800">
                                {format(parseISO(record.disembarkDate), 'dd MMM yyyy')}
                              </div>
                              {record.disembarkPort && (
                                <div className="text-xs text-gray-500">{record.disembarkPort}</div>
                              )}
                            </>
                          ) : (
                            <span className="inline-flex px-2 py-1 rounded text-xs font-medium bg-green-100 text-green-700">
                              On Board
                            </span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          {record.totalServiceDays ? (
                            <span className="text-sm font-medium text-gray-700">{record.totalServiceDays} days</span>
                          ) : record.boardingDate && !record.disembarkDate ? (
                            <span className="text-sm text-gray-600">
                              {differenceInDays(new Date(), parseISO(record.boardingDate))} days
                            </span>
                          ) : (
                            <span className="text-sm text-gray-400">—</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <div className="text-sm text-gray-700">
                            {record.vesselGrt ? `${record.vesselGrt.toLocaleString()} GRT` : '-'}
                          </div>
                          <div className="text-xs text-gray-500">
                            {record.vesselDwt ? `${record.vesselDwt.toLocaleString()} DWT` : '-'}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
        )}
      </div>

      <AddDocumentModal
        isOpen={isAddDocumentModalOpen}
        crewMemberId={id || ''}
        onClose={() => setIsAddDocumentModalOpen(false)}
        onSuccess={() => {
          if (id) {
            loadDocuments(id)
          }
        }}
      />

      <AddHealthDocumentModal
        isOpen={isAddHealthDocumentModalOpen}
        crewMemberId={id || ''}
        onClose={() => setIsAddHealthDocumentModalOpen(false)}
        onSuccess={() => {
          if (id) {
            loadDocuments(id)
          }
        }}
      />

      <ImageViewerModal
        isOpen={isImageViewerOpen}
        imageUrl={imageViewerUrl}
        documentId={imageViewerDocId || undefined}
        targetTable={imageViewerTargetTable || undefined}
        customUploadHandler={viewingCertificateId ? handleCertificateUploadHandler : undefined}
        onClose={() => {
          setIsImageViewerOpen(false)
          setImageViewerUrl(null)
          setImageViewerDocId(null)
          setImageViewerTargetTable(null)
          setViewingCertificateId(null)
        }}
        onFileChanged={() => {
          if (id) {
            loadDocuments(id)
            // Also reload certificates if we were viewing a certificate image
            if (viewingCertificateId) {
              maritimeService.certificates.getCrewCertificatesByCrewId(id)
                .then(certs => setCertificates(certs))
                .catch(() => {})
            }
          }
        }}
      />

      <AddCrewCertificateModal
        isOpen={showAddCertModal}
        onClose={() => setShowAddCertModal(false)}
        onSave={() => {
          if (id) {
            // Reload certificates
            setLoadingCertificates(true)
            maritimeService.certificates.getCrewCertificatesByCrewId(id)
              .then(certs => setCertificates(certs))
              .catch(() => setCertificates([]))
              .finally(() => setLoadingCertificates(false))
          }
        }}
        crewId={id}
      />
    </div>
  )
}
     