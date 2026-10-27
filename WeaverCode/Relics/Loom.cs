using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Weaver.WeaverCode.Alignment;

namespace Weaver.WeaverCode.Relics;

/// <summary>Starter relic: at the start of each combat, Align Lightning.</summary>
public sealed class Loom : WeaverRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LightningAlignment>()];

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) && Owner.PlayerCombatState?.TurnNumber <= 1)
        {
            await AlignCmd.Align<LightningAlignment>(new BlockingPlayerChoiceContext(), Owner.Creature, null);
        }
    }
}
