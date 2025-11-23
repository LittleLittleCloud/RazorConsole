export type TerminalComponentName = string;

/**
 * Writes pre-rendered Razor output into the xterm.js instance for the specified component.
 * Resolves once the text has been forwarded to the terminal bridge.
 */
export declare function writeToTerminal(componentName: TerminalComponentName, data: string): Promise<void>;
