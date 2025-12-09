import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { MaritimeInput } from '../../components/common/MaritimeInput';
import { logbookService } from '../../services/logbook.service';
import { EngineLogEntryResponseDto, CreateEngineLogEntryDto } from '../../types/logbook.types';
import { toast } from 'sonner';

export const EngineLogPage: React.FC = () => {
  const [entries, setEntries] = useState<EngineLogEntryResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [newEntry, setNewEntry] = useState<Partial<CreateEngineLogEntryDto>>({
    logDateTime: new Date().toISOString().slice(0, 16),
    mainEngineRPM: 0,
    mainEngineLoad: 0,
    mainEngineExhaustTemp: 0,
    mainEngineLubeOilPressure: 0,
    fuelOilConsumedME: 0
  });

  const fetchEntries = async () => {
    try {
      setLoading(true);
      const response = await logbookService.getEngineEntries({ page: 1, pageSize: 20 });
      setEntries(response.data);
    } catch (error) {
      console.error(error);
      toast.error('Failed to load engine log entries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEntries();
  }, []);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setNewEntry(prev => ({ 
      ...prev, 
      [name]: name === 'logDateTime' ? value : Number(value) 
    }));
  };

  const handleAdd = async () => {
    try {
      const entryToCreate: CreateEngineLogEntryDto = {
        logDateTime: new Date(newEntry.logDateTime || new Date()).toISOString(),
        watchPeriod: '08-12', // Default for now
        engineerOnWatch: 'Chief Engineer', // Default
        mainEngineStatus: 'RUNNING',
        mainEngineRPM: newEntry.mainEngineRPM,
        mainEngineLoad: newEntry.mainEngineLoad,
        mainEngineExhaustTemp: newEntry.mainEngineExhaustTemp,
        mainEngineLubeOilPressure: newEntry.mainEngineLubeOilPressure,
        fuelOilConsumedME: newEntry.fuelOilConsumedME,
      };

      await logbookService.createEngineEntry(entryToCreate);
      toast.success('Engine entry added');
      fetchEntries();
      setNewEntry({
        logDateTime: new Date().toISOString().slice(0, 16),
        mainEngineRPM: 0,
        mainEngineLoad: 0,
        mainEngineExhaustTemp: 0,
        mainEngineLubeOilPressure: 0,
        fuelOilConsumedME: 0
      });
    } catch (error) {
      console.error(error);
      toast.error('Failed to add entry');
    }
  };

  return (
    <LogbookGrid title="Engine Logbook - Main Engine Parameters">
      <div className="overflow-x-auto bg-white border border-gray-200">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50 text-blue-600 font-sans text-sm font-semibold">
              <th className="p-4 border-b border-gray-200">Time</th>
              <th className="p-4 border-b border-gray-200">RPM</th>
              <th className="p-4 border-b border-gray-200">Load (%)</th>
              <th className="p-4 border-b border-gray-200">Exh. Temp (°C)</th>
              <th className="p-4 border-b border-gray-200">Lube Press (Bar)</th>
              <th className="p-4 border-b border-gray-200">Fuel Cons (L/h)</th>
              <th className="p-4 border-b border-gray-200">Action</th>
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={7} className="p-4 text-center text-green-600 font-sans">Loading...</td>
              </tr>
            )}
            {!loading && entries.length === 0 && (
              <tr>
                <td colSpan={7} className="p-4 text-center text-gray-500 font-sans">No entries found.</td>
              </tr>
            )}
            {entries.map(entry => (
              <tr key={entry.id} className="border-b border-gray-200 hover:bg-gray-50">
                <td className="p-4 font-sans text-gray-900">{new Date(entry.logDateTime).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</td>
                <td className="p-4 font-sans text-gray-900">{entry.mainEngineRPM}</td>
                <td className="p-4 font-sans text-gray-900">{entry.mainEngineLoad}</td>
                <td className="p-4 font-sans text-gray-900">{entry.mainEngineExhaustTemp}</td>
                <td className="p-4 font-sans text-gray-900">{entry.mainEngineLubeOilPressure}</td>
                <td className="p-4 font-sans text-gray-900">{entry.fuelOilConsumedME}</td>
                <td className="p-4">
                  {entry.chiefEngineerSignature ? (
                    <span className="text-green-600 font-sans text-xs">SIGNED</span>
                  ) : (
                    <button className="text-blue-600 hover:underline font-sans text-sm">SIGN</button>
                  )}
                </td>
              </tr>
            ))}
            {/* Input Row */}
            <tr className="bg-white/5">
              <td className="p-2">
                <MaritimeInput 
                  type="datetime-local" 
                  name="logDateTime"
                  value={newEntry.logDateTime}
                  onChange={handleInputChange}
                  className="h-10 text-sm p-2 w-40" 
                />
              </td>
              <td className="p-2">
                <MaritimeInput 
                  type="number" 
                  name="mainEngineRPM"
                  value={newEntry.mainEngineRPM}
                  onChange={handleInputChange}
                  className="h-10 text-sm p-2 w-20" 
                  placeholder="RPM" 
                />
              </td>
              <td className="p-2">
                <MaritimeInput 
                  type="number" 
                  name="mainEngineLoad"
                  value={newEntry.mainEngineLoad}
                  onChange={handleInputChange}
                  className="h-10 text-sm p-2 w-20" 
                  placeholder="%" 
                />
              </td>
              <td className="p-2">
                <MaritimeInput 
                  type="number" 
                  name="mainEngineExhaustTemp"
                  value={newEntry.mainEngineExhaustTemp}
                  onChange={handleInputChange}
                  className="h-10 text-sm p-2 w-20" 
                  placeholder="°C" 
                />
              </td>
              <td className="p-2">
                <MaritimeInput 
                  type="number" 
                  name="mainEngineLubeOilPressure"
                  value={newEntry.mainEngineLubeOilPressure}
                  onChange={handleInputChange}
                  className="h-10 text-sm p-2 w-20" 
                  placeholder="Bar" 
                />
              </td>
              <td className="p-2">
                <MaritimeInput 
                  type="number" 
                  name="fuelOilConsumedME"
                  value={newEntry.fuelOilConsumedME}
                  onChange={handleInputChange}
                  className="h-10 text-sm p-2 w-20" 
                  placeholder="L/h" 
                />
              </td>
              <td className="p-2">
                <button 
                  onClick={handleAdd}
                  className="bg-blue-600 text-white px-4 py-2.5 font-semibold rounded-lg hover:bg-blue-700"
                >
                  Add
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </LogbookGrid>
  );
};




