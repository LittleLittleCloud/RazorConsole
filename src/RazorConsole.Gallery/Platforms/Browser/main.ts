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
 * Hook to capture console output and forward to callback
 */
export function onConsoleOutput(callback: ConsoleOutputCallback) {
    outputCallback = callback;
    
    // Intercept console methods
    const originalLog = console.log;
    const originalError = console.error;
    const originalWarn = console.warn;
    
    console.log = function(...args: any[]) {
        originalLog.apply(console, args);
        if (outputCallback) {
            outputCallback(args.join(' ') + '\r\n');
        }
    };
    
    console.error = function(...args: any[]) {
        originalError.apply(console, args);
        if (outputCallback) {
            outputCallback('\x1b[31m' + args.join(' ') + '\x1b[0m\r\n');
        }
    };
    
    console.warn = function(...args: any[]) {
        originalWarn.apply(console, args);
        if (outputCallback) {
            outputCallback('\x1b[33m' + args.join(' ') + '\x1b[0m\r\n');
        }
    };
}
