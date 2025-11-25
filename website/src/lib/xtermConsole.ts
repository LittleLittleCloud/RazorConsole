import { Terminal } from 'xterm'
import type { IDisposable, ITerminalOptions, ITheme } from 'xterm'
import 'xterm/css/xterm.css'

type TerminalConstructor = typeof Terminal
type TerminalType = InstanceType<typeof Terminal>

type TerminalOptions = Partial<ITerminalOptions> & { theme?: Partial<ITheme> }

type DotNetHelper = {
  invokeMethodAsync: (methodName: string, ...args: unknown[]) => Promise<unknown>
}

type RazorConsoleTerminalApi = {
  init: (elementId: string, options?: TerminalOptions) => void
  write: (elementId: string, text: string) => void
  clear: (elementId: string) => void
  dispose: (elementId: string) => void
  attachKeyListener: (elementId: string, helper: DotNetHelper) => void
}

declare global {
  interface Window {
    Terminal?: TerminalConstructor
    razorConsoleTerminal?: RazorConsoleTerminalApi
  }
}

const terminals = new Map<string, TerminalType>()
const keyHandlers = new Map<string, IDisposable>()

const defaultOptions: TerminalOptions = {
  convertEol: true,
  disableStdin: false,
  allowTransparency: false,
  theme: {
    background: '#1e1e1e',
    foreground: '#d4d4d4'
  },
  fontFamily: 'Consolas, "Courier New", monospace',
  fontSize: 14,
  lineHeight: 1.2,
  cursorBlink: false,
  scrollback: 1000
}

function getTerminalConstructor(): TerminalConstructor {
  const ctor = typeof window !== 'undefined' ? window.Terminal : undefined
  if (!ctor) {
    throw new Error('xterm.js is not loaded.')
  }

  return ctor
}

function mergeThemes(base: Partial<ITheme> | undefined, overrides: Partial<ITheme> | undefined): Partial<ITheme> | undefined {
  if (!base && !overrides) {
    return undefined
  }

  return { ...(base ?? {}), ...(overrides ?? {}) }
}

function ensureHostElement(elementId: string): HTMLElement {
  const host = document.getElementById(elementId)
  if (!host) {
    throw new Error(`Element with id '${elementId}' was not found.`)
  }

  return host
}

function getExistingTerminal(elementId: string): TerminalType {
  const terminal = terminals.get(elementId)
  if (!terminal) {
    throw new Error(`Terminal with id '${elementId}' has not been initialized.`)
  }

  return terminal
}

export function isTerminalAvailable(): boolean {
  return typeof window !== 'undefined' && typeof window.Terminal === 'function'
}

export function registerTerminalInstance(elementId: string, terminal: TerminalType): void {
  terminals.set(elementId, terminal)
}

export function getTerminalInstance(elementId: string): TerminalType | undefined {
  return terminals.get(elementId)
}

export function initTerminal(elementId: string, options?: TerminalOptions): TerminalType {
  const TerminalCtor = getTerminalConstructor()
  const host = ensureHostElement(elementId)

  disposeTerminal(elementId)

  const mergedOptions: ITerminalOptions = {
    ...defaultOptions,
    ...options,
    theme: mergeThemes(defaultOptions.theme, options?.theme)
  }

  const terminal = new TerminalCtor(mergedOptions)
  host.innerHTML = ''
  terminal.open(host)
  terminals.set(elementId, terminal)
  return terminal
}

export function writeToTerminal(elementId: string, text: string): void {
  if (typeof text !== 'string' || text.length === 0) {
    return
  }

  const terminal = getExistingTerminal(elementId)
  terminal.write(text)
}

export function clearTerminal(elementId: string): void {
  const terminal = getExistingTerminal(elementId)
  terminal.clear()
}

export function attachKeyListener(elementId: string, helper: DotNetHelper): void {
  const terminal = getExistingTerminal(elementId)

  keyHandlers.get(elementId)?.dispose()

  const subscription = terminal.onKey(event => {
    void helper.invokeMethodAsync(
      'HandleKeyboardEvent',
      event.key,
      event.domEvent.key,
      event.domEvent.ctrlKey,
      event.domEvent.altKey,
      event.domEvent.shiftKey
    )
  })

  keyHandlers.set(elementId, subscription)
}

export function disposeTerminal(elementId: string): void {
  keyHandlers.get(elementId)?.dispose()
  keyHandlers.delete(elementId)

  const terminal = terminals.get(elementId)
  if (!terminal) {
    return
  }

  terminal.dispose()
  terminals.delete(elementId)
}

function ensureGlobalApi(): void {
  if (typeof window === 'undefined') {
    return
  }

  if (typeof window.Terminal !== 'function') {
    window.Terminal = Terminal
  }

  const api: RazorConsoleTerminalApi = {
    init: (elementId, options) => {
      initTerminal(elementId, options)
    },
    write: writeToTerminal,
    clear: clearTerminal,
    dispose: disposeTerminal,
    attachKeyListener: (elementId, helper) => {
      attachKeyListener(elementId, helper)
    }
  }

  window.razorConsoleTerminal = api
}

ensureGlobalApi()

// ================================
// C# WASM Interop Functions
// ================================
// These functions call into the C# WebAssembly runtime via the main.js module.
// They are wrappers around the exported C# methods from the Registry class.

type WasmExports = {
  Registry: {
    RegisterComponent: (elementId: string) => Promise<void>
    HandleKeyboardEvent: (
      componentName: string,
      xtermKey: string,
      domKey: string,
      ctrlKey: boolean,
      altKey: boolean,
      shiftKey: boolean
    ) => Promise<void>
  }
}

let wasmExportsPromise: Promise<WasmExports> | null = null

/**
 * Path to the main.js WASM module.
 * This path is relative to the website's public directory.
 */
const WASM_MAIN_JS_PATH = '/wasm/wwwroot/main.js'

/**
 * Gets the WASM exports from main.js.
 * This dynamically imports main.js to get access to createRuntimeAndGetExports.
 */
async function getWasmExports(): Promise<WasmExports> {
  if (wasmExportsPromise === null) {
    wasmExportsPromise = (async () => {
      try {
        // Import the main.js module which provides createRuntimeAndGetExports
        const mainModule = await import(/* @vite-ignore */ WASM_MAIN_JS_PATH) as { createRuntimeAndGetExports?: () => Promise<WasmExports> }
        
        if (typeof mainModule.createRuntimeAndGetExports !== 'function') {
          throw new Error('main.js does not export createRuntimeAndGetExports function')
        }
        
        return await mainModule.createRuntimeAndGetExports()
      } catch (error) {
        const message = error instanceof Error ? error.message : String(error)
        throw new Error(`Failed to load WASM module from ${WASM_MAIN_JS_PATH}: ${message}`)
      }
    })()
  }
  return wasmExportsPromise
}

/**
 * Registers a Razor component so its renderer can stream updates into the terminal.
 * Calls into C# WASM: Registry.RegisterComponent(elementId)
 * @param elementId - The ID of the terminal element to register
 */
export async function registerComponent(elementId: string): Promise<void> {
  const exports = await getWasmExports()
  return exports.Registry.RegisterComponent(elementId)
}

/**
 * Forwards a keyboard event from xterm.js to the RazorConsole renderer.
 * Calls into C# WASM: Registry.HandleKeyboardEvent(componentName, xtermKey, domKey, ctrlKey, altKey, shiftKey)
 * @param componentName - The name of the component receiving the event
 * @param xtermKey - The key as reported by xterm.js
 * @param domKey - The key as reported by the DOM event
 * @param ctrlKey - Whether Ctrl was held
 * @param altKey - Whether Alt was held
 * @param shiftKey - Whether Shift was held
 */
export async function handleKeyboardEvent(
  componentName: string,
  xtermKey: string,
  domKey: string,
  ctrlKey: boolean,
  altKey: boolean,
  shiftKey: boolean
): Promise<void> {
  const exports = await getWasmExports()
  return exports.Registry.HandleKeyboardEvent(componentName, xtermKey, domKey, ctrlKey, altKey, shiftKey)
}

export type { DotNetHelper, RazorConsoleTerminalApi, TerminalOptions }
