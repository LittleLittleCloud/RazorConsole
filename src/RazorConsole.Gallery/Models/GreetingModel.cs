using System;
using System.Collections.Generic;

namespace RazorConsole.Gallery.Models;

public class GreetingModel
{
    public string? Name { get; set; }

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public List<string> Tips { get; } = new();
}
