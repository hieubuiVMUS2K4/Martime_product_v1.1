declare module 'jspdf-autotable' {
  import { jsPDF } from 'jspdf'
  
  interface AutoTableOptions {
    startY?: number
    head?: any[][]
    body?: any[][]
    theme?: 'striped' | 'grid' | 'plain'
    styles?: any
    headStyles?: any
    columnStyles?: any
  }
  
  export interface jsPDFWithAutoTable extends jsPDF {
    autoTable: (options: AutoTableOptions) => void
    lastAutoTable: {
      finalY: number
    }
  }
}

declare module 'jspdf' {
  interface jsPDF {
    autoTable: (options: any) => void
    lastAutoTable: {
      finalY: number
    }
  }
}
