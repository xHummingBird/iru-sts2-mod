using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Iru.IruCode.Extensions;

public static class IruStaticHoverTips
{
    public static readonly IHoverTip Hyper =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "IRU_HYPER.title"),
            new LocString(
                "static_hover_tips",
                "IRU_HYPER.description"));

    public static readonly IHoverTip Heat =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "IRU_HEAT.title"),
            new LocString(
                "static_hover_tips",
                "IRU_HEAT.description"));
}

public static class IruHoverTipText
{
    public const string HyperToken =
        "%HYPER%";

    public const string MaxHyperToken =
        "%MAXHYPER%";

    private static readonly StringName TextProperty =
        "text";

    public sealed class TextTarget
    {
        public required GodotObject TextNode
        {
            get;
            init;
        }

        public required string Template
        {
            get;
            init;
        }
    }

    public static List<TextTarget> CollectTargets(
        Node root)
    {
        List<TextTarget> targets = [];

        Collect(
            root,
            targets);

        return targets;
    }

    public static void RenderHyper(
        List<TextTarget> targets,
        int hyper,
        int maxHyper)
    {
        foreach (var target in targets)
        {
            if (!GodotObject.IsInstanceValid(
                    target.TextNode))
            {
                continue;
            }

            string text =
                target.Template
                    .Replace(
                        HyperToken,
                        hyper.ToString())
                    .Replace(
                        MaxHyperToken,
                        maxHyper.ToString());

            target.TextNode.Set(
                TextProperty,
                text);
        }
    }

    private static void Collect(
        Node root,
        List<TextTarget> targets)
    {
        Variant value =
            root.Get(
                TextProperty);

        if (value.VariantType ==
            Variant.Type.String)
        {
            string text =
                value.AsString();

            if (text.Contains(HyperToken) ||
                text.Contains(MaxHyperToken))
            {
                targets.Add(
                    new TextTarget
                    {
                        TextNode = root,
                        Template = text
                    });
            }
        }

        foreach (Node child in root.GetChildren())
        {
            Collect(
                child,
                targets);
        }
    }
}