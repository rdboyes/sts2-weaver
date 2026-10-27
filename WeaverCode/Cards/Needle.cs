using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Weaver.WeaverCode.Fuse;

namespace Weaver.WeaverCode.Cards;

public sealed class Needle() : WeaverCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy), IFusable
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [WeaverKeywords.Fuse];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

    public IEnumerable<(FuseStat Stat, DynamicVar Var)> FuseStats => [(FuseStat.Damage, DynamicVars.Damage)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
