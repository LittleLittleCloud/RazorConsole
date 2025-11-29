// Copyright (c) RazorConsole. All rights reserved.

using System.Text;

namespace RazorConsole.Core.Utilities;

public static class RuntimeEncoding
{

    public static void EnsureUtf8()
    {
        // Do not touch encoding when output/input is redirected (CI, logging, pipes, etc.)
        if (Console.IsOutputRedirected || Console.IsInputRedirected)
        {
            return;
        }

        // Already using UTF-8 — nothing to do
        if (Console.OutputEncoding.Equals(Encoding.UTF8) &&
            Console.InputEncoding.Equals(Encoding.UTF8))
        {
            return;
        }

        var originalInput = Console.InputEncoding;
        var originalOutput = Console.OutputEncoding;

        bool success;

        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Some environments accept the assignment but silently ignore it
            success = Console.OutputEncoding.Equals(Encoding.UTF8) & Console.InputEncoding.Equals(Encoding.UTF8);
        }
        catch
        {
            // PlatformNotSupportedException, IOException, UnauthorizedAccessException, etc.
            success = false;
        }

        // If UTF-8 could not be enabled — restore the original encodings
        if (!success)
        {
            TryRestore(originalInput, originalOutput);
        }
    }


    private static void TryRestore(Encoding input, Encoding output)
    {
        try
        {
            if (!Equals(Console.InputEncoding, input))
            {
                Console.InputEncoding = input;
            }
        }
        catch { /* intentionally ignored */ }

        try
        {
            if (!Equals(Console.OutputEncoding, output))
            {
                Console.OutputEncoding = output;
            }
        }
        catch { /* intentionally ignored */ }
    }
}
