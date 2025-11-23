import { useEffect, useRef, useState } from "react"
import { Terminal } from "@xterm/xterm"
import { FitAddon } from "@xterm/addon-fit"
import { WebLinksAddon } from "@xterm/addon-web-links"
import "@xterm/xterm/css/xterm.css"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { RefreshCw, AlertCircle } from "lucide-react"

export default function Gallery() {
  const terminalRef = useRef<HTMLDivElement>(null)
  const xtermRef = useRef<Terminal | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isReady, setIsReady] = useState(false)

  const initializeWasm = async () => {
    if (!terminalRef.current) return

    setIsLoading(true)
    setError(null)
    setIsReady(false)

    try {
      // Clean up existing terminal
      if (xtermRef.current) {
        xtermRef.current.dispose()
      }

      // Create new terminal
      const term = new Terminal({
        cursorBlink: true,
        fontSize: 14,
        fontFamily: 'Menlo, Monaco, "Courier New", monospace',
        theme: {
          background: "#1e1e1e",
          foreground: "#d4d4d4",
          cursor: "#ffffff",
          cursorAccent: "#000000",
          selectionBackground: "#3a3d41",
        },
        cols: 120,
        rows: 30,
      })

      const fitAddon = new FitAddon()
      const webLinksAddon = new WebLinksAddon()

      term.loadAddon(fitAddon)
      term.loadAddon(webLinksAddon)

      term.open(terminalRef.current)
      fitAddon.fit()

      xtermRef.current = term

      // Display loading message
      term.writeln("\x1b[1;32mRazorConsole Gallery - WASM Edition\x1b[0m")
      term.writeln("")
      term.writeln("Initializing WASM module...")
      term.writeln("")

      // Handle terminal resize
      const resizeObserver = new ResizeObserver(() => {
        fitAddon.fit()
      })
      resizeObserver.observe(terminalRef.current)

      // Import WASM loader dynamically
      const { loadWasmModule, isWasmSupported } = await import("@/lib/wasmLoader")

      // Check WASM support
      if (!isWasmSupported()) {
        throw new Error("WebAssembly is not supported in this browser")
      }

      // Load WASM module with callbacks
      const wasmModule = await loadWasmModule({
        onOutput: (data) => {
          term.write(data)
        },
        onError: (error) => {
          term.writeln(`\r\n\x1b[1;31mError: ${error}\x1b[0m\r\n`)
        },
        onReady: () => {
          setIsLoading(false)
          setIsReady(true)
        },
      })

      // Handle keyboard events
      term.onKey((event) => {
        const key = event.key
        
        // Send to WASM
        if (key.length === 1) {
          // Single character
          wasmModule.sendInput(key)
        } else {
          // Special key (Enter, Tab, Arrow keys, etc.)
          wasmModule.sendKeyPress(key)
        }
      })

      // Cleanup function
      return () => {
        resizeObserver.disconnect()
        wasmModule.dispose()
        term.dispose()
      }
    } catch (err) {
      console.error("Failed to initialize WASM:", err)
      setError(err instanceof Error ? err.message : "Failed to initialize WASM module")
      setIsLoading(false)
    }
  }

  useEffect(() => {
    let cleanup: (() => void) | undefined;
    
    initializeWasm().then(cleanupFn => {
      cleanup = cleanupFn;
    });

    return () => {
      if (cleanup) {
        cleanup();
      }
      if (xtermRef.current) {
        xtermRef.current.dispose()
      }
    }
  }, [])

  const handleReload = () => {
    initializeWasm()
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-4">Interactive Gallery</h1>
        <p className="text-lg text-slate-600 dark:text-slate-300">
          Experience RazorConsole components running directly in your browser via WebAssembly
        </p>
      </div>

      <Card className="mb-6">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>WASM Terminal</CardTitle>
              <CardDescription>
                {isLoading && "Loading WebAssembly module..."}
                {isReady && "Interactive terminal ready"}
                {error && "Failed to load"}
              </CardDescription>
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={handleReload}
              disabled={isLoading}
              className="gap-2"
            >
              <RefreshCw className={`w-4 h-4 ${isLoading ? "animate-spin" : ""}`} />
              Reload
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {error && (
            <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-start gap-3">
              <AlertCircle className="w-5 h-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" />
              <div>
                <p className="font-semibold text-red-900 dark:text-red-100">Error</p>
                <p className="text-sm text-red-700 dark:text-red-300">{error}</p>
              </div>
            </div>
          )}
          <div
            ref={terminalRef}
            className="rounded-lg overflow-hidden border border-slate-200 dark:border-slate-700"
            style={{ minHeight: "500px" }}
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>About This Demo</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-slate-600 dark:text-slate-300">
            This page demonstrates RazorConsole running as a WebAssembly application in your browser.
            The terminal you see above is powered by <strong>xterm.js</strong> and connects to a 
            .NET 10 WASM build of RazorConsole.Gallery.
          </p>
          <div>
            <h3 className="font-semibold mb-2">Technical Stack</h3>
            <ul className="list-disc list-inside space-y-1 text-slate-600 dark:text-slate-300">
              <li>.NET 10 with browser-wasm runtime</li>
              <li>xterm.js for terminal emulation</li>
              <li>JavaScript interop for input/output bridging</li>
              <li>RazorConsole.Core and RazorConsole.Gallery running client-side</li>
            </ul>
          </div>
          <div>
            <h3 className="font-semibold mb-2">Architecture</h3>
            <p className="text-slate-600 dark:text-slate-300">
              The WASM module captures Spectre.Console output and forwards it to the terminal,
              while keyboard events from xterm.js are sent back to the .NET application for
              interactive components.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
