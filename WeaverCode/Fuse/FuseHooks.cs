using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Weaver.WeaverCode.Fuse;

/// <summary>Always-on combat listener that drives Fuse.</summary>
public sealed class FuseHooks() : CustomSingletonModel(HookType.Combat)
{
    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay,
        ResourceInfo resources, CardLocation cardLocation)
    {
        if (cardLocation.pileType != PileType.None && FuseCmd.HasFuse(card))
            return cardLocation with { pileType = PileType.Exhaust };
        return cardLocation;
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
