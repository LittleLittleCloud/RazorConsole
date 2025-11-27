import { execSync } from 'child_process';
import path from 'path';
import { fileURLToPath } from 'url';
import fs from 'fs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const projectPath = path.resolve(__dirname, '../../src/RazorConsole.Website/RazorConsole.Website.csproj');

console.log(`Building WASM project: ${projectPath}`);

try {
    execSync(`dotnet publish "${projectPath}" --configuration Release`, { stdio: 'inherit' });
    
    // Copy the published wwwroot to public/wasm for serving
    const publishedWwwroot = path.resolve(__dirname, '../../artifacts/publish/RazorConsole.Website/release/wwwroot');
    const publicWasm = path.resolve(__dirname, '../public/wasm');
    
    console.log(`Copying WASM output from ${publishedWwwroot} to ${publicWasm}`);
    
    // Remove old wasm folder if it exists
    if (fs.existsSync(publicWasm)) {
        fs.rmSync(publicWasm, { recursive: true });
    }
    
    // Copy published wwwroot to public/wasm
    fs.cpSync(publishedWwwroot, publicWasm, { recursive: true });
    
    // Also copy DLL files from the bin folder for dynamic compilation
    // These are the pre-WASM .dll files that Roslyn can compile against
    const binFolder = path.resolve(__dirname, '../../artifacts/bin/RazorConsole.Website/release');
    const publicDlls = path.resolve(__dirname, '../public/wasm/dlls');
    
    console.log(`Copying DLL files for dynamic compilation...`);
    
    // Create dlls folder
    if (!fs.existsSync(publicDlls)) {
        fs.mkdirSync(publicDlls, { recursive: true });
    }
    
    // Copy all DLL files from bin
    if (fs.existsSync(binFolder)) {
        const files = fs.readdirSync(binFolder);
        let copiedCount = 0;
        
        for (const file of files) {
            if (file.endsWith('.dll')) {
                const sourcePath = path.join(binFolder, file);
                const destPath = path.join(publicDlls, file);
                fs.copyFileSync(sourcePath, destPath);
                copiedCount++;
            }
        }
        
        console.log(`Copied ${copiedCount} DLL files to ${publicDlls}`);
    }
    
    console.log('WASM files copied successfully');
} catch (e) {
    console.error('Error:', e.message);
    process.exit(1);
}
