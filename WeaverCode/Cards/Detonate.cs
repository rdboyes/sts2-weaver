using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Weaver.WeaverCode.Cards;

/// <summary>Choose a card in hand and play it X times, where X is its current cost.</summary>
public sealed class Detonate() : WeaverCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1) { PretendCardsCanBePlayed = true };
        var card = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs,
            c => c != this && !c.Keywords.Contains(CardKeyword.Unplayable), this)).FirstOrDefault();
        if (card == null) return;

        // Read the cost before playing: X-cost cards count your current energy, and plays may change the cost.
        int times = card.EnergyCost.GetAmountToSpend();
        if (times <= 0)
        {
            await CardCmd.Exhaust(choiceContext, card);
            return;
        }

        for (int i = 0; i < times; i++)
        {
            // The detonated card is used up: its last play sends it to the exhaust pile.
            if (i == times - 1) card.ExhaustOnNextPlay = true;
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
