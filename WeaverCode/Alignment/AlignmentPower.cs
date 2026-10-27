using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Weaver.WeaverCode.Fuse;
using Weaver.WeaverCode.Powers;

namespace Weaver.WeaverCode.Alignment;

/// <summary>
/// An Alignment sits on the player like a power and enhances every card its owner plays. If the card already has
/// the matching number (e.g. Lightning and damage), the bonus merges into it and shows on the card; otherwise the
/// alignment's rider is added as an extra effect. Fused effects keep the enhancement they were played with
/// (see <see cref="AddFusedBonus"/>), so the live alignment ignores hidden fused copies.
/// Only <see cref="AlignCmd"/> should apply these, so the slot limit is respected.
/// </summary>
public abstract class AlignmentPower : WeaverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>Placeholder art: reuse the matching base-game orb sprite, e.g. "lightning".</summary>
    protected abstract string OrbName { get; }
    public override string CustomPackedIconPath => $"res://images/orbs/{OrbName}_orb.png";
    public override string CustomBigIconPath => $"res://images/orbs/{OrbName}_orb.png";

    public static IReadOnlyList<AlignmentPower> On(Creature creature) =>
        creature.Powers.OfType<AlignmentPower>().ToList();

    /// <summary>Whether the bonus folds into a number the card already has, instead of adding a rider.</summary>
    public virtual bool MergesInto(CardModel card) => false;

    /// <summary>
    /// What this alignment adds to a card that is fused while aligned. Whether the bonus merged into the card or was
    /// a rider, it comes out as the same stat (e.g. Lightning is always +1 damage).
    /// </summary>
    public virtual void AddFusedBonus(FusedEffects effects) { }

    public virtual bool HasRider => true;

    /// <summary>Card text for the rider, e.g. "Gain 1 Block."</summary>
    public string RiderText => new LocString("powers", Id.Entry + ".rider").GetFormattedText();

    /// <summary>
    /// The extra effect added to cards this alignment doesn't merge into.
    /// </summary>
    public virtual Task Rider(PlayerChoiceContext choiceContext, Creature owner, CardModel card, Creature? target) =>
        Task.CompletedTask;

    /// <summary>Called when this alignment is pushed out by a new one. No effect by default.</summary>
    public virtual Task OnReplaced(PlayerChoiceContext choiceContext) => Task.CompletedTask;

    /// <summary>Cards the live alignment enhances: owned by this creature and not a fused copy.</summary>
    protected bool IsAlignedCard(CardModel? card) =>
        card != null && card.Owner.Creature == Owner && !FuseCmd.IsPayload(card);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!HasRider || !IsAlignedCard(cardPlay.Card) || MergesInto(cardPlay.Card)) return;
        await Rider(choiceContext, Owner, cardPlay.Card, cardPlay.Target);
    }

    /// <summary>The preferred target if it is a living enemy, otherwise a random hittable enemy.</summary>
    protected static Creature? EnemyTarget(Creature owner, Creature? preferred)
    {
        if (preferred is { IsHittable: true } && preferred.Side != owner.Side) return preferred;

        var enemies = owner.CombatState?.GetOpponentsOf(owner).Where(e => e.IsHittable).ToList();
        if (enemies == null || enemies.Count == 0) return null;
        return owner.Player!.RunState.Rng.CombatTargets.NextItem(enemies);
    }
}
