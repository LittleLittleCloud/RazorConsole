using System;
using System.Collections.Generic;
using System.Text;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlInlineTextElementTranslator : IVdomElementTranslator
{
    public int Priority => 20;

    private static readonly IReadOnlyDictionary<string, MarkupEnvelope> StyleEnvelopes =
        new Dictionary<string, MarkupEnvelope>(StringComparer.OrdinalIgnoreCase)
        {
            ["strong"] = new MarkupEnvelope("[bold]", "[/]"),
            ["b"] = new MarkupEnvelope("[bold]", "[/]"),
            ["em"] = new MarkupEnvelope("[italic]", "[/]"),
            ["i"] = new MarkupEnvelope("[italic]", "[/]"),
            ["mark"] = new MarkupEnvelope("[black on yellow]", "[/]"),
            ["del"] = new MarkupEnvelope("[strikethrough]", "[/]"),
            ["ins"] = new MarkupEnvelope("[underline]", "[/]"),
            ["code"] = new MarkupEnvelope("[grey53 on #1f1f1f]", "[/]", AllowNestedFormatting: false),
            ["abbr"] = new MarkupEnvelope("[underline]", "[/]"),
            ["cite"] = new MarkupEnvelope("[italic]", "[/]"),
            ["small"] = new MarkupEnvelope("[dim]", "[/]"),
            ["sub"] = new MarkupEnvelope("[dim]", "[/]"),
            ["sup"] = new MarkupEnvelope("[dim]", "[/]"),
        };

    private static readonly HashSet<string> QuoteTags = new(StringComparer.OrdinalIgnoreCase) { "q" };

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!IsSupportedInlineElement(node.TagName))
        {
            return false;
        }

        if (!TryBuildMarkup(node, out var markup))
        {
            return false;
        }

        renderable = new Markup(markup);
        return true;
    }

    private static bool IsSupportedInlineElement(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return false;
        }

        return StyleEnvelopes.ContainsKey(tagName) || QuoteTags.Contains(tagName);
    }

    internal static bool TryBuildMarkup(VNode node, out string markup)
    {
        var builder = new StringBuilder();

        if (!TryAppendElement(node, builder))
        {
            markup = string.Empty;
            return false;
        }

        markup = builder.ToString();
        return true;
    }

    private static bool TryAppendElement(VNode node, StringBuilder builder)
    {
        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        var tagName = node.TagName ?? string.Empty;

        if (StyleEnvelopes.TryGetValue(tagName, out var envelope))
        {
            builder.Append(envelope.Prefix);
            if (!TryAppendChildren(node.Children, builder, envelope.AllowNestedFormatting))
            {
                return false;
            }

            builder.Append(envelope.Suffix);
            return true;
        }

        if (QuoteTags.Contains(tagName))
        {
            builder.Append("“");
            if (!TryAppendChildren(node.Children, builder, allowNestedFormatting: true))
            {
                return false;
            }

            builder.Append("”");
            return true;
        }

        return false;
    }

    private static bool TryAppendChildren(IReadOnlyList<VNode> children, StringBuilder builder, bool allowNestedFormatting)
    {
        foreach (var child in children)
        {
            switch (child.Kind)
            {
                case VNodeKind.Text when !string.IsNullOrEmpty(child.Text):
                    builder.Append(Markup.Escape(child.Text));
                    break;
                case VNodeKind.Element when allowNestedFormatting:
                    if (!TryAppendElement(child, builder))
                    {
                        return false;
                    }

                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    private static void AppendNormalizedText(StringBuilder builder, string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        var normalized = new StringBuilder(text.Length);
        var pendingWhitespace = false;

        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                pendingWhitespace = true;
                continue;
            }

            if (pendingWhitespace)
            {
                AppendSpaceIfNeeded(builder, normalized);
                pendingWhitespace = false;
            }

            normalized.Append(ch);
        }

        if (pendingWhitespace)
        {
            AppendSpaceIfNeeded(builder, normalized);
        }

        if (normalized.Length == 0)
        {
            return;
        }

        builder.Append(Markup.Escape(normalized.ToString()));
    }

    private static void AppendSpaceIfNeeded(StringBuilder builder, StringBuilder normalized)
    {
        if (normalized.Length > 0)
        {
            if (normalized[^1] != ' ')
            {
                normalized.Append(' ');
            }

            return;
        }

        if (builder.Length == 0)
        {
            return;
        }

        if (!char.IsWhiteSpace(builder[^1]))
        {
            normalized.Append(' ');
        }
    }

    private readonly record struct MarkupEnvelope(string Prefix, string Suffix, bool AllowNestedFormatting = true);
}
