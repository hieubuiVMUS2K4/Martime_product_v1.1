import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { MaritimeInput } from '../../components/common/MaritimeInput';
import { SignaturePad } from '../../components/common/SignaturePad';
import { logbookService } from '../../services/logbook.service';
import { DeckLogEntryResponseDto, CreateDeckLogEntryDto } from '../../types/logbook.types';
import { toast } from 'sonner';
import { useTranslationSafe } from '@/contexts/I18nContext';

export const DeckLogPage: React.FC = () => {
  const { t } = useTranslationSafe();
  const [entries, setEntries] = useState<DeckLogEntryResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<CreateDeckLogEntryDto>({
    logDateTime: new Date().toISOString().slice(0, 16),
    watchPeriod: '00-04',
    officerOnWatch: '',
    entryType: 'ROUTINE',
    description: '',
    latitude: 0,
    longitude: 0,
    courseOverGround: 0,
    speedOverGround: 0,
    remarks: ''
  });

  const fetchEntries = async () => {
    try {
      setLoading(true);
      const response = await logbookService.getDeckEntries({ page: 1, pageSize: 20 });
      setEntries(response.data);
    } catch (error) {
      console.error(error);
      toast.error(t('logbooks.deckLog.loadFailed'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEntries();
  }, []);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ 
      ...prev, 
      [name]: ['latitude', 'longitude', 'courseOverGround', 'speedOverGround'].includes(name) ? Number(value) : value 
    }));
  };

  const handleSubmit = async () => {
    try {
      await logbookService.createDeckEntry({
        ...formData,
        logDateTime: new Date(formData.logDateTime).toISOString()
      });
      toast.success(t('logbooks.deckLog.entryAdded'));
      fetchEntries();
      setFormData(prev => ({
        ...prev,
        logDateTime: new Date().toISOString().slice(0, 16),
        description: '',
        remarks: ''
      }));
    } catch (error) {
      console.error(error);
      toast.error(t('logbooks.deckLog.createFailed'));
    }
  };

  const handleSign = async (id: string, signature: string) => {
    try {
      await logbookService.signDeckEntry(id, {
        signature,
        signedAt: new Date().toISOString()
      });
      toast.success(t('logbooks.common.signSuccess'));
      fetchEntries();
    } catch (error) {
      console.error(error);
      toast.error(t('logbooks.common.signFailed'));
    }
  };

  return (
    <LogbookGrid title={t('logbooks.deckLog.title')}>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Input Form */}
        <div className="lg:col-span-1 bg-white dark:bg-gray-800 p-6 border border-gray-200 dark:border-gray-700 rounded-lg shadow-sm">
          <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">{t('logbooks.deckLog.newEntry')}</h2>
          <div className="flex flex-col gap-4">
            <MaritimeInput 
              label={t('logbooks.deckLog.dateTimeUtc')} 
              type="datetime-local" 
              name="logDateTime"
              value={formData.logDateTime}
              onChange={handleInputChange}
            />
            <div className="grid grid-cols-2 gap-4">
              <MaritimeInput 
                label={t('logbooks.deckLog.watchPeriod')} 
                placeholder="00-04"
                name="watchPeriod"
                value={formData.watchPeriod}
                onChange={handleInputChange}
              />
              <MaritimeInput 
                label={t('logbooks.deckLog.officerOnWatch')} 
                placeholder="Officer Name"
                name="officerOnWatch"
                value={formData.officerOnWatch}
                onChange={handleInputChange}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <MaritimeInput 
                label={t('logbooks.deckLog.latitude')} 
                type="number"
                placeholder="0.0000"
                name="latitude"
                value={formData.latitude}
                onChange={handleInputChange}
              />
              <MaritimeInput 
                label={t('logbooks.deckLog.longitude')} 
                type="number"
                placeholder="0.0000"
                name="longitude"
                value={formData.longitude}
                onChange={handleInputChange}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <MaritimeInput 
                label={t('logbooks.deckLog.courseOverGround')} 
                type="number" 
                placeholder="090"
                name="courseOverGround"
                value={formData.courseOverGround}
                onChange={handleInputChange}
              />
              <MaritimeInput 
                label={t('logbooks.deckLog.speedOverGround')} 
                type="number" 
                placeholder="12.5"
                name="speedOverGround"
                value={formData.speedOverGround}
                onChange={handleInputChange}
              />
            </div>
            <MaritimeInput 
              label={t('logbooks.deckLog.description')} 
              placeholder="Noon position report..."
              name="description"
              value={formData.description}
              onChange={handleInputChange}
            />
            <button 
              onClick={handleSubmit}
              className="bg-blue-600 text-white font-semibold py-2 px-4 rounded-lg hover:bg-blue-700 transition-colors"
            >
              {t('logbooks.deckLog.addEntry')}
            </button>
          </div>
        </div>

        {/* Log Entries Timeline */}
        <div className="lg:col-span-2 flex flex-col gap-4">
          {loading && <div className="text-gray-600 dark:text-gray-400 text-center">{t('logbooks.common.loading')}</div>}
          {!loading && entries.length === 0 && (
            <div className="text-gray-500 dark:text-gray-400 text-center py-10">{t('logbooks.deckLog.noEntries')}</div>
          )}
          {entries.map(entry => (
            <div key={entry.id} className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-4 flex flex-col gap-2 relative shadow-sm">
              <div className="flex justify-between items-start">
                <span className="text-blue-600 dark:text-blue-400 font-semibold text-lg">{new Date(entry.logDateTime).toLocaleString()}</span>
                {entry.masterSignature ? (
                  <span className="bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 text-xs px-2 py-1 rounded font-semibold">{t('logbooks.deckLog.signed')}</span>
                ) : (
                  <span className="bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200 text-xs px-2 py-1 rounded font-semibold">{t('voyageLog.draft')}</span>
                )}
              </div>
              <div className="grid grid-cols-3 gap-4 text-sm text-gray-600 dark:text-gray-400 mt-2">
                <div>POS: <span className="text-gray-900 dark:text-white font-medium">{entry.latitude?.toFixed(4)}, {entry.longitude?.toFixed(4)}</span></div>
                <div>COG: <span className="text-gray-900 dark:text-white font-medium">{entry.courseOverGround}°</span></div>
                <div>SOG: <span className="text-gray-900 dark:text-white font-medium">{entry.speedOverGround} kts</span></div>
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                OOW: <span className="text-gray-900 dark:text-white font-medium">{entry.officerOnWatch}</span> | {t('logbooks.deckLog.watchPeriod')}: <span className="text-gray-900 dark:text-white font-medium">{entry.watchPeriod}</span>
              </div>
              <p className="text-gray-900 dark:text-white mt-2 border-t border-gray-200 dark:border-gray-700 pt-2">{entry.description}</p>
              
              {!entry.masterSignature && (
                <div className="mt-4 border-t border-gray-200 dark:border-gray-700 pt-4">
                  <SignaturePad onSign={(sig) => handleSign(entry.id, sig)} label={t('logbooks.deckLog.sign')} />
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </LogbookGrid>
  );
};
