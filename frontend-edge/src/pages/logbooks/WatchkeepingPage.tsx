import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { MaritimeInput } from '../../components/common/MaritimeInput';
import { CoordinatePicker } from '../../components/common/CoordinatePicker';
import { SignaturePad } from '../../components/common/SignaturePad';
import { toast } from 'sonner';
import { logbookService } from '../../services/logbook.service';
import type { WatchkeepingLogResponseDto } from '../../types/logbook.types';

// Watch periods (4-hour watches)
const WATCH_PERIODS = [
  { code: '00-04', name: '00:00 - 04:00 (Midnight Watch)' },
  { code: '04-08', name: '04:00 - 08:00 (Morning Watch)' },
  { code: '08-12', name: '08:00 - 12:00 (Forenoon Watch)' },
  { code: '12-16', name: '12:00 - 16:00 (Afternoon Watch)' },
  { code: '16-20', name: '16:00 - 20:00 (Dog Watch)' },
  { code: '20-24', name: '20:00 - 24:00 (First Watch)' },
];

const WATCH_TYPES = [
  { value: 'NAVIGATION', label: 'Navigation Watch (Bridge)' },
  { value: 'ENGINE', label: 'Engine Watch (Engine Room)' },
];

const SEA_STATES = ['Calm', 'Slight', 'Moderate', 'Rough', 'Very Rough', 'High', 'Very High'];
const VISIBILITY_CONDITIONS = ['Excellent', 'Good', 'Moderate', 'Poor', 'Fog', 'Heavy Fog'];

export const WatchkeepingPage: React.FC = () => {
  const [entries, setEntries] = useState<WatchkeepingLogResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    watchDate: new Date().toISOString().slice(0, 10),
    watchPeriod: '08-12',
    watchType: 'NAVIGATION',
    officerOnWatch: '',
    lookout: '',
    weatherConditions: '',
    seaState: 'Moderate',
    visibility: 'Good',
    courseLogged: 0,
    speedLogged: 0,
    positionLat: 0,
    positionLon: 0,
    distanceRun: 0,
    engineStatus: '',
    notableEvents: ''
  });

  const fetchEntries = async () => {
    try {
      setLoading(true);
      const response = await logbookService.getWatchkeepingEntries({ page: 1, pageSize: 20 });
      setEntries(response.data);
    } catch (error) {
      console.error(error);
      toast.error('Failed to load watchkeeping entries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEntries();
  }, []);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    const numericFields = ['courseLogged', 'speedLogged', 'positionLat', 'positionLon', 'distanceRun'];
    setFormData(prev => ({ 
      ...prev, 
      [name]: numericFields.includes(name) ? Number(value) : value 
    }));
  };

  const handleSubmit = async () => {
    // Validation
    if (!formData.officerOnWatch) {
      toast.error('Officer on Watch is required');
      return;
    }

    try {
      await logbookService.createWatchkeepingEntry(formData);
      toast.success('Watchkeeping entry added successfully');
      fetchEntries();
      setShowForm(false);
      // Reset form
      setFormData({
        watchDate: new Date().toISOString().slice(0, 10),
        watchPeriod: '08-12',
        watchType: 'NAVIGATION',
        officerOnWatch: '',
        lookout: '',
        weatherConditions: '',
        seaState: 'Moderate',
        visibility: 'Good',
        courseLogged: 0,
        speedLogged: 0,
        positionLat: 0,
        positionLon: 0,
        distanceRun: 0,
        engineStatus: '',
        notableEvents: ''
      });
    } catch (error) {
      console.error(error);
      toast.error('Failed to create entry');
    }
  };

  const handleSign = async (id: string, signature: string) => {
    try {
      await logbookService.signWatchkeepingEntry(id, { 
        signature, 
        signedAt: new Date().toISOString() 
      });
      toast.success('Entry signed by Master');
      fetchEntries();
    } catch (error) {
      console.error(error);
      toast.error('Failed to sign entry');
    }
  };

  return (
    <LogbookGrid 
      title="Watchkeeping Log - SOLAS Chapter V/28"
      actions={
        <button
          onClick={() => setShowForm(!showForm)}
          className="bg-industrial-text-amber text-black font-bold py-2 px-6 font-mono hover:bg-yellow-500 uppercase tracking-wider"
        >
          {showForm ? 'Cancel' : '+ New Watch'}
        </button>
      }
    >
      {showForm && (
        <div className="bg-industrial-surface p-6 border-2 border-industrial-text-amber mb-6">
          <h2 className="text-industrial-text-amber font-mono text-xl mb-6 uppercase">Record Watch Details</h2>
          
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Left Column - Basic Info */}
            <div className="flex flex-col gap-4">
              <h3 className="text-industrial-text-green font-mono text-sm uppercase border-b border-gray-700 pb-2">
                Watch Information
              </h3>
              
              <MaritimeInput
                label="Watch Date"
                type="date"
                name="watchDate"
                value={formData.watchDate}
                onChange={handleInputChange}
              />

              <div>
                <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                  Watch Period
                </label>
                <select
                  name="watchPeriod"
                  value={formData.watchPeriod}
                  onChange={handleInputChange}
                  className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-4 focus:border-industrial-text-amber focus:outline-none"
                >
                  {WATCH_PERIODS.map(wp => (
                    <option key={wp.code} value={wp.code}>{wp.name}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                  Watch Type
                </label>
                <select
                  name="watchType"
                  value={formData.watchType}
                  onChange={handleInputChange}
                  className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-4 focus:border-industrial-text-amber focus:outline-none"
                >
                  {WATCH_TYPES.map(wt => (
                    <option key={wt.value} value={wt.value}>{wt.label}</option>
                  ))}
                </select>
              </div>

              <MaritimeInput
                label="Officer on Watch"
                name="officerOnWatch"
                value={formData.officerOnWatch}
                onChange={handleInputChange}
                placeholder="Name / Rank"
              />

              <MaritimeInput
                label="Lookout"
                name="lookout"
                value={formData.lookout}
                onChange={handleInputChange}
                placeholder="AB name (if applicable)"
              />
            </div>

            {/* Right Column - Navigation Data */}
            <div className="flex flex-col gap-4">
              <h3 className="text-industrial-text-green font-mono text-sm uppercase border-b border-gray-700 pb-2">
                Navigation & Weather
              </h3>

              <div className="grid grid-cols-2 gap-4">
                <MaritimeInput
                  label="Course Logged (°T)"
                  type="number"
                  name="courseLogged"
                  value={formData.courseLogged}
                  onChange={handleInputChange}
                  placeholder="090"
                />
                <MaritimeInput
                  label="Speed Logged (kts)"
                  type="number"
                  step="0.1"
                  name="speedLogged"
                  value={formData.speedLogged}
                  onChange={handleInputChange}
                  placeholder="12.5"
                />
              </div>

              <MaritimeInput
                label="Distance Run (NM)"
                type="number"
                step="0.1"
                name="distanceRun"
                value={formData.distanceRun}
                onChange={handleInputChange}
                placeholder="Nautical miles this watch"
              />

              <CoordinatePicker
                label="Position - Latitude"
                type="latitude"
                value={formData.positionLat}
                onChange={lat => setFormData({ ...formData, positionLat: lat })}
              />

              <CoordinatePicker
                label="Position - Longitude"
                type="longitude"
                value={formData.positionLon}
                onChange={lon => setFormData({ ...formData, positionLon: lon })}
              />

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                    Sea State
                  </label>
                  <select
                    name="seaState"
                    value={formData.seaState}
                    onChange={handleInputChange}
                    className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-4 focus:border-industrial-text-amber focus:outline-none"
                  >
                    {SEA_STATES.map(ss => (
                      <option key={ss} value={ss}>{ss}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                    Visibility
                  </label>
                  <select
                    name="visibility"
                    value={formData.visibility}
                    onChange={handleInputChange}
                    className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-4 focus:border-industrial-text-amber focus:outline-none"
                  >
                    {VISIBILITY_CONDITIONS.map(vc => (
                      <option key={vc} value={vc}>{vc}</option>
                    ))}
                  </select>
                </div>
              </div>

              <MaritimeInput
                label="Weather Conditions"
                name="weatherConditions"
                value={formData.weatherConditions}
                onChange={handleInputChange}
                placeholder="e.g., Clear sky, light breeze"
              />
            </div>
          </div>

          {/* Full Width - Additional Info */}
          <div className="mt-6 space-y-4">
            <h3 className="text-industrial-text-green font-mono text-sm uppercase border-b border-gray-700 pb-2">
              Additional Information
            </h3>

            {formData.watchType === 'ENGINE' && (
              <MaritimeInput
                label="Engine Status"
                name="engineStatus"
                value={formData.engineStatus}
                onChange={handleInputChange}
                placeholder="e.g., Main Engine running at 85% MCR, All systems normal"
              />
            )}

            <div>
              <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                Notable Events / Observations
              </label>
              <textarea
                name="notableEvents"
                value={formData.notableEvents}
                onChange={handleInputChange}
                className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono p-4 focus:border-industrial-text-amber focus:outline-none h-32 resize-none"
                placeholder="Record any significant events: course alterations, vessels sighted, weather changes, alarms, drills, etc."
              />
            </div>
          </div>

          {/* Submit Button */}
          <div className="flex justify-end gap-4 mt-6">
            <button
              onClick={() => setShowForm(false)}
              className="text-white font-mono underline hover:text-industrial-text-amber"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              className="bg-industrial-text-green text-black font-bold py-3 px-8 font-mono hover:bg-green-500 uppercase tracking-wider"
            >
              Save Watch Entry
            </button>
          </div>
        </div>
      )}

      {/* Entries Table */}
      <div className="bg-industrial-surface border border-industrial-border overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-black text-industrial-text-amber font-mono text-sm uppercase">
              <th className="p-4 border-b border-industrial-border">Date</th>
              <th className="p-4 border-b border-industrial-border">Watch</th>
              <th className="p-4 border-b border-industrial-border">Type</th>
              <th className="p-4 border-b border-industrial-border">OOW</th>
              <th className="p-4 border-b border-industrial-border">Position</th>
              <th className="p-4 border-b border-industrial-border">C/S</th>
              <th className="p-4 border-b border-industrial-border">Weather</th>
              <th className="p-4 border-b border-industrial-border">Status</th>
              <th className="p-4 border-b border-industrial-border">Action</th>
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={9} className="p-4 text-center text-industrial-text-green font-mono">
                  Loading...
                </td>
              </tr>
            )}
            {!loading && entries.length === 0 && (
              <tr>
                <td colSpan={9} className="p-4 text-center text-gray-500 font-mono">
                  No watchkeeping entries recorded. Click "+ New Watch" to start logging.
                </td>
              </tr>
            )}
            {entries.map(entry => (
              <tr key={entry.id} className="border-b border-industrial-border hover:bg-white/5">
                <td className="p-4 font-mono text-white">{new Date(entry.watchDate).toLocaleDateString()}</td>
                <td className="p-4 font-mono text-white">{entry.watchPeriod}</td>
                <td className="p-4 font-mono text-white">
                  <span className={`px-2 py-1 text-xs ${entry.watchType === 'NAVIGATION' ? 'bg-blue-600' : 'bg-orange-600'}`}>
                    {entry.watchType}
                  </span>
                </td>
                <td className="p-4 font-mono text-white">{entry.officerOnWatch}</td>
                <td className="p-4 font-mono text-white text-xs">
                  {entry.positionLat?.toFixed(4)}, {entry.positionLon?.toFixed(4)}
                </td>
                <td className="p-4 font-mono text-white">
                  {entry.courseLogged}° / {entry.speedLogged} kts
                </td>
                <td className="p-4 font-mono text-white text-xs">
                  {entry.seaState} / {entry.visibility}
                </td>
                <td className="p-4">
                  {entry.masterSignature ? (
                    <span className="bg-industrial-text-green text-black text-xs px-2 py-1 font-mono font-bold">
                      SIGNED
                    </span>
                  ) : (
                    <span className="bg-yellow-600 text-black text-xs px-2 py-1 font-mono font-bold">
                      DRAFT
                    </span>
                  )}
                </td>
                <td className="p-4">
                  {!entry.masterSignature && (
                    <button
                      onClick={() => {/* Show signature modal */}}
                      className="text-industrial-text-amber hover:underline font-mono text-sm"
                    >
                      SIGN
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Notable Events Display */}
      {entries.length > 0 && (
        <div className="mt-6">
          <h3 className="text-industrial-text-amber font-mono text-lg mb-4 uppercase">Recent Notable Events</h3>
          <div className="space-y-4">
            {entries.filter(e => e.notableEvents).slice(0, 5).map(entry => (
              <div key={entry.id} className="bg-industrial-surface p-4 border border-industrial-border">
                <div className="flex justify-between items-start mb-2">
                  <span className="text-industrial-text-green font-mono text-sm">
                    {new Date(entry.watchDate).toLocaleDateString()} - {entry.watchPeriod}
                  </span>
                  <span className="text-gray-400 font-mono text-xs">{entry.officerOnWatch}</span>
                </div>
                <p className="text-white font-mono text-sm">{entry.notableEvents}</p>
              </div>
            ))}
          </div>
        </div>
      )}
    </LogbookGrid>
  );
};
