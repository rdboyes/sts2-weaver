using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Weaver.WeaverCode.Fuse;

/// <summary>Always-on combat listener that drives Fuse.</summary>
public sealed class FuseHooks() : CustomSingletonModel(HookType.Combat)
{
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card,
        bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (pileType != PileType.None && FuseCmd.HasFuse(card)) return (PileType.Exhaust, position);
        return (pileType, position);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        await FuseCmd.PlayFused(choiceContext, cardPlay);

        if (cardPlay.IsLastInSeries && FuseCmd.HasFuse(card) && !card.Owner.Creature.IsDead)
        {
            await FuseCmd.FuseIntoHand(choiceContext, card);
        }
    }
}
