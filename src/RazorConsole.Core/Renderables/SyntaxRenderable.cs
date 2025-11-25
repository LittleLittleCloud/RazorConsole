// Copyright (c) RazorConsole. All rights reserved.

using System.Globalization;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Syntax;

public sealed class SyntaxRenderable : Renderable
{
    private readonly SyntaxHighlightRenderModel _model;
    private readonly Lazy<IRenderable> _renderable;

    public SyntaxRenderable(SyntaxHighlightRenderModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _renderable = new Lazy<IRenderable>(CreateRenderable);
    }

    protected override Measurement Measure(RenderOptions options, int maxWidth)
        => _renderable.Value.Measure(options, maxWidth);

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => _renderable.Value.Render(options, maxWidth);

    private IRenderable CreateRenderable()
    {
        if (_model.Lines.Count == 0)
        {
            return new Markup(_model.PlaceholderMarkup);
        }

        if (!_model.ShowLineNumbers)
        {
            var markup = string.Join(Environment.NewLine, _model.Lines);
            return new Markup(markup);
        }

        var gutterMarkup = _model.LineNumberStyleMarkup;
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(1));
        grid.AddColumn(new GridColumn().PadLeft(0));

        var width = _model.Lines.Count.ToString(CultureInfo.InvariantCulture).Length;
        for (var index = 0; index < _model.Lines.Count; index++)
        {
            var lineNumber = (index + 1).ToString(CultureInfo.InvariantCulture).PadLeft(width);
            var numberMarkup = string.IsNullOrEmpty(gutterMarkup)
                ? Markup.Escape(lineNumber)
                : $"[{gutterMarkup}]{Markup.Escape(lineNumber)}[/]";

            var lineMarkup = _model.Lines[index];

            grid.AddRow(new Markup(numberMarkup), new Markup(lineMarkup));
        }

        return grid;
    }
}
