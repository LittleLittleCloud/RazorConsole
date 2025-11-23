const terminals = new Map();
const keyHandlers = new Map();
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
    scrollback: 1000
};
function getTerminalConstructor() {
    const ctor = typeof window !== 'undefined' ? window.Terminal : undefined;
    if (!ctor) {
        throw new Error('xterm.js is not loaded.');
    }
    return ctor;
}
function mergeThemes(base, overrides) {
    if (!base && !overrides) {
        return undefined;
    }
    return { ...(base ?? {}), ...(overrides ?? {}) };
}
function ensureHostElement(elementId) {
    const host = document.getElementById(elementId);
    if (!host) {
        throw new Error(`Element with id '${elementId}' was not found.`);
    }
    return host;
}
function getExistingTerminal(elementId) {
    const terminal = terminals.get(elementId);
    if (!terminal) {
        throw new Error(`Terminal with id '${elementId}' has not been initialized.`);
    }
    return terminal;
}
export function isTerminalAvailable() {
    return typeof window !== 'undefined' && typeof window.Terminal === 'function';
}
export function initTerminal(elementId, options) {
    const TerminalCtor = getTerminalConstructor();
    const host = ensureHostElement(elementId);
    disposeTerminal(elementId);
    const mergedOptions = {
        ...defaultOptions,
        ...options,
        theme: mergeThemes(defaultOptions.theme, options?.theme)
    };
    const terminal = new TerminalCtor(mergedOptions);
    host.innerHTML = '';
    terminal.open(host);
    terminals.set(elementId, terminal);
    return terminal;
}
export function writeToTerminal(elementId, text) {
    if (typeof text !== 'string' || text.length === 0) {
        return;
    }
    const terminal = getExistingTerminal(elementId);
    terminal.write(text);
}
export function clearTerminal(elementId) {
    const terminal = getExistingTerminal(elementId);
    terminal.clear();
}
export function attachKeyListener(elementId, helper) {
    const terminal = getExistingTerminal(elementId);
    keyHandlers.get(elementId)?.dispose();
    const subscription = terminal.onKey(event => {
        void helper.invokeMethodAsync('HandleKeyboardEvent', event.key, event.domEvent.key, event.domEvent.ctrlKey, event.domEvent.altKey, event.domEvent.shiftKey);
    });
    keyHandlers.set(elementId, subscription);
}
export function disposeTerminal(elementId) {
    keyHandlers.get(elementId)?.dispose();
    keyHandlers.delete(elementId);
    const terminal = terminals.get(elementId);
    if (!terminal) {
        return;
    }
    terminal.dispose();
    terminals.delete(elementId);
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
