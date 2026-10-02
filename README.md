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
AchievementsAPI achievements = AchievementsAPI.Create("my mod guid!");
Achievements = achievements;

string firstBloodName = LocaleRegistry.Get("achievement_first_blood_name", "First blood");
string firstBloodDesc = LocaleRegistry.Get("achievement_first_blood_description", "Lose the first drop of blood");
Sprite firstBloodIcon = AssetLoader.GetCachedSprite("achievement_first_blood_icon");
Achievements.Register(AchievementDefinition.Instant(
	id: "first_blood",
	name: firstBloodName,
	description: firstBloodDesc,
	icon: firstBloodIcon
	));

// Somewhere in the code
if (Plugin.Achievements.TryGet("first_blood", out RegisteredAchievement achievement))
{
	achievement.Unlock();
}
```

You can view a more detailed example in the open source code of my mod, which uses this same framework! (paste url here, u dumbass)

## Release map

- [x] Achievement registration
- [x] Progress tracking
- [x] Save/Load
- [x] Achievement UI
- [x] Notifications
- [x] Hidden achievements
- [x] Localization helpers
- [x] Achievement ranks
- [ ] Anticheat system
- [x] Disable achievement unlocking in the tutorial/sandbox

## To be added later

- [ ] Ability to upload custom achievement unlock sounds
- [ ] Debug commands
- [ ] Multiplayer support

## Contributing

Contributions are welcome.

Please keep the public API backwards compatible whenever possible and follow the existing code style.