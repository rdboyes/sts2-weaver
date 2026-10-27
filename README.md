# Weaver

A Slay the Spire 2 mod that adds the **Weaver**, a character who Aligns with the elements, Fuses cards together, and tears cards away.

## Mechanics

- **Alignment**: like the Defect's orbs, but you hold one at a time by default, and replacing it does nothing. An Alignment has no end-of-turn effect. Instead, it adds to every card you play:

  | Alignment | Effect |
  |---|---|
  | Lightning | +1 damage, or 1 damage to a target if the card deals none |
  | Frost | Gain 1 Plating |
  | Plasma | Cards cost 1 less |
  | Dark | Apply 1 Doom |
  | Glass | Deal 1 damage to ALL enemies |

- **Fuse**: a Fuse card resolves, then Exhausts. You choose a card in hand, and that card gains the Fuse card's effects, its cost and Fuse, for the rest of combat. Fused numbers combine, so two fused Pings give "Deal 6 damage", not two separate lines.
- **Exhaust**: Weaver also uses Exhaust as a theme, for example with Tear Away.

**Starter deck:** 4 Strike, 4 Defend, Ping and Tear Away.

**Starter relic:** Loom, which Aligns Lightning at the start of combat.

## Installing

1. Install [BaseLib](https://github.com/Alchyr/BaseLib-StS2) 3.4.7 or later into `<game>/mods/`.
2. Copy `Weaver.dll`, `Weaver.json` and `Weaver.pck` into `<game>/mods/Weaver/`.

## Building

You need the .NET 9 SDK, which `mise.toml` installs.

```sh
mise exec -- dotnet build
```

The build packs `Weaver.pck` and copies the mod into the game's `mods/Weaver/` folder. It finds the game folder through `Sts2PathDiscovery.props`.

The art is placeholder for now.
