import { execSync } from 'child_process';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const projectPath = path.resolve(__dirname, '../../src/RazorConsole.Website/RazorConsole.Website.csproj');

console.log(`Building WASM project: ${projectPath}`);

try {
    execSync(`dotnet publish "${projectPath}" --configuration Release`, { stdio: 'inherit' });
} catch (e) {
    process.exit(1);
}
