using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using Weaver.WeaverCode.Alignment;
using Weaver.WeaverCode.Fuse;

namespace Weaver.WeaverCode.Cards;

public sealed class DoubleWeave() : WeaverCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self), IFusable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Slots", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<AlignmentSlotsPower>()];

    public IEnumerable<(FuseStat Stat, DynamicVar Var)> FuseStats => [(FuseStat.AlignmentSlots, DynamicVars["Slots"])];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlignCmd.AddSlots(choiceContext, Owner.Creature, DynamicVars["Slots"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
