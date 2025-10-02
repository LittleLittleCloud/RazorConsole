using System;
using System.Collections.Generic;
using System.Composition;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class HtmlButtonElementTranslator : IVdomElementTranslator
    {
        private static readonly IReadOnlyDictionary<string, string> VariantMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["btn-primary"] = "primary",
            ["btn-success"] = "success",
            ["btn-warning"] = "warning",
            ["btn-danger"] = "danger",
            ["btn-error"] = "danger",
            ["btn-info"] = "neutral",
            ["btn-secondary"] = "neutral",
            ["btn-default"] = "neutral",
        };

        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element || !string.Equals(node.TagName, "button", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var descriptor = CreateDescriptor(node);
            renderable = ButtonRenderableBuilder.Build(descriptor, children);
            return true;
        }

        private static ButtonRenderableDescriptor CreateDescriptor(VNode node)
        {
            var label = ResolveButtonLabel(node);
            var icon = GetAttribute(node, "data-button-icon");
            var hotKey = ResolveButtonHotKey(node);
            var variant = ResolveButtonVariant(node);
            var style = GetAttribute(node, "data-button-style");
            var activeStyle = GetAttribute(node, "data-button-active-style");
            var disabledStyle = GetAttribute(node, "data-button-disabled-style");
            var borderStyle = GetAttribute(node, "data-button-border-style");
            var activeBorderStyle = GetAttribute(node, "data-button-active-border-style");
            var disabledBorderStyle = GetAttribute(node, "data-button-disabled-border-style");
            var isActive = ResolveButtonIsActive(node);
            var isDisabled = ResolveButtonDisabled(node);
            var expand = ResolveButtonExpand(node);
            var width = ResolveButtonWidth(node);
            var padding = ResolveButtonPadding(node);
            var isDefault = ResolveButtonIsDefault(node);

            return new ButtonRenderableDescriptor(
                label,
                icon,
                hotKey,
                variant,
                style,
                activeStyle,
                disabledStyle,
                borderStyle,
                activeBorderStyle,
                disabledBorderStyle,
                isActive,
                isDisabled,
                expand,
                width,
                padding,
                isDefault);
        }

        private static string? ResolveButtonLabel(VNode node)
        {
            var dataLabel = GetAttribute(node, "data-button-label");
            if (!string.IsNullOrWhiteSpace(dataLabel))
            {
                return dataLabel;
            }

            if (node.Attributes.TryGetValue("aria-label", out var ariaLabel) && !string.IsNullOrWhiteSpace(ariaLabel))
            {
                return ariaLabel;
            }

            return CollectInnerText(node);
        }

        private static string? ResolveButtonHotKey(VNode node)
        {
            var explicitHotKey = GetAttribute(node, "data-button-hotkey");
            if (!string.IsNullOrWhiteSpace(explicitHotKey))
            {
                return explicitHotKey;
            }

            var accessKey = GetAttribute(node, "accesskey");
            return string.IsNullOrWhiteSpace(accessKey) ? null : accessKey.Trim();
        }

        private static string? ResolveButtonVariant(VNode node)
        {
            var explicitVariant = GetAttribute(node, "data-button-variant");
            if (!string.IsNullOrWhiteSpace(explicitVariant))
            {
                return explicitVariant;
            }

            if (node.Attributes.TryGetValue("class", out var classValue))
            {
                foreach (var token in EnumerateClassNames(classValue))
                {
                    if (VariantMappings.TryGetValue(token, out var variant))
                    {
                        return variant;
                    }
                }
            }

            return null;
        }

        private static bool ResolveButtonIsActive(VNode node)
        {
            if (TryGetBoolAttribute(node, "data-button-active", out var value))
            {
                return value;
            }

            return HasClass(node, "active");
        }

        private static bool ResolveButtonDisabled(VNode node)
        {
            if (TryGetBoolAttribute(node, "data-button-disabled", out var value))
            {
                return value;
            }

            if (node.Attributes.ContainsKey("disabled"))
            {
                return true;
            }

            if (node.Attributes.TryGetValue("aria-disabled", out var ariaDisabled) && bool.TryParse(ariaDisabled, out var parsedAria))
            {
                return parsedAria;
            }

            return HasClass(node, "disabled");
        }

        private static bool ResolveButtonExpand(VNode node)
        {
            if (TryGetBoolAttribute(node, "data-button-expand", out var value))
            {
                return value;
            }

            return HasClass(node, "btn-block") || HasClass(node, "w-100");
        }

        private static int? ResolveButtonWidth(VNode node)
        {
            var widthRaw = GetAttribute(node, "data-button-width");
            return TryParsePositiveInt(widthRaw, out var width) ? width : null;
        }

        private static Padding? ResolveButtonPadding(VNode node)
        {
            var paddingRaw = GetAttribute(node, "data-button-padding");
            if (!string.IsNullOrWhiteSpace(paddingRaw) && TryParsePadding(paddingRaw, out var padding))
            {
                return padding;
            }

            return null;
        }

        private static bool ResolveButtonIsDefault(VNode node)
        {
            if (TryGetBoolAttribute(node, "data-button-default", out var value))
            {
                return value;
            }

            if (node.Attributes.TryGetValue("type", out var typeValue) && string.Equals(typeValue, "submit", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return HasClass(node, "btn-primary") || HasClass(node, "btn-default");
        }
    }
}
