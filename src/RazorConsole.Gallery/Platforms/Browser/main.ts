// TypeScript entry point for RazorConsole Gallery in browser
// Uses the dotnet WASM browser app approach from .NET 10

import { dotnet } from './_framework/dotnet.js'

interface DotNetExports {
    RazorConsole: {
        Gallery: {
            Platforms: {
                Browser: {
                    BrowserKeyboardInterop: {
                        HandleKeyFromJS: (key: string, shift: boolean, ctrl: boolean, alt: boolean) => void;
                    }
                }
            }
        }
    }
}

interface ConsoleOutputCallback {
    (data: string): void;
}

let outputCallback: ConsoleOutputCallback | null = null;

export async function initRazorConsole() {
    const { getAssemblyExports, getConfig, runMain } = await dotnet
        .withDiagnosticTracing(false)
        .withApplicationArguments(...[])
        .create();

    const config = getConfig();
    const exports = await getAssemblyExports(config.mainAssemblyName!) as DotNetExports;

    // Start the .NET application
    await runMain();

    return {
        sendKey: (key: string, shift: boolean = false, ctrl: boolean = false, alt: boolean = false) => {
            exports.RazorConsole.Gallery.Platforms.Browser.BrowserKeyboardInterop.HandleKeyFromJS(
                key, shift, ctrl, alt
            );
        }
    };
}

/**
 * Hook to capture console output and forward to callback.
 * This is called from wasmLoader.ts to set up the callback.
 */
export function onConsoleOutput(callback: ConsoleOutputCallback) {
    outputCallback = callback;
}

/**
 * Global function called by .NET BrowserConsole to output text.
 * This is invoked via JSImport from C#.
 */
(globalThis as any).onConsoleOutput = function(data: string) {
    if (outputCallback) {
        outputCallback(data);
    }
};
