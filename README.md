# CUAchievements

A simple, extensible and mod-friendly achievement framework for Casualties: Unknown.

## Features

- Achievement registration
- Counter and instant achievements
- Automatic progress tracking
- Persistent save data
- Lightweight API
- Built for mod interoperability

## Quick Example

```csharp
var registry = AchievementRegistry.Create(ModGUID);
registry.Register(AchievementDefinition.Counter(
    "eat_100000_food",
    "Hungry",
    "Eat 100.000 food!",
    "TODO ICON",
    100000
    ));
registry.Register(AchievementDefinition.Instant(
    "first_jump",
    "Yupieee!",
    "Do your first jump.",
    "TODO ICON"
    ));
```

```csharp
[HarmonyPatch(typeof(Body))]
internal class BodyPatch
{
    [HarmonyPatch(nameof(Body.Eat))]
    [HarmonyPostfix]
    private static void EatPostfix()
    {
        AchievementService.AddProgress("eat_100000_food");
    }

    [HarmonyPatch(nameof(Body.Jump))]
    [HarmonyPostfix]
    private static void JumpPostfix()
    {
        AchievementService.Unlock("first_jump");
    }
}
```

## Roadmap

- [x] Achievement registration
- [x] Progress tracking
- [x] Save/Load
- [ ] Achievement UI
- [ ] Notifications
- [ ] Hidden achievements
- [ ] Localization helpers
- [ ] Achievement ranks
- [ ] Anticheat system

## Contributing

Contributions are welcome.

Please keep the public API backwards compatible whenever possible and follow the existing code style.