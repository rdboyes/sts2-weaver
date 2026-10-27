using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Weaver.WeaverCode.Alignment;
using Weaver.WeaverCode.Fuse;

namespace Weaver.WeaverCode.Cards;

public sealed class ShadowWeft() : WeaverCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy), IFusable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DoomPower>(5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<DoomPower>(), HoverTipFactory.FromPower<DarkAlignment>()];

    public IEnumerable<(FuseStat Stat, DynamicVar Var)> FuseStats => [(FuseStat.Doom, DynamicVars.Doom)];

    public IEnumerable<FusedAlign> FuseAligns => [FusedAlign.Of<DarkAlignment>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<DoomPower>(choiceContext, cardPlay.Target, DynamicVars.Doom.BaseValue, Owner.Creature, this);
        await AlignCmd.Align<DarkAlignment>(choiceContext, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Doom.UpgradeValueBy(3m);
    }
}
