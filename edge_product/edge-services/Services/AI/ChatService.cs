using System.Text.Json;

namespace MaritimeEdge.Services.AI;

public interface IChatService
{
    Task<ChatResponse> ProcessMessageAsync(string message, string? context = null);
    List<SuggestedQuestion> GetSuggestedQuestions();
}

public class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    // Maritime knowledge base for offline/fallback responses
    private static readonly Dictionary<string, string> MaritimeKnowledge = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── SAFETY & EMERGENCY ──────────────────────────────────────────────────
        ["fire drill"] = "Fire drill procedures: 1) Sound general alarm (7 short + 1 long blast). 2) All crew muster at assigned stations. 3) Fire party proceeds to fire location with equipment. 4) Boundary cooling team stands by. 5) Conduct headcount. Reference: SOLAS Chapter III, Regulation 19.",
        ["man overboard"] = "Man Overboard (MOB) procedure: 1) Shout 'MAN OVERBOARD' and throw lifebuoy immediately. 2) Post dedicated lookout – never lose visual contact. 3) Sound 3 prolonged blasts on whistle. 4) Execute Williamson turn (best in restricted visibility) or Anderson turn. 5) Alert bridge/Master immediately. 6) Deploy rescue boat if conditions allow. Reference: SOLAS, ISM Code.",
        ["abandon ship"] = "Abandon Ship procedure: 1) Master gives order after all other options exhausted. 2) Sound signal: 7 short + 1 long blast, continuous on ship's whistle/alarm. 3) Don lifejacket and immersion suit immediately. 4) Muster at assigned lifeboat/liferaft station. 5) Bring EPIRB, SART, water, rations if time allows. 6) Launch sequence: leeward lifeboat first. Reference: SOLAS Chapter III.",
        ["muster station"] = "Muster stations are designated assembly points for emergencies. Each crew member's station is on the Muster List (posted prominently in crew areas). Know your station number, assigned duties (firefighter, lifeboat crew, medical team, etc.) and lifeboat assignment before departure. Reference: SOLAS Chapter III, Regulation 8.",
        ["ppe"] = "Personal Protective Equipment (PPE) required on ships: Hard hat (deck work), safety shoes with steel toe-cap, high-visibility vest, safety glasses/goggles (chemical/grinding work), hearing protection (engine room >85dB), work gloves, safety harness + lifeline (working at height/overside), chemical suit (hazardous cargo). Always inspect PPE before use and tag defective equipment. Reference: ISM Code, ILO MLC 2006.",
        ["enclosed space"] = "Enclosed space entry procedure: 1) Identify enclosed space (tanks, holds, voids, pump rooms). 2) Obtain Permit to Work. 3) Test atmosphere: O2 (19.5–23%), toxic gases (H2S, CO), flammable vapour. 4) Ventilate minimum 24hrs if possible. 5) Post standby person outside – never alone. 6) Establish communication. 7) Have rescue equipment ready (SCBA, rescue line). Reference: SOLAS Reg. XI-1/7, IMO MSC-MEPC.2/Circ.16.",
        ["hot work"] = "Hot work (welding, cutting, grinding) procedure: 1) Obtain Hot Work Permit. 2) Check for flammable vapours/liquids in work area and adjacent spaces. 3) Remove/protect combustible materials. 4) Post fire watcher with portable extinguisher. 5) Check for smouldering after work – minimum 1hr watch. 6) Never hot work on cargo tanks without gas-free certificate. Reference: ISM Code, SOLAS.",
        ["lifeboat"] = "Lifeboat weekly checks: painter secured, launching equipment free, engine starts, relief valve operational, fuel topped up, water/provisions within expiry. Monthly: full lowering to water (if feasible), load release tested. Annual: class society inspection. Totally enclosed: check self-righting, sprinkler system. Reference: SOLAS Chapter III, LSA Code.",
        ["life raft"] = "Life raft servicing: hydrostatic release mechanism replaces every 2 years; raft container serviced annually by approved station. Inflate using hydrostatic release (sinks to 4m, releases automatically) or manual painter pull. Boarding: jump from ship or ladder, right any inverted raft. Reference: SOLAS Chapter III, LSA Code.",
        ["immersion suit"] = "Immersion suit (survival suit): worn during abandon ship or cold water rescue. Donning time ≤2 minutes. Provides 100hrs survival in cold water. Check: zip seals, gloves, hood, face seal, retroreflective tape. Try on annually. Reference: SOLAS Reg. III/7, LSA Code.",
        ["epirb"] = "EPIRB (Emergency Position Indicating Radio Beacon): 406 MHz distress alert to COSPAS-SARSAT satellite system. Types: Float-free (auto activates at 1–4m depth), Manual. Registration mandatory with flag state – ensure vessel name, MMSI, owner contact are current. Test monthly: self-test only. Replace battery/hydrostatic release per manufacturer schedule. Reference: SOLAS Chapter IV.",
        ["breathing apparatus"] = "SCBA (Self-Contained Breathing Apparatus): used in fire-fighting and enclosed space rescue. Check before use: cylinder pressure (min 200 bar), harness, regulator, mask seal, low-pressure warning alarm. Duration: 30–45 min depending on cylinder size and breathing rate. Never enter IDLH atmosphere without SCBA. Maintain in ready-to-use condition. Reference: SOLAS, FSS Code.",

        // ── NAVIGATION ─────────────────────────────────────────────────────────
        ["colreg"] = "COLREGs (International Regulations for Preventing Collisions at Sea, 1972 as amended): 41 rules + 4 annexes. Critical rules: Rule 5 (Lookout – by sight, hearing and all means), Rule 6 (Safe Speed), Rule 7 (Risk of Collision – use radar/early assessment), Rule 8 (Action to Avoid – ample, bold, timely), Rule 16 (Give-way vessel action), Rule 17 (Stand-on vessel), Rule 18 (Responsibilities between vessels). Learn all sound/light signals.",
        ["navigation lights"] = "Navigation lights (COLREGs Annex I): Power-driven vessel underway: masthead light forward (white, 225°); if >50m second masthead light aft higher; sidelights (green starboard/red port, 112.5° each); white stern light (135°). At anchor: all-round white light (forward) + if >50m second all-round white aft. Aground: anchor lights + 2 all-round red lights. Not-under-command: 2 all-round red lights + running lights when making way.",
        ["buoyage"] = "IALA Buoyage System A vs B: Region A (Europe, Africa, Asia, Australia) – port-hand marks RED, starboard GREEN when entering from seaward. Region B (Americas, Japan, Korea, Philippines) – REVERSED (port GREEN, starboard RED). Cardinal marks (N/S/E/W) show safe side; isolated danger: black+red bands; safe water: red/white stripes; special marks: yellow. Always obtain local chart and pilot for unfamiliar waters.",
        ["watchkeeping"] = "Bridge watchkeeping (STCW Chapter VIII/SOLAS Reg. V/14): OOW must maintain continuous lookout, monitor course/speed/position every 15min minimum, log entries, check weather/traffic, update ECDIS/chart. Alone on bridge only if traffic/conditions permit. Hand over only when relief is fully oriented. Fatigue management critical – never stand watch impaired. Minimum rest: 10h/24h, 77h/7days.",
        ["radar"] = "Radar operation and anti-collision: Use on appropriate range scale, apply proper sea/rain clutter controls. Plot all echoes in busy waters (ARPA tracking). Assess CPA/TCPA – take action if CPA <1nm or TCPA <6min in open sea. Radar does not relieve duty of proper lookout. In fog: slow to safe speed, sound fog signals. Reference: SOLAS Chapter V, COLREGs Rule 7.",
        ["chart"] = "Nautical charts: Always use largest scale chart available for the area. Check edition date – update from Notices to Mariners (weekly NtM). Mark corrections in chart folio log. ECDIS official chart cells must be updated before each voyage. Paper chart backup required if ECDIS is primary means. Reference: SOLAS Chapter V, ECDIS performance standards.",
        ["anchor"] = "Anchoring procedure: 1) Select anchorage – check depth, holding ground, traffic separation. 2) Reduce speed well ahead. 3) Let go at zero or near-zero speed – 3–4 shackles in moderate weather. 4) Snub chain to dig in anchor. 5) Veered chain = 5× depth minimum. 6) Take bearings/use GPS to verify holding. 7) Post anchor watch, check bearings every 15min. Sound 1 prolonged blast + 4 short per minute in fog at anchor. Reference: COLREGs Rule 30.",
        ["mooring"] = "Mooring lines: Bow line, forward breast, forward spring, aft spring, aft breast, stern line. Springs prevent fore-aft movement – most important for surging. Never stand in bight of line under load. Wear PPE (hard hat, gloves, safety shoes). If rope parts: stand clear of snap-back zone. Use fairleads, bitts and bollards correctly rated for vessel displacement. Reference: ISM Code; OCIMF MEG4.",
        ["passage planning"] = "Passage planning (SOLAS Reg. V/34): 4 stages – 1) Appraisal (collect all info: charts, pubs, NOTAMs, weather, port entry). 2) Planning (plot on chart: waypoints, danger clearances, abort points). 3) Execution (monitor against plan, log deviations). 4) Monitoring (cross-check position every 15min; DR + electronic + visual). Master sign-off required. Reference: SOLAS Chapter V, IMO Resolution A.893(21).",
        ["gps"] = "GPS/GNSS navigation: Cross-check GPS position with independent means (radar, visual bearings, depth sounder) at regular intervals. HDOP <2 ideal. Loss of GPS signal – switch to DR using last known position, course, speed. Report GPS anomalies (jamming/spoofing) to authorities. Never rely on single navigation aid. Reference: SOLAS Chapter V.",

        // ── ENVIRONMENTAL ──────────────────────────────────────────────────────
        ["marpol"] = "MARPOL 73/78 (International Convention for Prevention of Marine Pollution): Annex I (Oil) – discharge <15ppm outside 12nm, prohibited in SPAs; Annex II (Noxious Liquid Substances in bulk); Annex III (Harmful Substances in packaged form); Annex IV (Sewage – 3nm with treatment, 12nm comminuted/disinfected); Annex V (Garbage – all plastics prohibited, food waste rules); Annex VI (Air Pollution – SOx, NOx, ODS limits). Record books mandatory. IMO 2020: max 0.5% sulfur fuel globally.",
        ["garbage disposal"] = "MARPOL Annex V – Garbage Management Plan required on vessels ≥100GT. Disposal rules at sea: Plastics – PROHIBITED anywhere. Food waste – ≥12nm ground to <25mm; ≥3nm in some SPAs. Cargo residues (non-HME) – ≥12nm. Cleaning agents (non-harmful) – ≥12nm. Animal carcasses – ≥100nm. Cooking oil – floating, prohibited <12nm. Garbage Record Book: must record all overboard discharges and port reception facility use.",
        ["oil spill"] = "Oil spill response (SOPEP – Shipboard Oil Pollution Emergency Plan, required for ships >400GT): 1) Stop source (close valves, stop pumps). 2) Alert Officer of Watch and Master. 3) Contain with boom/absorbent materials. 4) Report to nearest coastal state per Annex I. 5) Record in Oil Record Book. 6) Do NOT use dispersants without authority approval. 7) Clean up – segregate oily waste for port reception. SOPEP must be approved by flag state.",
        ["ballast water"] = "Ballast Water Management Convention (BWM 2004, in force 2017): All ships must have approved BWMP. D-1 Standard: exchange ≥200nm from shore, ≥200m depth. D-2 Standard: treatment system (IMO type-approved) – max 10 viable organisms/m3. Ballast Water Record Book mandatory. Port state control verifies compliance. Sediment management: clean tanks regularly, dispose at reception facility.",
        ["sewage"] = "Sewage discharge (MARPOL Annex IV): Prohibited within 3nm. Treated sewage (certified system) allowed ≥3nm. Comminuted/disinfected sewage allowed ≥12nm at >4 knots. In SPAs (Baltic, Caribbean, etc.): must use port reception. Holding tank or treatment plant required on passenger ships in SPAs. Sewage Record Book required for all passenger ships in Antarctic.",
        ["air pollution"] = "MARPOL Annex VI – Air Pollution: SOx limit 0.1% in ECAs (Emission Control Areas: Baltic, North Sea, North America, US Caribbean); 0.5% globally (IMO 2020). NOx Tier III in ECAs for new ships built after 2016. ODS: R-22 and halon banned as of 2020 except for fire suppression. VEC: Vapour Emission Control required in some ports. Fuel Oil Changeover Log must record switch to compliant fuel before entering ECA.",

        // ── MAINTENANCE ───────────────────────────────────────────────────────
        ["planned maintenance"] = "Planned Maintenance System (PMS – ISM Code §10): All critical equipment must be on schedule. Components: equipment register (make/model/serial), maintenance procedures (with intervals), work orders, spare parts inventory, maintenance history log. Use maker's manual + class society requirements for intervals. Senior engineer signs off completed work. Non-conformities and near-misses must be reported and recorded.",
        ["drydock"] = "Drydocking (Special Survey): Class societies require dry dock every 2.5–5 years (IACS rules). Scope: hull cleaning and coating (anti-fouling paint), propeller/shaft inspection and polishing, rudder bearing clearance, sea chest/sea valve overhaul, bossing/stern tube renewal, thruster overhaul. Prepare: repair list, approved dry-dock plan, spare parts, class and flag surveyors notified minimum 14 days ahead.",
        ["engine maintenance"] = "Main engine maintenance schedule (typical 2-stroke diesel): 1,000hrs – lubrication oil analysis, filter renewal, governor/fuel pump adjustment. 4,000hrs – fuel injector overhaul, exhaust valve reconditioning. 8,000hrs – piston/piston ring/liner inspection, turbocharger overhaul. 16,000hrs – crankshaft deflection, main bearing overhaul. 32,000hrs – major overhaul. Always follow maker's manual. Record all work in engine logbook and PMS.",
        ["generator"] = "Auxiliary generator maintenance: Weekly – check lube oil level, coolant, fuel, alarms. Monthly – test load sharing, governor operation, AVR. 500hrs – oil/filter change. 2,000hrs – valve clearance, injector overhaul. 8,000hrs – major overhaul. Blackout procedure: emergency generator auto-starts within 45sec (SOLAS). Test monthly under load. Maintain min 2 generators in parallel in port. Reference: SOLAS Chapter II-1.",
        ["boiler"] = "Boiler maintenance and operation: Daily – check water level (gauge glass), steam pressure, feed water quality (pH 10.5–11.5, chlorides <50ppm). Weekly – blow down gauge glass and low-water cut-out. Monthly – test safety relief valve. Annually – internal/external inspection. Never operate without functional low-water alarm and safety valve. Reference: Class society rules, maker's manual.",
        ["pump"] = "Bilge pump operation: Test bilge pump and bilge alarm weekly. Bilge water with oil content >15ppm must NOT be discharged overboard – use OWS (Oily Water Separator) and record in Oil Record Book Part I. Bilge high-level alarm must be functional at all times. Emergency bilge suction valves location must be known to all engineering watch officers. Reference: MARPOL Annex I, SOLAS Chapter II-1.",
        ["steering gear"] = "Steering gear checks (SOLAS Reg. II-1/29): Testing before entering port/confined waters: main and auxiliary steering – operate full travel each side; communications between bridge and steering gear room; emergency steering procedures; remote and local control. Check hydraulic oil level, pump operation. Record all tests in log. Reference: SOLAS Chapter II-1, Regulation 29.",

        // ── CARGO OPERATIONS ──────────────────────────────────────────────────
        ["cargo loading"] = "Cargo loading – key checks: 1) Verify stowage plan approved by Master. 2) Check cargo documentation (B/L, dangerous goods declaration, phytosanitary certs). 3) Inspect cargo condition before loading (note damage, leakage). 4) Monitor draught and stability continuously – maintain positive GM and satisfy stability criteria (GZ curve). 5) Secure all cargo per cargo securing manual. 6) Check trim/list within limits. Reference: SOLAS Chapter VI, CSS Code.",
        ["dangerous goods"] = "IMDG Code (International Maritime Dangerous Goods Code): 9 hazard classes: 1-Explosives, 2-Gases, 3-Flammable Liquids, 4-Flammable Solids, 5-Oxidizers, 6-Toxic/Infectious, 7-Radioactive, 8-Corrosive, 9-Misc. Labelling, placarding, segregation, and stowage mandatory. Dangerous Goods Manifest/declaration required. Emergency Schedules (EmS) in IMDG supplement. Reference: SOLAS Chapter VII, IMDG Code.",
        ["tanker operations"] = "Tanker cargo operations: COW (Crude Oil Washing) or IGS (Inert Gas System) required on crude oil tankers >20,000DWT. Pre-transfer meeting required before ship-shore transfer. Hose connection must have adequate support. Scuppers plugged, drip trays in place. Bonding cable connected (petroleum products). Oxygen level in cargo tanks: maintain <8% in crude/product tankers. Emergency shutdown procedure must be posted. Reference: SOLAS Chapter II-2, ICS/OCIMF Ship-To-Ship Transfer Guide.",
        ["bulk carrier"] = "Bulk carrier cargo operations: 1) Loading sequence – avoid overstressing hull structure; follow approved loading computer output. 2) Trimming – ensure even distribution, no high trim exceeding limits. 3) Moisture content – coal, grain, ore concentrates can liquefy (TML – Transportable Moisture Limit); test moisture before loading. 4) Hatch covers – inspect seals, drain holes; water ingress can shift/liquefy cargo. 5) ISF/IMSBC Code for solid bulk cargoes.",
        ["stability"] = "Ship stability fundamentals: GM (metacentric height) must be positive at all loading conditions. Stability criteria: GZ max ≥0.20m at ≥30°; area under GZ 0–30° ≥0.055m·rad; area 0–40° ≥0.09m·rad; GM₀ ≥0.15m. Free surface effect reduces effective GM. Shifting/suspended masses reduce GM. Use stability computer before departure. Reference: IMO Resolution MSC.267(85) – IS Code 2008.",
        ["lashing"] = "Cargo securing: All cargo must be secured per Cargo Securing Manual (CSM – required by SOLAS). Types: lashing bars, chains, straps, twist locks (containers), dunnage, blocking. Lashing angle 30–60° from horizontal is optimal. Check and retighten after rough weather. Dangerous goods – secure to class requirements. Record all securing operations in cargo log. Reference: SOLAS Chapter VI, CSS Code Amended 2014.",

        // ── CREW & CERTIFICATION ──────────────────────────────────────────────
        ["stcw"] = "STCW 2010 Manila Amendments: Minimum standards for seafarer training. Officer of the Watch (OOW) deck: STCW II/1. Chief Mate: II/2. Master: II/2. OOW engine: III/1. Chief Engineer: III/2. All ratings: basic safety training (BST) – personal survival, fire prevention, first aid, personal safety. Advanced: Advanced Fire Fighting, GMDSS, ECDIS, Proficiency in Survival Craft. Refresher every 5 years mandatory. Medical fitness: ENG1 or equivalent every 2 years.",
        ["rest hours"] = "Rest hour requirements (MLC 2006 / STCW 2010): Minimum 10 hours rest in any 24-hour period (can be split: max 2 periods, shortest ≥6hrs). Minimum 77 hours rest in any 7-day period. Exceptions (emergencies, drills) must be recorded and compensating rest given. Port State Control checks rest hour records – violations = detention. Record on approved form (Table A/B of STCW). Record in work/rest log.",
        ["mlc"] = "Maritime Labour Convention (MLC 2006): 5 major areas: 1) Minimum requirements (age ≥16, medical, STCW). 2) Conditions of employment (SEA contract, wages, overtime, repatriation). 3) Accommodation/recreation (space, lighting, noise, food quality). 4) Health & safety (risk assessment, machinery guarding, PPE). 5) Social security (medical care ashore, insurance). DMLC (Declaration of Maritime Labour Compliance) on board – part I (flag state) + part II (shipowner).",
        ["crew health"] = "Seafarer health and medical care: ship must carry Medicine Chest per flag state requirement (IMO Circular 960). Designated Medical Officer (usually Master if no doctor) responsible. Telemedicine available via CIRM (Italy), TMAS (international). Report illness records in official log. Repatriation obligations for sick/injured seafarer at owner's expense (MLC Standard A4.1). Medical certificates (ENG1 or equivalent) required for all crew.",
        ["wages"] = "Seafarer wages (MLC Title 2): Minimum wage set by ITF/ILO Joint Maritime Commission – currently $641/month for AB rating (review annually). Wages paid monthly. Account statement provided. Allotment (portion sent to family) must be facilitated. Maximum 1 month wages held. Deductions only per MLC allowed (allotments, flag state levy, ILO-approved). Unpaid wages – maritime lien against vessel. SEA (Seafarers' Employment Agreement) must specify wage rate.",
        ["repatriation"] = "Repatriation rights (MLC Standard A2.5): Seafarers entitled to repatriation at end of contract (max 12 months), dismissal by owner, inability to perform duties, ship call at port in distress, or ship sailing to war zone seafarer objects to. Owner pays: travel, accommodation, food, medical until home port. Flag state and port state must assist abandoned seafarers. Financial security certificate required.",

        // ── GMDSS & COMMUNICATIONS ────────────────────────────────────────────
        ["distress signals"] = "Maritime distress signals: MAYDAY (×3) – grave and imminent danger, life at risk; PAN PAN – urgent, no immediate risk to life; SECURITE – navigational/weather safety info. Transmit MAYDAY on VHF Ch.16 (and MF/HF if fitted). DSC distress alert on VHF Ch.70. EPIRB 406MHz for satellite coverage. Visual: orange smoke, red parachute/hand flare, orange flag with black square+circle. Sound: continuous horn. Reference: SOLAS Chapter IV, GMDSS.",
        ["vhf channels"] = "Key VHF marine channels: Ch.16 – International distress, safety & calling (guard at all times). Ch.70 – DSC digital selective calling distress and safety. Ch.13 – Bridge-to-bridge navigation safety (1W required on US inland). Ch.06 – Primary inter-ship safety channel. Ch.08 – Inter-ship working. Ch.09 – Boater calling (US). Ch.12,14,20,22 – Port operations (varies by port). Ch.67 – Coast Guard (UK). Ch.77 – Port operations. Channels 1–28, 60–88 – duplex working frequencies.",
        ["gmdss"] = "GMDSS (Global Maritime Distress and Safety System, SOLAS Chapter IV): Equipment by sea area: A1 (VHF range <30nm): VHF DSC, EPIRB, SART. A2 (+MF range 30–400nm): MF DSC, radiotelephony. A3 (Inmarsat coverage): Inmarsat terminal or HF DSC. A4 (polar, no Inmarsat): HF SSB. Required on: all SOLAS vessels (cargo ≥300GT, all passenger ships). Ship's radio license + operator GMDSS certificate mandatory. Test distress equipment every voyage.",
        ["inmarsat"] = "Inmarsat satellite communication: Fleet F77/F55 (Fleet Broadband 250/500), VSAT, FleetOne, IsatPhone Pro. Uses for: email, distress alert (MSI), SafetyNET (Navtex via satellite), crew welfare calls. Inmarsat-C: text only, LRIT position reporting, SafetyNET. Enhanced MSI for sea areas A3/A4. Never use Inmarsat GMDSS terminal for routine calls on distress frequency. Reference: SOLAS Chapter IV.",

        // ── PORT STATE CONTROL & INSPECTIONS ─────────────────────────────────
        ["port state control"] = "Port State Control (PSC) inspection: Any SOLAS vessel in foreign port may be boarded by PSC officer. Checks: flag state certificates (valid, correct), ISM/ISPS compliance, MARPOL records (Oil Record Book, Garbage Record Book), crew certificates (STCW), rest hours, fire/lifesaving appliances, structure (holds, hull). Deficiency = rectify before departure. Serious deficiency = Detention. MoU regions: Paris, Tokyo, US Coast Guard, etc. SIRENAC/Equasis database records detentions.",
        ["ism code"] = "ISM Code (International Safety Management Code, SOLAS Chapter IX): Requires Safety Management System (SMS) onboard. Key elements: safety policy, responsibilities, designated person ashore (DPA), resources, emergency preparedness, non-conformity reporting, maintenance, documentation, audits. DOC (Document of Compliance) held by company. SMC (Safety Management Certificate) on ship. Internal audit annually; external audit every 2.5 years. Non-conformities (NCO) must be closed out.",
        ["isps code"] = "ISPS Code (International Ship and Port Facility Security Code): SOLAS Chapter XI-2. Ship Security Plan (SSP) – confidential document approved by flag state. Ship Security Officer (SSO) on board; Company Security Officer (CSO) ashore. Security levels: 1 (normal), 2 (heightened), 3 (exceptional – specific threat). SSAS (Ship Security Alert System) installed. ISSC (International Ship Security Certificate) must be valid. PSC verifies compliance.",
        ["flag state"] = "Flag state responsibilities: Register vessel, issue certificates (SOLAS, MARPOL, MLC, etc.), conduct surveys, authorize class societies, enforce conventions. Open registries (Panama, Liberia, Marshall Islands, Bahamas, etc.) allow non-national ownership. Quality flags: Paris MOU white list preferred. Change of flag requires re-survey and new certificates. Bareboat charter registry allowed for dual registration.",

        // ── MEDICAL ──────────────────────────────────────────────────────────
        ["first aid"] = "First Aid at sea: Basic ABCDE assessment – Airway (clear), Breathing (rescue breaths), Circulation (CPR if no pulse: 30 compressions : 2 breaths), Disability (GCS), Exposure. Stop bleeding: direct pressure + elevation. Burns: cool water 10min, cover with cling film, do not break blisters. Fracture: immobilise, monitor. Shock: lie flat, elevate legs (unless head/chest injury). Always call TMAS (Telemedical Assistance Service) for advice. Document all treatment.",
        ["cpr"] = "CPR (Cardiopulmonary Resuscitation): 1) Call for help, AED. 2) Check responsiveness – no response. 3) Open airway: head tilt-chin lift. 4) Check breathing ≤10 seconds. 5) Start compressions IMMEDIATELY if no normal breathing: 30 compressions at centre of chest (hard and fast, 5–6cm depth, 100–120/min). 6) 2 rescue breaths after 30 compressions (1 second each). 7) Continue 30:2 until AED ready, EMS takes over, victim recovers, or too exhausted. AED: follow voice prompts.",
        ["seasickness"] = "Seasickness (motion sickness): caused by conflict between visual and vestibular signals. Prevention: stay on deck in fresh air (midship sections most stable), fix horizon in sight, ginger tablets, antihistamine (promethazine/cinnarizine – drowsiness warning, do not take if on watch duty). Remain hydrated. Treatment: antiemetic (ondansetron, prochlorperazine). Severe vomiting → IV fluids may be needed. Most crew desensitise within 2–3 days.",
        ["hypothermia"] = "Hypothermia at sea: core body temperature <35°C. Stages: mild (35–32°C) – shivering, confusion; moderate (32–28°C) – muscle rigidity, drowsiness; severe (<28°C) – unconscious, no pulse (may appear dead). Treatment: remove wet clothing, insulate, move to warm area, warm IV fluids if available, do NOT rub extremities (may trigger cardiac arrest). Gentle handling – cardiac arrest risk. Use immersion suit – extends survival time significantly in cold water.",

        // ── METEOROLOGY & OCEANOGRAPHY ───────────────────────────────────────
        ["weather"] = "Ship weather routing: Obtain synoptic forecast (NAVTEX, Inmarsat-C SafetyNET, SSB weather fax). Beaufort scale 0–12 (0=calm, 6=strong breeze 22–27kt, 8=gale 34–40kt, 12=hurricane ≥64kt). Significant wave height = average of top ⅓ of waves. Cross swell from different direction can cause extreme rolling/parametric rolling (container ships). Heavy weather: reduce speed, change course to reduce pounding/rolling. Lash all loose items. Reference: SOLAS Chapter V.",
        ["tropical cyclone"] = "Tropical cyclone (typhoon/hurricane/cyclone) avoidance: 1-2-3 Rule (US Navy): assume track uncertainty ±1°/day, ±2°/day after Day 2, ±3°/day after Day 3 – stay outside radius of sustained 34kt winds. Mariners: dangerous semicircle (right of track Northern Hemisphere, left in Southern) = worst conditions. Navigable semicircle = less severe. Rules: bear away from dangerous semicircle, present stern to swell if caught. Barometre dropping >3hPa/3hrs = danger signal. Reference: IMO MSC Circular 707.",
        ["tides"] = "Tides and tidal currents: Spring tides (new/full moon) – max range, max currents. Neap tides (half moon) – min range. Tide tables: obtain for ports. Tidal atlas shows current vectors. Squat effect increases in shallow water (under-keel clearance reduces as speed increases). Calculate UKC (Under Keel Clearance) = charted depth + predicted tide – squat – draft – safety margin (min 10% of draft). Reference: Admiralty Tide Tables, local pilot books.",
        ["fog"] = "Navigation in fog/reduced visibility (COLREGs Rule 19): Must proceed at Safe Speed. Make Ready: radar obs at appropriate scale, engine on standby, extra lookout, manual steering, sound fog signals (1 long blast ≥2min for power vessel underway making way). If you hear fog signal forward of beam – reduce to bare steerageway or stop. Never cross TSS in fog without essential need. Reference: COLREGs Rule 19, SOLAS Chapter V.",

        // ── SURVEYS & CERTIFICATES ────────────────────────────────────────────
        ["certificates"] = "Key ship certificates: Safety Construction (SC), Safety Equipment (SE), Safety Radio (SR), Load Line (LL), IOPP (MARPOL Annex I), ISMCC (Sewage), Garbage Management Plan, SMC (ISM), DOC (ISM company), ISSC (ISPS), CLC (Liability – oil tankers), MLC Maritime Labour Certificate. Renewal: most valid 5 years with annual/intermediate surveys. Expired certificate = PSC detention. Keep certified copies on board; originals in Master's custody.",
        ["survey"] = "Ship surveys: Annual Survey (in lieu of intermediate every 5yrs), Intermediate Survey (2.5yrs ± 6months), Special Survey/Renewal Survey (every 5yrs). Class surveys: continuous machinery survey (CMS) spreads work over 5yrs. Underwater survey: dry dock or in-water survey (IWS) with class approval. Enhanced Survey Programme (ESP) for bulk carriers/tankers >15yrs. Surveys by: IACS class societies (Lloyd's, DNV, BV, ABS, NK, CCS, RINA, IRS).",

        // ── OPERATIONAL ───────────────────────────────────────────────────────
        ["logbook"] = "Official Log Book (OLB): required by flag state, legally binding document. Must record: births/deaths, marriages on board (rare), crew changes, disciplinary actions, illness/injuries, collision/damage, significant weather events, stowaways, drug trafficking incidents. Entries: dated, signed by Master. Erasure prohibited – strike through with single line. Kept minimum 3 years. Reference: flag state merchant shipping regulations.",
        ["permit to work"] = "Permit to Work (PTW) system (ISM Code §10): Required for: enclosed space entry, hot work, work at height/overside, electrical isolation, hydraulic pressure, crane/lifting operations. Process: hazard identification → risk assessment → precautions → authorisation by Senior Officer → work execution → closeout. PTW valid for specific time/shift only. Cancel immediately if conditions change. Record and file PTWs.",
        ["toolbox talk"] = "Toolbox Talk (pre-task safety briefing): conducted before any non-routine or high-risk task. Covers: task description, roles/responsibilities, hazards identified, control measures (PPE, permits, equipment checks), emergency plan, communication. All participants sign attendance sheet. Typical topics: lifting operations, working at height, enclosed space, hot work, bunkering. Reference: ISM Code, ILO Guidelines on Occupational Safety.",
        ["bunkering"] = "Bunkering (fuel oil/MDO/LNG): Pre-bunkering meeting: agree quantity, grade, hose connection, communication signals, emergency stop. Scuppers plugged, absorbent materials ready, drip trays positioned. Bunker delivery note (BDN) received – retain minimum 3 years. MARPOL Annex VI: record in Oil Record Book Part II. Sample retained per MARPOL (sealed, 3 years). If spill: stop supply immediately, report to port authority, activate SOPEP. Reference: MARPOL Annex I & VI.",
    };

    public ChatService(
        ILogger<ChatService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ChatResponse> ProcessMessageAsync(string message, string? context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            return new ChatResponse { Reply = "Please enter a message.", Source = "system" };

        var provider = _configuration["AI:Provider"] ?? "Gemini";

        // Try Groq first if configured
        if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase))
        {
            var groqKey = _configuration["AI:GroqApiKey"];
            if (!string.IsNullOrEmpty(groqKey) && groqKey != "your-groq-api-key-here")
            {
                try
                {
                    var groqResponse = await CallGroqAsync(message, groqKey, context);
                    if (groqResponse != null)
                        return groqResponse;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Groq API call failed, falling back to offline knowledge base");
                }
            }
        }
        else
        {
            // Try Gemini API
            var apiKey = _configuration["AI:GeminiApiKey"];
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "your-gemini-api-key-here")
            {
                try
                {
                    var geminiResponse = await CallGeminiAsync(message, apiKey, context);
                    if (geminiResponse != null)
                        return geminiResponse;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gemini API call failed, falling back to offline knowledge base");
                }
            }
        }

        // Fallback: search offline knowledge base
        return SearchKnowledgeBase(message);
    }

    public List<SuggestedQuestion> GetSuggestedQuestions()
    {
        return
        [
            new() { Question = "What is the fire drill procedure?", Category = "Safety" },
            new() { Question = "Explain man overboard procedure", Category = "Safety" },
            new() { Question = "What are COLREGs?", Category = "Navigation" },
            new() { Question = "MARPOL garbage disposal rules?", Category = "Environmental" },
            new() { Question = "What are rest hour requirements?", Category = "Crew" },
            new() { Question = "Navigation lights for power-driven vessel?", Category = "Navigation" },
            new() { Question = "What is the STCW convention?", Category = "Crew" },
            new() { Question = "Ballast water management rules?", Category = "Environmental" },
        ];
    }

    private async Task<ChatResponse?> CallGroqAsync(string message, string apiKey, string? context)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var model = _configuration["AI:Model"] ?? "meta-llama/llama-4-scout-17b-16e-instruct";
        var maxTokens = int.TryParse(_configuration["AI:MaxOutputTokens"], out var t) ? t : 2048;
        var temperature = double.TryParse(_configuration["AI:Temperature"],
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var temp) ? temp : 0.7;

        var systemPrompt =
            "You are an intelligent assistant for maritime crew members aboard a vessel. " +
            "You can answer questions on ANY topic the crew member asks — not just maritime topics. " +
            "For maritime questions (safety, navigation, regulations, maintenance, crew duties), " +
            "give detailed, accurate answers referencing relevant IMO conventions (SOLAS, MARPOL, STCW, COLREGs, MLC 2006) when applicable. " +
            "For general questions (science, math, language, health, technology, etc.), answer helpfully and accurately. " +
            "For conversational messages (greetings, small talk), respond naturally and warmly. " +
            "Always be concise but complete. Use bullet points for procedures. " +
            "Respond in the same language the user writes in — Vietnamese if they write Vietnamese, English if English, etc. " +
            "If you are genuinely unsure about a safety-critical maritime procedure, recommend consulting the ship's Safety Management System (SMS) or senior officers.";

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = message + (context != null ? $"\n\nContext: {context}" : "") }
            },
            temperature,
            max_tokens = maxTokens,
        };

        var url = "https://api.groq.com/openai/v1/chat/completions";
        var response = await client.PostAsJsonAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Groq API returned {StatusCode}: {Body}", response.StatusCode, body[..Math.Min(300, body.Length)]);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var text = json.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return new ChatResponse
        {
            Reply = text ?? "No response generated.",
            Source = "gemini"  // reuse badge label
        };
    }

    private async Task<ChatResponse?> CallGeminiAsync(string message, string apiKey, string? context)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        var model = _configuration["AI:Model"] ?? "gemini-2.0-flash";
        var maxTokens = int.TryParse(_configuration["AI:MaxOutputTokens"], out var t) ? t : 2048;
        var temperature = double.TryParse(_configuration["AI:Temperature"],
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var temp) ? temp : 0.7;

        var systemPrompt =
            "You are an intelligent assistant for maritime crew members aboard a vessel. " +
            "You can answer questions on ANY topic the crew member asks — not just maritime topics. " +
            "For maritime questions (safety, navigation, regulations, maintenance, crew duties), " +
            "give detailed, accurate answers referencing relevant IMO conventions (SOLAS, MARPOL, STCW, COLREGs, MLC 2006) when applicable. " +
            "For general questions (science, math, language, health, technology, news, etc.), answer helpfully and accurately. " +
            "For conversational messages (greetings, small talk), respond naturally and warmly. " +
            "Always be concise but complete. Use bullet points for procedures. " +
            "Respond in the same language the user writes in — Vietnamese if they write Vietnamese, English if English, etc. " +
            "If you are genuinely unsure about a safety-critical maritime procedure, recommend consulting the ship's Safety Management System (SMS) or senior officers.";

        var userMessage = message + (context != null ? $"\n\nContext: {context}" : "");

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"System: {systemPrompt}\n\nUser: {userMessage}" }
                    }
                }
            },
            generationConfig = new
            {
                temperature,
                maxOutputTokens = maxTokens,
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
        HttpResponseMessage response;

        // Retry up to 3 times on 429 (rate limit)
        int attempt = 0;
        do
        {
            if (attempt > 0)
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
            response = await client.PostAsJsonAsync(url, requestBody);
            attempt++;
        } while ((int)response.StatusCode == 429 && attempt < 3);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Gemini API returned {StatusCode}: {Body}", response.StatusCode, body[..Math.Min(300, body.Length)]);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var text = json.GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return new ChatResponse
        {
            Reply = text ?? "No response generated.",
            Source = "gemini"
        };
    }

    // Maps synonyms/keywords → knowledge base key
    private static readonly Dictionary<string, string> SynonymMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── SAFETY ──────────────────────────────────────────────────────────
        ["fire"] = "fire drill",
        ["fire drill"] = "fire drill",
        ["fire alarm"] = "fire drill",
        ["fire fighting"] = "fire drill",
        ["fire emergency"] = "fire drill",
        ["fire procedure"] = "fire drill",
        ["fire signal"] = "fire drill",
        ["general alarm"] = "fire drill",
        ["diễn tập cháy"] = "fire drill",
        ["cứu hỏa"] = "fire drill",
        ["phòng cháy"] = "fire drill",
        ["báo cháy"] = "fire drill",
        ["chữa cháy"] = "fire drill",
        ["incendio"] = "fire drill",

        ["mob"] = "man overboard",
        ["man overboard"] = "man overboard",
        ["overboard"] = "man overboard",
        ["person overboard"] = "man overboard",
        ["fell overboard"] = "man overboard",
        ["rescue person"] = "man overboard",
        ["williamson turn"] = "man overboard",
        ["anderson turn"] = "man overboard",
        ["ngã xuống biển"] = "man overboard",
        ["người rơi xuống biển"] = "man overboard",

        ["abandon"] = "abandon ship",
        ["abandon ship"] = "abandon ship",
        ["evacuation"] = "abandon ship",
        ["evacuate"] = "abandon ship",
        ["bỏ tàu"] = "abandon ship",
        ["rời tàu"] = "abandon ship",
        ["tín hiệu bỏ tàu"] = "abandon ship",

        ["muster"] = "muster station",
        ["muster station"] = "muster station",
        ["assembly point"] = "muster station",
        ["muster list"] = "muster station",
        ["emergency station"] = "muster station",
        ["điểm tập kết"] = "muster station",
        ["trạm cứu sinh"] = "muster station",

        ["ppe"] = "ppe",
        ["personal protective"] = "ppe",
        ["safety equipment"] = "ppe",
        ["protective equipment"] = "ppe",
        ["hard hat"] = "ppe",
        ["safety shoes"] = "ppe",
        ["safety harness"] = "ppe",
        ["bảo hộ lao động"] = "ppe",
        ["đồ bảo hộ"] = "ppe",
        ["trang bị bảo hộ"] = "ppe",

        ["enclosed space"] = "enclosed space",
        ["confined space"] = "enclosed space",
        ["tank entry"] = "enclosed space",
        ["pump room"] = "enclosed space",
        ["void space"] = "enclosed space",
        ["cargo hold entry"] = "enclosed space",
        ["không gian kín"] = "enclosed space",
        ["vào két"] = "enclosed space",
        ["két kín"] = "enclosed space",

        ["hot work"] = "hot work",
        ["welding"] = "hot work",
        ["cutting"] = "hot work",
        ["grinding"] = "hot work",
        ["hàn"] = "hot work",
        ["hàn cắt"] = "hot work",
        ["công việc nóng"] = "hot work",
        ["giấy phép hàn"] = "hot work",

        ["lifeboat"] = "lifeboat",
        ["rescue boat"] = "lifeboat",
        ["lifeboat drill"] = "lifeboat",
        ["lowering lifeboat"] = "lifeboat",
        ["xuồng cứu sinh"] = "lifeboat",
        ["xuồng cứu hộ"] = "lifeboat",
        ["kiểm tra xuồng"] = "lifeboat",

        ["life raft"] = "life raft",
        ["liferaft"] = "life raft",
        ["bè cứu sinh"] = "life raft",
        ["phao bè"] = "life raft",

        ["immersion suit"] = "immersion suit",
        ["survival suit"] = "immersion suit",
        ["dry suit"] = "immersion suit",
        ["quần áo chống chìm"] = "immersion suit",
        ["bộ chống nước lạnh"] = "immersion suit",

        ["epirb"] = "epirb",
        ["emergency beacon"] = "epirb",
        ["406 mhz"] = "epirb",
        ["satellite beacon"] = "epirb",
        ["phao vô tuyến khẩn cấp"] = "epirb",

        ["scba"] = "breathing apparatus",
        ["breathing apparatus"] = "breathing apparatus",
        ["self contained breathing"] = "breathing apparatus",
        ["oxygen breathing"] = "breathing apparatus",
        ["bình thở"] = "breathing apparatus",
        ["thiết bị thở"] = "breathing apparatus",

        // ── NAVIGATION ──────────────────────────────────────────────────────
        ["colreg"] = "colreg",
        ["collision regulations"] = "colreg",
        ["rules of the road"] = "colreg",
        ["steering rules"] = "colreg",
        ["navigation law"] = "colreg",
        ["collision avoidance"] = "colreg",
        ["give way"] = "colreg",
        ["stand on vessel"] = "colreg",
        ["luật tránh va"] = "colreg",
        ["quy tắc tránh va"] = "colreg",
        ["quy định tránh đâm va"] = "colreg",

        ["light"] = "navigation lights",
        ["lights"] = "navigation lights",
        ["nav light"] = "navigation lights",
        ["navigation light"] = "navigation lights",
        ["running light"] = "navigation lights",
        ["masthead light"] = "navigation lights",
        ["sidelight"] = "navigation lights",
        ["stern light"] = "navigation lights",
        ["anchor light"] = "navigation lights",
        ["đèn hiệu"] = "navigation lights",
        ["đèn hàng hải"] = "navigation lights",
        ["đèn cột"] = "navigation lights",

        ["buoy"] = "buoyage",
        ["buoyage"] = "buoyage",
        ["iala"] = "buoyage",
        ["beacon"] = "buoyage",
        ["cardinal mark"] = "buoyage",
        ["lateral mark"] = "buoyage",
        ["safe water mark"] = "buoyage",
        ["phao tiêu"] = "buoyage",
        ["hệ thống phao"] = "buoyage",
        ["iala a"] = "buoyage",
        ["iala b"] = "buoyage",

        ["watch"] = "watchkeeping",
        ["watchkeeping"] = "watchkeeping",
        ["bridge watch"] = "watchkeeping",
        ["duty watch"] = "watchkeeping",
        ["oow"] = "watchkeeping",
        ["officer of the watch"] = "watchkeeping",
        ["lookout"] = "watchkeeping",
        ["canh trực"] = "watchkeeping",
        ["trực ca"] = "watchkeeping",
        ["sĩ quan trực ca"] = "watchkeeping",

        ["radar"] = "radar",
        ["arpa"] = "radar",
        ["cpa"] = "radar",
        ["tcpa"] = "radar",
        ["radar plotting"] = "radar",
        ["radar watch"] = "radar",
        ["ra đa"] = "radar",
        ["thiết bị ra đa"] = "radar",

        ["chart"] = "chart",
        ["nautical chart"] = "chart",
        ["ecdis"] = "chart",
        ["electronic chart"] = "chart",
        ["notice to mariners"] = "chart",
        ["ntm"] = "chart",
        ["hải đồ"] = "chart",
        ["bản đồ hàng hải"] = "chart",
        ["ecdis"] = "chart",

        ["anchor"] = "anchor",
        ["anchoring"] = "anchor",
        ["let go anchor"] = "anchor",
        ["swinging circle"] = "anchor",
        ["dragging anchor"] = "anchor",
        ["neo"] = "anchor",
        ["thả neo"] = "anchor",
        ["kéo neo"] = "anchor",

        ["mooring"] = "mooring",
        ["mooring lines"] = "mooring",
        ["berthing"] = "mooring",
        ["spring line"] = "mooring",
        ["breast line"] = "mooring",
        ["cột tàu"] = "mooring",
        ["dây cột tàu"] = "mooring",
        ["neo tàu"] = "mooring",

        ["passage plan"] = "passage planning",
        ["passage planning"] = "passage planning",
        ["voyage plan"] = "passage planning",
        ["route planning"] = "passage planning",
        ["kế hoạch hành trình"] = "passage planning",
        ["lập kế hoạch hành trình"] = "passage planning",

        ["gps"] = "gps",
        ["gnss"] = "gps",
        ["position fix"] = "gps",
        ["navigation system"] = "gps",
        ["định vị"] = "gps",
        ["hệ thống định vị"] = "gps",

        // ── ENVIRONMENTAL ───────────────────────────────────────────────────
        ["marpol"] = "marpol",
        ["pollution"] = "marpol",
        ["marine pollution"] = "marpol",
        ["annex i"] = "marpol",
        ["annex vi"] = "marpol",
        ["sulfur limit"] = "marpol",
        ["imo 2020"] = "marpol",
        ["ô nhiễm biển"] = "marpol",
        ["ngăn ngừa ô nhiễm"] = "marpol",

        ["garbage"] = "garbage disposal",
        ["waste"] = "garbage disposal",
        ["trash"] = "garbage disposal",
        ["rubbish"] = "garbage disposal",
        ["plastics overboard"] = "garbage disposal",
        ["garbage record"] = "garbage disposal",
        ["garbage management plan"] = "garbage disposal",
        ["rác thải"] = "garbage disposal",
        ["xử lý rác"] = "garbage disposal",
        ["sổ ghi rác"] = "garbage disposal",

        ["oil spill"] = "oil spill",
        ["spill"] = "oil spill",
        ["oil pollution"] = "oil spill",
        ["sopep"] = "oil spill",
        ["oil record book"] = "oil spill",
        ["tràn dầu"] = "oil spill",
        ["xử lý tràn dầu"] = "oil spill",
        ["sổ ghi dầu"] = "oil spill",

        ["ballast"] = "ballast water",
        ["ballast water"] = "ballast water",
        ["bwm"] = "ballast water",
        ["ballast exchange"] = "ballast water",
        ["nước dằn"] = "ballast water",
        ["quản lý nước dằn"] = "ballast water",

        ["sewage"] = "sewage",
        ["wastewater"] = "sewage",
        ["black water"] = "sewage",
        ["holding tank"] = "sewage",
        ["nước thải"] = "sewage",
        ["xử lý nước thải"] = "sewage",

        ["air pollution"] = "air pollution",
        ["exhaust emissions"] = "air pollution",
        ["sox"] = "air pollution",
        ["nox"] = "air pollution",
        ["eca"] = "air pollution",
        ["emission control"] = "air pollution",
        ["sulfur cap"] = "air pollution",
        ["khí thải"] = "air pollution",
        ["ô nhiễm không khí"] = "air pollution",

        // ── MAINTENANCE ─────────────────────────────────────────────────────
        ["maintenance"] = "planned maintenance",
        ["pms"] = "planned maintenance",
        ["planned maintenance"] = "planned maintenance",
        ["work order"] = "planned maintenance",
        ["maintenance schedule"] = "planned maintenance",
        ["bảo trì"] = "planned maintenance",
        ["bảo dưỡng"] = "planned maintenance",
        ["kế hoạch bảo dưỡng"] = "planned maintenance",

        ["dry dock"] = "drydock",
        ["drydock"] = "drydock",
        ["docking"] = "drydock",
        ["slipway"] = "drydock",
        ["special survey"] = "drydock",
        ["hull inspection"] = "drydock",
        ["anti fouling"] = "drydock",
        ["đà tàu"] = "drydock",
        ["sửa chữa tàu"] = "drydock",
        ["kiểm tra đáy tàu"] = "drydock",

        ["engine"] = "engine maintenance",
        ["engine maintenance"] = "engine maintenance",
        ["main engine"] = "engine maintenance",
        ["diesel engine"] = "engine maintenance",
        ["engine overhaul"] = "engine maintenance",
        ["piston"] = "engine maintenance",
        ["fuel injector"] = "engine maintenance",
        ["turbocharger"] = "engine maintenance",
        ["máy chính"] = "engine maintenance",
        ["động cơ chính"] = "engine maintenance",
        ["bảo dưỡng máy"] = "engine maintenance",
        ["đại tu máy"] = "engine maintenance",

        ["generator"] = "generator",
        ["aux engine"] = "generator",
        ["auxiliary engine"] = "generator",
        ["diesel generator"] = "generator",
        ["blackout"] = "generator",
        ["power failure"] = "generator",
        ["máy phát điện"] = "generator",
        ["mất điện"] = "generator",

        ["boiler"] = "boiler",
        ["steam boiler"] = "boiler",
        ["exhaust gas boiler"] = "boiler",
        ["nồi hơi"] = "boiler",

        ["bilge"] = "pump",
        ["bilge pump"] = "pump",
        ["pump"] = "pump",
        ["ows"] = "pump",
        ["oily water separator"] = "pump",
        ["bơm la canh"] = "pump",
        ["tách dầu nước"] = "pump",

        ["steering"] = "steering gear",
        ["steering gear"] = "steering gear",
        ["rudder"] = "steering gear",
        ["hydraulic steering"] = "steering gear",
        ["máy lái"] = "steering gear",
        ["hệ thống lái"] = "steering gear",
        ["bánh lái"] = "steering gear",

        // ── CARGO ───────────────────────────────────────────────────────────
        ["cargo"] = "cargo loading",
        ["loading"] = "cargo loading",
        ["cargo loading"] = "cargo loading",
        ["stowage"] = "cargo loading",
        ["cargo plan"] = "cargo loading",
        ["draught"] = "cargo loading",
        ["xếp hàng"] = "cargo loading",
        ["kế hoạch xếp hàng"] = "cargo loading",

        ["dangerous goods"] = "dangerous goods",
        ["imdg"] = "dangerous goods",
        ["hazmat"] = "dangerous goods",
        ["hazardous cargo"] = "dangerous goods",
        ["dg cargo"] = "dangerous goods",
        ["hàng nguy hiểm"] = "dangerous goods",
        ["hàng hóa nguy hiểm"] = "dangerous goods",

        ["tanker"] = "tanker operations",
        ["oil tanker"] = "tanker operations",
        ["chemical tanker"] = "tanker operations",
        ["igs"] = "tanker operations",
        ["inert gas"] = "tanker operations",
        ["cow"] = "tanker operations",
        ["ship shore safety checklist"] = "tanker operations",
        ["tàu chở dầu"] = "tanker operations",
        ["hoạt động tàu dầu"] = "tanker operations",

        ["bulk carrier"] = "bulk carrier",
        ["bulk cargo"] = "bulk carrier",
        ["grain"] = "bulk carrier",
        ["coal cargo"] = "bulk carrier",
        ["tml"] = "bulk carrier",
        ["liquefaction"] = "bulk carrier",
        ["tàu hàng rời"] = "bulk carrier",
        ["hàng rời"] = "bulk carrier",

        ["stability"] = "stability",
        ["gm"] = "stability",
        ["metacentric height"] = "stability",
        ["list"] = "stability",
        ["trim"] = "stability",
        ["loading computer"] = "stability",
        ["ổn định tàu"] = "stability",
        ["ổn tính"] = "stability",
        ["tính ổn định"] = "stability",

        ["lashing"] = "lashing",
        ["cargo securing"] = "lashing",
        ["securing"] = "lashing",
        ["twist lock"] = "lashing",
        ["dunnage"] = "lashing",
        ["chằng buộc hàng"] = "lashing",
        ["cố định hàng"] = "lashing",

        // ── CREW & CERTS ────────────────────────────────────────────────────
        ["stcw"] = "stcw",
        ["training"] = "stcw",
        ["certification"] = "stcw",
        ["certificate"] = "stcw",
        ["seafarer training"] = "stcw",
        ["coc"] = "stcw",
        ["cop"] = "stcw",
        ["basic safety"] = "stcw",
        ["bst"] = "stcw",
        ["đào tạo"] = "stcw",
        ["chứng chỉ"] = "stcw",
        ["bằng cấp"] = "stcw",
        ["huấn luyện"] = "stcw",

        ["rest hour"] = "rest hours",
        ["rest hours"] = "rest hours",
        ["working hours"] = "rest hours",
        ["hours of rest"] = "rest hours",
        ["fatigue"] = "rest hours",
        ["overtime"] = "rest hours",
        ["giờ nghỉ"] = "rest hours",
        ["thời gian nghỉ"] = "rest hours",
        ["mệt mỏi"] = "rest hours",

        ["mlc"] = "mlc",
        ["maritime labour"] = "mlc",
        ["seafarer rights"] = "mlc",
        ["crew rights"] = "mlc",
        ["sea contract"] = "mlc",
        ["sea agreement"] = "mlc",
        ["quyền thuyền viên"] = "mlc",
        ["lao động hàng hải"] = "mlc",
        ["hợp đồng thuyền viên"] = "mlc",

        ["crew health"] = "crew health",
        ["medical"] = "crew health",
        ["medical care"] = "crew health",
        ["doctor"] = "crew health",
        ["sick"] = "crew health",
        ["illness"] = "crew health",
        ["injury"] = "crew health",
        ["medicine chest"] = "crew health",
        ["y tế"] = "crew health",
        ["sức khỏe thuyền viên"] = "crew health",
        ["ốm đau"] = "crew health",
        ["chấn thương"] = "crew health",

        ["wages"] = "wages",
        ["salary"] = "wages",
        ["pay"] = "wages",
        ["allotment"] = "wages",
        ["lương"] = "wages",
        ["tiền lương"] = "wages",
        ["trả lương"] = "wages",

        ["repatriation"] = "repatriation",
        ["going home"] = "repatriation",
        ["end of contract"] = "repatriation",
        ["hồi hương"] = "repatriation",
        ["về nhà"] = "repatriation",
        ["kết thúc hợp đồng"] = "repatriation",

        // ── GMDSS & COMMS ────────────────────────────────────────────────────
        ["distress"] = "distress signals",
        ["distress signal"] = "distress signals",
        ["mayday"] = "distress signals",
        ["sos"] = "distress signals",
        ["pan pan"] = "distress signals",
        ["securite"] = "distress signals",
        ["emergency signal"] = "distress signals",
        ["tín hiệu cứu nạn"] = "distress signals",
        ["kêu cứu"] = "distress signals",

        ["vhf"] = "vhf channels",
        ["vhf radio"] = "vhf channels",
        ["channel 16"] = "vhf channels",
        ["ch 16"] = "vhf channels",
        ["radio channel"] = "vhf channels",
        ["bộ đàm"] = "vhf channels",
        ["kênh vhf"] = "vhf channels",
        ["liên lạc vô tuyến"] = "vhf channels",

        ["gmdss"] = "gmdss",
        ["dsc"] = "gmdss",
        ["digital selective calling"] = "gmdss",
        ["navtex"] = "gmdss",
        ["sart"] = "gmdss",
        ["sea area"] = "gmdss",
        ["hệ thống cứu nạn hàng hải"] = "gmdss",

        ["inmarsat"] = "inmarsat",
        ["satellite phone"] = "inmarsat",
        ["fleet broadband"] = "inmarsat",
        ["vsat"] = "inmarsat",
        ["liên lạc vệ tinh"] = "inmarsat",
        ["điện thoại vệ tinh"] = "inmarsat",

        // ── PSC & INSPECTIONS ────────────────────────────────────────────────
        ["psc"] = "port state control",
        ["port state control"] = "port state control",
        ["inspection"] = "port state control",
        ["psc inspection"] = "port state control",
        ["detention"] = "port state control",
        ["deficiency"] = "port state control",
        ["kiểm tra cảng quốc gia"] = "port state control",
        ["thanh tra cảng"] = "port state control",
        ["bị giữ tàu"] = "port state control",

        ["ism"] = "ism code",
        ["ism code"] = "ism code",
        ["safety management"] = "ism code",
        ["sms"] = "ism code",
        ["smc"] = "ism code",
        ["doc"] = "ism code",
        ["dpa"] = "ism code",
        ["non conformity"] = "ism code",
        ["quản lý an toàn"] = "ism code",
        ["hệ thống quản lý an toàn"] = "ism code",

        ["isps"] = "isps code",
        ["isps code"] = "isps code",
        ["ship security"] = "isps code",
        ["security level"] = "isps code",
        ["security plan"] = "isps code",
        ["sso"] = "isps code",
        ["ssas"] = "isps code",
        ["an ninh tàu"] = "isps code",
        ["kế hoạch an ninh"] = "isps code",

        ["flag state"] = "flag state",
        ["flag"] = "flag state",
        ["registry"] = "flag state",
        ["open registry"] = "flag state",
        ["quốc gia đăng ký"] = "flag state",
        ["cờ tàu"] = "flag state",

        ["certificate"] = "certificates",
        ["ship certificates"] = "certificates",
        ["iopp"] = "certificates",
        ["load line"] = "certificates",
        ["solas certificate"] = "certificates",
        ["giấy chứng nhận tàu"] = "certificates",
        ["chứng nhận an toàn"] = "certificates",

        ["survey"] = "survey",
        ["annual survey"] = "survey",
        ["class survey"] = "survey",
        ["special survey"] = "survey",
        ["esp"] = "survey",
        ["kiểm tra thường niên"] = "survey",
        ["đăng kiểm"] = "survey",

        // ── OPERATIONAL ─────────────────────────────────────────────────────
        ["logbook"] = "logbook",
        ["log book"] = "logbook",
        ["official log"] = "logbook",
        ["engine log"] = "logbook",
        ["deck log"] = "logbook",
        ["nhật ký tàu"] = "logbook",
        ["sổ nhật ký"] = "logbook",

        ["ptw"] = "permit to work",
        ["permit to work"] = "permit to work",
        ["work permit"] = "permit to work",
        ["giấy phép làm việc"] = "permit to work",
        ["phiếu công việc"] = "permit to work",

        ["toolbox talk"] = "toolbox talk",
        ["safety briefing"] = "toolbox talk",
        ["pre task briefing"] = "toolbox talk",
        ["họp an toàn"] = "toolbox talk",
        ["họp trước ca làm việc"] = "toolbox talk",

        ["bunkering"] = "bunkering",
        ["bunker"] = "bunkering",
        ["fueling"] = "bunkering",
        ["fuel transfer"] = "bunkering",
        ["bdn"] = "bunkering",
        ["tiếp nhận nhiên liệu"] = "bunkering",
        ["nạp dầu"] = "bunkering",

        // ── MEDICAL ─────────────────────────────────────────────────────────
        ["first aid"] = "first aid",
        ["cpr"] = "cpr",
        ["resuscitation"] = "cpr",
        ["cardiac arrest"] = "cpr",
        ["aed"] = "cpr",
        ["sơ cứu"] = "first aid",
        ["cấp cứu"] = "first aid",
        ["hô hấp nhân tạo"] = "cpr",
        ["tim phổi nhân tạo"] = "cpr",

        ["seasick"] = "seasickness",
        ["seasickness"] = "seasickness",
        ["motion sickness"] = "seasickness",
        ["nausea"] = "seasickness",
        ["say sóng"] = "seasickness",
        ["say tàu"] = "seasickness",

        ["hypothermia"] = "hypothermia",
        ["cold water"] = "hypothermia",
        ["cold exposure"] = "hypothermia",
        ["hạ thân nhiệt"] = "hypothermia",
        ["nước lạnh"] = "hypothermia",

        // ── WEATHER ─────────────────────────────────────────────────────────
        ["weather"] = "weather",
        ["forecast"] = "weather",
        ["beaufort"] = "weather",
        ["wave height"] = "weather",
        ["heavy weather"] = "weather",
        ["storm"] = "tropical cyclone",
        ["thời tiết"] = "weather",
        ["dự báo thời tiết"] = "weather",
        ["sóng"] = "weather",

        ["typhoon"] = "tropical cyclone",
        ["hurricane"] = "tropical cyclone",
        ["cyclone"] = "tropical cyclone",
        ["tropical storm"] = "tropical cyclone",
        ["bão"] = "tropical cyclone",
        ["siêu bão"] = "tropical cyclone",
        ["bão nhiệt đới"] = "tropical cyclone",

        ["tide"] = "tides",
        ["tides"] = "tides",
        ["tidal current"] = "tides",
        ["ukc"] = "tides",
        ["under keel clearance"] = "tides",
        ["squat"] = "tides",
        ["thủy triều"] = "tides",
        ["dòng triều"] = "tides",
        ["dự báo thủy triều"] = "tides",

        ["fog"] = "fog",
        ["reduced visibility"] = "fog",
        ["restricted visibility"] = "fog",
        ["fog signal"] = "fog",
        ["sương mù"] = "fog",
        ["tầm nhìn hạn chế"] = "fog",
    };

    private static readonly HashSet<string> GreetingWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "hello", "hi", "hey", "xin chao", "xin chào", "chào", "chao", "chao ban", "chào bạn",
        "good morning", "good afternoon", "good evening", "greetings", "howdy",
        "xin chào bạn", "bạn ơi", "ban oi", "chào buổi sáng", "chào buổi chiều",
        "hola", "bonjour", "salut", "ciao", "yo", "sup",
    };

    private ChatResponse SearchKnowledgeBase(string message)
    {
        var lowerMessage = message.Trim().ToLowerInvariant();

        // 1. Handle greetings
        if (GreetingWords.Contains(lowerMessage) ||
            GreetingWords.Any(g => lowerMessage == g || lowerMessage.StartsWith(g + " ") || lowerMessage.StartsWith(g + "!")))
        {
            return new ChatResponse
            {
                Reply = "Hello! I'm your maritime assistant. I can help you with:\n" +
                        "• Safety procedures (fire drill, man overboard, abandon ship)\n" +
                        "• Navigation rules (COLREGs, navigation lights)\n" +
                        "• Environmental regulations (MARPOL)\n" +
                        "• Crew standards (STCW, rest hours, MLC)\n" +
                        "• Maintenance procedures\n\n" +
                        "What would you like to know?",
                Source = "knowledge_base"
            };
        }

        // 2. Try synonym map first (handles "training" → "stcw", "fire" → "fire drill", etc.)
        var synonymMatch = SynonymMap
            .Where(kv => lowerMessage.Contains(kv.Key))
            .OrderByDescending(kv => kv.Key.Length) // Prefer longer/more specific matches
            .FirstOrDefault();

        if (synonymMatch.Value != null && MaritimeKnowledge.TryGetValue(synonymMatch.Value, out var synonymAnswer))
        {
            return new ChatResponse { Reply = synonymAnswer, Source = "knowledge_base" };
        }

        // 3. Try exact key match in knowledge base
        var exactMatch = MaritimeKnowledge
            .Where(kv => lowerMessage.Contains(kv.Key) ||
                         kv.Key.Split(' ').All(word => lowerMessage.Contains(word)))
            .OrderByDescending(kv => kv.Key.Length)
            .FirstOrDefault();

        if (exactMatch.Value != null)
        {
            return new ChatResponse { Reply = exactMatch.Value, Source = "knowledge_base" };
        }

        // 4. No match — show helpful suggestions
        return new ChatResponse
        {
            Reply = "I'm not sure about that topic in offline mode. Try asking:\n" +
                    "• \"fire drill\" or \"fire procedure\"\n" +
                    "• \"man overboard\" or \"MOB\"\n" +
                    "• \"COLREGs\" or \"navigation lights\"\n" +
                    "• \"MARPOL\" or \"garbage disposal\"\n" +
                    "• \"STCW\" or \"training\" or \"certification\"\n" +
                    "• \"rest hours\" or \"watchkeeping\"\n" +
                    "• \"maintenance\" or \"engine\"\n" +
                    "• \"distress\" or \"VHF\" or \"Mayday\"\n\n" +
                    "Connect to server for more comprehensive answers.",
            Source = "fallback"
        };
    }
}

public class ChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public class SuggestedQuestion
{
    public string Question { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Context { get; set; }
}
