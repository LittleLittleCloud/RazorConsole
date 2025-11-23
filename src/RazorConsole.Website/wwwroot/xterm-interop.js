/** @typedef {import('xterm').ITerminalOptions} ITerminalOptions */
/** @typedef {import('xterm').ITheme} ITheme */

/** @typedef {{ theme?: Partial<ITheme> }} RazorConsoleTerminalOverrides */
/** @typedef {Partial<ITerminalOptions> & RazorConsoleTerminalOverrides} RazorConsoleTerminalOptions */

/** @type {Map<string, import('xterm').IDisposable>} */
const keyHandlers = new Map();

const terminalInstances = new Map();
/** @type {RazorConsoleTerminalOptions} */
const defaultOptions = {
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
    scrollback: 1000,
    cols: 80,
    rows: 150,
    rendererType: 'dom'
};
/**
 * @returns {typeof import('xterm').Terminal}
 */
function getTerminalConstructor() {
    const ctor = typeof window !== 'undefined' ? window.Terminal : undefined;
    if (!ctor) {
        throw new Error('xterm.js is not loaded.');
    }
    return ctor;
}
/**
 * @param {Partial<ITheme> | undefined} base
 * @param {Partial<ITheme> | undefined} overrides
 * @returns {Partial<ITheme> | undefined}
 */
function mergeThemes(base, overrides) {
    if (!base && !overrides) {
        return undefined;
    }
    return { ...(base ?? {}), ...(overrides ?? {}) };
}
/**
 * @param {string} elementId
 * @returns {HTMLElement}
 */
function ensureHostElement(elementId) {
    const host = document.getElementById(elementId);
    if (!host) {
        throw new Error(`Element with id '${elementId}' was not found.`);
    }
    return host;
}
/**
 * @param {string} elementId
 * @returns {import('xterm').Terminal}
 */
function getExistingTerminal(elementId) {
    console.log(`Getting existing terminal: ${elementId}`);
    const terminal = getTerminalInstance(elementId);
    console.log('Terminal found:', terminal);
    if (!terminal) {
        throw new Error(`Terminal with id '${elementId}' has not been initialized.`);
    }
    return terminal;
}
export function isTerminalAvailable() {
    return typeof window !== 'undefined' && typeof window.Terminal === 'function';
}

export function registerTerminalInstance(elementId, terminal) {
    terminalInstances.set(elementId, terminal);
}

export function getTerminalInstance(elementId) {
    return terminalInstances.get(elementId);
}
/**
 * @param {string} elementId
 * @param {RazorConsoleTerminalOptions | undefined} options
 * @returns {import('xterm').Terminal}
 */
export function initTerminal(elementId, options) {
    const TerminalCtor = getTerminalConstructor();
    const host = ensureHostElement(elementId);
    disposeTerminal(elementId);
    const mergedOptions = {
        ...defaultOptions,
        ...options,
        theme: mergeThemes(defaultOptions.theme, options?.theme)
    };

    console.log('Merged terminal options:', mergedOptions);
    const terminal = new TerminalCtor(mergedOptions);
    host.innerHTML = '';
    terminal.open(host);
    return terminal;
}
/**
 * @param {string} elementId
 * @param {string} text
 * @returns {void}
 */
export function writeToTerminal(elementId, text) {
    if (typeof text !== 'string' || text.length === 0) {
        return;
    }
    const terminal = getExistingTerminal(elementId);
    terminal.write(text);
}
/**
 * @param {string} elementId
 */
export function clearTerminal(elementId) {
    const terminal = getExistingTerminal(elementId);
    terminal.clear();
}
/**
 * @param {string} elementId
 * @param {{ invokeMethodAsync: (methodName: string, ...args: unknown[]) => Promise<unknown> }} helper
 */
export function attachKeyListener(elementId, helper) {
    const terminal = getExistingTerminal(elementId);
    keyHandlers.get(elementId)?.dispose();
    const subscription = terminal.onKey(event => {
        void helper.invokeMethodAsync('HandleKeyboardEvent', elementId, event.key, event.domEvent.key, event.domEvent.ctrlKey, event.domEvent.altKey, event.domEvent.shiftKey);
    });
    keyHandlers.set(elementId, subscription);
}
/**
 * @param {string} elementId
 */
export function disposeTerminal(elementId) {
    terminalInstances.delete(elementId);
    keyHandlers.get(elementId)?.dispose();
    keyHandlers.delete(elementId);
}
function ensureGlobalApi() {
    if (typeof window === 'undefined') {
        return;
    }
    const api = {
        init: (elementId, options) => {
            initTerminal(elementId, options);
        },
        write: writeToTerminal,
        clear: clearTerminal,
        dispose: disposeTerminal,
        attachKeyListener: (elementId, helper) => {
            attachKeyListener(elementId, helper);
        }
    };
    window.razorConsoleTerminal = api;
}
ensureGlobalApi();
