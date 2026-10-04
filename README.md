# CUAchievements
An achievement framework for **Casualties Unknown**: provides mods with a unified way to add achievements featuring pop-up notifications and a shared menu.

<img width="1888" height="811" alt="Achievements menu" src="https://github.com/user-attachments/assets/729f661f-dd9c-4e03-b6bf-9971547f1366" />

## Features
- Pop-up notification upon earning an achievement.
- Achievements menu with search and statistics.
- Three difficulty levels for achievements.
- Hidden achievements displayed as a single "X hidden achievements" card.
- The achievements for each mod are grouped separately.
- An anti-cheat system that disables achievements if an "unsafe" console command is entered.

## For players

### Installation
1. Install [BepInEx](https://github.com/bepinex/bepinex) if you haven't already.
2. Download and install [CUCoreLib](https://www.nexusmods.com/scavprototype/mods/341).
3. Download the latest release from the [Releases](https://github.com/J1ND3R9/CUAchievements/releases) page.
4. Place the CUAchievements folder into the BepInEx/plugins folder.
5. Launch the game. The framework will notify you in-game that it has been successfully installed.


### Something doesn't work?
Check `BepInEx/LogOutput.log` for errors and ask on [Discord](https://discord.gg/AJDjmyeEFe) or open an issue, attaching the log.

## For developers

### Quick start
1. Add a reference to `CUAchievements.dll` in your project.
2. Declare it as a dependency of your plugin (see below), so BepInEx loads it first.

```csharp
using BepInEx;
using CUAchievements.Definitions;
using CUAchievements.Services;
using CUCoreLib.Registries;

[BepInPlugin(ModGUID, ModName, ModVersion)]
// HardDependency: your mod will not load if CUAchievements is missing.
[BepInDependency("com.cuachievements", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
	 public const string ModGUID = "com.myachievementmod";
	 public const string ModName = "My achievement mod!";
	 public const string ModVersion = "1.0.0.0";

    // Keep the API instance in a static field to reach it from anywhere in your mod.
	internal static AchievementsAPI Achievements;

	public void Awake()
	{
        // Create the API for your mod. The GUID ties achievements to your plugin, so IDs never conflict with other mods.
		Achievements = AchievementsAPI.Create(ModGUID);

		// Try to use LocaleRegistry from CUCoreLib for the name and description.
		string luminaryOfScienceName = LocaleRegistry.Get("achievement_luminary_of_science_name", "Luminary of science");
		string luminaryOfScienceDesc = LocaleRegistry.Get("achievement_luminary_of_science_desc", "Keeping a light bulb in mouth was probably a bad idea.");

		// CUCoreLib is also used here to load the sprite.
		Sprite luminaryOfScienceIcon = AssetLoader.LoadEmbeddedSprite("Assets.Images.luminary.png");
		
		// An important clarification: if your achievement icon is not pixel art, it is recommended to switch the filter to bilinear.
		luminaryOfScienceIcon.texture.filterMode = FilterMode.Bilinear;

        // --- Instant achievement: earned with a single call ---
		Achievements.Register(AchievementDefinition.Instant(
		    id: "luminary_of_science",			// unique within your mod
		    name: luminaryOfScienceName,
		    description: luminaryOfScienceDesc,
		    icon: luminaryOfScienceIcon,		
		    hidden: true						// shown only as part of "X hidden achievements"
		    ));
	
		string paintItYellowName = LocaleRegistry.Get("achievement_paint_it_yellow_name", "Paint It, Yellow");
		string paintItYellowDesc = LocaleRegistry.Get("achievement_paint_it_yellow_desc", "Lose 15 liters of blood in a single run.");

        // --- Counter achievement: earned after reaching maxProgress ---
		Achievements.Register(AchievementDefinition.Counter(
		    id: "paint_it_yellow",
		    name: paintItYellowName,
		    description: paintItYellowDesc,
		    maxProgress: 15f,					// value needed to unlock
		    icon: null,							// null = default icon
		    scope: AchievementScope.Run,		// progress is counted per run, not for the whole game
		    progressDecimals: 2					// digits after the decimal point shown in the menu
		    ));
	}

	// Call these from your gameplay code (e.g. a Harmony patch).
    internal static void OnLightBulbEaten()
    {
        // TryGet returns false if the achievement isn't registered, so no exceptions.
        if (Achievements.TryGet("luminary_of_science", out RegisteredAchievement lightBulb))
            lightBulb.Unlock();
    }

	internal static void OnBloodLost(float liters)
    {
        if (Achievements.TryGet("paint_it_yellow", out RegisteredAchievement blood))
            blood.AddProgress(liters);                // unlocks automatically at maxProgress
    }
}
```

Achievements are tied to your plugin's GUID, so there will be no ID conflicts between mods.
Read more: (a link to the documentation and a mod example will be here)

## A small roadmap
- [ ] Multiplayer support
- [ ] Ability to add custom achievement unlock sounds

## Discord and Support
If any problems or questions arise, feel free to write on the Discord server! Don't hesitate to ask even the "dumbest" questions. :)

https://discord.gg/AJDjmyeEFe

## Icons
It is recommended to use a square PNG image sized 256x256 (minimum 152x152). 
For smooth artwork, set `texture.filterMode = FilterMode.Bilinear`. 
For pixel art, use `FilterMode.Point`.

## Contributing
Any contribution is welcome! Please adhere to C# coding styles and patterns. Extending existing components is preferable to adding new ones.
