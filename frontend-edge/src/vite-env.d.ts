/// <reference types="vite/client" />

export {}

declare module '*.css' {
  const content: string
  export default content
}

declare module '*.scss' {
  const content: string
  export default content
}

declare global {
  interface ImportMetaEnv {
    readonly VITE_API_URL: string
    readonly VITE_WS_URL: string
    readonly VITE_VESSEL_ID: string
    readonly VITE_VESSEL_NAME: string
    readonly VITE_IMO_NUMBER: string
  }

  interface ImportMeta {
    readonly env: ImportMetaEnv
  }
}
