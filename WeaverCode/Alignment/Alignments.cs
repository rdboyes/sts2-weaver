using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Weaver.WeaverCode.Fuse;

namespace Weaver.WeaverCode.Alignment;

/// <summary>Cards deal 1 more damage. Cards without damage instead deal 1 damage to their target (or a random enemy).</summary>
public sealed class LightningAlignment : AlignmentPower
{
    protected override string OrbName => "lightning";

    public override bool MergesInto(CardModel card) => card.DynamicVars.ContainsKey("Damage");

    public override void AddFusedBonus(FusedEffects effects) => effects.Add(FuseStat.Damage, 1m);

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource)
    {
        if (!IsAlignedCard(cardSource) || !MergesInto(cardSource!) || !props.IsPoweredAttack()) return 0m;
        return 1m;
    }

    public override async Task Rider(PlayerChoiceContext choiceContext, Creature owner, CardModel card, Creature? target)
    {
        var enemy = EnemyTarget(owner, target);
        if (enemy == null) return;
        VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_lightning");
        await CreatureCmd.Damage(choiceContext, enemy, 1m, ValueProp.Unpowered, owner);
    }
}

/// <summary>Cards gain 1 more Block. Cards without Block instead also grant 1 Block.</summary>
public sealed class FrostAlignment : AlignmentPower
{
    protected override string OrbName => "frost";

    public override bool MergesInto(CardModel card) => card.DynamicVars.ContainsKey("Block");

    public override void AddFusedBonus(FusedEffects effects) => effects.Add(FuseStat.Block, 1m);

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!IsAlignedCard(cardSource) || !MergesInto(cardSource!) || !props.IsPoweredCardOrMonsterMoveBlock()) return 0m;
        return 1m;
    }

    public override async Task Rider(PlayerChoiceContext choiceContext, Creature owner, CardModel card, Creature? target)
    {
        await CreatureCmd.GainBlock(owner, 1m, ValueProp.Unpowered, null);
    }
}

/// <summary>Cards cost 1 less energy (never below 0). The discount is part of the cost a Fuse card passes on.</summary>
public sealed class PlasmaAlignment : AlignmentPower
{
    protected override string OrbName => "plasma";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override bool HasRider => false;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!IsAlignedCard(card) || originalCost <= 0) return false;
        modifiedCost = originalCost - 1;
        return true;
    }
}

/// <summary>Cards also apply 1 Doom to their target (or a random enemy).</summary>
public sealed class DarkAlignment : AlignmentPower
{
    protected override string OrbName => "dark";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DoomPower>()];

    public override void AddFusedBonus(FusedEffects effects) => effects.Add(FuseStat.Doom, 1m);

    public override async Task Rider(PlayerChoiceContext choiceContext, Creature owner, CardModel card, Creature? target)
    {
        var enemy = EnemyTarget(owner, target);
        if (enemy == null) return;
        await PowerCmd.Apply<DoomPower>(choiceContext, enemy, 1m, owner, null);
    }
}

/// <summary>Cards also deal 1 damage to ALL enemies.</summary>
public sealed class GlassAlignment : AlignmentPower
{
    protected override string OrbName => "glass";

    public override void AddFusedBonus(FusedEffects effects) => effects.Add(FuseStat.DamageAll, 1m);

    public override async Task Rider(PlayerChoiceContext choiceContext, Creature owner, CardModel card, Creature? target)
    {
        var enemies = owner.CombatState?.GetOpponentsOf(owner).Where(e => e.IsHittable).ToList();
        if (enemies == null || enemies.Count == 0) return;
        await CreatureCmd.Damage(choiceContext, enemies, 1m, ValueProp.Unpowered, owner);
    }
}
