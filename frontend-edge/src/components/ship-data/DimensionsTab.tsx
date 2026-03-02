import type { SaveShipData, ShipLoadLine, ShipPilotCardData } from '@/types/ship-data.types';
import { SectionCard, FormField, MetricField, CheckboxField, DynamicListHeader, DynamicListDelete } from './ShipDataFields';

interface DimensionsTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
  onChildChange: <T>(collection: string, index: number, field: keyof T, value: any) => void;
  onChildAdd: (collection: string, defaultItem: any) => void;
  onChildRemove: (collection: string, index: number) => void;
}

const LOAD_LINE_TYPES = [
  'Tropical (T)', 'Summer (S)', 'Winter (W)', 'Winter North Atlantic (WNA)',
  'Tropical Fresh Water (TF)', 'Fresh Water (F)',
];

const ENGINE_ORDERS = [
  'Dead Slow Ahead', 'Slow Ahead', 'Half Ahead', 'Full Ahead',
  'Navigation Full', 'Sea Speed', 'Dead Slow Astern', 'Slow Astern',
  'Half Astern', 'Full Astern',
];

export function DimensionsTab({ data, onChange, onChildChange, onChildAdd, onChildRemove }: DimensionsTabProps) {
  const numSet = (field: string) => (value: number | undefined) => onChange(field, value);
  const strSet = (field: string) => (value: string) => {
    onChange(field, value ? parseFloat(value) : undefined);
  };

  return (
    <div className="space-y-4">
      {/* Principal Dimensions */}
      <SectionCard title="Principal Dimensions">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <MetricField label="LOA" valueM={data.loa} onChange={numSet('loa')} />
          <MetricField label="LBP" valueM={data.lbp} onChange={numSet('lbp')} />
          <MetricField label="Breadth Moulded" valueM={data.breadthMoulded} onChange={numSet('breadthMoulded')} />
          <MetricField label="Depth Moulded" valueM={data.depthMoulded} onChange={numSet('depthMoulded')} />
          <MetricField label="Draft Moulded" valueM={data.draftMoulded} onChange={numSet('draftMoulded')} />
          <MetricField label="Draft Scantling" valueM={data.draftScantling} onChange={numSet('draftScantling')} />
          <MetricField label="Draft Full Ballast" valueM={data.draftFullBallast} onChange={numSet('draftFullBallast')} />
          <MetricField label="H Max. Airdraft" valueM={data.hMaxAirdraft} onChange={numSet('hMaxAirdraft')} />
          <FormField label="Airdraft Reduction (Mast Fouled)" value={data.airdraftReductionMastFouled} onChange={strSet('airdraftReductionMastFouled')} type="number" suffix="m" />
          <MetricField label="D-Distance" valueM={data.dDistance} onChange={numSet('dDistance')} />
          <MetricField label="Bridge to Aft" valueM={data.bridgeToAft} onChange={numSet('bridgeToAft')} />
          <MetricField label="Bridge to Bow" valueM={data.bridgeToBow} onChange={numSet('bridgeToBow')} />
          <MetricField label="Bow to Bulbous Bow" valueM={data.bowToBulbousBow} onChange={numSet('bowToBulbousBow')} />
          <MetricField label="Parallel Body (Ballast)" valueM={data.parallelBodyBallast} onChange={numSet('parallelBodyBallast')} />
          <MetricField label="Parallel Body (Loaded)" valueM={data.parallelBodyLoaded} onChange={numSet('parallelBodyLoaded')} />
        </div>
      </SectionCard>

      {/* Displacement & Coefficients */}
      <SectionCard title="Displacement & Coefficients">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label="Light Ship" value={data.lightShip} onChange={strSet('lightShip')} type="number" suffix="MT" />
          <div className="flex items-end gap-2">
            <FormField label="Block Coefficient" value={data.blockCoefficient} onChange={strSet('blockCoefficient')} type="number" className="flex-1" />
            <CheckboxField label="N/A" checked={data.blockCoefficientNA} onChange={(v) => onChange('blockCoefficientNA', v)} />
          </div>
          <FormField label="TPC at Summer Draft" value={data.tpcAtSummerDraft} onChange={strSet('tpcAtSummerDraft')} type="number" suffix="MT/cm" />
          <FormField label="FWA" value={data.freshWaterAllowanceFwa} onChange={strSet('freshWaterAllowanceFwa')} type="number" suffix="mm" />
        </div>
      </SectionCard>

      {/* Tonnage */}
      <SectionCard title="Tonnage">
        <div className="grid grid-cols-3 md:grid-cols-3 gap-3">
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">International</div>
            <div className="space-y-2">
              <FormField label="Gross Tonnage" value={data.grossTonnageInternational} onChange={strSet('grossTonnageInternational')} type="number" />
              <FormField label="Nett Tonnage" value={data.nettTonnageInternational} onChange={strSet('nettTonnageInternational')} type="number" />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">Suez Canal</div>
            <div className="space-y-2">
              <FormField label="Gross Tonnage" value={data.grossTonnageSuezCanal} onChange={strSet('grossTonnageSuezCanal')} type="number" />
              <FormField label="Nett Tonnage" value={data.nettTonnageSuezCanal} onChange={strSet('nettTonnageSuezCanal')} type="number" />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">Panama Canal</div>
            <div className="space-y-2">
              <FormField label="Gross Tonnage" value={data.grossTonnagePanamaCanal} onChange={strSet('grossTonnagePanamaCanal')} type="number" />
              <FormField label="Nett Tonnage" value={data.nettTonnagePanamaCanal} onChange={strSet('nettTonnagePanamaCanal')} type="number" />
            </div>
          </div>
        </div>
      </SectionCard>

      {/* Load Lines */}
      <SectionCard
        title="Load Lines"
        headerAction={
          <DynamicListHeader
            label="Add Load Line"
            onAdd={() => onChildAdd('loadLines', { loadLineType: '', draftM: undefined, freeboardM: undefined, displacementMt: undefined, deadweightMt: undefined, sortOrder: data.loadLines.length })}
          />
        }
      >
        {data.loadLines.length === 0 ? (
          <p className="text-sm text-gray-400 italic">No load lines added. Click "Add Load Line" to begin.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 text-left">
                  <th className="py-2 px-2 text-xs font-medium text-gray-500 w-48">Type</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">Draft (m)</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">Freeboard (m)</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">Displacement (MT)</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">Deadweight (MT)</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500 w-10"></th>
                </tr>
              </thead>
              <tbody>
                {data.loadLines.map((line, i) => (
                  <tr key={i} className="border-b border-gray-100">
                    <td className="py-1 px-1">
                      <select
                        value={line.loadLineType ?? ''}
                        onChange={(e) => onChildChange<ShipLoadLine>('loadLines', i, 'loadLineType', e.target.value)}
                        className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded"
                      >
                        <option value="">Select...</option>
                        {LOAD_LINE_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                      </select>
                    </td>
                    <td className="py-1 px-1">
                      <input type="number" step="0.01" value={line.draftM ?? ''} onChange={(e) => onChildChange<ShipLoadLine>('loadLines', i, 'draftM', e.target.value ? parseFloat(e.target.value) : undefined)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded" />
                    </td>
                    <td className="py-1 px-1">
                      <input type="number" step="0.01" value={line.freeboardM ?? ''} onChange={(e) => onChildChange<ShipLoadLine>('loadLines', i, 'freeboardM', e.target.value ? parseFloat(e.target.value) : undefined)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded" />
                    </td>
                    <td className="py-1 px-1">
                      <input type="number" step="0.01" value={line.displacementMt ?? ''} onChange={(e) => onChildChange<ShipLoadLine>('loadLines', i, 'displacementMt', e.target.value ? parseFloat(e.target.value) : undefined)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded" />
                    </td>
                    <td className="py-1 px-1">
                      <input type="number" step="0.01" value={line.deadweightMt ?? ''} onChange={(e) => onChildChange<ShipLoadLine>('loadLines', i, 'deadweightMt', e.target.value ? parseFloat(e.target.value) : undefined)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded" />
                    </td>
                    <td className="py-1 px-1"><DynamicListDelete onDelete={() => onChildRemove('loadLines', i)} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </SectionCard>

      {/* Tanker-specific (Manifold Data) */}
      <SectionCard title="Tanker-Specific Manifold Data">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <MetricField label="Manifold to Waterline (Ballast)" valueM={data.manifoldToWaterlineBallast} onChange={numSet('manifoldToWaterlineBallast')} />
          <MetricField label="Manifold to Waterline (Loaded)" valueM={data.manifoldToWaterlineLoaded} onChange={numSet('manifoldToWaterlineLoaded')} />
          <MetricField label="Deck to Manifold" valueM={data.deckToManifold} onChange={numSet('deckToManifold')} />
          <MetricField label="Stern to Manifold" valueM={data.sternToManifold} onChange={numSet('sternToManifold')} />
          <MetricField label="Shipside to Manifold" valueM={data.shipsideToManifold} onChange={numSet('shipsideToManifold')} />
          <MetricField label="Bow to Manifold" valueM={data.bowToManifold} onChange={numSet('bowToManifold')} />
          <MetricField label="Manifold to Keel" valueM={data.manifoldToKeel} onChange={numSet('manifoldToKeel')} />
          <MetricField label="Manifold to Bridge" valueM={data.manifoldToBridge} onChange={numSet('manifoldToBridge')} />
          <FormField label="Max Loading Rate (ship)" value={data.maxLoadingRateShip} onChange={strSet('maxLoadingRateShip')} type="number" suffix="m³/h" />
          <FormField label="Number of Lines" value={data.numberOfLines} onChange={(v) => onChange('numberOfLines', v ? parseInt(v) : undefined)} type="number" />
          <FormField label="Max Allowable Pressure" value={data.maxAllowablePressurePsi} onChange={strSet('maxAllowablePressurePsi')} type="number" suffix="PSI" />
          <FormField label="Venting System (ship)" value={data.ventingSystemShip} onChange={(v) => onChange('ventingSystemShip', v)} />
        </div>
      </SectionCard>

      {/* Pilot Card Data */}
      <SectionCard
        title="Pilot Card - Speed Table"
        headerAction={
          <DynamicListHeader
            label="Add Row"
            onAdd={() => onChildAdd('pilotCardData', { engineOrder: '', mainEngineRPM: undefined, speedLoadedKts: undefined, speedBallastKts: undefined, sortOrder: data.pilotCardData.length })}
          />
        }
      >
        {data.pilotCardData.length === 0 ? (
          <p className="text-sm text-gray-400 italic">No pilot card data. Click "Add Row" to begin.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 text-left">
                  <th className="py-2 px-2 text-xs font-medium text-gray-500 w-48">Engine Order</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">M/E RPM</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">Speed Loaded (kts)</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">Speed Ballast (kts)</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500 w-10"></th>
                </tr>
              </thead>
              <tbody>
                {data.pilotCardData.map((row, i) => (
                  <tr key={i} className="border-b border-gray-100">
                    <td className="py-1 px-1">
                      <select
                        value={row.engineOrder ?? ''}
                        onChange={(e) => onChildChange<ShipPilotCardData>('pilotCardData', i, 'engineOrder', e.target.value)}
                        className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded"
                      >
                        <option value="">Select...</option>
                        {ENGINE_ORDERS.map(o => <option key={o} value={o}>{o}</option>)}
                      </select>
                    </td>
                    <td className="py-1 px-1">
                      <input type="number" value={row.mainEngineRPM ?? ''} onChange={(e) => onChildChange<ShipPilotCardData>('pilotCardData', i, 'mainEngineRPM', e.target.value ? parseFloat(e.target.value) : undefined)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded" />
                    </td>
                    <td className="py-1 px-1">
                      <input type="number" step="0.1" value={row.speedLoadedKts ?? ''} onChange={(e) => onChildChange<ShipPilotCardData>('pilotCardData', i, 'speedLoadedKts', e.target.value ? parseFloat(e.target.value) : undefined)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded" />
                    </td>
                    <td className="py-1 px-1">
                      <input type="number" step="0.1" value={row.speedBallastKts ?? ''} onChange={(e) => onChildChange<ShipPilotCardData>('pilotCardData', i, 'speedBallastKts', e.target.value ? parseFloat(e.target.value) : undefined)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded" />
                    </td>
                    <td className="py-1 px-1"><DynamicListDelete onDelete={() => onChildRemove('pilotCardData', i)} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </SectionCard>
    </div>
  );
}
