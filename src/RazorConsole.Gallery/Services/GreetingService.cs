using System;
using RazorConsole.Gallery.Models;

namespace RazorConsole.Gallery.Services;

public sealed class GreetingService
{
    private readonly GreetingModel _model;

    public GreetingService()
    {
        _model = new GreetingModel
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Timestamp = DateTime.Now
        };

        _model.Tips.AddRange(new[]
        {
            "Experiment with different Spectre widgets.",
            "Update the Razor component with your own data.",
            "Try piping the output through other CLI tools."
        });
    }

    public GreetingModel GetSnapshot()
    {
        var snapshot = new GreetingModel
        {
            Name = _model.Name,
            Date = _model.Date,
            Timestamp = _model.Timestamp
        };

        snapshot.Tips.AddRange(_model.Tips);
        return snapshot;
    }

    public void UpdateName(string? name)
    {
        var trimmed = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        if (string.Equals(_model.Name, trimmed, StringComparison.Ordinal))
        {
            return;
        }

        _model.Name = trimmed;
        _model.Timestamp = DateTime.Now;
    }
}
