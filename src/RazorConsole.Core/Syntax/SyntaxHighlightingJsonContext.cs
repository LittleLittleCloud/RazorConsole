// Copyright (c) RazorConsole. All rights reserved.

using System.Text.Json.Serialization;

namespace RazorConsole.Core.Rendering.Syntax;

[JsonSerializable(typeof(SyntaxHighlightingService.SyntaxHighlightPayload))]
internal partial class SyntaxHighlightingJsonContext : JsonSerializerContext
{
}
