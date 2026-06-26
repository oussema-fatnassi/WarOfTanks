import { readFile } from 'node:fs/promises'
import { join } from 'node:path'
import { spawnSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'

const repoRoot = fileURLToPath(new URL('../../', import.meta.url))
const backendEnvPath = join(repoRoot, 'BACKEND', '.env')

if (process.env.E2E_SKIP_DB_CLEANUP === '1') {
  console.log('Skipping E2E data cleanup because E2E_SKIP_DB_CLEANUP=1.')
  process.exit(0)
}

const backendEnv = await readBackendEnv()
const dbName =
  process.env.E2E_DB_NAME ??
  process.env.MONGODB_DB_NAME ??
  backendEnv.MONGODB_DB_NAME ??
  'waroftanks'

if (!/^[a-zA-Z0-9_-]+$/.test(dbName)) {
  throw new Error(`Refusing to clean invalid Mongo database name: ${dbName}`)
}

const cleanupScript = `
const e2ePlayers = db.players
  .find({ username: /^e2e_/ }, { projection: { _id: 1 } })
  .toArray();
const playerIds = e2ePlayers.map((player) => player._id);
const matchDelete = db.matches.deleteMany({
  $or: [
    { playerId: { $in: playerIds } },
    { "playerSnapshot.username": /^e2e_/ }
  ]
});
const playerDelete = db.players.deleteMany({ username: /^e2e_/ });
print(JSON.stringify({
  database: db.getName(),
  deletedPlayers: playerDelete.deletedCount,
  deletedMatches: matchDelete.deletedCount
}));
`

const result = spawnSync(
  'docker',
  [
    'compose',
    'exec',
    '-T',
    'mongo',
    'mongosh',
    '--quiet',
    dbName,
    '--eval',
    cleanupScript,
  ],
  {
    cwd: repoRoot,
    encoding: 'utf8',
  },
)

if (result.status !== 0) {
  console.error(result.stderr || result.stdout)
  console.error(
    'E2E data cleanup failed. Start the Docker backend stack first, or set E2E_SKIP_DB_CLEANUP=1 to bypass cleanup intentionally.',
  )
  process.exit(result.status ?? 1)
}

console.log(`Cleaned E2E data: ${result.stdout.trim()}`)

async function readBackendEnv() {
  try {
    const raw = await readFile(backendEnvPath, 'utf8')
    return Object.fromEntries(
      raw
        .split('\n')
        .map((line) => line.trim())
        .filter((line) => line && !line.startsWith('#'))
        .map((line) => {
          const equalsIndex = line.indexOf('=')
          if (equalsIndex === -1) return [line, '']
          return [line.slice(0, equalsIndex), line.slice(equalsIndex + 1)]
        }),
    )
  } catch {
    return {}
  }
}
