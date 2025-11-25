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
