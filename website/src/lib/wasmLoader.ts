/**
 * WASM Loader for RazorConsole Gallery
 * 
 * This module handles loading and initializing the RazorConsole.Gallery WASM module,
 * and provides an interface for bidirectional communication between the browser and
 * the .NET WASM runtime.
 */

export interface WasmModule {
  sendInput: (input: string) => void;
  sendKeyPress: (key: string) => void;
  dispose: () => void;
}

export interface WasmCallbacks {
  onOutput?: (data: string) => void;
  onError?: (error: string) => void;
  onReady?: () => void;
}

// Singleton instance to prevent double initialization
let wasmModuleInstance: WasmModule | null = null;
let wasmInitPromise: Promise<WasmModule> | null = null;

/**
 * Loads the RazorConsole.Gallery WASM module
 * @param callbacks Callbacks for handling WASM events
 * @returns Promise that resolves to a WasmModule interface
 */
export async function loadWasmModule(callbacks: WasmCallbacks): Promise<WasmModule> {
  // Return existing instance if already loaded
  if (wasmModuleInstance) {
    console.log('Returning existing WASM module instance');
    return wasmModuleInstance;
  }
  
  // Return existing promise if already loading
  if (wasmInitPromise) {
    console.log('WASM module is already loading, waiting for it...');
    return wasmInitPromise;
  }
  
  // Start loading
  wasmInitPromise = loadWasmModuleInternal(callbacks);
  
  try {
    wasmModuleInstance = await wasmInitPromise;
    return wasmModuleInstance;
  } catch (error) {
    // Reset on error so it can be retried
    wasmInitPromise = null;
    throw error;
  }
}

async function loadWasmModuleInternal(callbacks: WasmCallbacks): Promise<WasmModule> {
  const { onOutput, onError, onReady } = callbacks;

  try {
    // Load dotnet.js using a script tag to avoid Vite processing
    const dotnetLoaderUrl = getWasmBundleUrl();
    
    console.log('Loading dotnet loader from:', dotnetLoaderUrl);
    
    await new Promise<void>((resolve, reject) => {
      const script = document.createElement('script');
      script.src = dotnetLoaderUrl;
      script.type = 'module';
      script.onload = () => {
        console.log('dotnet loader loaded successfully');
        resolve();
      };
      script.onerror = (err) => {
        console.error('Failed to load dotnet loader', err);
        reject(new Error('Failed to load dotnet loader'));
      };
      document.head.appendChild(script);
    });
    
    console.log('Waiting for dotnetRuntime global...');
    
    // Wait for dotnetRuntime global to be available
    await new Promise<void>((resolve, reject) => {
      let attempts = 0;
      const maxAttempts = 100; // 10 seconds max
      
      const checkDotnet = () => {
        attempts++;
        if (attempts % 10 === 0) {
          console.log(`Checking for dotnetRuntime global (attempt ${attempts}/${maxAttempts})`);
        }
        
        if ((window as any).dotnetRuntime) {
          console.log('dotnetRuntime global found!');
          resolve();
        } else if (attempts >= maxAttempts) {
          reject(new Error('Timeout waiting for dotnetRuntime global'));
        } else {
          setTimeout(checkDotnet, 100);
        }
      };
      checkDotnet();
    });
    
    const dotnet = (window as any).dotnetRuntime;
    console.log('Got dotnet:', dotnet);
    
    // Set up console output capture before initializing
    const originalLog = console.log;
    const originalError = console.error;
    const originalWarn = console.warn;
    
    if (onOutput) {
      console.log = function(...args: any[]) {
        originalLog.apply(console, args);
        onOutput(args.join(' ') + '\r\n');
      };
      
      console.error = function(...args: any[]) {
        originalError.apply(console, args);
        onOutput('\x1b[31m' + args.join(' ') + '\x1b[0m\r\n');
      };
      
      console.warn = function(...args: any[]) {
        originalWarn.apply(console, args);
        onOutput('\x1b[33m' + args.join(' ') + '\x1b[0m\r\n');
      };
    }
    
    console.log('Initializing .NET runtime...');
    
    // Initialize the .NET runtime
    const { getAssemblyExports, getConfig, runMain } = await dotnet
      .withDiagnosticTracing(false)
      .withApplicationArguments(...[])
      .create();

    console.log('Getting config...');
    const config = getConfig();
    console.log('Getting exports...');
    const exports = await getAssemblyExports(config.mainAssemblyName!);

    console.log('Running main...');
    // Start the .NET application
    await runMain();
    
    console.log('.NET application started!');
    
    // Notify ready
    if (onReady) {
      onReady();
    }
    
    // Get the browser keyboard interop exports
    const browserInterop = (exports as any).RazorConsole?.Gallery?.Platforms?.Browser?.BrowserKeyboardInterop;
    
    return {
      sendInput: (input: string) => {
        // Send character input
        if (browserInterop?.HandleKeyFromJS) {
          browserInterop.HandleKeyFromJS(input, false, false, false);
        }
      },
      sendKeyPress: (key: string) => {
        // For special keys, pass them directly
        if (browserInterop?.HandleKeyFromJS) {
          browserInterop.HandleKeyFromJS(key, false, false, false);
        }
      },
      dispose: () => {
        // Restore console
        console.log = originalLog;
        console.error = originalError;
        console.warn = originalWarn;
      },
    };
  } catch (error) {
    console.error('Error in loadWasmModule:', error);
    if (onError) {
      onError(error instanceof Error ? error.message : "Unknown error loading WASM");
    }
    throw error;
  }
}

/**
 * Gets the URL for the WASM bundle
 * This should point to the AppBundle generated by the .NET build
 */
export function getWasmBundleUrl(): string {
  // Use environment variable or default to production path
  const basePath = import.meta.env.BASE_URL || '/';
  return `${basePath}wasm/dotnet-loader.js`;
}

/**
 * Checks if WASM is supported in the current browser
 */
export function isWasmSupported(): boolean {
  try {
    if (typeof WebAssembly === "object" &&
        typeof WebAssembly.instantiate === "function") {
      const module = new WebAssembly.Module(Uint8Array.of(0x0, 0x61, 0x73, 0x6d, 0x01, 0x00, 0x00, 0x00));
      if (module instanceof WebAssembly.Module) {
        return new WebAssembly.Instance(module) instanceof WebAssembly.Instance;
      }
    }
  } catch (e) {
    // WebAssembly not supported
  }
  return false;
}
