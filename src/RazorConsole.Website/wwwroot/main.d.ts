/**
 * WASM exports from the C# runtime
 */
export interface WasmExports {
	Registry: {
		RegisterComponent: (elementId: string) => Promise<void>;
		HandleKeyboardEvent: (
			componentName: string,
			xtermKey: string,
			domKey: string,
			ctrlKey: boolean,
			altKey: boolean,
			shiftKey: boolean
		) => Promise<void>;
	};
}

/**
 * Creates the .NET runtime and returns the assembly exports.
 * This is the only exported function from main.js.
 */
export declare function createRuntimeAndGetExports(): Promise<WasmExports>;
