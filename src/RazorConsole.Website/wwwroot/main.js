console.log('main.js loaded');

import { dotnet } from './_framework/dotnet.js'

const { setModuleImports } = await dotnet.create();

let exportsPromise = null;

async function createRuntimeAndGetExports() {
    const { getAssemblyExports, getConfig } = await dotnet.create();
    const config = getConfig();
    return await getAssemblyExports(config.mainAssemblyName);
}

/**
 * Gets the terminal API from the global window object.
 * The terminal API is set up by xtermConsole.ts in the website bundle.
 * @returns {object} The terminal API with init, write, clear, dispose, and attachKeyListener methods
 */
function getTerminalApi() {
    if (typeof window === 'undefined' || !window.razorConsoleTerminal) {
        throw new Error('Terminal API is not available. Make sure xtermConsole.ts is loaded first.');
    }
    return window.razorConsoleTerminal;
}

setModuleImports('main.js', {
    writeToTerminal: (componentName, data) => getTerminalApi().write(componentName, data),
    initTerminal: (componentName, options) => getTerminalApi().init(componentName, options),
    clearTerminal: (componentName) => getTerminalApi().clear(componentName),
    disposeTerminal: (componentName) => getTerminalApi().dispose(componentName),
    attachKeyListener: (componentName, helper) => getTerminalApi().attachKeyListener(componentName, helper),
    isTerminalAvailable: () => typeof window !== 'undefined' && !!window.razorConsoleTerminal
});

export async function registerComponent(elementID)
{
    if (exportsPromise === null) {
        exportsPromise = createRuntimeAndGetExports();
    }

    const exports = await exportsPromise;
    return exports.Registry.RegisterComponent(elementID);
}

export async function handleKeyboardEvent(componentName, xtermKey, domKey, ctrlKey, altKey, shiftKey) {
    if (exportsPromise === null) {
        exportsPromise = createRuntimeAndGetExports();
    }

    const exports = await exportsPromise;
    return exports.Registry.HandleKeyboardEvent(componentName, xtermKey, domKey, ctrlKey, altKey, shiftKey);
}
