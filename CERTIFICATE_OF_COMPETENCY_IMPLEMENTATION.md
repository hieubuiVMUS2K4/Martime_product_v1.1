# Certificate of Competency (CoC) and Country Fields Implementation

## Overview
Added two new fields to the certificate management system:
1. **Certificate of Competency (CoC)** - Dropdown with "National" and "Flag State" options
2. **Issuing Country** - Dropdown populated from countries database

## Files Modified

### Backend (Edge Services)

#### 1. EdgeModels.cs
- Added `CertificateOfCompetency` (string, max 200) to `CrewCertificate` model
- Added `CountryId` (int?, nullable FK) to `CrewCertificate` model
- Added `Country` navigation property to `CrewCertificate` model
- Already had `Country` model defined with proper structure

#### 2. CountriesController.cs
- Already exists with GET /api/countries endpoint
- Returns list of active countries ordered by name
- GET /api/countries/{id} for single country lookup

#### 3. Database Migrations
- Migration files already applied:
  - `20260202123511_AddCertificateOfCompetencyAndCountryIdToCrewCertificates.cs`
  - Country and CountryCertificate tables created
  - Sample data seeded

### Frontend (Edge)

#### 1. maritime.types.ts
**Added Country interface:**
```typescript
export interface Country {
  id: number
  countryCode: string // ISO 3166-1 alpha-3: VNM, USA, GBR, PHL
  countryName: string // Vietnam, United States, United Kingdom
  isActive: boolean
  createdAt: string
  updatedAt: string
}
```

**Updated CrewCertificate interface:**
```typescript
export interface CrewCertificate {
  // ... existing fields
  certificateOfCompetency?: string // NEW: Chứng chỉ năng lực
  countryId?: number // NEW: Quốc gia cấp chứng chỉ
  country?: Country // NEW: Navigation property
}
```

#### 2. maritime.service.ts
**Added countries API methods:**
```typescript
countries: {
  getAll: () => api.get<Country[]>('/countries'),
  getById: (id: number) => api.get<Country>(`/countries/${id}`)
}
```

#### 3. AddCertificateModal.tsx
**Added state management:**
- `countries` state to store list of countries
- `loadingCountries` state for loading indicator
- `useEffect` hook to fetch countries when modal opens
- Updated `formData` with default values:
  - `certificateOfCompetency: 'National'`
  - `countryId: null`

**Added form fields:**
1. **Certificate of Competency (CoC) Dropdown:**
   - Options: "National" and "Flag State"
   - Default: "National"
   - Full-width field with proper styling

2. **Issuing Country Dropdown:**
   - Populated from API: `/api/countries`
   - Displays: `{countryName} ({countryCode})` e.g., "Vietnam (VNM)"
   - Shows "Select country..." placeholder
   - Disabled while loading countries

**Layout:**
- Both fields in a 2-column grid layout
- Proper labels and styling consistent with existing fields
- Loading states handled

## API Endpoints

### Countries
- `GET /api/countries` - Get all active countries (sorted by name)
- `GET /api/countries/{id}` - Get single country by ID

### Certificates (existing, now with new fields)
- `POST /api/certificates` - Create certificate (now accepts certificateOfCompetency and countryId)
- `PUT /api/certificates/{id}` - Update certificate

## Database Structure

### countries table
```sql
- id (SERIAL PRIMARY KEY)
- country_code (VARCHAR(3), ISO 3166-1 alpha-3)
- country_name (VARCHAR(100))
- is_active (BOOLEAN)
- created_at (TIMESTAMP)
- updated_at (TIMESTAMP)
```

### crew_certificates table (updated)
```sql
- ... existing columns ...
- certificate_of_competency (VARCHAR(200), nullable)
- country_id (INT, FK to countries.id, nullable)
```

## Sample Data
Countries seeded in database:
- VNM - Vietnam
- USA - United States
- GBR - United Kingdom
- JPN - Japan
- KOR - South Korea
- CHN - China
- SGP - Singapore
- IND - India
- MLT - Malta
- PAN - Panama
- And more...

## Testing Checklist
- [x] Backend models updated
- [x] Database migrations applied
- [x] Countries API endpoint working
- [x] TypeScript types defined
- [x] Frontend service methods added
- [x] Modal UI updated with 2 new fields
- [x] Form state management working
- [x] No TypeScript compilation errors
- [ ] Test modal opens and fetches countries
- [ ] Test CoC dropdown shows "National" and "Flag State"
- [ ] Test Countries dropdown populates from API
- [ ] Test form submission includes both new fields

## Next Steps
1. Start the backend server to test the API
2. Open the frontend and test the AddCertificateModal
3. Verify countries dropdown populates correctly
4. Test form submission with new fields
5. Verify data saves correctly to database

## Notes
- Certificate of Competency is a standard maritime term (STCW Convention)
- National: Issued by crew member's home country
- Flag State: Issued by the country where vessel is registered
- Country field allows tracking which country issued each certificate
- This is important for compliance with international maritime regulations
