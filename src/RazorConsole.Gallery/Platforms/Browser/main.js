// JavaScript interop for RazorConsole Gallery in browser
export function initialize() {
    console.log("RazorConsole Gallery Browser Interop initialized");
}

// Hook console output to send to parent window or xterm
const originalConsoleLog = console.log;
const originalConsoleError = console.error;
const originalConsoleWarn = console.warn;

export function captureConsoleOutput(callback) {
    console.log = function(...args) {
        originalConsoleLog.apply(console, args);
        if (callback) {
            callback('log', args.join(' '));
        }
    };
    
    console.error = function(...args) {
        originalConsoleError.apply(console, args);
        if (callback) {
            callback('error', args.join(' '));
        }
    };
    
    console.warn = function(...args) {
        originalConsoleWarn.apply(console, args);
        if (callback) {
            callback('warn', args.join(' '));
        }
    };
}

// Send keyboard events to the .NET app
export function sendKeyboardEvent(key) {
    // This will be called from xterm.js
    // Implementation will depend on how RazorConsole handles input
    console.log("Keyboard event:", key);
}
