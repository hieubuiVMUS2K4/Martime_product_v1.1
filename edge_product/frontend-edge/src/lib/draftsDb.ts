import Dexie, { type Table } from 'dexie';

export interface FormDraft {
  key: string;       // Unique key, e.g. 'noon-report-form' or 'departure-report-form'
  data: any;         // Form state payload
  updatedAt: number; // Timestamp in milliseconds
}

class DraftsDatabase extends Dexie {
  drafts!: Table<FormDraft, string>;

  constructor() {
    super('FormDraftsDatabase');
    this.version(1).stores({
      drafts: 'key, updatedAt',
    });
  }
}

export const draftsDb = new DraftsDatabase();
