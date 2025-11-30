// Copyright (c) RazorConsole. All rights reserved.

namespace RazorConsole.Core.Focus;

internal interface IFocusEventDispatcher
{
    Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken);
}
