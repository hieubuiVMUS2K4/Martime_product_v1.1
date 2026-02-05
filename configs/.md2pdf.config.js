module.exports = {
  stylesheet_encoding: 'utf-8',
  body_class: 'markdown-body',
  marked_options: {
    headerIds: false,
    smartypants: true,
  },
  css: `
    body {
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      font-size: 11pt;
      line-height: 1.6;
      color: #333;
      max-width: 100%;
    }
    h1 { 
      color: #1a73e8; 
      border-bottom: 3px solid #1a73e8;
      padding-bottom: 10px;
      margin-top: 30px;
      page-break-after: avoid;
    }
    h2 { 
      color: #0d47a1; 
      border-bottom: 2px solid #64b5f6;
      padding-bottom: 8px;
      margin-top: 25px;
      page-break-after: avoid;
    }
    h3 { 
      color: #1565c0; 
      margin-top: 20px;
      page-break-after: avoid;
    }
    table {
      border-collapse: collapse;
      width: 100%;
      margin: 15px 0;
      font-size: 10pt;
    }
    table th {
      background-color: #1a73e8;
      color: white;
      padding: 10px;
      text-align: left;
    }
    table td {
      border: 1px solid #ddd;
      padding: 8px;
    }
    table tr:nth-child(even) {
      background-color: #f8f9fa;
    }
    code {
      background-color: #f5f5f5;
      padding: 2px 6px;
      border-radius: 3px;
      font-family: 'Consolas', 'Monaco', monospace;
      font-size: 10pt;
    }
    pre {
      background-color: #263238;
      color: #aed581;
      padding: 15px;
      border-radius: 5px;
      overflow-x: auto;
      font-size: 9pt;
    }
    .mermaid {
      background-color: white;
      padding: 15px;
      text-align: center;
      page-break-inside: avoid;
    }
  `,
  pdf_options: {
    format: 'A4',
    margin: {
      top: '20mm',
      right: '15mm',
      bottom: '20mm',
      left: '15mm'
    },
    printBackground: true,
    displayHeaderFooter: true,
    headerTemplate: '<div style="font-size: 9px; text-align: center; width: 100%; color: #666;">Maritime Management System v1.1</div>',
    footerTemplate: '<div style="font-size: 9px; text-align: center; width: 100%; color: #666;"><span class="pageNumber"></span> / <span class="totalPages"></span></div>',
  },
  launch_options: {
    args: ['--no-sandbox']
  }
};
