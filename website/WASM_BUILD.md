# Building RazorConsole Gallery for WASM

This document describes how to build and deploy the RazorConsole Gallery as a WebAssembly application for the website.

## Prerequisites

- .NET 10 SDK
- wasm-tools workload installed: `dotnet workload install wasm-tools`

## Building the WASM Module

### 1. Build RazorConsole.Gallery for browser-wasm target

```bash
cd /path/to/RazorConsole
dotnet build src/RazorConsole.Gallery/RazorConsole.Gallery.csproj -f net10.0-browser -c Release
```

The output will be generated at:
```
artifacts/bin/RazorConsole.Gallery/release_net10.0-browser_browser-wasm/AppBundle/
```

### 2. Copy WASM bundle to website public folder

```bash
# From repository root
cp -r artifacts/bin/RazorConsole.Gallery/release_net10.0-browser_browser-wasm/AppBundle/* website/public/wasm/
```

### 3. Build the website

```bash
cd website
npm install
npm run build
```

## Development Workflow

During development, you can use the debug build:

```bash
# Build WASM module
dotnet build src/RazorConsole.Gallery/RazorConsole.Gallery.csproj -f net10.0-browser

# Copy to website
cp -r artifacts/bin/RazorConsole.Gallery/debug_net10.0-browser_browser-wasm/AppBundle/* website/public/wasm/

# Run website dev server
cd website
npm run dev
```

Then navigate to http://localhost:5173/gallery to see the WASM terminal.

## Architecture

The WASM integration consists of several components:

### .NET Side (RazorConsole.Gallery)

- **Platforms/Browser/BrowserProgram.cs**: Entry point for browser-wasm target
- **Platforms/Browser/main.js**: JavaScript interop for console I/O
- **Platforms/Browser/MockNuGetUpgradeChecker.cs**: Browser-compatible service implementation

### Website Side (React/TypeScript)

- **src/pages/Gallery.tsx**: Main Gallery page with xterm.js terminal
- **src/lib/wasmLoader.ts**: WASM module loader and JS interop utilities

### Communication Flow

1. User opens `/gallery` page in browser
2. React app loads and initializes xterm.js terminal
3. WASM loader fetches and initializes dotnet.js runtime
4. RazorConsole.Gallery app starts in WASM
5. Console output is captured and forwarded to xterm.js
6. Keyboard events from xterm.js are sent to WASM app
7. User interacts with RazorConsole components in the browser

## Troubleshooting

### WASM module fails to load

- Check browser console for errors
- Verify wasm files are in `public/wasm/_framework/`
- Ensure CORS headers allow loading WASM files
- Check browser supports WebAssembly

### Terminal doesn't display output

- Verify WASM module initialized successfully
- Check JavaScript console for JS interop errors
- Ensure callbacks are properly set up in wasmLoader.ts

### Build errors

- Verify .NET 10 SDK is installed: `dotnet --version`
- Check wasm-tools workload: `dotnet workload list`
- Clean and rebuild: `dotnet clean && dotnet build`

## Future Enhancements

- [ ] Implement actual dotnet.js runtime loading
- [ ] Complete JS interop for bidirectional I/O
- [ ] Add support for Spectre.Console ANSI sequences
- [ ] Implement keyboard input handling
- [ ] Add support for interactive components (buttons, inputs, etc.)
- [ ] Optimize WASM bundle size
- [ ] Add service worker for offline support
- [ ] Implement hot reload for development
