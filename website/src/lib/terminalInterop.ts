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

export type { DotNetHelper, RazorConsoleTerminalApi, TerminalOptions }
