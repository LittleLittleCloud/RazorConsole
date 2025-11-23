import { useEffect, useRef, useState } from "react";
import {
    registerComponent,
    attachKeyListener,
    handleKeyboardEvent,
    registerTerminalInstance,
} from "razor-console";
import 'xterm/css/xterm.css';
import { Terminal } from "xterm";
import { useTheme } from "@/components/ThemeProvider";

interface XTermPreviewProps {
    elementId: string;
    className?: string;
}

const TERMINAL_THEME = {
    light: {
        background: '#fafafa',
        foreground: '#383a42',
        cursor: '#383a42',
        selectionBackground: 'rgba(0, 0, 0, 0.15)',
        black: '#383a42',
        red: '#cd3131',
        green: '#0dbc79',
        yellow: '#e5e510',
        blue: '#2472c8',
        magenta: '#bc3fbc',
        cyan: '#11a8cd',
        white: '#e5e5e5',
        brightBlack: '#666666',
        brightRed: '#cd3131',
        brightGreen: '#14ce96',
        brightYellow: '#f5f543',
        brightBlue: '#3b8eea',
        brightMagenta: '#d670d6',
        brightCyan: '#29b8db',
        brightWhite: '#e5e5e5',
    },
    dark: {
        background: '#1e1e1e',
        foreground: '#cccccc',
        cursor: '#ffffff',
        selectionBackground: 'rgba(255, 255, 255, 0.15)',
        black: '#000000',
        red: '#cd3131',
        green: '#0dbc79',
        yellow: '#e5e510',
        blue: '#2472c8',
        magenta: '#bc3fbc',
        cyan: '#11a8cd',
        white: '#e5e5e5',
        brightBlack: '#666666',
        brightRed: '#cd3131',
        brightGreen: '#14ce96',
        brightYellow: '#f5f543',
        brightBlue: '#3b8eea',
        brightMagenta: '#d670d6',
        brightCyan: '#29b8db',
        brightWhite: '#e5e5e5',
    }
};

export default function XTermPreview({
    elementId,
    className = "",
}: XTermPreviewProps) {
    const terminalRef = useRef<HTMLDivElement>(null);
    const xtermRef = useRef<Terminal | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const { theme } = useTheme();
    const [isDark, setIsDark] = useState(true);

    useEffect(() => {
        const checkTheme = () => {
            if (theme === 'system') {
                setIsDark(window.matchMedia('(prefers-color-scheme: dark)').matches);
            } else {
                setIsDark(theme === 'dark');
            }
        };
        
        checkTheme();
        
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        const handler = () => checkTheme();
        
        mediaQuery.addEventListener('change', handler);
        return () => mediaQuery.removeEventListener('change', handler);
    }, [theme]);

    useEffect(() => {
        if (xtermRef.current) {
            xtermRef.current.options.theme = isDark ? TERMINAL_THEME.dark : TERMINAL_THEME.light;
        }
    }, [isDark]);

    useEffect(() => {
        let cancelled = false;
        let disposed = false;
        let disposeTimer: ReturnType<typeof setTimeout> | null = null;

        if (terminalRef.current === null) {
            console.log("Terminal host element is not available");
            return;
        }

        const term = new Terminal({
            fontFamily: "'Cascadia Code', 'Fira Code', Consolas, 'Courier New', monospace",
            fontSize: 14,
            lineHeight: 1.2,
            cursorBlink: true,
            scrollback: 1000,
            cursorInactiveStyle: 'none',
            theme: isDark ? TERMINAL_THEME.dark : TERMINAL_THEME.light,
            allowProposedApi: true
        });
        
        xtermRef.current = term;

        const disposeSafely = () => {
            if (disposed) {
                return;
            }
            disposed = true;
            term.dispose();
            xtermRef.current = null;
        };
        console.log("Initializing terminal preview for", elementId);
        async function startPreview() {
            setError(null);
            setIsLoading(true);
            try {
                if (!terminalRef.current) {
                    console.error("Terminal host element was not found");
                    throw new Error("Terminal host element was not found");
                }
                term.open(terminalRef.current);
                registerTerminalInstance(elementId, term);
                await registerComponent(elementId)

                attachKeyListener(elementId, {
                  invokeMethodAsync: async (methodName: string, ...args: unknown[]) => {
                    console.debug(`Key event forwarded from preview via ${methodName}`, args)
                    await handleKeyboardEvent(...(args as [string, string, string, boolean, boolean, boolean]))
                    return null
                  }
                })
                if (!cancelled) {
                    setIsLoading(false);
                }
            } catch (err) {
                if (!cancelled) {
                    const message =
                        err instanceof Error
                            ? err.message
                            : "Failed to initialize preview";
                    setError(message);
                    setIsLoading(false);
                }
                disposeSafely();
            }
        }

        startPreview();

        return () => {
            cancelled = true;
            if (disposeTimer !== null) {
                clearTimeout(disposeTimer);
            }
            disposeTimer = window.setTimeout(disposeSafely, 0);
        };
    }, [elementId]);

    if (error) {
        return (
            <div className="p-4 bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 rounded">
                Error: {error}
            </div>
        );
    }

    return (
        <div className={`relative rounded-xl overflow-hidden border border-slate-200 dark:border-slate-800 ${className}`}>
            {/* Window Title Bar */}
            <div className="bg-slate-100 dark:bg-[#2d2d2d] px-4 py-2 flex items-center gap-2 border-b border-slate-200 dark:border-[#1e1e1e]">
                <div className="flex gap-1.5">
                    <div className="w-3 h-3 rounded-full bg-[#ff5f56] border border-[#e0443e]" />
                    <div className="w-3 h-3 rounded-full bg-[#ffbd2e] border border-[#dea123]" />
                    <div className="w-3 h-3 rounded-full bg-[#27c93f] border border-[#1aab29]" />
                </div>
                <div className="flex-1 text-center text-xs text-slate-500 font-medium font-sans select-none">
                    RazorConsole
                </div>
                <div className="w-12" /> {/* Spacer for centering */}
            </div>

            {isLoading && (
                <div className="absolute inset-0 flex items-center justify-center bg-slate-900/50 text-white z-10">
                    Loading preview...
                </div>
            )}
            <div
                ref={terminalRef}
                id={elementId}
                className="w-full h-full"
                style={{ 
                    backgroundColor: isDark ? TERMINAL_THEME.dark.background : TERMINAL_THEME.light.background,
                    padding: '12px'
                }}
            />
        </div>
    );
}
