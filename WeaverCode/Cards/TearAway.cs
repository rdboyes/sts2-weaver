using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Weaver.WeaverCode.Cards;

/// <summary>Exhaust a card in hand and draw cards equal to its current cost.</summary>
public sealed class TearAway() : WeaverCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        var card = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, c => c != this, this)).FirstOrDefault();
        if (card == null) return;

        // Read the cost before exhausting, so fused costs and alignment discounts count.
        int draw = card.EnergyCost.GetAmountToSpend();
        await CardCmd.Exhaust(choiceContext, card);
        if (draw > 0) await CardPileCmd.Draw(choiceContext, draw, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
