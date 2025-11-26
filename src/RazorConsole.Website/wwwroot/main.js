console.log('main.js loaded');

import { dotnet } from './_framework/dotnet.js'

let dotnetInstancePromise = null;

async function getDotnetInstance() {
    if (!dotnetInstancePromise) {
        dotnetInstancePromise = dotnet.create();
    }
    return dotnetInstancePromise;
}

const { setModuleImports } = await getDotnetInstance();

/**
 * Creates the .NET runtime and returns the assembly exports.
 * This is the only exported function from main.js.
 * All other APIs are in xtermConsole.ts.
 */
export async function createRuntimeAndGetExports() {
    const { getAssemblyExports, getConfig } = await getDotnetInstance();
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
