# Swmarly Valheim Pizza Mod

Pizza cooking for Valheim 1.0. The mod uses the vanilla Food Preparation Table and vanilla Stone Oven; it does not add a custom preparation station or oven.

## Installation

Install BepInExPack for Valheim and the current Jötunn release required by `manifest.json`. Copy the built `SwmarlyValheimPizzaMod.dll` into:

```text
BepInEx/plugins/SwmarlyValheimPizzaMod/
```

For a local build, point MSBuild at the game installation and BepInEx installation:

```powershell
dotnet build -c Release /p:ValheimPath="C:\Program Files (x86)\Steam\steamapps\common\Valheim" /p:BepInExPath="C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx"
```

The Release build copies the plugin into `Package/BepInEx/plugins/SwmarlyValheimPizzaMod/`.

## Gameplay

The complete loop is:

```text
Tomatoes + Lox Milk + Barley Flour
    -> Food Preparation Table ingredients
Pizza Dough + Tomato Sauce + Lox Cheese + toppings
    -> Uncooked Pizza
Uncooked Pizza
    -> vanilla Stone Oven
Finished Pizza
```

### Tomatoes

The initial discovery path is deliberately reliable and uses normal food preparation:

```text
1 Carrot + 1 Barley -> 2 Tomato Seeds
```

Plant Tomato Seeds with a Cultivator. The tomato and seed-tomato plants use Valheim's normal `Plant` and `Pickable` components, including cultivated-ground checks, spacing checks, growth, harvesting, seed return, world-save persistence, and network synchronization. Tomatoes can grow anywhere the corresponding vanilla cultivated crop can grow, including Meadows, Black Forest, and Plains.

The seed recipe is a temporary discovery path until a dedicated wild Plains tomato plant asset is added. It can be replaced in `TomatoCropSystem` without changing the crop mechanics.

### Ingredients

| Item | Recipe | Station |
| --- | --- | --- |
| Pizza Dough | 12 Barley Flour | Food Preparation Table |
| Tomato Sauce | 3 Tomato + 1 Onion | Food Preparation Table |
| Lox Milker | 8 Fine Wood + 4 Iron + 6 Leather Scraps | Forge |
| Lox Cheese | 2 Lox Milk | Food Preparation Table |

Equip the Lox Milker and interact with a tamed Lox. The server validates the target, taming state, distance, equipped tool, inventory reward, and cooldown. Each Lox stores its last milking time in its ZDO, so the 25 in-game-minute cooldown survives save/reload and works on dedicated servers. A successful milking gives 2 Lox Milk by default.

Lox milking messages are:

- `You milk the Lox.`
- `This Lox has already been milked recently.`
- `The Lox must be tamed first.`

## Pizzas

Every pizza uses 1 Pizza Dough, 1 Tomato Sauce, 1 Lox Cheese, and the listed toppings. Recipes appear through normal Valheim discovery when all ingredients are known. Uncooked pizzas are not edible and must be placed in a vanilla Stone Oven.

| Pizza | Toppings | Health | Stamina | Eitr | Healing | Duration |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| Cheese Pizza | — | 55 | 55 | 0 | 4 | 25 min |
| Mushroom Pizza | Mushroom | 45 | 75 | 0 | 4 | 25 min |
| Onion Pizza | Onion | 40 | 90 | 0 | 4 | 25 min |
| Veggie Pizza | Carrot, Turnip, Onion, Mushroom | 55 | 105 | 0 | 5 | 30 min |
| Hunter's Pizza | Boar Meat, Deer Meat, Mushroom | 90 | 65 | 0 | 5 | 30 min |
| Sausage Pizza | Sausages, Onion | 100 | 55 | 0 | 5 | 30 min |
| Wolf Pizza | Wolf Meat, Onion | 115 | 55 | 0 | 6 | 30 min |
| Lox Pizza | Lox Meat | 125 | 45 | 0 | 6 | 30 min |
| Lox & Cloudberry Pizza | Lox Meat, Cloudberries | 105 | 75 | 0 | 6 | 30 min |
| Serpent Pizza | Serpent Meat, Onion | 135 | 60 | 0 | 7 | 30 min |
| Fisherman's Pizza | Raw Fish, Onion | 75 | 90 | 0 | 5 | 30 min |
| Mistlands Pizza | Seeker Meat, Jotun Puffs | 80 | 55 | 35 | 5 | 30 min |
| Mage Pizza | Magecap, Jotun Puffs, Royal Jelly | 45 | 55 | 75 | 5 | 30 min |
| Ashlands Pizza | Asksvin Meat, Fiddlehead | 145 | 70 | 0 | 7 | 30 min |

The Lox & Cloudberry Pizza description is: “An ancient culinary disagreement, reborn in Valheim.” Fish uses the current `FishRaw` ingredient. The Ashlands recipe uses current `AsksvinMeat` and `Fiddleheadfern` prefabs (displayed as Fiddlehead).

## Configuration

The generated BepInEx config exposes ingredient costs, Lox cooldown and yield, Stone Oven cook time, and a separate Health/Stamina/Eitr/Healing/Duration section for every finished pizza. Set the Stone Oven time to `0` only if you want the plugin's safe 10-second fallback; Jötunn's conversion API requires an explicit value.

## Assets and compatibility

The implementation reuses vanilla Bread Dough, carrot crop, seed crop, and Lox Pie visuals where practical. Asset lookup is isolated in `AssetRegistry` so custom tomato, milk, cheese, and pizza art can be substituted later without changing gameplay registration. Missing optional cosmetic assets do not prevent the core content from loading.

Jötunn is a hard dependency because it provides current prefab, item, piece, localization, cooking-conversion, and multiplayer registration APIs. The plugin is marked as requiring the same mod on all clients and the server. Lox cooldowns and rewards are server-authoritative; crop and oven state use vanilla synchronized systems.
