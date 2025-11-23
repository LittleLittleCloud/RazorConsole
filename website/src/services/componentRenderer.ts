import { disposeTerminal, isTerminalAvailable } from '@/lib/terminalInterop'

declare global {
  interface Window {
    Blazor?: {
      start: (options?: Record<string, unknown>) => Promise<void>
      rootComponents: {
        add: (element: Element, componentIdentifier: string, parameters: Record<string, unknown>) => Promise<DynamicRootHandle>
      }
    }
    __razorConsolePreview?: {
      started?: boolean
      scriptUrl?: string
      elementsRegistered?: boolean
    }
  }
}

interface DynamicRootHandle {
  setParameters: (parameters: Record<string, unknown>) => Promise<void>
  dispose: () => void
}

interface PreviewDefinition {
  tagName: string
  componentType: string
}

const previewRegistry: Record<string, PreviewDefinition> = {
  Align: {
    tagName: 'razor-console-align',
    componentType: 'RazorConsole.Website.RazorConsoleComponents.AlignDemo, RazorConsole.Website'
  }
}

const activeHosts = new Map<string, HTMLElement>()
let blazorStartPromise: Promise<void> | null = null
let bootstrapScriptUrlPromise: Promise<string> | null = null
let bootstrapScriptLoadPromise: Promise<void> | null = null

function resolveWasmAssetPath(relativePath: string): string {
  let path = relativePath

  if (path.startsWith('./')) {
    path = path.slice(2)
  }

  if (path.startsWith('/')) {
    path = path.slice(1)
  }

  return `/wasm/wwwroot/${path}`
}

function ensurePreviewState() {
  if (!window.__razorConsolePreview) {
    window.__razorConsolePreview = {}
  }
}

function registerPreviewElements() {
  ensurePreviewState()

  if (!window.Blazor) {
    throw new Error('Blazor runtime is not available to register preview elements.')
  }

  if (window.__razorConsolePreview?.elementsRegistered) {
    return
  }

  Object.values(previewRegistry).forEach(definition => {
    if (customElements.get(definition.tagName)) {
      return
    }

    class RazorConsolePreviewElement extends HTMLElement {
      private rootHandle: DynamicRootHandle | null = null
      private pendingRootPromise: Promise<DynamicRootHandle> | null = null

      static get observedAttributes() {
        return ['terminal-id', 'component-id']
      }

      connectedCallback() {
        this.pendingRootPromise = window.Blazor!.rootComponents.add(this, definition.componentType, this.buildParameters())
        this.pendingRootPromise.then(handle => {
          this.rootHandle = handle
        }).catch(err => {
          console.error('Failed to mount preview component:', err)
        })
      }

      disconnectedCallback() {
        if (this.rootHandle) {
          this.rootHandle.dispose()
        }
        this.rootHandle = null
        this.pendingRootPromise = null
      }

      attributeChangedCallback(_name: string, oldValue: string | null, newValue: string | null) {
        if (oldValue === newValue) {
          return
        }

        const update = async () => {
          const handle = this.rootHandle ?? (await this.pendingRootPromise)
          if (handle) {
            await handle.setParameters(this.buildParameters())
          }
        }

        update().catch(err => {
          console.error('Failed to update preview parameters:', err)
        })
      }

      private buildParameters(): Record<string, unknown> {
        const terminalId = this.getAttribute('terminal-id') ?? undefined
        const componentId = this.getAttribute('component-id') ?? undefined

        return {
          TerminalId: terminalId,
          ComponentId: componentId
        }
      }
    }

    customElements.define(definition.tagName, RazorConsolePreviewElement)
  })

  if (window.__razorConsolePreview) {
    window.__razorConsolePreview.elementsRegistered = true
  }
}

async function getBootstrapScriptUrl(): Promise<string> {
  if (!bootstrapScriptUrlPromise) {
    bootstrapScriptUrlPromise = (async () => {
      const response = await fetch('/wasm/wwwroot/index.html')
      if (!response.ok) {
        throw new Error('Failed to load Blazor boot manifest.')
      }

      const html = await response.text()
      const match = html.match(/<script\s+src="([^"]*blazor\.webassembly[^"]*\.js)"/i)
      if (!match) {
        throw new Error('Unable to locate Blazor bootstrap script in published output.')
      }

      let scriptPath = match[1]
      if (scriptPath.startsWith('./')) {
        scriptPath = scriptPath.slice(2)
      }

      if (!scriptPath.startsWith('/')) {
        scriptPath = `/wasm/wwwroot/${scriptPath}`
      }

      return scriptPath
    })()
  }

  return bootstrapScriptUrlPromise
}

function loadBootstrapScript(scriptUrl: string): Promise<void> {
  if (bootstrapScriptLoadPromise) {
    return bootstrapScriptLoadPromise
  }

  bootstrapScriptLoadPromise = new Promise<void>((resolve, reject) => {
    const existing = document.querySelector<HTMLScriptElement>('script[data-razor-console-blazor="true"]')

    if (existing) {
      if (existing.dataset.loaded === 'true') {
        resolve()
        return
      }

      existing.addEventListener('load', () => resolve(), { once: true })
      existing.addEventListener('error', () => reject(new Error(`Failed to load Blazor bootstrap script from ${scriptUrl}.`)), { once: true })
      return
    }

    const script = document.createElement('script')
    script.type = 'module'
    script.dataset.razorConsoleBlazor = 'true'
    script.setAttribute('autostart', 'false')
    script.src = scriptUrl
    script.addEventListener('load', () => {
      script.dataset.loaded = 'true'
      resolve()
    }, { once: true })
    script.addEventListener('error', () => {
      reject(new Error(`Failed to load Blazor bootstrap script from ${scriptUrl}.`))
    }, { once: true })

    document.head.appendChild(script)
  })

  return bootstrapScriptLoadPromise
}

async function ensureBlazorStarted(): Promise<void> {
  ensurePreviewState()

  if (window.__razorConsolePreview?.started) {
    return
  }

  if (!blazorStartPromise) {
    blazorStartPromise = (async () => {
      const scriptUrl = await getBootstrapScriptUrl()
      await loadBootstrapScript(scriptUrl)

      if (!window.Blazor) {
        throw new Error('Blazor runtime failed to initialize.')
      }

      await window.Blazor.start({
        loadBootResource: (_type: string, resourceName: string | undefined, defaultUri: string) => {
          if (resourceName === 'dotnet.js') {
            return '/wasm/dotnet.js'
          }

          return resolveWasmAssetPath(defaultUri)
        }
      })

      ensurePreviewState()
      if (window.__razorConsolePreview) {
        window.__razorConsolePreview.started = true
        window.__razorConsolePreview.scriptUrl = scriptUrl
      }

      registerPreviewElements()
    })()
  }

  await blazorStartPromise
}

export async function mountComponentPreview(componentName: string, terminalElementId: string): Promise<() => void> {
  const definition = previewRegistry[componentName]
  if (!definition) {
    throw new Error(`Component '${componentName}' is not registered for live preview.`)
  }

  await ensureBlazorStarted()

  if (!isTerminalAvailable()) {
    throw new Error('xterm.js not loaded')
  }

  if (activeHosts.has(terminalElementId)) {
    const existing = activeHosts.get(terminalElementId)!
    return () => {
      disposeTerminal(terminalElementId)
      existing.remove()
      activeHosts.delete(terminalElementId)
    }
  }

  const terminalElement = document.getElementById(terminalElementId)
  if (!terminalElement) {
    throw new Error(`Terminal host element '${terminalElementId}' was not found.`)
  }

  const host = document.createElement(definition.tagName)
  host.setAttribute('terminal-id', terminalElementId)
  host.setAttribute('component-id', componentName.toLowerCase())

  const attachmentPoint = terminalElement.parentElement ?? terminalElement
  attachmentPoint.appendChild(host)
  activeHosts.set(terminalElementId, host)

  return () => {
    if (activeHosts.get(terminalElementId) === host) {
      disposeTerminal(terminalElementId)
      host.remove()
      activeHosts.delete(terminalElementId)
    }
  }
}
