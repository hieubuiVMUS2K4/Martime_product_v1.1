# Xây dựng Giao diện Quản lý Hải trình như MarineTraffic

## ⚠️ Lưu ý quan trọng

**KHÔNG THỂ** lấy code/API của MarineTraffic vì:
1. ❌ Vi phạm bản quyền
2. ❌ Code của họ là proprietary (độc quyền)
3. ❌ API của họ tính phí và có giới hạn
4. ❌ Vi phạm Terms of Service

## ✅ Giải pháp: Xây dựng tương tự với công nghệ mở

### 🗺️ 1. Map Libraries (Thư viện bản đồ)

#### Option 1: Leaflet (Recommended - Miễn phí)
```bash
npm install leaflet react-leaflet
npm install --save-dev @types/leaflet
```

**Ưu điểm:**
- ✅ Miễn phí, open-source
- ✅ Nhẹ, dễ sử dụng
- ✅ Hỗ trợ markers, polylines, popups
- ✅ Plugin phong phú

**Ví dụ code:**
```tsx
import { MapContainer, TileLayer, Marker, Popup, Polyline } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';

const VoyageMap = () => {
  const route = [
    [21.0285, 105.8542], // Hanoi
    [16.0544, 108.2022], // Da Nang
    [10.8231, 106.6297], // Ho Chi Minh
  ];

  return (
    <MapContainer center={[16.0, 108.0]} zoom={6} style={{ height: '600px' }}>
      <TileLayer
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        attribution='&copy; OpenStreetMap contributors'
      />
      <Polyline positions={route} color="blue" />
      <Marker position={route[0]}>
        <Popup>Starting Port</Popup>
      </Marker>
    </MapContainer>
  );
};
```

#### Option 2: Google Maps API (Tính phí nhưng có $200 credit/tháng)
```bash
npm install @react-google-maps/api
```

#### Option 3: Mapbox (Miễn phí đến 50k requests/tháng)
```bash
npm install mapbox-gl
npm install react-map-gl
```

---

### 🚢 2. Ship Tracking Data Sources

#### A. AIS Data APIs (Automatic Identification System)

##### AISHub (Miễn phí - Community)
```bash
API: http://www.aishub.net/
- Miễn phí với giới hạn
- Cần đăng ký API key
```

##### VesselFinder API (Freemium)
```bash
https://www.vesselfinder.com/api
- Basic plan: Miễn phí
- Pro: $99/tháng
```

##### MarineTraffic API (Tính phí)
```bash
https://www.marinetraffic.com/en/ais-api-services
- Có API chính thức nhưng đắt ($500+/tháng)
```

##### AIS Stream (Open Source Alternative)
```bash
https://aisstream.io/
- WebSocket real-time
- Miễn phí tier: 15k messages/day
```

#### B. Mock Data (Development)
```typescript
// types/voyage.types.ts
export interface VoyagePosition {
  lat: number;
  lng: number;
  timestamp: string;
  speed: number;
  heading: number;
}

export interface Vessel {
  id: string;
  name: string;
  mmsi: string; // Maritime Mobile Service Identity
  imo: string;  // International Maritime Organization number
  type: string;
  currentPosition: VoyagePosition;
  route: VoyagePosition[];
  destination: string;
  eta: string;
}
```

---

### 🏗️ 3. Architecture cho Voyage Management

```
frontend/src/pages/VoyageManagement/
├── VoyageManagementPage.tsx       # Main page
├── VoyageManagementPage.css
├── components/
│   ├── VoyageMap/
│   │   ├── VoyageMap.tsx          # Map component
│   │   ├── VoyageMap.css
│   │   └── index.ts
│   ├── VesselMarker/
│   │   ├── VesselMarker.tsx       # Custom vessel icon
│   │   └── VesselMarker.css
│   ├── RoutePolyline/
│   │   └── RoutePolyline.tsx      # Draw route on map
│   ├── VesselInfo/
│   │   ├── VesselInfoPanel.tsx    # Side panel info
│   │   └── VesselInfoPanel.css
│   └── VoyageControls/
│       ├── VoyageControls.tsx     # Play/pause, speed controls
│       └── VoyageControls.css
└── index.ts
```

---

### 📦 4. Implementation Plan

#### Phase 1: Basic Map Display (Week 1)
```bash
# Install dependencies
npm install leaflet react-leaflet
npm install --save-dev @types/leaflet

# Features:
- Display map with OpenStreetMap tiles
- Add single vessel marker
- Show vessel info popup
```

#### Phase 2: Route Visualization (Week 2)
```bash
# Features:
- Draw vessel route (polyline)
- Multiple waypoints
- Port markers
- Distance calculation
```

#### Phase 3: Real-time Tracking (Week 3)
```bash
# Features:
- Integrate AIS API (AISHub or AIS Stream)
- WebSocket connection
- Live position updates
- Animation movement
```

#### Phase 4: Advanced Features (Week 4)
```bash
# Features:
- Historical route playback
- Speed/heading indicators
- Weather overlay (optional)
- Multiple vessels tracking
```

---

### 💻 5. Sample Code Structure

#### A. Voyage Management Page
```tsx
// pages/VoyageManagement/VoyageManagementPage.tsx
import React, { useState, useEffect } from 'react';
import { VoyageMap } from './components/VoyageMap';
import { VesselInfoPanel } from './components/VesselInfoPanel';
import { VoyageControls } from './components/VoyageControls';
import type { Vessel } from '../../types/voyage.types';
import './VoyageManagementPage.css';

export const VoyageManagementPage: React.FC = () => {
  const [selectedVessel, setSelectedVessel] = useState<Vessel | null>(null);
  const [vessels, setVessels] = useState<Vessel[]>([]);
  const [isLive, setIsLive] = useState(true);

  useEffect(() => {
    // Fetch vessels from API
    fetchVessels();
    
    // Setup WebSocket for live updates
    if (isLive) {
      setupWebSocket();
    }
  }, [isLive]);

  return (
    <div className="voyage-management-page">
      <div className="map-container">
        <VoyageMap
          vessels={vessels}
          selectedVessel={selectedVessel}
          onVesselClick={setSelectedVessel}
        />
        <VoyageControls
          isLive={isLive}
          onToggleLive={() => setIsLive(!isLive)}
        />
      </div>
      
      {selectedVessel && (
        <VesselInfoPanel
          vessel={selectedVessel}
          onClose={() => setSelectedVessel(null)}
        />
      )}
    </div>
  );
};
```

#### B. Map Component with Leaflet
```tsx
// components/VoyageMap/VoyageMap.tsx
import React from 'react';
import { MapContainer, TileLayer, Marker, Polyline, Popup } from 'react-leaflet';
import L from 'leaflet';
import type { Vessel } from '../../../types/voyage.types';
import './VoyageMap.css';

// Custom vessel icon
const vesselIcon = new L.Icon({
  iconUrl: '/icons/ship-icon.png',
  iconSize: [32, 32],
  iconAnchor: [16, 16],
  popupAnchor: [0, -16],
});

interface VoyageMapProps {
  vessels: Vessel[];
  selectedVessel: Vessel | null;
  onVesselClick: (vessel: Vessel) => void;
}

export const VoyageMap: React.FC<VoyageMapProps> = ({
  vessels,
  selectedVessel,
  onVesselClick,
}) => {
  return (
    <MapContainer
      center={[16.0, 108.0]}
      zoom={6}
      style={{ height: '100%', width: '100%' }}
    >
      <TileLayer
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        attribution='&copy; OpenStreetMap contributors'
      />

      {vessels.map((vessel) => (
        <React.Fragment key={vessel.id}>
          {/* Vessel marker */}
          <Marker
            position={[vessel.currentPosition.lat, vessel.currentPosition.lng]}
            icon={vesselIcon}
            eventHandlers={{
              click: () => onVesselClick(vessel),
            }}
          >
            <Popup>
              <div>
                <strong>{vessel.name}</strong>
                <p>Speed: {vessel.currentPosition.speed} knots</p>
                <p>Destination: {vessel.destination}</p>
              </div>
            </Popup>
          </Marker>

          {/* Route polyline */}
          {vessel.route && vessel.route.length > 0 && (
            <Polyline
              positions={vessel.route.map((pos) => [pos.lat, pos.lng])}
              color={selectedVessel?.id === vessel.id ? 'blue' : 'gray'}
              weight={selectedVessel?.id === vessel.id ? 3 : 2}
              opacity={selectedVessel?.id === vessel.id ? 1 : 0.5}
            />
          )}
        </React.Fragment>
      ))}
    </MapContainer>
  );
};
```

#### C. Vessel Info Panel
```tsx
// components/VesselInfo/VesselInfoPanel.tsx
import React from 'react';
import { Card, CardHeader, CardBody } from '../../../components/common';
import type { Vessel } from '../../../types/voyage.types';
import './VesselInfoPanel.css';

interface VesselInfoPanelProps {
  vessel: Vessel;
  onClose: () => void;
}

export const VesselInfoPanel: React.FC<VesselInfoPanelProps> = ({
  vessel,
  onClose,
}) => {
  return (
    <div className="vessel-info-panel">
      <Card>
        <CardHeader>
          <h3>{vessel.name}</h3>
          <button onClick={onClose}>×</button>
        </CardHeader>
        <CardBody>
          <div className="info-grid">
            <div className="info-item">
              <label>MMSI</label>
              <span>{vessel.mmsi}</span>
            </div>
            <div className="info-item">
              <label>IMO</label>
              <span>{vessel.imo}</span>
            </div>
            <div className="info-item">
              <label>Type</label>
              <span>{vessel.type}</span>
            </div>
            <div className="info-item">
              <label>Speed</label>
              <span>{vessel.currentPosition.speed} knots</span>
            </div>
            <div className="info-item">
              <label>Heading</label>
              <span>{vessel.currentPosition.heading}°</span>
            </div>
            <div className="info-item">
              <label>Destination</label>
              <span>{vessel.destination}</span>
            </div>
            <div className="info-item">
              <label>ETA</label>
              <span>{vessel.eta}</span>
            </div>
            <div className="info-item full-width">
              <label>Last Update</label>
              <span>{vessel.currentPosition.timestamp}</span>
            </div>
          </div>
        </CardBody>
      </Card>
    </div>
  );
};
```

---

### 🎨 6. Features như MarineTraffic

#### ✅ Có thể làm (Miễn phí/Rẻ):
1. **Map Display** - OpenStreetMap (miễn phí)
2. **Vessel Markers** - Custom icons
3. **Route Drawing** - Polylines
4. **Vessel Info** - Side panel/popup
5. **Historical Routes** - Database lưu trữ
6. **Multiple Vessels** - Không giới hạn
7. **Search & Filter** - Frontend logic
8. **Export Data** - CSV/Excel

#### ⚠️ Khó làm (Cần API tính phí):
1. **Real-time AIS data** - Cần API như AISHub ($)
2. **Global coverage** - Cần nhiều data sources
3. **High-frequency updates** - WebSocket premium
4. **Satellite tracking** - Rất đắt
5. **Weather overlay** - Weather API ($)

---

### 💰 7. Cost Estimation

| Service | Free Tier | Paid Tier |
|---------|-----------|-----------|
| **Leaflet** | ✅ Free forever | - |
| **OpenStreetMap** | ✅ Free | - |
| **AIS Stream** | ✅ 15k msgs/day | $49/mo (unlimited) |
| **VesselFinder API** | ✅ Basic | $99/mo (Pro) |
| **Mapbox** | ✅ 50k loads/mo | $5 per 1k loads |
| **Google Maps** | ✅ $200 credit/mo | Pay as you go |

**Recommended cho Development:**
- Leaflet + OpenStreetMap: **$0**
- AIS Stream Free Tier: **$0**
- Mock data: **$0**
- **Total: $0/month**

**Recommended cho Production:**
- Leaflet + OpenStreetMap: **$0**
- AIS Stream Paid: **$49/month**
- **Total: $49/month**

---

### 🚀 8. Quick Start Steps

#### Step 1: Install Dependencies
```bash
cd frontend
npm install leaflet react-leaflet
npm install --save-dev @types/leaflet
```

#### Step 2: Create Types
```bash
# Create types/voyage.types.ts
# (Code mẫu ở trên)
```

#### Step 3: Create Basic Map Page
```bash
# Create pages/VoyageManagement/
# (Code mẫu ở trên)
```

#### Step 4: Add Mock Data
```typescript
// services/voyage.service.ts
export const VoyageService = {
  getMockVessels: (): Vessel[] => [
    {
      id: '1',
      name: 'VESSEL ONE',
      mmsi: '123456789',
      imo: 'IMO1234567',
      type: 'Cargo',
      currentPosition: {
        lat: 16.0544,
        lng: 108.2022,
        timestamp: new Date().toISOString(),
        speed: 12.5,
        heading: 45,
      },
      route: [
        { lat: 21.0285, lng: 105.8542, timestamp: '', speed: 10, heading: 180 },
        { lat: 16.0544, lng: 108.2022, timestamp: '', speed: 12, heading: 90 },
      ],
      destination: 'HO CHI MINH',
      eta: '2025-10-10 14:00',
    },
  ],
};
```

#### Step 5: Add Route
```tsx
// routes/AppRoutes.tsx
<Route path="/voyages" element={<VoyageManagementPage />} />
```

#### Step 6: Test
```bash
npm run dev
# Navigate to /voyages
```

---

### 📚 9. Resources & Documentation

#### Leaflet
- Docs: https://leafletjs.com/
- React Leaflet: https://react-leaflet.js.org/

#### AIS Data
- AIS Stream: https://aisstream.io/
- AISHub: http://www.aishub.net/
- VesselFinder: https://www.vesselfinder.com/

#### Icon Sets
- Ship Icons: https://www.flaticon.com/free-icons/ship
- Maritime Icons: https://fontawesome.com/icons/ship

#### Learning
- Leaflet Tutorials: https://leafletjs.com/examples.html
- GeoJSON: https://geojson.org/
- AIS Protocol: https://en.wikipedia.org/wiki/Automatic_identification_system

---

### 🎯 10. Development Timeline

#### Week 1: Foundation
- [x] Install Leaflet
- [x] Basic map display
- [x] Single vessel marker
- [x] Route drawing

#### Week 2: Features
- [ ] Multiple vessels
- [ ] Vessel info panel
- [ ] Search & filter
- [ ] Controls (zoom, pan)

#### Week 3: Data Integration
- [ ] AIS API integration
- [ ] WebSocket real-time
- [ ] Historical data
- [ ] Database storage

#### Week 4: Polish
- [ ] Animations
- [ ] Performance optimization
- [ ] Mobile responsive
- [ ] Testing

---

## 🎊 Conclusion

**KHÔNG nên** lấy code/API của MarineTraffic (vi phạm pháp luật).

**NÊN** xây dựng tương tự với:
- ✅ Leaflet (map library - miễn phí)
- ✅ OpenStreetMap (tiles - miễn phí)
- ✅ AIS Stream hoặc AISHub (data - freemium)
- ✅ Custom development (full control)

**Chi phí:** $0 - $49/tháng tùy nhu cầu!

---

Bạn muốn tôi tạo code implementation đầy tiên cho Voyage Management page không? 🚢🗺️
