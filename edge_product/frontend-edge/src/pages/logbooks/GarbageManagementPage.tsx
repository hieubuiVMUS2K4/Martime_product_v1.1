import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { GarbagePartIForm } from '../../components/logbooks/GarbagePartIForm';
import { GarbagePartIIForm } from '../../components/logbooks/GarbagePartIIForm';
import { toast } from 'sonner';
import { logbookService } from '../../services/logbook.service';
import type { 
  GarbagePartIResponseDto,
  GarbagePartIIResponseDto 
} from '../../types/logbook.types';
import { useTranslationSafe } from '@/contexts/I18nContext';

// MARPOL Annex V Part I Categories (A-I)
const PART_I_CATEGORIES = [
  { code: 'A', name: 'Plastics', seaDischarge: false, description: 'All types of plastic' },
  { code: 'B', name: 'Food Wastes', seaDischarge: true, description: 'Food preparation waste' },
  { code: 'C', name: 'Domestic Wastes', seaDischarge: true, description: 'Paper, rags, glass, metal, bottles, crockery' },
  { code: 'D', name: 'Cooking Oil', seaDischarge: false, description: 'Edible oils' },
  { code: 'E', name: 'Incinerator Ashes', seaDischarge: true, description: 'Ash from incineration' },
  { code: 'F', name: 'Operational Wastes', seaDischarge: false, description: 'Maintenance/cleaning materials' },
  { code: 'G', name: 'Cargo Residues (non-HME)', seaDischarge: true, description: 'Non-harmful cargo residues - cleaned' },
  { code: 'H', name: 'Cargo Residues (HME)', seaDischarge: false, description: 'Harmful cargo residues - cleaned' },
  { code: 'I', name: 'Animal Carcasses', seaDischarge: true, description: 'Dead animals' },
];

// Part II Categories (J-K - Cargo Residues)
const PART_II_CATEGORIES = [
  { code: 'J', name: 'Cargo Residues (non-HME)', seaDischarge: true, description: 'Non-harmful cargo residues in wash water' },
  { code: 'K', name: 'Cargo Residues (HME)', seaDischarge: false, description: 'Harmful cargo residues - STRICTLY PROHIBITED TO SEA' },
];

type TabType = 'part-i' | 'part-ii';

export const GarbageManagementPage: React.FC = () => {
  const { locale, t } = useTranslationSafe();
  const isVi = locale === 'vi';
  const [activeTab, setActiveTab] = useState<TabType>('part-i');
  const [partIEntries, setPartIEntries] = useState<GarbagePartIResponseDto[]>([]);
  const [partIIEntries, setPartIIEntries] = useState<GarbagePartIIResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [signModal, setSignModal] = useState<{
    show: boolean;
    entryId: string | null;
    type: 'part-i' | 'part-ii';
  }>({ show: false, entryId: null, type: 'part-i' });
  const [masterSignature, setMasterSignature] = useState('Captain');

  // Part I form state
  const [partIForm, setPartIForm] = useState({
    operationDate: new Date().toISOString().split('T')[0],
    operationTime: new Date().toTimeString().slice(0, 5),
    operationEndTime: '',
    category: '',
    description: '',
    amountToSea: '',
    amountToReception: '',
    amountIncinerated: '',
    latitude: 0,
    longitude: 0,
    portName: '',
    receptionFacilityName: '',
    receiptNumber: '',
    incinerationStartTime: '',
    incinerationEndTime: '',
    incineratorDetails: '',
    exceptionalDischargeReason: '',
    waterDepth: '',
    remarks: '',
    officerInCharge: 'Chief Officer'
  });

  // Part II form state
  const [partIIForm, setPartIIForm] = useState({
    operationDate: new Date().toISOString().split('T')[0],
    operationTime: new Date().toTimeString().slice(0, 5),
    operationEndTime: '',
    category: '',
    startLatitude: 0,
    startLongitude: 0,
    endLatitude: 0,
    endLongitude: 0,
    amountToSea: '',
    amountToReception: '',
    portName: '',
    receptionFacilityName: '',
    receiptNumber: '',
    cargoDescription: '',
    holdNumbersWashed: '',
    remarks: '',
    officerInCharge: 'Chief Officer'
  });

  const handleStartEditPartI = (entry: GarbagePartIResponseDto) => {
    setPartIForm({
      operationDate: entry.operationDate ? entry.operationDate.slice(0, 10) : new Date().toISOString().split('T')[0],
      operationTime: entry.operationTime ? entry.operationTime.slice(0, 5) : new Date().toTimeString().slice(0, 5),
      operationEndTime: entry.operationEndTime ? entry.operationEndTime.slice(0, 5) : '',
      category: entry.category || '',
      description: entry.description || '',
      amountToSea: entry.estimatedAmountDischargedToSea?.toString() || '',
      amountToReception: entry.estimatedAmountToReceptionFacilities?.toString() || '',
      amountIncinerated: entry.estimatedAmountIncinerated?.toString() || '',
      latitude: entry.dischargeLatitude ?? 0,
      longitude: entry.dischargeLongitude ?? 0,
      portName: entry.portName || '',
      receptionFacilityName: entry.receptionFacilityName || '',
      receiptNumber: entry.receiptNumber || '',
      incinerationStartTime: entry.incinerationStartTime ? new Date(entry.incinerationStartTime).toTimeString().slice(0, 5) : '',
      incinerationEndTime: entry.incinerationEndTime ? new Date(entry.incinerationEndTime).toTimeString().slice(0, 5) : '',
      incineratorDetails: entry.incineratorDetails || '',
      exceptionalDischargeReason: entry.exceptionalDischargeReason || '',
      waterDepth: entry.waterDepth?.toString() || '',
      remarks: entry.remarks || '',
      officerInCharge: entry.officerInCharge || 'Chief Officer'
    });
    setEditingId(entry.id);
    setShowForm(true);
  };

  const handleStartEditPartII = (entry: GarbagePartIIResponseDto) => {
    setPartIIForm({
      operationDate: entry.operationDate ? entry.operationDate.slice(0, 10) : new Date().toISOString().split('T')[0],
      operationTime: entry.operationTime ? entry.operationTime.slice(0, 5) : new Date().toTimeString().slice(0, 5),
      operationEndTime: entry.operationEndTime ? entry.operationEndTime.slice(0, 5) : '',
      category: entry.category || '',
      startLatitude: entry.startLatitude ?? 0,
      startLongitude: entry.startLongitude ?? 0,
      endLatitude: entry.endLatitude ?? 0,
      endLongitude: entry.endLongitude ?? 0,
      amountToSea: entry.estimatedAmountDischargedToSea?.toString() || '',
      amountToReception: entry.estimatedAmountToReceptionFacilities?.toString() || '',
      portName: entry.portName || '',
      receptionFacilityName: entry.receptionFacilityName || '',
      receiptNumber: entry.receiptNumber || '',
      cargoDescription: entry.cargoDescription || '',
      holdNumbersWashed: entry.holdNumbersWashed || '',
      remarks: entry.remarks || '',
      officerInCharge: entry.officerInCharge || 'Chief Officer'
    });
    setEditingId(entry.id);
    setShowForm(true);
  };

  useEffect(() => {
    fetchEntries();
  }, [activeTab]);

  // Debug: Log form changes
  useEffect(() => {
    console.log('[Part I Form State Updated]', partIForm.category, partIForm.description);
  }, [partIForm]);

  useEffect(() => {
    console.log('[Part II Form State Updated]', partIIForm.category);
  }, [partIIForm]);

  // Part I onChange handlers
  const handlePartIChange = (field: string, value: any) => {
    console.log(`[Part I] Changing ${field} to:`, value);
    setPartIForm(prev => {
      const newForm = { ...prev, [field]: value };
      console.log('[Part I] New form state:', newForm);
      return newForm;
    });
  };

  const handlePartICategorySelect = (categoryCode: string, categoryName: string) => {
    console.log(`[Part I] Category selected: ${categoryCode} - ${categoryName}`);
    setPartIForm(prev => ({
      ...prev,
      category: categoryCode,
      description: categoryName
    }));
  };

  // Part II onChange handlers
  const handlePartIIChange = (field: string, value: any) => {
    console.log(`[Part II] Changing ${field} to:`, value);
    setPartIIForm(prev => {
      const newForm = { ...prev, [field]: value };
      console.log('[Part II] New form state:', newForm);
      return newForm;
    });
  };

  const handlePartIICategorySelect = (categoryCode: string) => {
    console.log(`[Part II] Category selected: ${categoryCode}`);
    setPartIIForm(prev => ({
      ...prev,
      category: categoryCode,
      amountToSea: categoryCode === 'K' ? '' : prev.amountToSea
    }));
  };

  const fetchEntries = async () => {
    try {
      setLoading(true);
      if (activeTab === 'part-i') {
        const response = await logbookService.getGarbagePartIEntries({ page: 1, pageSize: 20 });
        setPartIEntries(response.data);
      } else {
        const response = await logbookService.getGarbagePartIIEntries({ page: 1, pageSize: 20 });
        setPartIIEntries(response.data);
      }
    } catch (error) {
      console.error(error);
      toast.error(
        isVi
          ? `Không thể tải các bản ghi ${activeTab === 'part-i' ? 'Phần I' : 'Phần II'}`
          : `Failed to load ${activeTab === 'part-i' ? 'Part I' : 'Part II'} entries`
      );
    } finally {
      setLoading(false);
    }
  };

  const handlePartISubmit = async () => {
    try {
      // Validation
      if (!partIForm.category) {
        toast.error(t('logbooks.garbageRecord.selectCategory'));
        return;
      }

      if (!partIForm.description || partIForm.description.trim().length === 0) {
        toast.error(t('logbooks.garbageRecord.descriptionRequired'));
        return;
      }

      const totalAmount = parseFloat(partIForm.amountToSea || '0') +
        parseFloat(partIForm.amountToReception || '0') +
        parseFloat(partIForm.amountIncinerated || '0');

      if (totalAmount <= 0) {
        toast.error(t('logbooks.garbageRecord.amountGreaterThanZero'));
        return;
      }

      // Check sea discharge prohibition
      const category = PART_I_CATEGORIES.find(c => c.code === partIForm.category);
      if (parseFloat(partIForm.amountToSea) > 0 && category && !category.seaDischarge) {
        let catName = category.name;
        if (isVi) {
          if (category.code === 'A') catName = 'Chất dẻo (Nhựa)';
          else if (category.code === 'B') catName = 'Chất thải thực phẩm';
          else if (category.code === 'C') catName = 'Chất thải sinh hoạt';
          else if (category.code === 'D') catName = 'Dầu ăn';
          else if (category.code === 'E') catName = 'Tro lò đốt';
          else if (category.code === 'F') catName = 'Chất thải khai thác';
          else if (category.code === 'G') catName = 'Dư lượng hàng hóa (không HME)';
          else if (category.code === 'H') catName = 'Dư lượng hàng hóa (HME)';
          else if (category.code === 'I') catName = 'Xác động vật';
        }
        toast.error(
          isVi
            ? `Loại ${category.code} (${catName}) không được phép xả ra biển theo MARPOL Phụ lục V`
            : `Category ${category.code} (${category.name}) cannot be discharged to sea per MARPOL Annex V`
        );
        return;
      }

      // Require position for sea discharge
      if (parseFloat(partIForm.amountToSea) > 0 && (!partIForm.latitude || !partIForm.longitude)) {
        toast.error(t('logbooks.garbageRecord.positionRequired'));
        return;
      }

      // Require port for reception
      if (parseFloat(partIForm.amountToReception) > 0 && !partIForm.portName && !partIForm.receptionFacilityName) {
        toast.error(t('logbooks.garbageRecord.portOrFacilityRequired'));
        return;
      }

      const entry = {
        operationDate: partIForm.operationDate,
        operationTime: partIForm.operationTime + ':00', // Convert HH:mm to HH:mm:ss for TimeSpan
        ...(partIForm.operationEndTime && partIForm.operationEndTime.trim().length > 0 && {
          operationEndTime: partIForm.operationEndTime + ':00'
        }),
        category: partIForm.category,
        description: partIForm.description,
        ...(parseFloat(partIForm.amountToSea) > 0 && {
          estimatedAmountDischargedToSea: parseFloat(partIForm.amountToSea)
        }),
        ...(parseFloat(partIForm.amountToReception) > 0 && {
          estimatedAmountToReceptionFacilities: parseFloat(partIForm.amountToReception)
        }),
        ...(parseFloat(partIForm.amountIncinerated) > 0 && {
          estimatedAmountIncinerated: parseFloat(partIForm.amountIncinerated)
        }),
        ...(partIForm.latitude && partIForm.latitude !== 0 && {
          dischargeLatitude: partIForm.latitude
        }),
        ...(partIForm.longitude && partIForm.longitude !== 0 && {
          dischargeLongitude: partIForm.longitude
        }),
        ...(partIForm.portName && partIForm.portName.trim().length > 0 && {
          portName: partIForm.portName.trim()
        }),
        ...(partIForm.receptionFacilityName && partIForm.receptionFacilityName.trim().length > 0 && {
          receptionFacilityName: partIForm.receptionFacilityName.trim()
        }),
        ...(partIForm.receiptNumber && partIForm.receiptNumber.trim().length > 0 && {
          receiptNumber: partIForm.receiptNumber.trim()
        }),
        // IMPORTANT: Completely omit these fields if empty, don't send empty strings or null
        ...(partIForm.incinerationStartTime && partIForm.incinerationStartTime.trim().length > 0 && {
          incinerationStartTime: `${partIForm.operationDate}T${partIForm.incinerationStartTime}:00`
        }),
        ...(partIForm.incinerationEndTime && partIForm.incinerationEndTime.trim().length > 0 && {
          incinerationEndTime: `${partIForm.operationDate}T${partIForm.incinerationEndTime}:00`
        }),
        ...(partIForm.incineratorDetails && partIForm.incineratorDetails.trim().length > 0 && {
          incineratorDetails: partIForm.incineratorDetails.trim()
        }),
        ...(partIForm.exceptionalDischargeReason && partIForm.exceptionalDischargeReason.trim().length > 0 && {
          exceptionalDischargeReason: partIForm.exceptionalDischargeReason.trim()
        }),
        ...(partIForm.waterDepth && parseFloat(partIForm.waterDepth) > 0 && {
          waterDepth: parseFloat(partIForm.waterDepth)
        }),
        ...(partIForm.remarks && partIForm.remarks.trim().length > 0 && {
          remarks: partIForm.remarks.trim()
        }),
        officerInCharge: partIForm.officerInCharge
      };

      console.log('=== Submitting Part I Entry ===');
      console.log('Payload:', JSON.stringify(entry, null, 2));

      if (editingId) {
        await logbookService.updateGarbagePartIEntry(editingId, entry);
        toast.success(t('logbooks.garbageRecord.entryUpdated') || 'Entry updated successfully');
      } else {
        await logbookService.createGarbagePartIEntry(entry);
        toast.success(t('logbooks.garbageRecord.entrySaved'));
      }
      fetchEntries();
      setShowForm(false);
      resetPartIForm();
    } catch (error: any) {
      console.error(error);
      toast.error(error.response?.data?.error || t('logbooks.garbageRecord.saveFailed'));
    }
  };

  const handlePartIISubmit = async () => {
    try {
      // Validation
      if (!partIIForm.category) {
        toast.error(t('logbooks.garbageRecord.selectCategoryJK'));
        return;
      }

      const totalAmount = parseFloat(partIIForm.amountToSea || '0') +
        parseFloat(partIIForm.amountToReception || '0');

      if (totalAmount <= 0) {
        toast.error(t('logbooks.garbageRecord.amountGreaterThanZero'));
        return;
      }

      // CRITICAL: Check Category K
      if (partIIForm.category === 'K' && parseFloat(partIIForm.amountToSea) > 0) {
        toast.error(t('logbooks.garbageRecord.marpolViolationK'));
        return;
      }

      if (partIIForm.category === 'K' && parseFloat(partIIForm.amountToReception) <= 0) {
        toast.error(t('logbooks.garbageRecord.categoryKReceptionOnly'));
        return;
      }

      // MANDATORY: Check positions
      if (partIIForm.startLatitude === 0 && partIIForm.startLongitude === 0) {
        toast.error(t('logbooks.garbageRecord.startPositionRequired'));
        return;
      }

      if (partIIForm.endLatitude === 0 && partIIForm.endLongitude === 0) {
        toast.error(t('logbooks.garbageRecord.endPositionRequired'));
        return;
      }

      if (!partIIForm.cargoDescription) {
        toast.error(t('logbooks.garbageRecord.cargoDescriptionRequired'));
        return;
      }

      if (!partIIForm.holdNumbersWashed) {
        toast.error(t('logbooks.garbageRecord.holdNumbersWashedRequired'));
        return;
      }

      const entry = {
        operationDate: partIIForm.operationDate,
        operationTime: partIIForm.operationTime + ':00', // Convert HH:mm to HH:mm:ss for TimeSpan
        ...(partIIForm.operationEndTime && partIIForm.operationEndTime.trim().length > 0 && {
          operationEndTime: partIIForm.operationEndTime + ':00'
        }),
        category: partIIForm.category,
        startLatitude: partIIForm.startLatitude,
        startLongitude: partIIForm.startLongitude,
        endLatitude: partIIForm.endLatitude,
        endLongitude: partIIForm.endLongitude,
        ...(parseFloat(partIIForm.amountToSea) > 0 && {
          estimatedAmountDischargedToSea: parseFloat(partIIForm.amountToSea)
        }),
        ...(parseFloat(partIIForm.amountToReception) > 0 && {
          estimatedAmountToReceptionFacilities: parseFloat(partIIForm.amountToReception)
        }),
        ...(partIIForm.portName && partIIForm.portName.trim().length > 0 && {
          portName: partIIForm.portName.trim()
        }),
        ...(partIIForm.receptionFacilityName && partIIForm.receptionFacilityName.trim().length > 0 && {
          receptionFacilityName: partIIForm.receptionFacilityName.trim()
        }),
        ...(partIIForm.receiptNumber && partIIForm.receiptNumber.trim().length > 0 && {
          receiptNumber: partIIForm.receiptNumber.trim()
        }),
        cargoDescription: partIIForm.cargoDescription.trim(),
        holdNumbersWashed: partIIForm.holdNumbersWashed.trim(),
        ...(partIIForm.remarks && partIIForm.remarks.trim().length > 0 && {
          remarks: partIIForm.remarks.trim()
        }),
        officerInCharge: partIIForm.officerInCharge
      };

      console.log('=== Submitting Part II Entry ===');
      console.log('Payload:', JSON.stringify(entry, null, 2));

      if (editingId) {
        await logbookService.updateGarbagePartIIEntry(editingId, entry);
        toast.success(t('logbooks.garbageRecord.entryUpdated') || 'Entry updated successfully');
      } else {
        await logbookService.createGarbagePartIIEntry(entry);
        toast.success(t('logbooks.garbageRecord.entrySaved'));
      }
      fetchEntries();
      setShowForm(false);
      resetPartIIForm();
    } catch (error: any) {
      console.error(error);
      toast.error(error.response?.data?.error || t('logbooks.garbageRecord.saveFailed'));
    }
  };

  const handleSignEntry = (entryId: string, type: 'part-i' | 'part-ii', alreadySigned: boolean) => {
    if (alreadySigned) {
      toast.info(t('logbooks.garbageRecord.alreadySigned'));
      return;
    }
    setSignModal({ show: true, entryId, type });
  };

  const confirmSign = async () => {
    if (!signModal.entryId || !masterSignature.trim()) {
      toast.error(t('logbooks.garbageRecord.masterSignatureRequired'));
      return;
    }

    try {
      const signData = {
        signature: masterSignature.trim(),
        masterSignature: masterSignature.trim(),
        signedAt: new Date().toISOString()
      };

      if (signModal.type === 'part-i') {
        await logbookService.signGarbagePartIEntry(signModal.entryId, signData);
      } else {
        await logbookService.signGarbagePartIIEntry(signModal.entryId, signData);
      }
      toast.success(t('logbooks.common.signSuccess'));

      setSignModal({ show: false, entryId: null, type: 'part-i' });
      setMasterSignature('Captain');
      fetchEntries();
    } catch (error: any) {
      console.error(error);
      toast.error(error.response?.data?.error || t('logbooks.common.signFailed'));
    }
  };

  const resetPartIForm = () => {
    setPartIForm({
      operationDate: new Date().toISOString().split('T')[0],
      operationTime: new Date().toTimeString().slice(0, 5),
      operationEndTime: '',
      category: '',
      description: '',
      amountToSea: '',
      amountToReception: '',
      amountIncinerated: '',
      latitude: 0,
      longitude: 0,
      portName: '',
      receptionFacilityName: '',
      receiptNumber: '',
      incinerationStartTime: '',
      incinerationEndTime: '',
      incineratorDetails: '',
      exceptionalDischargeReason: '',
      waterDepth: '',
      remarks: '',
      officerInCharge: 'Chief Officer'
    });
    setEditingId(null);
  };

  const resetPartIIForm = () => {
    setPartIIForm({
      operationDate: new Date().toISOString().split('T')[0],
      operationTime: new Date().toTimeString().slice(0, 5),
      operationEndTime: '',
      category: '',
      startLatitude: 0,
      startLongitude: 0,
      endLatitude: 0,
      endLongitude: 0,
      amountToSea: '',
      amountToReception: '',
      portName: '',
      receptionFacilityName: '',
      receiptNumber: '',
      cargoDescription: '',
      holdNumbersWashed: '',
      remarks: '',
      officerInCharge: 'Chief Officer'
    });
    setEditingId(null);
  };

  return (
    <LogbookGrid
      title={t('logbooks.garbageRecord.marpolTitle')}
      actions={
        <button
          onClick={() => {
            if (showForm) {
              setShowForm(false);
              resetPartIForm();
              resetPartIIForm();
            } else {
              setShowForm(true);
            }
          }}
          className="bg-blue-600 text-white font-semibold py-2.5 px-6 rounded-lg shadow-md hover:bg-blue-700"
        >
          {showForm ? t('common.cancel') : t('logbooks.garbageRecord.newEntry')}
        </button>
      }
    >
      {/* Tab Switcher */}
      <div className="mb-6 border-b border-gray-200">
        <div className="flex gap-0">
          <button
            onClick={() => {
              setActiveTab('part-i');
              setShowForm(false);
              resetPartIForm();
              resetPartIIForm();
            }}
            className={`px-6 py-3 font-sans font-semibold text-base border-b-2 transition-colors ${
              activeTab === 'part-i'
                ? 'text-blue-600 border-blue-600 bg-blue-50'
                : 'text-gray-500 border-transparent hover:text-blue-600 hover:bg-gray-50'
            }`}
          >
            {t('logbooks.garbageRecord.regularGarbage')}
          </button>
          <button
            onClick={() => {
              setActiveTab('part-ii');
              setShowForm(false);
              resetPartIForm();
              resetPartIIForm();
            }}
            className={`px-6 py-3 font-sans font-semibold text-base border-b-2 transition-colors ${
              activeTab === 'part-ii'
                ? 'text-blue-600 border-blue-600 bg-blue-50'
                : 'text-gray-500 border-transparent hover:text-blue-600 hover:bg-gray-50'
            }`}
          >
            {t('logbooks.garbageRecord.cargoResidues')}
          </button>
        </div>
      </div>

      {/* Forms */}
      {showForm && activeTab === 'part-i' && (
        <GarbagePartIForm
          form={partIForm}
          onChange={handlePartIChange}
          onCategorySelect={handlePartICategorySelect}
          categories={PART_I_CATEGORIES}
          onSubmit={handlePartISubmit}
          onCancel={() => setShowForm(false)}
        />
      )}

      {showForm && activeTab === 'part-ii' && (
        <GarbagePartIIForm
          form={partIIForm}
          onChange={handlePartIIChange}
          onCategorySelect={handlePartIICategorySelect}
          categories={PART_II_CATEGORIES}
          onSubmit={handlePartIISubmit}
          onCancel={() => setShowForm(false)}
        />
      )}

      {/* Entries Table */}
      <div className="bg-white border border-gray-200 overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50 text-blue-600 font-sans text-sm font-semibold">
              <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.dateTime')}</th>
              <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.category')}</th>
              {activeTab === 'part-i' ? (
                <>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.intoSea')} (m³)</th>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.toReception')} (m³)</th>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.incinerated')} (m³)</th>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.location')}</th>
                </>
              ) : (
                <>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.cargoDetails')}</th>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.intoSea')} (m³)</th>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.toReception')} (m³)</th>
                  <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.location')}</th>
                </>
              )}
              <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.officer')}</th>
              <th className="p-4 border-b border-gray-200">{t('logbooks.garbageRecord.status')}</th>
              <th className="p-4 border-b border-gray-200">{t('common.action') || 'Action'}</th>
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={9} className="p-4 text-center text-green-600 font-sans">
                  {t('common.loading')}
                </td>
              </tr>
            )}
            {!loading && activeTab === 'part-i' && partIEntries.length === 0 && (
              <tr>
                <td colSpan={9} className="p-4 text-center text-gray-500 font-sans">
                  {t('logbooks.garbageRecord.noEntries')}
                </td>
              </tr>
            )}
            {!loading && activeTab === 'part-ii' && partIIEntries.length === 0 && (
              <tr>
                <td colSpan={9} className="p-4 text-center text-gray-500 font-sans">
                  {t('logbooks.garbageRecord.noEntries')}
                </td>
              </tr>
            )}

            {/* Part I Entries */}
            {activeTab === 'part-i' && partIEntries.map(entry => {
              const category = PART_I_CATEGORIES.find(c => c.code === entry.category);
              let catName = category?.name || entry.description;
              if (isVi) {
                if (entry.category === 'A') catName = 'Chất dẻo (Nhựa)';
                else if (entry.category === 'B') catName = 'Chất thải thực phẩm';
                else if (entry.category === 'C') catName = 'Chất thải sinh hoạt';
                else if (entry.category === 'D') catName = 'Dầu ăn';
                else if (entry.category === 'E') catName = 'Tro lò đốt';
                else if (entry.category === 'F') catName = 'Chất thải khai thác';
                else if (entry.category === 'G') catName = 'Dư lượng hàng hóa (không HME)';
                else if (entry.category === 'H') catName = 'Dư lượng hàng hóa (HME)';
                else if (entry.category === 'I') catName = 'Xác động vật';
              }
              const isSigned = !!entry.masterSignature;
              return (
                <tr 
                  key={entry.id} 
                  onClick={() => handleSignEntry(entry.id, 'part-i', isSigned)}
                  className={`border-b border-gray-200 hover:bg-gray-50 ${
                    !isSigned ? 'cursor-pointer hover:bg-blue-50' : ''
                  }`}
                  title={!isSigned ? t('logbooks.garbageRecord.signEntry') : t('logbooks.deckLog.signed')}
                >
                  <td className="p-4 font-sans text-gray-900 text-sm">
                    {new Date(entry.operationDate).toLocaleDateString()}
                    <br />
                    <span className="text-xs text-gray-500">{entry.operationTime}</span>
                  </td>
                  <td className="p-4 font-sans text-gray-900">
                    <span className="font-bold text-blue-600">{entry.category}</span> - {catName}
                  </td>
                  <td className="p-4 font-sans text-gray-900">
                    {entry.estimatedAmountDischargedToSea?.toFixed(3) || '-'}
                  </td>
                  <td className="p-4 font-sans text-gray-900">
                    {entry.estimatedAmountToReceptionFacilities?.toFixed(3) || '-'}
                  </td>
                  <td className="p-4 font-sans text-gray-900">
                    {entry.estimatedAmountIncinerated?.toFixed(3) || '-'}
                  </td>
                  <td className="p-4 font-sans text-gray-900 text-xs">
                    {entry.portName || (entry.dischargeLatitude && entry.dischargeLongitude 
                      ? `${entry.dischargeLatitude.toFixed(2)}°, ${entry.dischargeLongitude.toFixed(2)}°`
                      : '-')}
                  </td>
                  <td className="p-4 font-sans text-gray-900 text-sm">{entry.officerInCharge}</td>
                  <td className="p-4">
                    {entry.masterSignature ? (
                      <span className="bg-green-600 text-white text-xs px-2 py-1 font-sans font-bold">
                        {t('logbooks.deckLog.signed')}
                      </span>
                    ) : (
                      <span className="bg-yellow-600 text-black text-xs px-2 py-1 font-sans font-bold">
                        {t('logbooks.abstractLog.draft')}
                      </span>
                    )}
                  </td>
                  <td className="p-4">
                    <div className="flex gap-2">
                      {!isSigned && (
                        <>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleStartEditPartI(entry);
                            }}
                            className="text-amber-600 hover:underline font-sans text-sm font-semibold"
                          >
                            {t('common.edit') || 'EDIT'}
                          </button>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleSignEntry(entry.id, 'part-i', false);
                            }}
                            className="text-green-600 hover:underline font-sans text-sm font-semibold"
                          >
                            {t('common.sign') || 'SIGN'}
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}

            {/* Part II Entries */}
            {activeTab === 'part-ii' && partIIEntries.map(entry => {
              const category = PART_II_CATEGORIES.find(c => c.code === entry.category);
              let catName = category?.name;
              if (isVi) {
                if (entry.category === 'J') catName = 'Dư lượng hàng hóa (không HME)';
                else if (entry.category === 'K') catName = 'Dư lượng hàng hóa (HME)';
              }
              const isSigned = !!entry.masterSignature;
              return (
                <tr 
                  key={entry.id} 
                  onClick={() => handleSignEntry(entry.id, 'part-ii', isSigned)}
                  className={`border-b border-gray-200 hover:bg-gray-50 ${
                    !isSigned ? 'cursor-pointer hover:bg-blue-50' : ''
                  }`}
                  title={!isSigned ? t('logbooks.garbageRecord.signEntry') : t('logbooks.deckLog.signed')}
                >
                  <td className="p-4 font-sans text-gray-900 text-sm">
                    {new Date(entry.operationDate).toLocaleDateString()}
                    <br />
                    <span className="text-xs text-gray-500">{entry.operationTime}</span>
                  </td>
                  <td className="p-4 font-sans text-gray-900">
                    <span className={`font-bold ${entry.category === 'K' ? 'text-red-600' : 'text-blue-600'}`}>
                      {entry.category}
                    </span> - {catName}
                  </td>
                  <td className="p-4 font-sans text-gray-900 text-sm">
                    {entry.cargoDescription}
                    <br />
                    <span className="text-xs text-gray-500">{entry.holdNumbersWashed}</span>
                  </td>
                  <td className="p-4 font-sans text-gray-900">
                    {entry.estimatedAmountDischargedToSea?.toFixed(3) || '-'}
                  </td>
                  <td className="p-4 font-sans text-gray-900">
                    {entry.estimatedAmountToReceptionFacilities?.toFixed(3) || '-'}
                  </td>
                  <td className="p-4 font-sans text-gray-900 text-xs">
                    {t('logbooks.garbageRecord.start')}: {entry.startLatitude.toFixed(2)}°, {entry.startLongitude.toFixed(2)}°
                    <br />
                    {t('logbooks.garbageRecord.end')}: {entry.endLatitude.toFixed(2)}°, {entry.endLongitude.toFixed(2)}°
                  </td>
                  <td className="p-4 font-sans text-gray-900 text-sm">{entry.officerInCharge}</td>
                  <td className="p-4">
                    {entry.masterSignature ? (
                      <span className="bg-green-600 text-white text-xs px-2 py-1 font-sans font-bold">
                        {t('logbooks.deckLog.signed')}
                      </span>
                    ) : (
                      <span className="bg-yellow-600 text-black text-xs px-2 py-1 font-sans font-bold">
                        {t('logbooks.abstractLog.draft')}
                      </span>
                    )}
                  </td>
                  <td className="p-4">
                    <div className="flex gap-2">
                      {!isSigned && (
                        <>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleStartEditPartII(entry);
                            }}
                            className="text-amber-600 hover:underline font-sans text-sm font-semibold"
                          >
                            {t('common.edit') || 'EDIT'}
                          </button>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleSignEntry(entry.id, 'part-ii', false);
                            }}
                            className="text-green-600 hover:underline font-sans text-sm font-semibold"
                          >
                            {t('common.sign') || 'SIGN'}
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Sign Confirmation Modal */}
      {signModal.show && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl p-6 max-w-md w-full mx-4">
            <h3 className="text-xl font-bold text-blue-600 font-sans mb-4">
              🖊 {t('logbooks.garbageRecord.signEntry')} {signModal.type === 'part-i' ? 'Part I' : 'Part II'}
            </h3>
            <p className="text-gray-700 font-sans mb-4">
              {t('logbooks.voyageLog.signInstructions')}
            </p>
            <div className="mb-6">
              <label className="text-blue-600 font-sans text-sm font-semibold block mb-2">
                {t('logbooks.garbageRecord.masterSignature')}
              </label>
              <input
                type="text"
                value={masterSignature}
                onChange={e => setMasterSignature(e.target.value)}
                className="w-full bg-white border-2 border-gray-200 text-gray-900 font-sans p-3 focus:border-blue-500 focus:outline-none"
                placeholder={t('logbooks.garbageRecord.enterMasterName')}
                autoFocus
              />
            </div>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => {
                  setSignModal({ show: false, entryId: null, type: 'part-i' });
                  setMasterSignature('Captain');
                }}
                className="px-6 py-2.5 border-2 border-gray-300 text-gray-700 font-sans font-semibold rounded hover:bg-gray-50"
              >
                {t('common.cancel')}
              </button>
              <button
                onClick={confirmSign}
                className="px-6 py-2.5 bg-green-600 text-white font-sans font-semibold rounded hover:bg-green-700"
              >
                ✓ {t('logbooks.garbageRecord.signEntry')}
              </button>
            </div>
          </div>
        </div>
      )}
    </LogbookGrid>
  );
};
