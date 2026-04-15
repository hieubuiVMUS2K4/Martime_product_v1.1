
import type { SaveShipData, ShipMainEngine, ShipAuxiliaryEngine, ShipPropeller, ShipBowthruster, ShipSternthruster, ShipRudder, ShipShaftGenerator, ShipBoiler } from '@/types/ship-data.types';
import { SectionCard, FormField, PowerField, CheckboxField, DynamicListHeader, DynamicListDelete } from './ShipDataFields';
import { calcPitchRatio } from '@/types/ship-data.types';
import { useTranslationSafe } from '@/contexts/I18nContext';

interface MachineryTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
  onChildChange: <T>(collection: string, index: number, field: keyof T, value: any) => void;
  onChildAdd: (collection: string, defaultItem: any) => void;
  onChildRemove: (collection: string, index: number) => void;
}

const FUEL_GRADES = ['HFO', 'VLSFO', 'ULSFO', 'MGO', 'MDO', 'LNG', 'Methanol', 'Other'];
const PROPELLER_TYPES = ['Fixed Pitch', 'Controllable Pitch'];
const ROTATION_OPTIONS = ['Clockwise', 'Counter-Clockwise'];
const RUDDER_TYPES = ['Conventional', 'Semi-Balanced', 'Balanced', 'Flap', 'Schilling', 'Becker'];
const BOILER_TYPES = ['Auxiliary', 'Composite', 'Exhaust Gas', 'Thermal Oil Heater', 'Incinerator'];

export function MachineryTab({ data, onChange, onChildChange, onChildAdd, onChildRemove }: MachineryTabProps) {
  const { t } = useTranslationSafe();
  return (
    <div className="space-y-4">
      {/* Main Engines */}
      <SectionCard
        title={t('shipData.machinery.mainEngines')}
        headerAction={
          <DynamicListHeader label={t('shipData.machinery.addME')} onAdd={() => onChildAdd('mainEngines', { meType: '', meFuelGrade: '', mePowerKW: undefined, mcrKW: undefined, sortOrder: data.mainEngines.length })} />
        }
      >
        {data.mainEngines.length === 0 ? (
          <p className="text-sm text-gray-400 italic">{t('shipData.machinery.noMainEngines')}</p>
        ) : (
          <div className="space-y-3">
            {data.mainEngines.map((eng, i) => (
              <div key={i} className="flex items-start gap-2 p-3 bg-gray-50 rounded border border-gray-100">
                <div className="flex-1 grid grid-cols-2 md:grid-cols-4 gap-3">
                  <FormField label={t('shipData.machinery.meType', { n: i+1 })} value={eng.meType} onChange={(v) => onChildChange<ShipMainEngine>('mainEngines', i, 'meType', v)} className="col-span-2" />
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.machinery.fuelGrade')}</label>
                    <select value={eng.meFuelGrade ?? ''} onChange={(e) => onChildChange<ShipMainEngine>('mainEngines', i, 'meFuelGrade', e.target.value)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded">
                      <option value="">{t('shipData.common.select')}</option>
                      {FUEL_GRADES.map(g => <option key={g} value={g}>{g}</option>)}
                    </select>
                  </div>
                  <PowerField label={t('shipData.machinery.power')} valueKW={eng.mePowerKW} onChange={(v) => onChildChange<ShipMainEngine>('mainEngines', i, 'mePowerKW', v)} />
                  <PowerField label={t('shipData.machinery.mcr')} valueKW={eng.mcrKW} onChange={(v) => onChildChange<ShipMainEngine>('mainEngines', i, 'mcrKW', v)} />
                </div>
                <DynamicListDelete onDelete={() => onChildRemove('mainEngines', i)} />
              </div>
            ))}
          </div>
        )}
      </SectionCard>

      {/* Auxiliary Engines */}
      <SectionCard
        title={t('shipData.machinery.auxiliaryEngines')}
        headerAction={
          <DynamicListHeader label={t('shipData.machinery.addAE')} onAdd={() => onChildAdd('auxiliaryEngines', { aeType: '', aeFuelGrade: '', aePowerKW: undefined, sortOrder: data.auxiliaryEngines.length })} />
        }
      >
        {data.auxiliaryEngines.length === 0 ? (
          <p className="text-sm text-gray-400 italic">{t('shipData.machinery.noAuxEngines')}</p>
        ) : (
          <div className="space-y-3">
            {data.auxiliaryEngines.map((eng, i) => (
              <div key={i} className="flex items-start gap-2 p-3 bg-gray-50 rounded border border-gray-100">
                <div className="flex-1 grid grid-cols-2 md:grid-cols-4 gap-3">
                  <FormField label={t('shipData.machinery.aeType', { n: i+1 })} value={eng.aeType} onChange={(v) => onChildChange<ShipAuxiliaryEngine>('auxiliaryEngines', i, 'aeType', v)} className="col-span-2" />
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.machinery.fuelGrade')}</label>
                    <select value={eng.aeFuelGrade ?? ''} onChange={(e) => onChildChange<ShipAuxiliaryEngine>('auxiliaryEngines', i, 'aeFuelGrade', e.target.value)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded">
                      <option value="">{t('shipData.common.select')}</option>
                      {FUEL_GRADES.map(g => <option key={g} value={g}>{g}</option>)}
                    </select>
                  </div>
                  <PowerField label={t('shipData.machinery.power')} valueKW={eng.aePowerKW} onChange={(v) => onChildChange<ShipAuxiliaryEngine>('auxiliaryEngines', i, 'aePowerKW', v)} />
                </div>
                <DynamicListDelete onDelete={() => onChildRemove('auxiliaryEngines', i)} />
              </div>
            ))}
          </div>
        )}
      </SectionCard>

      {/* Propellers */}
      <SectionCard
        title={t('shipData.machinery.propellers')}
        headerAction={
          <DynamicListHeader label={t('shipData.machinery.addPropeller')} onAdd={() => onChildAdd('propellers', { propellerType: '', numberOfBlades: undefined, rotation: '', diameterMm: undefined, propellerPitchGeometricMm: undefined, pitchRatio: undefined, sortOrder: data.propellers.length })} />
        }
      >
        {data.propellers.length === 0 ? (
          <p className="text-sm text-gray-400 italic">{t('shipData.machinery.noPropellers')}</p>
        ) : (
          <div className="space-y-3">
            {data.propellers.map((prop, i) => (
              <div key={i} className="flex items-start gap-2 p-3 bg-gray-50 rounded border border-gray-100">
                <div className="flex-1 grid grid-cols-2 md:grid-cols-4 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.machinery.type')}</label>
                    <select value={prop.propellerType ?? ''} onChange={(e) => onChildChange<ShipPropeller>('propellers', i, 'propellerType', e.target.value)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded">
                      <option value="">{t('shipData.common.select')}</option>
                      {PROPELLER_TYPES.map(pt => <option key={pt} value={pt}>{pt}</option>)}
                    </select>
                  </div>
                  <FormField label={t('shipData.machinery.noOfBlades')} value={prop.numberOfBlades} onChange={(v) => onChildChange<ShipPropeller>('propellers', i, 'numberOfBlades', v ? parseInt(v) : undefined)} type="number" />
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.machinery.rotation')}</label>
                    <select value={prop.rotation ?? ''} onChange={(e) => onChildChange<ShipPropeller>('propellers', i, 'rotation', e.target.value)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded">
                      <option value="">{t('shipData.common.select')}</option>
                      {ROTATION_OPTIONS.map(r => <option key={r} value={r}>{r}</option>)}
                    </select>
                  </div>
                  <FormField label={t('shipData.machinery.diameter')} value={prop.diameterMm} onChange={(v) => onChildChange<ShipPropeller>('propellers', i, 'diameterMm', v ? parseFloat(v) : undefined)} type="number" suffix="mm" />
                  <FormField label={t('shipData.machinery.pitchGeometric')} value={prop.propellerPitchGeometricMm} onChange={(v) => onChildChange<ShipPropeller>('propellers', i, 'propellerPitchGeometricMm', v ? parseFloat(v) : undefined)} type="number" suffix="mm" />
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.machinery.pitchRatio')}</label>
                    <input type="text" value={calcPitchRatio(prop.propellerPitchGeometricMm, prop.diameterMm) ?? ''} disabled className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-500" />
                  </div>
                </div>
                <DynamicListDelete onDelete={() => onChildRemove('propellers', i)} />
              </div>
            ))}
          </div>
        )}
      </SectionCard>

      {/* Bow Thrusters */}
      <SectionCard
        title={t('shipData.machinery.bowThrusters')}
        headerAction={
          !data.bowthrusterNA ? (
            <DynamicListHeader label={t('shipData.common.add')} onAdd={() => onChildAdd('bowthrusters', { powerKW: undefined, sortOrder: data.bowthrusters.length })} />
          ) : undefined
        }
      >
        <div className="mb-2">
          <CheckboxField label={t('shipData.common.na')} checked={data.bowthrusterNA} onChange={(v) => onChange('bowthrusterNA', v)} />
        </div>
        {!data.bowthrusterNA && data.bowthrusters.map((bt, i) => (
          <div key={i} className="flex items-center gap-2 mb-2">
            <PowerField label={t('shipData.machinery.bowthruster', { n: i+1 })} valueKW={bt.powerKW} onChange={(v) => onChildChange<ShipBowthruster>('bowthrusters', i, 'powerKW', v)} className="flex-1" />
            <DynamicListDelete onDelete={() => onChildRemove('bowthrusters', i)} />
          </div>
        ))}
      </SectionCard>

      {/* Stern Thrusters */}
      <SectionCard
        title={t('shipData.machinery.sternThrusters')}
        headerAction={
          !data.sternthrusterNA ? (
            <DynamicListHeader label={t('shipData.common.add')} onAdd={() => onChildAdd('sternthrusters', { powerKW: undefined, sortOrder: data.sternthrusters.length })} />
          ) : undefined
        }
      >
        <div className="mb-2">
          <CheckboxField label={t('shipData.common.na')} checked={data.sternthrusterNA} onChange={(v) => onChange('sternthrusterNA', v)} />
        </div>
        {!data.sternthrusterNA && data.sternthrusters.map((st, i) => (
          <div key={i} className="flex items-center gap-2 mb-2">
            <PowerField label={t('shipData.machinery.sternthruster', { n: i+1 })} valueKW={st.powerKW} onChange={(v) => onChildChange<ShipSternthruster>('sternthrusters', i, 'powerKW', v)} className="flex-1" />
            <DynamicListDelete onDelete={() => onChildRemove('sternthrusters', i)} />
          </div>
        ))}
      </SectionCard>

      {/* Rudders */}
      <SectionCard
        title={t('shipData.machinery.rudders')}
        headerAction={
          <DynamicListHeader label={t('shipData.machinery.addRudder')} onAdd={() => onChildAdd('rudders', { rudderType: '', sortOrder: data.rudders.length })} />
        }
      >
        {data.rudders.length === 0 ? (
          <p className="text-sm text-gray-400 italic">{t('shipData.machinery.noRudders')}</p>
        ) : (
          <div className="space-y-2">
            {data.rudders.map((r, i) => (
              <div key={i} className="flex items-center gap-2">
                <div className="flex-1">
                  <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.machinery.rudderType', { n: i+1 })}</label>
                  <select value={r.rudderType ?? ''} onChange={(e) => onChildChange<ShipRudder>('rudders', i, 'rudderType', e.target.value)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded">
                    <option value="">{t('shipData.common.select')}</option>
                    {RUDDER_TYPES.map(rt => <option key={rt} value={rt}>{rt}</option>)}
                  </select>
                </div>
                <DynamicListDelete onDelete={() => onChildRemove('rudders', i)} />
              </div>
            ))}
          </div>
        )}
      </SectionCard>

      {/* Shaft Generators */}
      <SectionCard
        title={t('shipData.machinery.shaftGenerators')}
        headerAction={
          !data.shaftGeneratorNA ? (
            <DynamicListHeader label={t('shipData.common.add')} onAdd={() => onChildAdd('shaftGenerators', { maxPowerKW: undefined, sortOrder: data.shaftGenerators.length })} />
          ) : undefined
        }
      >
        <div className="mb-2">
          <CheckboxField label={t('shipData.common.na')} checked={data.shaftGeneratorNA} onChange={(v) => onChange('shaftGeneratorNA', v)} />
        </div>
        {!data.shaftGeneratorNA && data.shaftGenerators.map((sg, i) => (
          <div key={i} className="flex items-center gap-2 mb-2">
            <PowerField label={t('shipData.machinery.shaftGen', { n: i+1 })} valueKW={sg.maxPowerKW} onChange={(v) => onChildChange<ShipShaftGenerator>('shaftGenerators', i, 'maxPowerKW', v)} className="flex-1" />
            <DynamicListDelete onDelete={() => onChildRemove('shaftGenerators', i)} />
          </div>
        ))}
      </SectionCard>

      {/* Harbour Generator */}
      <SectionCard title={t('shipData.machinery.harbourGenerator')}>
        <div className="grid grid-cols-2 gap-3">
          <FormField label={t('shipData.machinery.maker')} value={data.harbourGeneratorMaker} onChange={(v) => onChange('harbourGeneratorMaker', v)} />
          <PowerField label={t('shipData.machinery.maxPower')} valueKW={data.harbourGeneratorMaxPowerKW} onChange={(v) => onChange('harbourGeneratorMaxPowerKW', v)} />
        </div>
      </SectionCard>

      {/* Boilers */}
      <SectionCard
        title={t('shipData.machinery.boilers')}
        headerAction={
          <DynamicListHeader label={t('shipData.machinery.addBoiler')} onAdd={() => onChildAdd('boilers', { boilerType: '', model: '', sortOrder: data.boilers.length })} />
        }
      >
        {data.boilers.length === 0 ? (
          <p className="text-sm text-gray-400 italic">{t('shipData.machinery.noBoilers')}</p>
        ) : (
          <div className="space-y-2">
            {data.boilers.map((b, i) => (
              <div key={i} className="flex items-center gap-2">
                <div className="flex-1 grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.machinery.type')}</label>
                    <select value={b.boilerType ?? ''} onChange={(e) => onChildChange<ShipBoiler>('boilers', i, 'boilerType', e.target.value)} className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded">
                      <option value="">{t('shipData.common.select')}</option>
                      {BOILER_TYPES.map(bt => <option key={bt} value={bt}>{bt}</option>)}
                    </select>
                  </div>
                  <FormField label={t('shipData.machinery.model')} value={b.model} onChange={(v) => onChildChange<ShipBoiler>('boilers', i, 'model', v)} />
                </div>
                <DynamicListDelete onDelete={() => onChildRemove('boilers', i)} />
              </div>
            ))}
          </div>
        )}
      </SectionCard>

      {/* Anchor Chains */}
      <SectionCard title={t('shipData.machinery.anchorChains')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.machinery.port')} value={data.anchorChainPort} onChange={(v) => onChange('anchorChainPort', v)} />
          <FormField label={t('shipData.machinery.starboard')} value={data.anchorChainStarboard} onChange={(v) => onChange('anchorChainStarboard', v)} />
          <div className="flex items-end gap-2">
            <FormField label={t('shipData.machinery.stern')} value={data.anchorChainStern} onChange={(v) => onChange('anchorChainStern', v)} className="flex-1" disabled={data.anchorChainSternNA} />
            <CheckboxField label={t('shipData.common.na')} checked={data.anchorChainSternNA} onChange={(v) => onChange('anchorChainSternNA', v)} />
          </div>
        </div>
      </SectionCard>

      {/* Azimuth Engines */}
      <SectionCard title={t('shipData.machinery.azimuthEngines')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.machinery.fwdCount')} value={data.azimuthEngFwdCount} onChange={(v) => onChange('azimuthEngFwdCount', v ? parseInt(v) : undefined)} type="number" />
          <PowerField label={t('shipData.machinery.fwdMaxPower')} valueKW={data.azimuthEngFwdMaxPowerKW} onChange={(v) => onChange('azimuthEngFwdMaxPowerKW', v)} />
          <FormField label={t('shipData.machinery.aftCount')} value={data.azimuthEngAftCount} onChange={(v) => onChange('azimuthEngAftCount', v ? parseInt(v) : undefined)} type="number" />
          <PowerField label={t('shipData.machinery.aftMaxPower')} valueKW={data.azimuthEngAftMaxPowerKW} onChange={(v) => onChange('azimuthEngAftMaxPowerKW', v)} />
        </div>
      </SectionCard>
    </div>
  );
}
