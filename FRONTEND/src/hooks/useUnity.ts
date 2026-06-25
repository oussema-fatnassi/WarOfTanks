import { useEffect, useRef, useState } from 'react'

interface UnityBuildConfig {
  loaderUrl: string
  dataUrl: string
  frameworkUrl: string
  codeUrl: string
}

export const useUnity = (config: UnityBuildConfig) => {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const [unityInstance, setUnityInstance] = useState<UnityInstance | null>(null)
  const [loadingProgress, setLoadingProgress] = useState(0)
  const [isLoaded, setIsLoaded] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return

    const script = document.createElement('script')
    script.src = config.loaderUrl

    script.onload = () => {
      createUnityInstance(
        canvas,
        {
          dataUrl: config.dataUrl,
          frameworkUrl: config.frameworkUrl,
          codeUrl: config.codeUrl,
          streamingAssetsUrl: 'StreamingAssets',
        },
        (progress) => setLoadingProgress(Math.round(progress * 100)),
      )
        .then((instance) => {
          setUnityInstance(instance)
          setIsLoaded(true)
        })
        .catch((err: unknown) => setError(String(err)))
    }

    script.onerror = () => setError('Failed to load Unity loader script.')
    document.body.appendChild(script)

    return () => {
      if (document.body.contains(script)) document.body.removeChild(script)
    }
    // config values are module-level constants — safe to omit from deps
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return { canvasRef, unityInstance, loadingProgress, isLoaded, error }
}
