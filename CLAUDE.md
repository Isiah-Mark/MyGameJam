# CLAUDE.md

Guidance for working in this repository.

## Project

**Comfy Jam: Summer** — a Unity 2D game jam project. A beach/lifeguard game: swimmers drown in
the sea, and the player hires and deploys lifeguards to rescue them, earning currency per save.

- **Engine:** Unity `6000.3.17f1` (Unity 6), Universal Render Pipeline, new Input System.
- **No CLI build/test loop.** This is a Unity project — code is compiled, run, and tested by
  opening the project in the Unity Editor. There is no `dotnet test` / npm workflow. `.csproj`
  and `.sln` files are Unity-generated; do not hand-edit them.

## Two code styles in this repo

The codebase has two distinct areas. Match the style of whichever you're editing.

1. **Shop/Inventory/Economy systems** (`Assets/Scripts/{Core,Economy,Inventory,Lifeguards,Shop,UI}`):
   newer, deliberate architecture. `ComfyJam.*` namespaces, interface-driven (`IShop`,
   `ICurrencyWallet`, `ILifeguardRoster`), XML doc comments, `_camelCase` serialized fields,
   event-bus decoupling. **Prefer this style for new gameplay-systems code.**

2. **Swimmer gameplay** (`Assets/Scripts/Swimmer.cs`, `SwimmerManager.cs`, `SwimmerSpawner.cs`,
   `CamerMovement/`): older, no namespaces, public fields, `[Header]`-grouped inspector vars.
   Self-contained MonoBehaviours driving the in-sea swimmer AI (idle/wander/drown/sink/panic).

`Assets/TextMesh Pro/` is third-party (TMP examples) — never edit.

## Architecture: the event boundary

`ComfyJam.Core.GameEvents` (`Assets/Scripts/Core/GameEvents.cs`) is the **single boundary**
between the shop/inventory/economy systems and the gameplay/AI side. It's a static event hub.
Because the events are static, every subscriber **must** unsubscribe in `OnDisable`/`OnDestroy`
or handlers leak across play sessions. Follow this pattern when adding listeners.

Flow: gameplay raises `PersonSaved` → `CurrencyManager` earns; UI/hot-wheel calls
`LifeguardRoster.TryDeploy` → roster raises `DeployRequested` → gameplay spawns the lifeguard;
gameplay later raises `LifeguardReturned` / `LifeguardDied` → roster updates state.

## Key singletons (one per scene, `Instance` accessor, guarded in `Awake`)

- **`CurrencyManager`** (`Economy/`) — the wallet. `TrySpend` both checks and deducts; `Add`
  earns. Raises `CurrencyChanged`.
- **`LifeguardRoster`** (`Inventory/`) — source of truth for every hired lifeguard. Owns all
  `LifeguardState` transitions (`Available → Deployed → Dead`); set state *through* the roster,
  never directly. Dead lifeguards stay in the list (for loss counting) but never become usable.
- **`ShopController`** (`Shop/`) — hiring kiosk. `TryHire` checks roster cap, then spends, then
  adds to roster (refunds on the should-never-happen add failure). Raises `HireFailed(reason)`.
- **`SwimmerManager`** — tracks drowned count, updates UI.

## Data: lifeguard types

`LifeguardTypeSO` (ScriptableObject, `Assets > Create > ComfyJam > Lifeguard Type`) defines each
kind of lifeguard (id, display name, icon, hire cost, and stats). **Adding a new lifeguard type
is a data-only change** — create an asset and add it to a `_catalog` list in the inspector
(`ShopView`, `HotWheelView`). No code change needed.

Stats are `Speed`, `Stamina`, `Skill` (1–10) and feed the save loop: Speed = time to reach a
swimmer, Stamina = saves before exhaustion, Skill = success on tough rescues. **Speed** is the
one actively used right now (lifeguards are being authored with differing speeds); Stamina and
Skill exist so the rescue/deploy logic has a home as it lands. `ShopItemButton` can show Speed as
an optional filled bar — wire its `_speedFill`, or leave it empty to keep the card minimal.

## UI

- **`ShopView`** — builds one `ShopItemButton` per catalog type, greys out unaffordable/blocked
  hires, flashes `HireFailed` feedback.
- **`HotWheelView`** — radial deploy menu. Hold Interact to open, Previous/Next to cycle, release
  to deploy. Reads the shared `InputSystem_Actions` asset (`Interact`/`Previous`/`Next`).
- UI subscribes to manager events in `Start` (managers exist by then via `Awake`) and
  unsubscribes in `OnDestroy`.

## Testing without the gameplay side

`ComfyJam.Debugging.GameplaySimulator` stands in for the gameplay/AI side so the shop, roster,
and economy can be exercised with no teammate code. It exposes `[ContextMenu]` actions
(Save Person, Hire One, Deploy One, Kill/Return Last Deployed, Log Status) usable in play mode
via the component's right-click menu. Scene: `Assets/Scenes/InventoryTest.unity`.

## Scenes

`Assets/Scenes/`: `Day1.unity` (main gameplay), `InventoryTest.unity` (shop/inventory sandbox),
`SampleScene.unity`.

## Conventions

- Validate inputs defensively and log with a bracketed source tag, e.g.
  `Debug.LogWarning("[LifeguardRoster] ...")`.
- Serialized private fields use `_camelCase` with `[Tooltip]`/`[SerializeField]`; expose
  read-only access via expression-bodied properties.
- Keep comments lean and purposeful (see existing files).

## Committing Unity changes

A new `.cs` script needs its Unity-generated `.meta` file to be committed alongside it. Let the
user focus the Unity Editor first so it generates the `.meta`, then commit the `.cs` + `.meta`
together.