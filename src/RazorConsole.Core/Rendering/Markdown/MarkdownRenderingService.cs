using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace RazorConsole.Core.Rendering.Markdown;

public sealed class MarkdownRenderingService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownRenderingService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public MarkdownRenderModel Render(MarkdownRenderRequest request)
    {
        var document = Markdig.Markdown.Parse(request.Content, _pipeline);
        var elements = new List<MarkdownElement>();

        foreach (var block in document)
        {
            var element = ConvertBlock(block, request);
            if (element != null)
            {
                elements.Add(element);
            }
        }

        return new MarkdownRenderModel(elements);
    }

    private MarkdownElement? ConvertBlock(Block block, MarkdownRenderRequest request)
    {
        switch (block)
        {
            case HeadingBlock heading:
                return new HeadingElement(heading.Level, ExtractText(heading.Inline));

            case ParagraphBlock paragraph:
                return new ParagraphElement(ExtractText(paragraph.Inline));

            case CodeBlock codeBlock:
                var code = ExtractCodeBlockContent(codeBlock);
                var language = codeBlock is FencedCodeBlock fenced ? fenced.Info : null;
                return new CodeBlockElement(code, language, request.ShowCodeLineNumbers);

            case ListBlock list:
                var items = new List<string>();
                foreach (var item in list)
                {
                    if (item is ListItemBlock listItem)
                    {
                        var itemText = ExtractListItemText(listItem);
                        items.Add(itemText);
                    }
                }
                return new ListElement(list.IsOrdered, items);

            case QuoteBlock quote:
                var quoteText = new StringBuilder();
                foreach (var quoteChild in quote)
                {
                    if (quoteChild is ParagraphBlock quotePara)
                    {
                        quoteText.AppendLine(ExtractText(quotePara.Inline));
                    }
                }
                return new QuoteElement(quoteText.ToString().TrimEnd());

            case ThematicBreakBlock:
                return new ThematicBreakElement();

            default:
                return null;
        }
    }

    private string ExtractText(ContainerInline? inline)
    {
        if (inline == null)
            return string.Empty;

        var result = new StringBuilder();
        foreach (var item in inline)
        {
            AppendInlineText(item, result);
        }
        return result.ToString();
    }

    private void AppendInlineText(Inline inline, StringBuilder result)
    {
        switch (inline)
        {
            case LiteralInline literal:
                result.Append(literal.Content.ToString());
                break;

            case CodeInline code:
                result.Append('`');
                result.Append(code.Content);
                result.Append('`');
                break;

            case EmphasisInline emphasis:
                var delimiter = emphasis.DelimiterCount == 2 ? "**" : "*";
                result.Append(delimiter);
                foreach (var child in emphasis)
                {
                    AppendInlineText(child, result);
                }
                result.Append(delimiter);
                break;

            case LinkInline link:
                result.Append('[');
                foreach (var child in link)
                {
                    AppendInlineText(child, result);
                }
                result.Append("](");
                result.Append(link.Url);
                result.Append(')');
                break;

            case LineBreakInline:
                result.AppendLine();
                break;

            case ContainerInline container:
                foreach (var child in container)
                {
                    AppendInlineText(child, result);
                }
                break;
        }
    }

    private string ExtractCodeBlockContent(CodeBlock codeBlock)
    {
        var sb = new StringBuilder();
        var lines = codeBlock.Lines;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines.Lines[i];
            sb.Append(line.Slice.ToString());
            if (i < lines.Count - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private string ExtractListItemText(ListItemBlock listItem)
    {
        var sb = new StringBuilder();
        foreach (var block in listItem)
        {
            if (block is ParagraphBlock para)
            {
                sb.Append(ExtractText(para.Inline));
            }
        }
        return sb.ToString();
    }

    public static string EncodePayload(MarkdownRenderModel model)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            WriteIndented = false
        };
        options.Converters.Add(new MarkdownElementConverter());

        var json = JsonSerializer.Serialize(model, options);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static MarkdownRenderModel DecodePayload(string payload)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true
        };
        options.Converters.Add(new MarkdownElementConverter());

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var model = JsonSerializer.Deserialize<MarkdownRenderModel>(json, options);
        return model ?? throw new InvalidOperationException("Failed to deserialize markdown model.");
    }
}

// Custom JSON converter for polymorphic MarkdownElement
internal class MarkdownElementConverter : JsonConverter<MarkdownElement>
{
    public override MarkdownElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("$type", out var typeProperty))
        {
            throw new JsonException("Missing $type discriminator");
        }

        var typeName = typeProperty.GetString();
        return typeName switch
        {
            "heading" => new HeadingElement(
                root.GetProperty("level").GetInt32(),
                root.GetProperty("content").GetString() ?? string.Empty),
            "paragraph" => new ParagraphElement(
                root.GetProperty("content").GetString() ?? string.Empty),
            "codeBlock" => new CodeBlockElement(
                root.GetProperty("code").GetString() ?? string.Empty,
                root.TryGetProperty("language", out var lang) && lang.ValueKind != JsonValueKind.Null ? lang.GetString() : null,
                root.GetProperty("showLineNumbers").GetBoolean()),
            "list" => JsonSerializer.Deserialize<ListElement>(root.GetRawText(), options),
            "quote" => new QuoteElement(
                root.GetProperty("content").GetString() ?? string.Empty),
            "thematicBreak" => new ThematicBreakElement(),
            _ => throw new JsonException($"Unknown type: {typeName}")
        };
    }

    public override void Write(Utf8JsonWriter writer, MarkdownElement value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case HeadingElement heading:
                writer.WriteString("$type", "heading");
                writer.WriteNumber("level", heading.Level);
                writer.WriteString("content", heading.Content);
                break;
            case ParagraphElement paragraph:
                writer.WriteString("$type", "paragraph");
                writer.WriteString("content", paragraph.Content);
                break;
            case CodeBlockElement codeBlock:
                writer.WriteString("$type", "codeBlock");
                writer.WriteString("code", codeBlock.Code);
                if (codeBlock.Language != null)
                    writer.WriteString("language", codeBlock.Language);
                writer.WriteBoolean("showLineNumbers", codeBlock.ShowLineNumbers);
                break;
            case ListElement list:
                writer.WriteString("$type", "list");
                writer.WriteBoolean("isOrdered", list.IsOrdered);
                writer.WritePropertyName("items");
                JsonSerializer.Serialize(writer, list.Items, options);
                break;
            case QuoteElement quote:
                writer.WriteString("$type", "quote");
                writer.WriteString("content", quote.Content);
                break;
            case ThematicBreakElement:
                writer.WriteString("$type", "thematicBreak");
                break;
        }

        writer.WriteEndObject();
    }
}
