using BaseLib.Abstracts;
using BaseLib.Utils;
using Iru.IruCode.Character;

namespace Iru.IruCode.Potions;

[Pool(typeof(IruPotionPool))]
public abstract class IruPotion : CustomPotionModel;