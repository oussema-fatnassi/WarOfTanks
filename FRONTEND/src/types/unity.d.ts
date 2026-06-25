interface UnityInstance {
  SendMessage: (objectName: string, methodName: string, value?: string | number) => void
  Quit: () => Promise<void>
}

interface UnityConfig {
  dataUrl: string
  frameworkUrl: string
  codeUrl: string
  streamingAssetsUrl: string
  companyName?: string
  productName?: string
  productVersion?: string
}

declare function createUnityInstance(
  canvas: HTMLCanvasElement,
  config: UnityConfig,
  onProgress?: (progress: number) => void,
): Promise<UnityInstance>
