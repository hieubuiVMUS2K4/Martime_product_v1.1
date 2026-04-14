import type { SaveShipData, ShipLoadLine, ShipPilotCardData } from '@/types/ship-data.types';
import { SectionCard, FormField, MetricField, CheckboxField, DynamicListHeader, DynamicListDelete } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

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
  const { t } = useTranslationSafe();
  const numSet = (field: string) => (value: number | undefined) => onChange(field, value);
  const strSet = (field: string) => (value: string) => {
    onChange(field, value ? parseFloat(value) : undefined);
  };

  return (
    <div className="space-y-4">
      {/* Principal Dimensions */}
      <SectionCard title={t('shipData.dimensions.principalDimensions')}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <MetricField label={t('shipData.dimensions.loa')} valueM={data.loa} onChange={numSet('loa')} />
          <MetricField label={t('shipData.dimensions.lbp')} valueM={data.lbp} onChange={numSet('lbp')} />
          <MetricField label={t('shipData.dimensions.breadthMoulded')} valueM={data.breadthMoulded} onChange={numSet('breadthMoulded')} />
          <MetricField label={t('shipData.dimensions.depthMoulded')} valueM={data.depthMoulded} onChange={numSet('depthMoulded')} />
          <MetricField label={t('shipData.dimensions.draftMoulded')} valueM={data.draftMoulded} onChange={numSet('draftMoulded')} />
          <MetricField label={t('shipData.dimensions.draftScantling')} valueM={data.draftScantling} onChange={numSet('draftScantling')} />
          <MetricField label={t('shipData.dimensions.draftFullBallast')} valueM={data.draftFullBallast} onChange={numSet('draftFullBallast')} />
          <MetricField label={t('shipData.dimensions.hMaxAirdraft')} valueM={data.hMaxAirdraft} onChange={numSet('hMaxAirdraft')} />
          <FormField label={t('shipData.dimensions.airdraftReductionMastFouled')} value={data.airdraftReductionMastFouled} onChange={strSet('airdraftReductionMastFouled')} type="number" suffix="m" />
          <MetricField label={t('shipData.dimensions.dDistance')} valueM={data.dDistance} onChange={numSet('dDistance')} />
          <MetricField label={t('shipData.dimensions.bridgeToAft')} valueM={data.bridgeToAft} onChange={numSet('bridgeToAft')} />
          <MetricField label={t('shipData.dimensions.bridgeToBow')} valueM={data.bridgeToBow} onChange={numSet('bridgeToBow')} />
          <MetricField label={t('shipData.dimensions.bowToBulbousBow')} valueM={data.bowToBulbousBow} onChange={numSet('bowToBulbousBow')} />
          <MetricField label={t('shipData.dimensions.parallelBodyBallast')} valueM={data.parallelBodyBallast} onChange={numSet('parallelBodyBallast')} />
          <MetricField label={t('shipData.dimensions.parallelBodyLoaded')} valueM={data.parallelBodyLoaded} onChange={numSet('parallelBodyLoaded')} />
        </div>
      </SectionCard>

      {/* Displacement & Coefficients */}
      <SectionCard title={t('shipData.dimensions.displacementCoefficients')}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label={t('shipData.dimensions.lightShip')} value={data.lightShip} onChange={strSet('lightShip')} type="number" suffix="MT" />
          <div className="flex items-end gap-2">
            <FormField label={t('shipData.dimensions.blockCoefficient')} value={data.blockCoefficient} onChange={strSet('blockCoefficient')} type="number" className="flex-1" />
            <CheckboxField label={t('shipData.common.na')} checked={data.blockCoefficientNA} onChange={(v) => onChange('blockCoefficientNA', v)} />
          </div>
          <FormField label={t('shipData.dimensions.tpcAtSummerDraft')} value={data.tpcAtSummerDraft} onChange={strSet('tpcAtSummerDraft')} type="number" suffix="MT/cm" />
          <FormField label={t('shipData.dimensions.fwa')} value={data.freshWaterAllowanceFwa} onChange={strSet('freshWaterAllowanceFwa')} type="number" suffix="mm" />
        </div>
      </SectionCard>

      {/* Tonnage */}
      <SectionCard title={t('shipData.dimensions.tonnage')}>
        <div className="grid grid-cols-3 md:grid-cols-3 gap-3">
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">{t('shipData.dimensions.international')}</div>
            <div className="space-y-2">
              <FormField label={t('shipData.dimensions.grossTonnage')} value={data.grossTonnageInternational} onChange={strSet('grossTonnageInternational')} type="number" />
              <FormField label={t('shipData.dimensions.nettTonnage')} value={data.nettTonnageInternational} onChange={strSet('nettTonnageInternational')} type="number" />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">{t('shipData.dimensions.suezCanal')}</div>
            <div className="space-y-2">
              <FormField label={t('shipData.dimensions.grossTonnage')} value={data.grossTonnageSuezCanal} onChange={strSet('grossTonnageSuezCanal')} type="number" />
              <FormField label={t('shipData.dimensions.nettTonnage')} value={data.nettTonnageSuezCanal} onChange={strSet('nettTonnageSuezCanal')} type="number" />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">{t('shipData.dimensions.panamaCanal')}</div>
            <div className="space-y-2">
              <FormField label={t('shipData.dimensions.grossTonnage')} value={data.grossTonnagePanamaCanal} onChange={strSet('grossTonnagePanamaCanal')} type="number" />
              <FormField label={t('shipData.dimensions.nettTonnage')} value={data.nettTonnagePanamaCanal} onChange={strSet('nettTonnagePanamaCanal')} type="number" />
            </div>
          </div>
        </div>
      </SectionCard>

      {/* Load Lines */}
      <SectionCard
        title={t('shipData.dimensions.loadLines')}
        headerAction={
          <DynamicListHeader
            label={t('shipData.dimensions.addLoadLine')}
            onAdd={() => onChildAdd('loadLines', { loadLineType: '', draftM: undefined, freeboardM: undefined, displacementMt: undefined, deadweightMt: undefined, sortOrder: data.loadLines.length })}
          />
        }
      >
        {data.loadLines.length === 0 ? (
          <p className="text-sm text-gray-400 italic">{t('shipData.dimensions.noLoadLines')}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 text-left">
                  <th className="py-2 px-2 text-xs font-medium text-gray-500 w-48">{t('shipData.dimensions.type')}</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">{t('shipData.dimensions.draftM')}</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">{t('shipData.dimensions.freeboardM')}</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">{t('shipData.dimensions.displacementMT')}</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">{t('shipData.dimensions.deadweightMT')}</th>
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
                      <option value="">{t('shipData.common.select')}</option>
                        {LOAD_LINE_TYPES.map(lt => <option key={lt} value={lt}>{lt}</option>)}
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
      <SectionCard title={t('shipData.dimensions.tankerManifold')}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <MetricField label={t('shipData.dimensions.manifoldToWaterlineBallast')} valueM={data.manifoldToWaterlineBallast} onChange={numSet('manifoldToWaterlineBallast')} />
          <MetricField label={t('shipData.dimensions.manifoldToWaterlineLoaded')} valueM={data.manifoldToWaterlineLoaded} onChange={numSet('manifoldToWaterlineLoaded')} />
          <MetricField label={t('shipData.dimensions.deckToManifold')} valueM={data.deckToManifold} onChange={numSet('deckToManifold')} />
          <MetricField label={t('shipData.dimensions.sternToManifold')} valueM={data.sternToManifold} onChange={numSet('sternToManifold')} />
          <MetricField label={t('shipData.dimensions.shipsideToManifold')} valueM={data.shipsideToManifold} onChange={numSet('shipsideToManifold')} />
          <MetricField label={t('shipData.dimensions.bowToManifold')} valueM={data.bowToManifold} onChange={numSet('bowToManifold')} />
          <MetricField label={t('shipData.dimensions.manifoldToKeel')} valueM={data.manifoldToKeel} onChange={numSet('manifoldToKeel')} />
          <MetricField label={t('shipData.dimensions.manifoldToBridge')} valueM={data.manifoldToBridge} onChange={numSet('manifoldToBridge')} />
          <FormField label={t('shipData.dimensions.maxLoadingRateShip')} value={data.maxLoadingRateShip} onChange={strSet('maxLoadingRateShip')} type="number" suffix="m³/h" />
          <FormField label={t('shipData.dimensions.numberOfLines')} value={data.numberOfLines} onChange={(v) => onChange('numberOfLines', v ? parseInt(v) : undefined)} type="number" />
          <FormField label={t('shipData.dimensions.maxAllowablePressure')} value={data.maxAllowablePressurePsi} onChange={strSet('maxAllowablePressurePsi')} type="number" suffix="PSI" />
          <FormField label={t('shipData.dimensions.ventingSystemShip')} value={data.ventingSystemShip} onChange={(v) => onChange('ventingSystemShip', v)} />
        </div>
      </SectionCard>

      {/* Pilot Card Data */}
      <SectionCard
        title={t('shipData.dimensions.pilotCardSpeedTable')}
        headerAction={
          <DynamicListHeader
            label={t('shipData.dimensions.addRow')}
            onAdd={() => onChildAdd('pilotCardData', { engineOrder: '', mainEngineRPM: undefined, speedLoadedKts: undefined, speedBallastKts: undefined, sortOrder: data.pilotCardData.length })}
          />
        }
      >
        {data.pilotCardData.length === 0 ? (
          <p className="text-sm text-gray-400 italic">{t('shipData.dimensions.noPilotCardData')}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 text-left">
                  <th className="py-2 px-2 text-xs font-medium text-gray-500 w-48">{t('shipData.dimensions.engineOrder')}</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">{t('shipData.dimensions.meRPM')}</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">{t('shipData.dimensions.speedLoadedKts')}</th>
                  <th className="py-2 px-2 text-xs font-medium text-gray-500">{t('shipData.dimensions.speedBallastKts')}</th>
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
                        <option value="">{t('shipData.common.select')}</option>
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
