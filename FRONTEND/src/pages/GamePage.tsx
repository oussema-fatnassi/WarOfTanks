import React, { useEffect } from 'react'
import { useAuth } from '../hooks/useAuth'
import { useUnity } from '../hooks/useUnity'

const UNITY_BUILD = {
  loaderUrl: '/Build/WarOfTanks.loader.js',
  dataUrl: '/Build/WarOfTanks.data',
  frameworkUrl: '/Build/WarOfTanks.framework.js',
  codeUrl: '/Build/WarOfTanks.wasm',
}

const GamePage: React.FC = () => {
  const { accessToken } = useAuth()
  const { canvasRef, unityInstance, loadingProgress, isLoaded, error } = useUnity(UNITY_BUILD)

  useEffect(() => {
    if (isLoaded && unityInstance && accessToken) {
      unityInstance.SendMessage('AuthManager', 'SetToken', accessToken)
    }
  }, [isLoaded, unityInstance, accessToken])

  if (error) {
    return (
      <div className="flex items-center justify-center h-screen text-red-400">
        <p>Failed to load game: {error}</p>
      </div>
    )
  }

  return (
    <div className="relative flex items-center justify-center min-h-screen bg-black">
      {!isLoaded && (
        <div className="absolute flex flex-col items-center gap-3 z-10">
          <p className="text-white text-lg font-semibold">Loading... {loadingProgress}%</p>
          <div className="w-64 h-2 bg-gray-700 rounded overflow-hidden">
            <div
              className="h-full bg-green-500 transition-all duration-200"
              style={{ width: `${loadingProgress}%` }}
            />
          </div>
        </div>
      )}
      <canvas
        ref={canvasRef}
        id="unity-canvas"
        style={{ width: '960px', height: '600px', display: isLoaded ? 'block' : 'none' }}
      />
    </div>
  )
}

export default GamePage
