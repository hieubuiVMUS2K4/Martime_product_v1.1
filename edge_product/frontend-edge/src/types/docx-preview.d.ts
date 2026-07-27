declare module 'docx-preview' {
  export interface Options {
    className?: string;
    inWrapper?: boolean;
    ignoreWidth?: boolean;
    ignoreHeight?: boolean;
    ignoreFonts?: boolean;
    breakPages?: boolean;
    debug?: boolean;
    experimental?: boolean;
    classNamePrefix?: string;
    renderHeaders?: boolean;
    renderFooters?: boolean;
    renderFootnotes?: boolean;
    renderEndnotes?: boolean;
    renderComments?: boolean;
    renderAltChunks?: boolean;
    useBase64URL?: boolean;
    useImgStyle?: boolean;
    trimXmlDeclaration?: boolean;
  }

  export function renderAsync(
    data: Blob | ArrayBuffer | Uint8Array,
    bodyContainer: HTMLElement,
    styleContainer?: HTMLElement,
    options?: Options
  ): Promise<any>;
}
