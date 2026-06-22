import { mkdir, mkdtemp, readFile, rm, stat, writeFile } from 'node:fs/promises'
import { tmpdir } from 'node:os'
import { join } from 'node:path'
import { spawnSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'

const repository =
  process.env.UNITY_RELEASE_REPOSITORY ?? 'oussema-fatnassi/WarOfTanks'
const assetName = 'waroftanks-webgl-build.tar.gz'
const bridgeMarker = 'wot:web-client-config'
const assetDownloadUrl =
  process.env.UNITY_RELEASE_ASSET_URL ??
  `https://github.com/${repository}/releases/latest/download/${assetName}`
const outputPath =
  process.env.UNITY_BUILD_OUTPUT_PATH ??
  fileURLToPath(new URL('../public/UnityBuild/', import.meta.url))

if (process.env.VERCEL !== '1' && process.env.FETCH_UNITY_BUILD !== '1') {
  console.log(
    'Skipping Unity release download outside Vercel. Set FETCH_UNITY_BUILD=1 to fetch it locally.',
  )
  process.exit(0)
}

const archiveResponse = await fetch(assetDownloadUrl)
if (!archiveResponse.ok) {
  if (process.env.VERCEL_ENV === 'preview') {
    console.warn(
      `Unity release is not available yet (${archiveResponse.status}); continuing with the frontend preview only.`,
    )
    process.exit(0)
  }

  throw new Error(
    `Unable to download the latest Unity release (${archiveResponse.status}). Run the WebGL Build workflow on main first.`,
  )
}

const temporaryDirectory = await mkdtemp(join(tmpdir(), 'waroftanks-unity-'))
const archivePath = join(temporaryDirectory, assetName)

try {
  await writeFile(archivePath, Buffer.from(await archiveResponse.arrayBuffer()))
  await rm(outputPath, { recursive: true, force: true })
  await mkdir(outputPath, { recursive: true })

  const extraction = spawnSync('tar', ['-xzf', archivePath, '-C', outputPath], {
    encoding: 'utf8',
  })

  if (extraction.status !== 0) {
    throw new Error(
      `Unable to extract Unity release: ${extraction.stderr || extraction.stdout}`,
    )
  }

  const indexPath = join(outputPath, 'index.html')
  await stat(indexPath)
  await installWebClientBridge(indexPath)
  console.log('Installed the latest Unity release into public/UnityBuild.')
} finally {
  await rm(temporaryDirectory, { recursive: true, force: true })
}

async function installWebClientBridge(indexPath) {
  let html = await readFile(indexPath, 'utf8')
  if (html.includes(bridgeMarker)) return

  if (!html.includes('createUnityInstance(') || !html.includes('</body>')) {
    throw new Error('Unity index.html has an unsupported template shape.')
  }

  html = html.replace(
    'createUnityInstance(',
    'window.warOfTanksUnityInstancePromise = createUnityInstance(',
  )

  const bridgeScript = `
    <script>
      window.addEventListener('message', function (event) {
        if (
          event.origin !== window.location.origin ||
          event.source !== window.parent ||
          !event.data ||
          event.data.type !== '${bridgeMarker}'
        ) return;

        window.warOfTanksUnityInstancePromise.then(function (unityInstance) {
          unityInstance.SendMessage(
            'GameManager',
            'ConfigureWebClient',
            JSON.stringify({
              apiBaseUrl: event.data.apiBaseUrl || '',
              accessToken: event.data.accessToken || ''
            })
          );
        });
      });

    </script>
  `

  html = html.replace('</body>', `${bridgeScript}\n  </body>`)
  await writeFile(indexPath, html)
}
