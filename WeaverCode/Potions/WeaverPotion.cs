using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Weaver.WeaverCode.Character;
using Weaver.WeaverCode.Extensions;

namespace Weaver.WeaverCode.Potions;

[Pool(typeof(WeaverPotionPool))]
public abstract class WeaverPotion : CustomPotionModel
{
	public override string? CustomPackedImagePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
	public override string? CustomPackedOutlinePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}