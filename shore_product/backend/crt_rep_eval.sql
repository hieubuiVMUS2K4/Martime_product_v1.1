CREATE TABLE "ReportEvaluations" ("Id" uuid NOT NULL PRIMARY KEY, "ReportId" uuid NOT NULL, "Status" varchar(20) NOT NULL, "ContentVi" text NOT NULL, "CreatedDate" timestamp with time zone NOT NULL);
