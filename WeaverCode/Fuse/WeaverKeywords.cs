using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Weaver.WeaverCode.Fuse;

public static class WeaverKeywords
{
    /// <summary>
    /// When played, this card exhausts and its effects (including Fuse) and cost are added to a card in your hand.
    /// See <see cref="FuseHooks"/>.
    /// </summary>
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.After)] public static CardKeyword Fuse;
}
