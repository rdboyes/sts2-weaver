using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Weaver.WeaverCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Weaver.WeaverCode.Cards;
using Weaver.WeaverCode.Relics;

namespace Weaver.WeaverCode.Character;

public class Weaver : PlaceholderCharacterModel
{
    public const string CharacterId = "Weaver";
    
    public static readonly Color Color = new("b98cff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 75;
    public override int StartingGold => 99;
    
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeWeaver>(),
        ModelDb.Card<StrikeWeaver>(),
        ModelDb.Card<StrikeWeaver>(),
        ModelDb.Card<StrikeWeaver>(),
        ModelDb.Card<DefendWeaver>(),
        ModelDb.Card<DefendWeaver>(),
        ModelDb.Card<DefendWeaver>(),
        ModelDb.Card<DefendWeaver>(),
        ModelDb.Card<Ping>(),
        ModelDb.Card<Carapace>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<Loom>()
    ];
    
    public override CardPoolModel CardPool => ModelDb.CardPool<WeaverCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<WeaverRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<WeaverPotionPool>();
    
    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets. 
        These are just some of the simplest assets, given some placeholders to differentiate your character with. 
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    // Placeholder art: a single image used for combat, the rest site and the merchant, in place of the Ironclad.
    // BaseLib converts these plain Godot scenes into the game's node types.
    public override string CustomVisualPath => "res://Weaver/scenes/weaver_combat.tscn";
    public override string CustomRestSiteAnimPath => "res://Weaver/scenes/weaver_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://Weaver/scenes/weaver_merchant.tscn";

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}