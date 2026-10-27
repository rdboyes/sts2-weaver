using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Weaver.WeaverCode.Alignment;

/// <summary>A power that reacts whenever its owner changes to an alignment they didn't already have.</summary>
public interface IAfterAlignmentChanged
{
    Task AfterAlignmentChanged(PlayerChoiceContext choiceContext, AlignmentPower newAlignment);
}

public static class AlignCmd
{
    public const int DefaultSlots = 1;

    private sealed class ChangeCount
    {
        public ICombatState? Combat;
        public int Count;
    }

    private static readonly NotNullSpireField<Creature, ChangeCount> Changes = new(() => new ChangeCount());

    public static int Slots(Creature creature) =>
        DefaultSlots + (creature.Powers.OfType<AlignmentSlotsPower>().FirstOrDefault()?.Amount ?? 0);

    /// <summary>How strong each alignment is: 1, plus any <see cref="ResonancePower"/>.</summary>
    public static int Strength(Creature creature) =>
        1 + (creature.Powers.OfType<ResonancePower>().FirstOrDefault()?.Amount ?? 0);

    /// <summary>
    /// How many times the creature has changed to an alignment it didn't already have this combat, including
    /// aligning at the start of combat.
    /// </summary>
    public static int ChangesThisCombat(Creature creature)
    {
        var changes = Changes[creature];
        return changes.Combat == creature.CombatState ? changes.Count : 0;
    }

    /// <summary>
    /// Align the creature with a new alignment. If every slot is full, the oldest alignment is replaced
    /// (which has no effect unless that alignment overrides <see cref="AlignmentPower.OnReplaced"/>).
    /// </summary>
    public static async Task Align<T>(PlayerChoiceContext choiceContext, Creature creature, CardModel? cardSource)
        where T : AlignmentPower
    {
        var current = AlignmentPower.On(creature);

        // Alignments don't stack; re-aligning to one you already have just refreshes its position.
        var existing = current.OfType<T>().FirstOrDefault();
        bool isChange = existing == null;
        if (existing != null)
        {
            await PowerCmd.Remove(existing);
            current = AlignmentPower.On(creature);
        }

        int toReplace = current.Count - Slots(creature) + 1;
        foreach (var old in current.Take(Math.Max(0, toReplace)))
        {
            await old.OnReplaced(choiceContext);
            await PowerCmd.Remove(old);
        }

        var alignment = await PowerCmd.Apply<T>(choiceContext, creature, Strength(creature), creature, cardSource);
        if (!isChange || alignment == null) return;

        var changes = Changes[creature];
        if (changes.Combat != creature.CombatState)
        {
            changes.Combat = creature.CombatState;
            changes.Count = 0;
        }
        changes.Count++;

        foreach (var listener in creature.Powers.OfType<IAfterAlignmentChanged>().ToList())
        {
            await listener.AfterAlignmentChanged(choiceContext, alignment);
        }
    }

    /// <summary>Extra slots last for the rest of combat.</summary>
    public static async Task AddSlots(PlayerChoiceContext choiceContext, Creature creature, int amount,
        CardModel? cardSource)
    {
        await PowerCmd.Apply<AlignmentSlotsPower>(choiceContext, creature, amount, creature, cardSource);
    }

    /// <summary>Bring every current alignment up to date with <see cref="Strength"/>.</summary>
    public static void RefreshStrength(Creature creature)
    {
        int strength = Strength(creature);
        foreach (var alignment in AlignmentPower.On(creature))
        {
            if (alignment.Amount != strength) alignment.SetAmount(strength);
        }
    }
}
