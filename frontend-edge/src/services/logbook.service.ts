import { apiClient } from './api.client';
import { 
  PaginatedLogbookResponse, 
  LogbookPaginationDto,
  CreateDeckLogEntryDto,
  DeckLogEntryResponseDto,
  CreateEngineLogEntryDto,
  EngineLogEntryResponseDto,
  CreateOilRecordEntryDto,
  OilRecordEntryResponseDto,
  CreateWatchkeepingLogDto,
  WatchkeepingLogResponseDto,
  CreateGarbageRecordDto,
  GarbageRecordResponseDto,
  CreateBallastWaterRecordDto,
  BallastWaterRecordResponseDto,
  SignLogbookDto
} from '../types/logbook.types';

class LogbookService {
  private buildQueryString(params: any) {
    const query = new URLSearchParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== undefined && params[key] !== null) {
        query.append(key, params[key].toString());
      }
    });
    return query.toString();
  }

  // Deck Log
  async getDeckEntries(params: LogbookPaginationDto) {
    const queryString = this.buildQueryString(params);
    return await apiClient.get<PaginatedLogbookResponse<DeckLogEntryResponseDto>>(`/logbooks/deck?${queryString}`);
  }

  async createDeckEntry(data: CreateDeckLogEntryDto) {
    return await apiClient.post<DeckLogEntryResponseDto>('/logbooks/deck', data);
  }

  async signDeckEntry(id: string, data: SignLogbookDto) {
    return await apiClient.post(`/logbooks/deck/${id}/sign`, {
      masterSignature: data.signature,
      signedAt: data.signedAt
    });
  }

  // Engine Log
  async getEngineEntries(params: LogbookPaginationDto) {
    const queryString = this.buildQueryString(params);
    return await apiClient.get<PaginatedLogbookResponse<EngineLogEntryResponseDto>>(`/logbooks/engine?${queryString}`);
  }

  async createEngineEntry(data: CreateEngineLogEntryDto) {
    return await apiClient.post<EngineLogEntryResponseDto>('/logbooks/engine', data);
  }

  async signEngineEntry(id: string, data: SignLogbookDto) {
    return await apiClient.post(`/logbooks/engine/${id}/sign`, {
      chiefEngineerSignature: data.signature,
      chiefEngineerRemarks: data.remarks,
      signedAt: data.signedAt
    });
  }

  // Oil Record
  async getOilEntries(params: LogbookPaginationDto) {
    const queryString = this.buildQueryString(params);
    return await apiClient.get<PaginatedLogbookResponse<OilRecordEntryResponseDto>>(`/logbooks/oil?${queryString}`);
  }

  async createOilEntry(data: CreateOilRecordEntryDto) {
    return await apiClient.post<OilRecordEntryResponseDto>('/logbooks/oil', data);
  }

  async signOilEntry(id: string, data: SignLogbookDto) {
    return await apiClient.post(`/logbooks/oil/${id}/sign`, {
      masterSignature: data.signature,
      signedAt: data.signedAt
    });
  }

  // Watchkeeping Log
  async getWatchkeepingEntries(params: LogbookPaginationDto) {
    const queryString = this.buildQueryString(params);
    return await apiClient.get<PaginatedLogbookResponse<WatchkeepingLogResponseDto>>(`/logbooks/watchkeeping?${queryString}`);
  }

  async createWatchkeepingEntry(data: CreateWatchkeepingLogDto) {
    return await apiClient.post<WatchkeepingLogResponseDto>('/logbooks/watchkeeping', data);
  }

  async signWatchkeepingEntry(id: string, data: SignLogbookDto) {
    return await apiClient.post(`/logbooks/watchkeeping/${id}/sign`, {
      masterSignature: data.signature,
      signedAt: data.signedAt
    });
  }

  // Garbage Record
  async getGarbageEntries(params: LogbookPaginationDto) {
    const queryString = this.buildQueryString(params);
    return await apiClient.get<PaginatedLogbookResponse<GarbageRecordResponseDto>>(`/logbooks/garbage?${queryString}`);
  }

  async createGarbageEntry(data: CreateGarbageRecordDto) {
    return await apiClient.post<GarbageRecordResponseDto>('/logbooks/garbage', data);
  }

  async signGarbageEntry(id: string, data: SignLogbookDto) {
    return await apiClient.post(`/logbooks/garbage/${id}/sign`, {
      masterSignature: data.signature,
      signedAt: data.signedAt
    });
  }

  // Ballast Water Record
  async getBallastWaterEntries(params: LogbookPaginationDto) {
    const queryString = this.buildQueryString(params);
    return await apiClient.get<PaginatedLogbookResponse<BallastWaterRecordResponseDto>>(`/logbooks/ballast?${queryString}`);
  }

  async createBallastWaterEntry(data: CreateBallastWaterRecordDto) {
    return await apiClient.post<BallastWaterRecordResponseDto>('/logbooks/ballast', data);
  }

  async signBallastWaterEntry(id: string, data: SignLogbookDto) {
    return await apiClient.post(`/logbooks/ballast/${id}/sign`, {
      masterSignature: data.signature,
      signedAt: data.signedAt
    });
  }
}

export const logbookService = new LogbookService();
