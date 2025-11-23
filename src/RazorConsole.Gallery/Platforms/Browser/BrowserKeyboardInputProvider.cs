#if BROWSER
using System.Runtime.Versioning;
using System.Threading.Channels;

namespace RazorConsole.Gallery.Platforms.Browser;

/// <summary>
/// Browser-compatible keyboard input provider that receives keys from JavaScript interop
/// instead of polling Console.ReadKey() which is not available in browser.
/// </summary>
[SupportedOSPlatform("browser")]
public sealed class BrowserKeyboardInputProvider
{
    private readonly Channel<ConsoleKeyInfo> _keyQueue;
    
    public BrowserKeyboardInputProvider()
    {
        _keyQueue = Channel.CreateUnbounded<ConsoleKeyInfo>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }
    
    /// <summary>
    /// Enqueues a key received from JavaScript.
    /// Called by BrowserKeyboardInterop when JavaScript sends a key event.
    /// </summary>
    public void EnqueueKey(ConsoleKeyInfo keyInfo)
    {
        _keyQueue.Writer.TryWrite(keyInfo);
    }
    
    /// <summary>
    /// Waits for and returns the next key from the queue.
    /// This replaces Console.ReadKey() for browser environments.
    /// </summary>
    public async ValueTask<ConsoleKeyInfo> ReadKeyAsync(CancellationToken cancellationToken = default)
    {
        return await _keyQueue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Checks if a key is available without blocking.
    /// This replaces Console.KeyAvailable for browser environments.
    /// </summary>
    public bool KeyAvailable => _keyQueue.Reader.Count > 0;
}
#endif
