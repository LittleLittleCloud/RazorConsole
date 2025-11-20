// Copyright (c) RazorConsole. All rights reserved.

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal static class AnimatedRenderableRegistry
{
    private static readonly AsyncLocal<Stack<List<IAnimatedConsoleRenderable>?>> ScopeStack = new();

    public static IDisposable PushScope(List<IAnimatedConsoleRenderable> animatedRenderables)
    {
        var stack = ScopeStack.Value ??= new Stack<List<IAnimatedConsoleRenderable>?>();
        stack.Push(animatedRenderables);
        return new Scope(stack);
    }

    public static void Register(IAnimatedConsoleRenderable renderable)
    {
        var stack = ScopeStack.Value;
        if (stack is null || stack.Count == 0)
        {
            return;
        }

        var current = stack.Peek();
        current?.Add(renderable);
    }

    private sealed class Scope : IDisposable
    {
        private readonly Stack<List<IAnimatedConsoleRenderable>?> _stack;
        private bool _disposed;

        public Scope(Stack<List<IAnimatedConsoleRenderable>?> stack)
        {
            _stack = stack;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_stack.Count > 0)
            {
                _stack.Pop();
            }

            _disposed = true;
        }
    }
}
