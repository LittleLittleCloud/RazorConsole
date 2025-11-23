console.log('main.js loaded');

import { dotnet } from './_framework/dotnet.js'
import { writeToTerminal as forwardToTerminal } from './xterm-interop.js';
const { setModuleImports } = await dotnet.create();

let exportsPromise = null;

async function createRuntimeAndGetExports() {
    const { getAssemblyExports, getConfig } = await dotnet.create();
    const config = getConfig();
    return await getAssemblyExports(config.mainAssemblyName);
}



setModuleImports('main.js', {
    writeToTerminal: (componentName, data) => forwardToTerminal(componentName, data)
});

export async function registerComponent(componentName)
{
    if (exportsPromise === null) {
        exportsPromise = createRuntimeAndGetExports();
    }

    const exports = await exportsPromise;

    console.log(`Registering component from main.js: ${componentName}`);

    return exports.Registry.RegisterComponent(componentName);
}

export async function writeToTerminal(componentName, data) {
    console.log(`Writing to terminal from main.js: ${componentName}, ${data}`);
    forwardToTerminal(componentName, data);
}
