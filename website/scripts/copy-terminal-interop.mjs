import { mkdir, readFile, writeFile } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)
const projectRoot = path.resolve(__dirname, '..')

const buildOutput = path.resolve(projectRoot, 'build', 'terminal', 'terminalInterop.js')
const targets = [
  path.resolve(projectRoot, 'public', 'js', 'xterm-interop.js'),
  path.resolve(projectRoot, '..', 'src', 'RazorConsole.Website', 'wwwroot', 'js', 'xterm-interop.js')
]

async function copyFileContents() {
  const contents = await readFile(buildOutput, 'utf8')
  await Promise.all(targets.map(async target => {
    await mkdir(path.dirname(target), { recursive: true })
    await writeFile(target, contents, 'utf8')
  }))
}

copyFileContents().catch(error => {
  console.error('Failed to copy terminal interop build output.', error)
  process.exitCode = 1
})
