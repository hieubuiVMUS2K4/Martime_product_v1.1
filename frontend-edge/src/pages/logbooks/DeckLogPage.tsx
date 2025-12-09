import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { MaritimeInput } from '../../components/common/MaritimeInput';
import { SignaturePad } from '../../components/common/SignaturePad';
import { logbookService } from '../../services/logbook.service';
import { DeckLogEntryResponseDto, CreateDeckLogEntryDto } from '../../types/logbook.types';
import { toast } from 'sonner';

export const DeckLogPage: React.FC = () => {
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
      toast.error('Failed to load deck log entries');
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
      toast.success('Entry added successfully');
      fetchEntries();
      setFormData(prev => ({
        ...prev,
        logDateTime: new Date().toISOString().slice(0, 16),
        description: '',
        remarks: ''
      }));
    } catch (error) {
      console.error(error);
      toast.error('Failed to create entry');
    }
  };

  const handleSign = async (id: string, signature: string) => {
    try {
      await logbookService.signDeckEntry(id, {
        signature,
        signedAt: new Date().toISOString()
      });
      toast.success('Entry signed successfully');
      fetchEntries();
    } catch (error) {
      console.error(error);
      toast.error('Failed to sign entry');
    }
  };

  return (
    <LogbookGrid title="Deck Logbook - SOLAS Chapter V">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Input Form */}
        <div className="lg:col-span-1 bg-industrial-surface p-6 border border-industrial-border">
          <h2 className="text-industrial-text-amber font-mono text-xl mb-4 uppercase">New Entry</h2>
          <div className="flex flex-col gap-4">
            <MaritimeInput 
              label="Date & Time (UTC)" 
              type="datetime-local" 
              name="logDateTime"
              value={formData.logDateTime}
              onChange={handleInputChange}
            />
            <div className="grid grid-cols-2 gap-4">
              <MaritimeInput 
                label="Watch Period" 
                placeholder="00-04"
                name="watchPeriod"
                value={formData.watchPeriod}
                onChange={handleInputChange}
              />
              <MaritimeInput 
                label="OOW" 
                placeholder="Officer Name"
                name="officerOnWatch"
                value={formData.officerOnWatch}
                onChange={handleInputChange}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <MaritimeInput 
                label="Latitude" 
                type="number"
                placeholder="0.0000"
                name="latitude"
                value={formData.latitude}
                onChange={handleInputChange}
              />
              <MaritimeInput 
                label="Longitude" 
                type="number"
                placeholder="0.0000"
                name="longitude"
                value={formData.longitude}
                onChange={handleInputChange}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <MaritimeInput 
                label="COG (°)" 
                type="number" 
                placeholder="090"
                name="courseOverGround"
                value={formData.courseOverGround}
                onChange={handleInputChange}
              />
              <MaritimeInput 
                label="SOG (kts)" 
                type="number" 
                placeholder="12.5"
                name="speedOverGround"
                value={formData.speedOverGround}
                onChange={handleInputChange}
              />
            </div>
            <MaritimeInput 
              label="Description / Event" 
              placeholder="Noon position report..."
              name="description"
              value={formData.description}
              onChange={handleInputChange}
            />
            <button 
              onClick={handleSubmit}
              className="bg-industrial-text-amber text-black font-bold py-3 mt-4 font-mono hover:bg-yellow-500 uppercase tracking-wider"
            >
              Add Entry
            </button>
          </div>
        </div>

        {/* Log Entries Timeline */}
        <div className="lg:col-span-2 flex flex-col gap-4">
          {loading && <div className="text-industrial-text-green font-mono text-center">Loading entries...</div>}
          {!loading && entries.length === 0 && (
            <div className="text-gray-500 font-mono text-center py-10">No entries yet. Start logging above.</div>
          )}
          {entries.map(entry => (
            <div key={entry.id} className="bg-industrial-surface border border-industrial-border p-4 flex flex-col gap-2 relative">
              <div className="flex justify-between items-start">
                <span className="text-industrial-text-green font-mono text-lg">{new Date(entry.logDateTime).toLocaleString()}</span>
                {entry.masterSignature ? (
                  <span className="bg-industrial-text-green text-black text-xs px-2 py-1 font-mono font-bold">SIGNED</span>
                ) : (
                  <span className="bg-yellow-600 text-black text-xs px-2 py-1 font-mono font-bold">DRAFT</span>
                )}
              </div>
              <div className="grid grid-cols-3 gap-4 text-sm font-mono text-gray-400 mt-2">
                <div>POS: <span className="text-white">{entry.latitude?.toFixed(4)}, {entry.longitude?.toFixed(4)}</span></div>
                <div>COG: <span className="text-white">{entry.courseOverGround}°</span></div>
                <div>SOG: <span className="text-white">{entry.speedOverGround} kts</span></div>
              </div>
              <div className="text-sm font-mono text-gray-400">
                OOW: <span className="text-white">{entry.officerOnWatch}</span> | Watch: <span className="text-white">{entry.watchPeriod}</span>
              </div>
              <p className="text-white font-mono mt-2 border-t border-gray-700 pt-2">{entry.description}</p>
              
              {!entry.masterSignature && (
                <div className="mt-4 border-t border-gray-700 pt-4">
                  <SignaturePad onSign={(sig) => handleSign(entry.id, sig)} label="Sign this Entry" />
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </LogbookGrid>
  );
};
