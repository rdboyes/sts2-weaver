using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Weaver.WeaverCode.Alignment;

namespace Weaver.WeaverCode.Fuse;

/// <summary>The kinds of simple effect that fused cards are broken down into, so they can be summed.</summary>
public enum FuseStat
{
    /// <summary>Damage to the card's target, or a random enemy.</summary>
    Damage,
    /// <summary>Damage to ALL enemies.</summary>
    DamageAll,
    Block,
    /// <summary>Doom applied to the card's target, or a random enemy.</summary>
    Doom,
    Draw,
    AlignmentSlots
}

/// <summary>A card whose effects can be described entirely as <see cref="FuseStat"/>s and Alignments.</summary>
public interface IFusable
{
    /// <summary>
    /// Each stat this card applies once, and the var holding its amount. When another card is fused into this one,
    /// matching stats are added straight to these vars so the card shows a single combined number.
    /// </summary>
    IEnumerable<(FuseStat Stat, DynamicVar Var)> FuseStats { get; }

    IEnumerable<FusedAlign> FuseAligns => [];
}

/// <summary>An "Align X" effect carried by a fused card.</summary>
public sealed record FusedAlign(Type Alignment, Func<PlayerChoiceContext, Creature, CardModel?, Task> Apply)
{
    public static FusedAlign Of<T>() where T : AlignmentPower =>
        new(typeof(T), (ctx, creature, card) => AlignCmd.Align<T>(ctx, creature, card));

    public string ShortName =>
        new LocString("powers", ModelDb.GetId(Alignment).Entry + ".short").GetFormattedText();
}

/// <summary>Everything fused onto a card that couldn't be folded into the card's own numbers.</summary>
public sealed class FusedEffects
{
    public readonly Dictionary<FuseStat, decimal> Stats = [];
    public readonly List<FusedAlign> Aligns = [];

    /// <summary>Fused copies of cards that can't be broken down into stats; they are played as-is.</summary>
    public readonly List<CardModel> Others = [];

    public bool IsEmpty => Stats.Count == 0 && Aligns.Count == 0 && Others.Count == 0;

    public decimal this[FuseStat stat] => Stats.GetValueOrDefault(stat);

    public void Add(FuseStat stat, decimal amount)
    {
        if (amount == 0) return;
        Stats[stat] = this[stat] + amount;
    }

    /// <summary>Adds an Align effect. Only the latest of each alignment is kept, since re-aligning just refreshes it.</summary>
    public void AddAlign(FusedAlign align)
    {
        Aligns.RemoveAll(a => a.Alignment == align.Alignment);
        Aligns.Add(align);
    }

    public void AddAll(FusedEffects other)
    {
        foreach (var (stat, amount) in other.Stats) Add(stat, amount);
        foreach (var align in other.Aligns) AddAlign(align);
        Others.AddRange(other.Others);
    }

    public FusedEffects Copy()
    {
        var copy = new FusedEffects();
        copy.AddAll(this);
        return copy;
    }
}
