using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Syntax;

public sealed class SyntaxRenderable : Renderable
{
    private readonly SyntaxHighlightRenderModel _model;
    private IRenderable? _cachedRenderable;

    public SyntaxRenderable(SyntaxHighlightRenderModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    protected override Measurement Measure(RenderOptions options, int maxWidth)
        => BuildRenderable().Measure(options, maxWidth);

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => BuildRenderable().Render(options, maxWidth);

    private IRenderable BuildRenderable()
    {
        if (_cachedRenderable is not null)
        {
            return _cachedRenderable;
        }

        if (_model.Lines.Count == 0)
        {
            _cachedRenderable = new Markup(_model.PlaceholderMarkup);
            return _cachedRenderable;
        }

        if (!_model.ShowLineNumbers)
        {
            var markup = string.Join(Environment.NewLine, _model.Lines);
            _cachedRenderable = new Markup(markup);
            return _cachedRenderable;
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
            if (string.IsNullOrEmpty(lineMarkup))
            {
                lineMarkup = " ";
            }

            grid.AddRow(new Markup(numberMarkup), new Markup(lineMarkup));
        }

        _cachedRenderable = grid;
        return grid;
    }
}
