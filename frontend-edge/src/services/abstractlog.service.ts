import { apiClient } from './api.client'
import { API_CONFIG } from '@/config/app.config'
import type {
  AbstractLogListItem,
  AbstractLogVoyage,
  CreateAbstractLogDto,
  UpdateAbstractLogVoyageDto,
  AbstractLogLeg,
  CreateAbstractLogLegDto,
  UpdateAbstractLogLegDto,
  AbstractLogDailyEntry,
  CreateAbstractLogDailyEntryDto,
  UpdateAbstractLogDailyEntryDto,
} from '../types/abstractlog.types'

class AbstractLogService {
  // ── Voyage-level ──

  async getAll(voyageId?: string): Promise<AbstractLogListItem[]> {
    const params = voyageId ? `?voyageId=${voyageId}` : ''
    return await apiClient.get<AbstractLogListItem[]>(`/logbooks/abstract-log${params}`)
  }

  async getDetail(id: string): Promise<AbstractLogVoyage> {
    return await apiClient.get<AbstractLogVoyage>(`/logbooks/abstract-log/${id}`)
  }

  async create(dto: CreateAbstractLogDto): Promise<AbstractLogVoyage> {
    return await apiClient.post<AbstractLogVoyage>('/logbooks/abstract-log', dto)
  }

  async update(id: string, dto: UpdateAbstractLogVoyageDto): Promise<AbstractLogVoyage> {
    return await apiClient.put<AbstractLogVoyage>(`/logbooks/abstract-log/${id}`, dto)
  }

  async delete(id: string): Promise<void> {
    return await apiClient.delete<void>(`/logbooks/abstract-log/${id}`)
  }

  async autoFill(id: string): Promise<AbstractLogVoyage> {
    return await apiClient.post<AbstractLogVoyage>(`/logbooks/abstract-log/${id}/auto-fill`, {})
  }

  async recalculate(id: string): Promise<AbstractLogVoyage> {
    return await apiClient.post<AbstractLogVoyage>(`/logbooks/abstract-log/${id}/recalculate`, {})
  }

  async exportExcel(id: string): Promise<Blob> {
    const response = await fetch(`${API_CONFIG.BASE_URL}/logbooks/abstract-log/${id}/export/excel`)
    if (!response.ok) throw new Error('Export failed')
    return await response.blob()
  }

  async exportPdf(id: string): Promise<Blob> {
    const response = await fetch(`${API_CONFIG.BASE_URL}/logbooks/abstract-log/${id}/export/pdf`)
    if (!response.ok) throw new Error('Export failed')
    return await response.blob()
  }

  // ── Leg-level ──

  async createLeg(abstractLogId: string, dto: CreateAbstractLogLegDto): Promise<AbstractLogLeg> {
    return await apiClient.post<AbstractLogLeg>(`/logbooks/abstract-log/${abstractLogId}/legs`, dto)
  }

  async updateLeg(legId: string, dto: UpdateAbstractLogLegDto): Promise<AbstractLogLeg> {
    return await apiClient.put<AbstractLogLeg>(`/logbooks/abstract-log/legs/${legId}`, dto)
  }

  async deleteLeg(legId: string): Promise<void> {
    return await apiClient.delete<void>(`/logbooks/abstract-log/legs/${legId}`)
  }

  // ── Daily Entry-level ──

  async createEntry(legId: string, dto: CreateAbstractLogDailyEntryDto): Promise<AbstractLogDailyEntry> {
    return await apiClient.post<AbstractLogDailyEntry>(`/logbooks/abstract-log/legs/${legId}/entries`, dto)
  }

  async updateEntry(entryId: string, dto: UpdateAbstractLogDailyEntryDto): Promise<AbstractLogDailyEntry> {
    return await apiClient.put<AbstractLogDailyEntry>(`/logbooks/abstract-log/entries/${entryId}`, dto)
  }

  async deleteEntry(entryId: string): Promise<void> {
    return await apiClient.delete<void>(`/logbooks/abstract-log/entries/${entryId}`)
  }
}

export const abstractLogService = new AbstractLogService()
