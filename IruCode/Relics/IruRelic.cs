using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Iru.IruCode.Character;
using Iru.IruCode.Extensions;
using Godot;

namespace Iru.IruCode.Relics;

[Pool(typeof(IruRelicPool))]
public abstract class IruRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}