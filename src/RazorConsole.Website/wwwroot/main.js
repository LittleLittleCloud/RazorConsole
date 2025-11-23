console.log('main.js loaded');

import { dotnet } from './_framework/dotnet.js'
import {
    writeToTerminal as forwardToTerminal,
    initTerminal as forwardInitTerminal,
    clearTerminal as forwardClearTerminal,
    disposeTerminal as forwardDisposeTerminal,
    attachKeyListener as forwardAttachKeyListener,
    isTerminalAvailable as forwardIsTerminalAvailable,
    registerTerminalInstance as forwardRegisterTerminalInstance
} from './xterm-interop.js';
const { setModuleImports } = await dotnet.create();

let exportsPromise = null;

async function createRuntimeAndGetExports() {
    const { getAssemblyExports, getConfig } = await dotnet.create();
    const config = getConfig();
    return await getAssemblyExports(config.mainAssemblyName);
}

setModuleImports('main.js', {
    writeToTerminal: (componentName, data) => forwardToTerminal(componentName, data),
    initTerminal: (componentName, options) => forwardInitTerminal(componentName, options),
    clearTerminal: (componentName) => forwardClearTerminal(componentName),
    disposeTerminal: (componentName) => forwardDisposeTerminal(componentName),
    attachKeyListener: (componentName, helper) => forwardAttachKeyListener(componentName, helper),
    isTerminalAvailable: () => forwardIsTerminalAvailable()
});

export async function registerComponent(elementID)
{
    if (exportsPromise === null) {
        exportsPromise = createRuntimeAndGetExports();
    }

    const exports = await exportsPromise;
    return exports.Registry.RegisterComponent(elementID);
}

export function registerTerminalInstance(elementId, terminal) {
    if (!terminal) {
        throw new Error(`Cannot register terminal '${elementId}' because no instance was provided.`);
    }

    forwardRegisterTerminalInstance(elementId, terminal);
}

export async function handleKeyboardEvent(componentName, xtermKey, domKey, ctrlKey, altKey, shiftKey) {
    if (exportsPromise === null) {
        exportsPromise = createRuntimeAndGetExports();
    }

    const exports = await exportsPromise;
    return exports.Registry.HandleKeyboardEvent(componentName, xtermKey, domKey, ctrlKey, altKey, shiftKey);
}

export async function writeToTerminal(componentName, data) {
    forwardToTerminal(componentName, data);
}

export async function initTerminal(componentName, options) {
    forwardInitTerminal(componentName, options);
}

export function clearTerminal(componentName) {
    console.log(`Clearing terminal from main.js: ${componentName}`);
    forwardClearTerminal(componentName);
}

export function disposeTerminal(componentName) {
    console.log(`Disposing terminal from main.js: ${componentName}`);
    forwardDisposeTerminal(componentName);
}

export function attachKeyListener(componentName, helper) {
    forwardAttachKeyListener(componentName, helper);
}

export function isTerminalAvailable() {
    return forwardIsTerminalAvailable();
}
