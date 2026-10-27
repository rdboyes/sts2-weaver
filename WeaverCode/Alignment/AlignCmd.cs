using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Weaver.WeaverCode.Alignment;

public static class AlignCmd
{
    public const int DefaultSlots = 1;

    public static int Slots(Creature creature) =>
        DefaultSlots + (creature.Powers.OfType<AlignmentSlotsPower>().FirstOrDefault()?.Amount ?? 0);

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

        await PowerCmd.Apply<T>(choiceContext, creature, 1m, creature, cardSource);
    }

    /// <summary>Extra slots last for the rest of combat.</summary>
    public static async Task AddSlots(PlayerChoiceContext choiceContext, Creature creature, int amount,
        CardModel? cardSource)
    {
        await PowerCmd.Apply<AlignmentSlotsPower>(choiceContext, creature, amount, creature, cardSource);
    }
}
