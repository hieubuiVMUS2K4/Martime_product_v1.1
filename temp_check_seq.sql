-- Get a crew member to test with
SELECT "Id" FROM crew_members LIMIT 1;

-- Get a certificate type to test with
SELECT "Id" FROM certificates WHERE "IsActive" = true LIMIT 1;
