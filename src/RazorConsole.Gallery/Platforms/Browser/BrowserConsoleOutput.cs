using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text;

namespace RazorConsole.Gallery.Platforms.Browser;

/// <summary>
/// Captures console output and forwards it to JavaScript for rendering in xterm.js.
/// </summary>
[SupportedOSPlatform("browser")]
public partial class BrowserConsoleOutput
{
    private static readonly StringBuilder _outputBuffer = new();
    private static readonly object _bufferLock = new();

    /// <summary>
    /// Captures and forwards console output to the browser.
    /// </summary>
    [JSExport]
    public static void CaptureOutput(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        lock (_bufferLock)
        {
            _outputBuffer.Append(data);
        }

        // Forward to JavaScript
        SendOutputToJS(data);
    }

    /// <summary>
    /// Gets all captured output since last call.
    /// </summary>
    [JSExport]
    public static string GetOutput()
    {
        lock (_bufferLock)
        {
            var output = _outputBuffer.ToString();
            _outputBuffer.Clear();
            return output;
        }
    }

    [JSImport("onConsoleOutput", "main")]
    private static partial void SendOutputToJS(string data);
}
