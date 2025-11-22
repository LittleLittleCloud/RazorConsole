(function () {
    const terminals = new Map();

    function getTerminal(elementId) {
        const term = terminals.get(elementId);
        if (!term) {
            throw new Error("Terminal with id '" + elementId + "' has not been initialized.");
        }

        return term;
    }

    window.razorConsoleTerminal = {
        init: function (elementId, options) {
            if (typeof window.Terminal === "undefined") {
                throw new Error("xterm.js is not loaded.");
            }

            const host = document.getElementById(elementId);
            if (!host) {
                throw new Error("Element with id '" + elementId + "' was not found.");
            }

            const config = Object.assign({ 
                convertEol: true, 
                disableStdin: false,
                allowTransparency: false,
                theme: {
                    background: '#1e1e1e',
                    foreground: '#d4d4d4'
                },
                fontFamily: 'Consolas, "Courier New", monospace',
                fontSize: 14,
                lineHeight: 1.2,
                cursorBlink: false,
                scrollback: 1000
            }, options || {});
            const terminal = new window.Terminal(config);
            terminal.open(host);

            terminals.set(elementId, terminal);

            return terminal;
        },
        attachKeyListener: function (elementId, dotnetHelper) {
            const terminal = getTerminal(elementId);

            terminal.onKey(function (event) {
                dotnetHelper.invokeMethodAsync("HandleKeyboardEvent", event.key, event.domEvent.key, event.domEvent.ctrlKey, event.domEvent.altKey, event.domEvent.shiftKey);
            });
        },
        write: function (elementId, text) {
            const terminal = getTerminal(elementId);

            if (typeof text !== "string" || text.length === 0) {
                return;
            }

            terminal.write(text);
        },
        dispose: function (elementId) {
            const terminal = terminals.get(elementId);

            if (!terminal) {
                return;
            }

            terminal.dispose();
            terminals.delete(elementId);
        }
    };
})();
