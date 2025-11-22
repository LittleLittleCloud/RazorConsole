// WASM loader wrapper - loads dotnet.js and exposes dotnet global
// This file is copied to the AppBundle during build and must be served alongside _framework/dotnet.js
import { dotnet } from './_framework/dotnet.js';

// Expose dotnet as a global for the app to use
window.dotnetRuntime = dotnet;

console.log('dotnet runtime exposed successfully');
