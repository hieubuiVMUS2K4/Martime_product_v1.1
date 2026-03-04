/**
 * Crew DTOs - Re-exported from Maritime.Shared
 * All crew DTOs are now defined in the shared library.
 * This file provides global using aliases for backward compatibility.
 */

// Re-export shared Crew DTOs into global scope
global using CrewMemberDto = Maritime.Shared.DTOs.Crew.CrewMemberDto;
global using RankDto = Maritime.Shared.DTOs.Crew.RankDto;
global using CrewDetailDto = Maritime.Shared.DTOs.Crew.CrewDetailDto;
global using CreateCrewRequest = Maritime.Shared.DTOs.Crew.CreateCrewRequest;
global using UpdateCrewRequest = Maritime.Shared.DTOs.Crew.UpdateCrewRequest;

// Re-export shared Certificate DTOs
global using CertificateDto = Maritime.Shared.DTOs.Crew.CertificateDto;
global using CrewCertificateDto = Maritime.Shared.DTOs.Crew.CrewCertificateDto;
global using CrewCertificateRequest = Maritime.Shared.DTOs.Crew.CrewCertificateRequest;
global using CreateCertificateRequest = Maritime.Shared.DTOs.Crew.CreateCertificateRequest;
global using CreateIdentityDocumentDto = Maritime.Shared.DTOs.Crew.CreateIdentityDocumentDto;

// Re-export shared Sync DTOs
global using SyncQueueItemDto = Maritime.Shared.DTOs.Sync.SyncQueueItemDto;
global using SyncPullResponse = Maritime.Shared.DTOs.Sync.SyncPullResponse;
global using SyncStatusDto = Maritime.Shared.DTOs.Sync.SyncStatusDto;
global using SyncAcknowledgeDto = Maritime.Shared.DTOs.Sync.SyncAcknowledgeDto;
