using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Weaver.WeaverCode.Alignment;
using Weaver.WeaverCode.Fuse;

namespace Weaver.WeaverCode.Cards;

public sealed class Shatterweave() : WeaverCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self), IFusable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(12m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<GlassAlignment>()];

    public IEnumerable<(FuseStat Stat, DynamicVar Var)> FuseStats => [(FuseStat.Block, DynamicVars.Block)];

    public IEnumerable<FusedAlign> FuseAligns => [FusedAlign.Of<GlassAlignment>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await AlignCmd.Align<GlassAlignment>(choiceContext, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
