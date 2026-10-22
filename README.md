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
3. Download the latest release from the [NexusMods](https://www.nexusmods.com/scavprototype/mods/801) or [Releases](https://github.com/J1ND3R9/CUAchievements/releases) page.
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
using CUAchievements;
using CUCoreLib.Registries;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInDependency("com.cuachievements", BepInDependency.DependencyFlags.HardDependency)] // HardDependency: your mod will not load if CUAchievements is missing.
public class Plugin : BaseUnityPlugin
{
	 public const string ModGUID = "com.myachievementmod";
	 public const string ModName = "My achievement mod!";
	 public const string ModVersion = "1.0.0.0";

	internal static AchievementsAPI Achievements; // Keep the API instance in a static field to reach it from anywhere in your mod.

	public void Awake()
	{
		Achievements = AchievementsAPI.Create(ModGUID); // Create the API for your mod. The GUID ties achievements to your plugin, so IDs never conflict with other mods.

		LocalizedText luminaryOfScienceName = new LocalizedText("achievement_luminary_of_science_name", "Luminary of science"); // If you want to localize an achievement, use LocalizedText instead of LocaleRegistry.Get().
		LocalizedText luminaryOfScienceDesc = new LocalizedText("achievement_luminary_of_science_desc", "Keeping a light bulb in mouth was probably a bad idea.");

		Sprite luminaryOfScienceIcon = AssetLoader.LoadEmbeddedSprite("Assets.Images.luminary.png"); // CUCoreLib is also used here to load the sprite.
		
		luminaryOfScienceIcon.texture.filterMode = FilterMode.Bilinear; // An important clarification: if your achievement icon is not pixel art, it is recommended to switch the filter to bilinear.

		Achievements.Register(AchievementDefinition.Instant( // --- Instant achievement: earned with a single call ---
		    id: "luminary_of_science", // unique within your mod
		    name: luminaryOfScienceName,
		    description: luminaryOfScienceDesc,
		    icon: luminaryOfScienceIcon,		
		    hidden: true // shown only as part of "X hidden achievements"
		    ));
	
		LocalizedText paintItYellowName = new LocalizedText("achievement_paint_it_yellow_name", "Paint It, Yellow");
		LocalizedText paintItYellowDesc = new LocalizedText("achievement_paint_it_yellow_desc", "Lose 15 liters of blood in a single run.");

		Achievements.Register(AchievementDefinition.Counter( // --- Counter achievement: earned after reaching maxProgress ---
		    id: "paint_it_yellow",
		    name: paintItYellowName,
		    description: paintItYellowDesc,
		    maxProgress: 15f, // value needed to unlock
		    icon: null, // null = default icon
		    scope: AchievementScope.Run, // progress is counted per run, not for the whole game
		    progressDecimals: 2	// digits after the decimal point shown in the menu
		    ));
	}

    internal static void OnLightBulbEaten() // Call these from your gameplay code (e.g. a Harmony patch).
    {
        if (Achievements.TryGet("luminary_of_science", out RegisteredAchievement lightBulb)) // TryGet returns false if the achievement isn't registered, so no exceptions.
            lightBulb.Unlock();
    }

	internal static void OnBloodLost(float liters)
    {
        if (Achievements.TryGet("paint_it_yellow", out RegisteredAchievement blood))
            blood.AddProgress(liters); // unlocks automatically at maxProgress
    }
}
```

Achievements are tied to your plugin's GUID, so there will be no ID conflicts between mods.
Read more: (a link to the documentation will be here)
There is a mod here that makes use of almost everything CUAchievements has to offer: [Floydba Achievements](https://github.com/J1ND3R9/FloydbaAchievements)

## A small roadmap
- [ ] Create documentation (at the moment, there is only some XML documentation)
- [ ] Multiplayer support
- [ ] Ability to add custom achievement unlock sounds
- [ ] Sort settings

## Discord and Support
If any problems or questions arise, feel free to write on the Discord server! Don't hesitate to ask even the "dumbest" questions. :)

https://discord.gg/AJDjmyeEFe

## Icons
It is recommended to use a square PNG image sized 256x256 (minimum 152x152). 
For smooth artwork, set `texture.filterMode = FilterMode.Bilinear`. 
For pixel art, use `FilterMode.Point`.

## Contributing
Any contribution is welcome! Please adhere to C# coding styles and patterns. Extending existing components is preferable to adding new ones.

Please submit all pull requests to the dev branch.
