using System;
using System.Threading;
using System.Threading.Tasks;

namespace RazorConsole.Core.Focus;

internal interface IFocusEventDispatcher
{
    Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken);
}
