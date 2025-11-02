using System;
using Spectre.Console;

AnsiConsole.Write(
    new FigletText("RazorConsole")
        .Color(Color.Blue));

AnsiConsole.MarkupLine("[bold]Console Information Tool[/]");
AnsiConsole.WriteLine();

var table = new Table();
table.Border(TableBorder.Rounded);
table.AddColumn(new TableColumn("[bold yellow]Property[/]").Width(30));
table.AddColumn(new TableColumn("[bold yellow]Value[/]"));

// System.Console properties
table.AddRow("[cyan]== System.Console Properties ==[/]", "");
table.AddRow("IsInputRedirected", Console.IsInputRedirected.ToString());
table.AddRow("IsOutputRedirected", Console.IsOutputRedirected.ToString());
table.AddRow("IsErrorRedirected", Console.IsErrorRedirected.ToString());

try
{
    table.AddRow("BufferHeight", Console.BufferHeight.ToString());
    table.AddRow("BufferWidth", Console.BufferWidth.ToString());
    table.AddRow("WindowHeight", Console.WindowHeight.ToString());
    table.AddRow("WindowWidth", Console.WindowWidth.ToString());
}
catch (Exception ex)
{
    table.AddRow("Window/Buffer Size", $"[red]Error: {ex.Message}[/]");
}

try
{
    table.AddRow("CursorLeft", Console.CursorLeft.ToString());
    table.AddRow("CursorTop", Console.CursorTop.ToString());
    // CursorVisible is only supported on Windows
    if (OperatingSystem.IsWindows())
    {
        table.AddRow("CursorVisible", Console.CursorVisible.ToString());
    }
}
catch (Exception ex)
{
    table.AddRow("Cursor Information", $"[red]Error: {ex.Message}[/]");
}

table.AddRow("OutputEncoding", Console.OutputEncoding.EncodingName);
table.AddRow("InputEncoding", Console.InputEncoding.EncodingName);

// Spectre.Console capabilities
table.AddRow("[cyan]== Spectre.Console Capabilities ==[/]", "");
table.AddRow("ColorSystem", AnsiConsole.Profile.Capabilities.ColorSystem.ToString());
table.AddRow("Ansi", AnsiConsole.Profile.Capabilities.Ansi.ToString());
table.AddRow("Links", AnsiConsole.Profile.Capabilities.Links.ToString());
table.AddRow("Legacy", AnsiConsole.Profile.Capabilities.Legacy.ToString());
table.AddRow("Interactive", AnsiConsole.Profile.Capabilities.Interactive.ToString());
table.AddRow("Unicode", AnsiConsole.Profile.Capabilities.Unicode.ToString());

// Environment
table.AddRow("[cyan]== Environment Information ==[/]", "");
table.AddRow("Operating System", Environment.OSVersion.ToString());
table.AddRow("Platform", Environment.OSVersion.Platform.ToString());
table.AddRow(".NET Version", Environment.Version.ToString());
table.AddRow("TERM", Environment.GetEnvironmentVariable("TERM") ?? "[dim]Not set[/]");
table.AddRow("TERM_PROGRAM", Environment.GetEnvironmentVariable("TERM_PROGRAM") ?? "[dim]Not set[/]");
table.AddRow("COLORTERM", Environment.GetEnvironmentVariable("COLORTERM") ?? "[dim]Not set[/]");
table.AddRow("TERMINAL_EMULATOR", Environment.GetEnvironmentVariable("TERMINAL_EMULATOR") ?? "[dim]Not set[/]");

AnsiConsole.Write(table);
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[dim]This information can help diagnose console-related issues.[/]");
