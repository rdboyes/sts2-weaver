using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Weaver.WeaverCode.Powers;

namespace Weaver.WeaverCode.Cards;

public sealed class AdaptiveWeave() : WeaverCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<AdaptiveWeavePower>(6m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AdaptiveWeavePower>(choiceContext, Owner.Creature, DynamicVars["AdaptiveWeavePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AdaptiveWeavePower"].UpgradeValueBy(3m);
    }
}
