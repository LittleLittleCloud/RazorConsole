export type TerminalComponentName = string;

/**
 * Registers a Razor component so its renderer can stream updates into the terminal.
 */
export declare function registerComponent(componentName: TerminalComponentName): Promise<void>;

/**
 * Forwards a keyboard event from xterm.js to the RazorConsole renderer for the component.
 */
export declare function handleKeyboardEvent(
	componentName: TerminalComponentName,
	xtermKey: string,
	domKey: string,
	ctrlKey: boolean,
	altKey: boolean,
	shiftKey: boolean
): Promise<void>;

/**
 * Writes pre-rendered Razor output into the xterm.js instance for the specified component.
 * Resolves once the text has been forwarded to the terminal bridge.
 */
export declare function writeToTerminal(componentName: TerminalComponentName, data: string): Promise<void>;

/**
 * Initializes an xterm.js instance for the specified component.
 */
export declare function initTerminal(componentName: TerminalComponentName, options?: Record<string, unknown>): void;

/**
 * Clears existing text from the xterm.js instance.
 */
export declare function clearTerminal(componentName: TerminalComponentName): void;

/**
 * Disposes the xterm.js instance, freeing resources.
 */
export declare function disposeTerminal(componentName: TerminalComponentName): void;

/**
 * Tracks xterm.js instances so they can be retrieved for later operations.
 */
export declare function registerTerminalInstance(componentName: TerminalComponentName, terminal: import('xterm').Terminal): void;

/**
 * Attaches a key listener to the xterm.js instance.
 */
export declare function attachKeyListener(componentName: TerminalComponentName, helper: {
	invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}): void;

/**
 * Returns true when xterm.js is available in the current environment.
 */
export declare function isTerminalAvailable(): boolean;
