import { useEffect, useRef, useState } from "react";
import {
    registerComponent,
    attachKeyListener,
    handleKeyboardEvent,
    registerTerminalInstance,
} from "razor-console";
import { Terminal } from "xterm";
interface XTermPreviewProps {
    elementId: string;
    className?: string;
}

export default function XTermPreview({
    elementId,
    className = "",
}: XTermPreviewProps) {
    const terminalRef = useRef<HTMLDivElement>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    useEffect(() => {
        let cancelled = false;
        let disposed = false;
        let disposeTimer: ReturnType<typeof setTimeout> | null = null;

        if (terminalRef.current === null) {
            console.log("Terminal host element is not available");
            return;
        }

        const term = new Terminal({
            fontFamily: "monospace",
            fontSize: 14,
            lineHeight: 1.2,
            cursorBlink: false,
            scrollback: 1000,
            cursorInactiveStyle: 'none'
        });

        const disposeSafely = () => {
            if (disposed) {
                return;
            }
            disposed = true;
            term.dispose();
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
        <div className={`relative ${className}`}>
            {isLoading && (
                <div className="absolute inset-0 flex items-center justify-center bg-slate-900/50 text-white">
                    Loading preview...
                </div>
            )}
            <div
                ref={terminalRef}
                id={elementId}
                className="w-full h-full bg-[#1e1e1e] rounded"
            />
        </div>
    );
}
