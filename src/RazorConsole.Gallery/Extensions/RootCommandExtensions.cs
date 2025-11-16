using System.CommandLine;
using System.Globalization;
using System.Runtime.InteropServices;

namespace RazorConsole.Gallery.Extensions;

public static class RootCommandExtensions
{

    public static RootCommand AddDebugCliCommand(this RootCommand root)
    {
        var debugCommand = new Command("debug-cli");
        debugCommand.SetAction((parseResult) =>
        {
            Console.WriteLine();
            // N/A = not avaliable
            var windowSizeX = "N/A";
            var windowSizeY = "N/A";
            var bufferSizeX = "N/A";
            var bufferSizeY = "N/A";
            try
            {
                windowSizeX = Console.WindowWidth.ToString();
                windowSizeY = Console.WindowHeight.ToString();
                bufferSizeX = Console.BufferWidth.ToString();
                bufferSizeY = Console.BufferHeight.ToString();
            }
            catch (Exception) { }

            Console.WriteLine($$"""
                {
                    "os": {
                        "architecture": "{{RuntimeInformation.ProcessArchitecture}}",
                        "description": "{{RuntimeInformation.OSDescription}}",
                        "framework": "{{RuntimeInformation.FrameworkDescription}}",
                        "rid": "{{RuntimeInformation.RuntimeIdentifier}}"
                    },
                    "culture": {
                        "ui": "{{CultureInfo.CurrentUICulture}}",
                        "current": "{{CultureInfo.CurrentCulture}}"
                    },
                    "console": {
                        "encoding": {
                            "input": "{{Console.InputEncoding.EncodingName}}",
                            "output": "{{Console.OutputEncoding.EncodingName}}"
                        },
                        "redirection": {
                            "inputRedirected": "{{Console.IsInputRedirected}}",
                            "outputRedirected": "{{Console.IsOutputRedirected}}",
                            "errorRedirected": "{{Console.IsErrorRedirected}}"
                        },
                        "size": {
                            "window": {
                                "x": "{{windowSizeX}}",
                                "y": "{{windowSizeY}}"
                            },
                            "buffer": {
                                "x": "{{bufferSizeX}}",
                                "y": "{{bufferSizeY}}",
                            }
                        },
                        "color": {
                            "foreground": "{{Console.ForegroundColor}}",
                            "background": "{{Console.BackgroundColor}}"
                        }
                    }
                }
                """);
        });

        root.Subcommands.Add(debugCommand);
        return root;
    }

}
