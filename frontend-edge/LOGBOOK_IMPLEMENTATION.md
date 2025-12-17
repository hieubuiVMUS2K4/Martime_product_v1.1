# Maritime Logbook Implementation Guide

## Overview
This document details the implementation of the Digital Logbook system in the `frontend-edge` application. The system covers three main logbooks:
1. **Deck Logbook** (SOLAS Chapter V)
2. **Engine Logbook** (Main Engine Parameters)
3. **Oil Record Book** (Part I - Machinery Space)

## Architecture

### 1. Service Layer
- **File**: `src/services/logbook.service.ts`
- **Description**: Handles all API communication with the backend logbook endpoints.
- **Key Methods**:
  - `getDeckEntries`, `createDeckEntry`, `signDeckEntry`
  - `getEngineEntries`, `createEngineEntry`, `signEngineEntry`
  - `getOilEntries`, `createOilEntry`, `signOilEntry`

### 2. Data Types
- **File**: `src/types/logbook.types.ts`
- **Description**: TypeScript interfaces mirroring the Backend DTOs.
- **Key Interfaces**:
  - `CreateDeckLogEntryDto`, `DeckLogEntryResponseDto`
  - `CreateEngineLogEntryDto`, `EngineLogEntryResponseDto`
  - `CreateOilRecordEntryDto`, `OilRecordEntryResponseDto`

### 3. UI Components
- **Common Components**:
  - `MaritimeInput`: High-contrast input field for industrial environments.
  - `LogbookGrid`: Standard layout wrapper with title and grid background.
  - `SignaturePad`: Digital signature capture component.

### 4. Pages
- **Deck Log**: `src/pages/logbooks/DeckLogPage.tsx`
  - Features: Timeline view, Form entry, Digital Signature.
- **Engine Log**: `src/pages/logbooks/EngineLogPage.tsx`
  - Features: Tabular/Spreadsheet view, Quick entry row.
- **Oil Record**: `src/pages/logbooks/OilRecordPage.tsx`
  - Features: Wizard-style step-by-step form (Operation -> Details -> Confirm).

## Usage

### Adding a New Log Entry
Navigate to the respective logbook page via the sidebar.
- **Deck**: Fill the form on the left, click "ADD ENTRY".
- **Engine**: Use the bottom row of the table to enter parameters, click "ADD".
- **Oil**: Follow the 3-step wizard to select operation code and enter details.

### Signing Entries
- **Deck/Oil**: Click "Sign this Entry" on the specific log entry.
- **Engine**: Click "SIGN" in the action column (Chief Engineer only).

## Pending Tasks (Future Work)
- [ ] Fix pre-existing type errors in `KanbanBoard.tsx` and `MaintenancePage.tsx` to enable clean build.
- [ ] Implement "Sync Status" indicator (currently backend supports it, frontend needs UI).
- [ ] Add role-based access control (RBAC) for signing (e.g., only Master can sign Deck Log).
