import React from 'react';
import { MaritimeInput } from '../common/MaritimeInput';
import { CoordinatePicker } from '../common/CoordinatePicker';

interface PartIIFormProps {
  form: any;
  onChange: (field: string, value: any) => void;
  onCategorySelect: (categoryCode: string) => void;
  categories: any[];
  onSubmit: () => void;
  onCancel: () => void;
}

export const GarbagePartIIForm: React.FC<PartIIFormProps> = ({
  form,
  onChange,
  onCategorySelect,
  categories,
  onSubmit,
  onCancel
}) => {
  const selectedCategory = categories.find(c => c.code === form.category);
  const isHME = form.category === 'K';

  return (
    <div className="bg-white p-6 border border-gray-200 rounded-lg shadow-sm mb-6">
      <h2 className="text-blue-600 font-sans text-xl font-bold mb-6">
        New Part II Entry - Cargo Residues
      </h2>

      {/* Date & Time */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
        <MaritimeInput
          label="Operation Date"
          type="date"
          value={form.operationDate}
          onChange={e => onChange('operationDate', e.target.value)}
        />
        <MaritimeInput
          label="Start Time"
          type="time"
          value={form.operationTime}
          onChange={e => onChange('operationTime', e.target.value)}
        />
        <MaritimeInput
          label="Stop Time (Optional)"
          type="time"
          value={form.operationEndTime}
          onChange={e => onChange('operationEndTime', e.target.value)}
        />
      </div>

      {/* Category Selection (J or K only) */}
      <div className="mb-6">
        <label className="text-blue-600 font-sans text-sm font-semibold block mb-2">
          Cargo Residues Category *
        </label>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          {categories.map(cat => (
            <button
              type="button"
              key={cat.code}
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('=== Part II Category Button Clicked ===');
                console.log('Selected category:', cat.code);
                onCategorySelect(cat.code);
              }}
              className={`p-4 border-4 transition-all text-left cursor-pointer relative ${
                form.category === cat.code
                  ? 'border-blue-600 bg-blue-100 shadow-lg scale-105'
                  : 'border-gray-200 hover:border-blue-400 hover:shadow-md'
              }`}
            >
              {form.category === cat.code && (
                <div className="absolute top-2 right-2 bg-blue-600 text-white rounded-full w-8 h-8 flex items-center justify-center text-lg font-bold">
                  ✓
                </div>
              )}
              <div className="flex items-start justify-between">
                <div className="pr-10">
                  <div className={`font-bold font-sans text-2xl mb-1 ${
                    form.category === cat.code ? 'text-blue-700' : 'text-blue-600'
                  }`}>
                    {cat.code}
                  </div>
                  <div className={`font-sans font-semibold mb-2 ${
                    form.category === cat.code ? 'text-gray-900 font-bold' : 'text-gray-900'
                  }`}>
                    {cat.name}
                  </div>
                  <div className="text-gray-600 font-sans text-xs">
                    {cat.description}
                  </div>
                </div>
                {!cat.seaDischarge && (
                  <span className="text-red-600 text-xs font-sans border border-red-600 px-2 py-1 bg-red-50 font-bold">
                    NO SEA
                  </span>
                )}
              </div>
            </button>
          ))}
        </div>
        {selectedCategory && (
          <div className="mt-4 p-3 bg-green-50 border-2 border-green-500 rounded">
            <div className="flex items-center gap-2">
              <span className="text-green-600 text-2xl">✓</span>
              <div>
                <p className="text-green-700 font-sans font-bold">
                  Selected Category: <span className="text-xl">{selectedCategory.code}</span> - {selectedCategory.name}
                </p>
                <p className="text-green-600 text-sm mt-1 font-sans">
                  {selectedCategory.description}
                </p>
              </div>
            </div>
          </div>
        )}
        {!selectedCategory && (
          <p className="text-orange-600 text-sm mt-3 font-sans font-semibold">
            ⚠ Please select a cargo residues category (J or K) above to continue
          </p>
        )}
        {isHME && (
          <div className="mt-3 p-3 bg-red-50 border-2 border-red-500 rounded">
            <p className="text-red-700 font-sans font-bold text-sm">
              ⚠ MARPOL ANNEX V: Category K (HME) cargo residues are STRICTLY PROHIBITED from discharge to sea.
              Must be discharged to reception facilities only.
            </p>
          </div>
        )}
      </div>

      {/* Start & End Positions - MANDATORY */}
      <div className="mb-6 p-4 border-2 border-purple-500 bg-purple-50 rounded">
        <h3 className="text-purple-700 font-sans font-bold mb-4">
          Position at Start & End of Discharge (Mandatory)
        </h3>
        <div className="grid grid-cols-1 gap-6">
          {/* Start Position */}
          <div>
            <h4 className="text-purple-600 font-sans font-semibold mb-3">Start Position</h4>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <CoordinatePicker
                label="Start Latitude"
                type="latitude"
                value={form.startLatitude}
                onChange={lat => onChange('startLatitude', lat)}
              />
              <CoordinatePicker
                label="Start Longitude"
                type="longitude"
                value={form.startLongitude}
                onChange={lon => onChange('startLongitude', lon)}
              />
            </div>
          </div>

          {/* End Position */}
          <div>
            <h4 className="text-purple-600 font-sans font-semibold mb-3">End Position</h4>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <CoordinatePicker
                label="End Latitude"
                type="latitude"
                value={form.endLatitude}
                onChange={lat => onChange('endLatitude', lat)}
              />
              <CoordinatePicker
                label="End Longitude"
                type="longitude"
                value={form.endLongitude}
                onChange={lon => onChange('endLongitude', lon)}
              />
            </div>
          </div>
        </div>
      </div>

      {/* 2-Column Amounts (Sea / Reception) */}
      <div className="mb-6 p-4 border-2 border-blue-200 bg-blue-50/30 rounded">
        <h3 className="text-blue-600 font-sans font-bold mb-4">Estimated Amounts (m³)</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <MaritimeInput
              label="Into Sea"
              type="number"
              step="0.001"
              value={form.amountToSea}
              onChange={e => onChange('amountToSea', e.target.value)}
              placeholder="0.000"
              disabled={isHME}
            />
            {isHME && (
              <p className="text-red-600 text-xs mt-1 font-bold">
                Category K (HME) cannot be discharged to sea
              </p>
            )}
          </div>
          <div>
            <MaritimeInput
              label="To Reception Facilities"
              type="number"
              step="0.001"
              value={form.amountToReception}
              onChange={e => onChange('amountToReception', e.target.value)}
              placeholder="0.000"
            />
            {isHME && (
              <p className="text-green-600 text-xs mt-1 font-bold">
                Required for Category K (HME)
              </p>
            )}
          </div>
        </div>
      </div>

      {/* Conditional: Reception Facility Details */}
      {parseFloat(form.amountToReception) > 0 && (
        <div className="mb-6 p-4 border-2 border-green-500 bg-green-50 rounded">
          <h3 className="text-green-700 font-sans font-bold mb-3">Reception Facility Details</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <MaritimeInput
              label="Port Name"
              value={form.portName}
              onChange={e => onChange('portName', e.target.value)}
              placeholder="e.g., Port of Singapore"
            />
            <MaritimeInput
              label="Reception Facility Name"
              value={form.receptionFacilityName}
              onChange={e => onChange('receptionFacilityName', e.target.value)}
              placeholder="Facility name"
            />
            <MaritimeInput
              label="Receipt Number"
              value={form.receiptNumber}
              onChange={e => onChange('receiptNumber', e.target.value)}
              placeholder="Receipt #"
            />
          </div>
        </div>
      )}

      {/* Cargo Details - MANDATORY for Part II */}
      <div className="mb-6 p-4 border-2 border-indigo-500 bg-indigo-50 rounded">
        <h3 className="text-indigo-700 font-sans font-bold mb-4">Cargo Details (Mandatory)</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <MaritimeInput
            label="Cargo Description *"
            value={form.cargoDescription}
            onChange={e => onChange('cargoDescription', e.target.value)}
            placeholder="e.g., Wheat, Coal, Iron Ore"
          />
          <MaritimeInput
            label="Hold Numbers Washed *"
            value={form.holdNumbersWashed}
            onChange={e => onChange('holdNumbersWashed', e.target.value)}
            placeholder="e.g., Hold 1, 2, 3 or All Holds"
          />
        </div>
      </div>

      {/* Officer & Remarks */}
      <div className="mb-4">
        <MaritimeInput
          label="Officer In Charge *"
          value={form.officerInCharge}
          onChange={e => onChange('officerInCharge', e.target.value)}
        />
      </div>

      <div className="mb-6">
        <label className="text-blue-600 font-sans text-sm font-semibold block mb-2">
          Remarks (Optional)
        </label>
        <textarea
          value={form.remarks}
          onChange={e => onChange('remarks', e.target.value)}
          className="w-full bg-white border-2 border-gray-200 text-gray-900 font-sans p-4 focus:border-blue-500 focus:outline-none h-20 resize-none"
          placeholder="Additional notes..."
        />
      </div>

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={onCancel}
          className="px-6 py-2.5 border-2 border-gray-300 text-gray-700 font-sans font-semibold rounded hover:bg-gray-50"
        >
          Cancel
        </button>
        <button
          type="button"
          onClick={onSubmit}
          className="px-6 py-2.5 bg-green-600 text-white font-sans font-semibold rounded hover:bg-green-700"
        >
          Save Entry
        </button>
      </div>
    </div>
  );
};
