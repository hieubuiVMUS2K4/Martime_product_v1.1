// Excel Export Template - Match CSV Structure
// This is a reference design for the Personal Particular table

/*
CSV STRUCTURE ANALYSIS:

1. PERSONAL PARTICULAR TABLE (Complex merged cells):

Row 1 Headers:
[empty] [empty] [empty] Name [empty] Family name [empty] Middle name [empty] [empty] First name [empty] [empty] Date of Birth [empty] [empty] Place of Birth [empty] [empty] [empty] [empty] Nationality

Row 1 Values:
[empty] [empty] [empty] [empty] [empty] HOANG [empty] VAN [empty] [empty] HAI [empty] [empty] 4/20/1994 [empty] [empty] THANH HOA [empty] [empty] [empty] [empty] Vietnamese

Row 2:
[empty] [empty] [empty] ID No. [empty] 038094024335 [empty] [empty] [empty] [empty] Address [empty] TDP Nam Thành... [empty] [empty] [empty] [empty] [empty] [empty] [empty] [empty]

Row 3:
[empty] [empty] [empty] Home Tel [empty] [empty] [empty] Hand phone [empty] 0367839744 [empty] Email [empty] hoanghai200494@gmail.com [empty] [empty] [empty] [empty] [empty] Marital status [empty] [empty] Single

Row 4:
[empty] [empty] [empty] Height [empty] 163 [empty] Weight [empty] 52 [empty] Overall size [empty] 160 [empty] Shoe's size [empty] [empty] 24 [empty] Catering size [empty] [empty] [empty] Blood Group [empty] B

Row 5:
[empty] [empty] [empty] Contact person/Next of Kin [empty] [empty] Name [empty] [empty] Hoang Van Thuy [empty] [empty] [empty] [empty] Phone No. [empty] 0386147308 [empty] [empty] [empty] [empty] Covid-19 Vaccinated [empty] [empty] [empty] [empty] Smoker

Row 6:
[empty] [empty] [empty] [empty] [empty] [empty] Relation [empty] [empty] Bố [empty] [empty] [empty] Address [empty] TDP Nam Thành...

Key observations:
- 3 empty columns at start (for logo/spacing)
- Name field has sub-columns: Family | Middle | First
- Contact person spans 2 rows vertically
- Overall size field added
- Layout is ~40 columns wide in CSV

2. SERVICE RECORDS TABLE (Hierarchical headers):

Header Row 1:
Rank | Vessel | [empty cols] | Vessel's general information | [merged] | Engine Detail | [merged] | Ballast Water Treatment System | [merged] | Exhaust Gas Scrubber | [merged] | ECDIS | [merged] | Boarding Records

Header Row 2:
[empty] | [empty cols] | Flag | Type | GRT | Trade Area | Year Built | Maker | Type/Model | K.W | Maker | [empty] | [empty] | Type/Model | Maker | Type/Model | Maker | [empty] | [empty] | Type/Model | Embark Date | Disembark Date | Duration

*/

// Implementation plan:
// 1. Expand column count to ~40 columns for proper spacing
// 2. Use mergeCells extensively for proper layout
// 3. Create hierarchical headers for Service Records
// 4. Add Overall size, proper spacing like template
