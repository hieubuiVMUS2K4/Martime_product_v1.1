SELECT "Id", "CrewId", "FullName", "EdgeChangesViewed", left("EdgeChanges", 80) as changes FROM crew_members WHERE "CrewId" IN ('CREW-MS-008', 'CREW-TS-019');
