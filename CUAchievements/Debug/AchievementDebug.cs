using CUAchievements.Anticheat;
using CUAchievements.Patches;
using CUAchievements.Plugin;
using CUAchievements.Registry;
using CUAchievements.UI;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CUAchievements.Debug
{
    internal static class AchievementDebug
    {
#if DEBUG
        internal static void Register()
        {
            AchievementsAPI achievements = AchievementsAPI.Create(CUAchievementsInfo.GUID);

            RegisterAchievements(achievements);
            RegisterCommands();
        }

        private static void RegisterAchievements(AchievementsAPI achievements)
        {
            Sprite icon = AssetLoader.GetCachedSprite("achievement_debug_icon");
            for (int i = 1; i < 5; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testCounter{i}", "Ачивка счётчик", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 10, icon));
            }

            for (int i = 1; i < 5; i++)
            {
                achievements.Register(AchievementDefinition.Instant($"testInstant{i}", "Ачивка", "Приятно, граждане, наблюдать, как сторонники тоталитаризма в науке освещают чрезвычайно интересные особенности картины в целом, однако конкретные выводы, разумеется, заблокированы в рамках своих собственных рациональных ограничений. Но высокое качество позиционных исследований не оставляет шанса для инновационных методов управления процессами.", null));
            }

            for (int i = 1; i < 3; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testCounterDifficult{i}", "Сложная ачивка", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 3, null, "", null, AchievementDifficulty.Difficult));
            }

            for (int i = 1; i < 3; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testCounterHard{i}", "Оч. сложная ачивка", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 3, null, "", null, AchievementDifficulty.Hard));
            }

            for (int i = 1; i < 3; i++)
            {
                achievements.Register(AchievementDefinition.Counter($"testHidden{i}", "Скрытая ачивка", "Не следует, однако, забывать, что синтетическое тестирование прекрасно подходит для реализации дальнейших направлений развития.", 3, null, "", null, AchievementDifficulty.Hard, AchievementScope.Lifetime, null, true));
            }
        }

        private static void RegisterCommands()
        {
            ConsoleCommandRegistry.Register(
               "progress_achievement",
               "Add progress to achievement",
               args =>
               {
                   if (args.Length < 2) return;
                   if (string.IsNullOrEmpty(args[1]))
                       return;
                   if (AchievementRegistry.TryGetFull(args[1], out RegisteredAchievement ach))
                       ach.AddProgress();
                   else
                       CUCoreUtils.ConsoleLog(ConsoleScript.instance, $"Achievement with id {args[1]} not found!");
               },
               new Dictionary<int, List<string>>
              {
                  { 0, AchievementRegistry.GetAll().Keys.ToList() }
              },
               ("id", "Achievement ID")
               );

            ConsoleCommandRegistry.Register(
                "toggle_anticheat",
                "Toggle anticheat",
                args =>
                {
                    AnticheatPatches.AnticheatDisabled = !AnticheatPatches.AnticheatDisabled;
                    CUCoreUtils.ConsoleLog(ConsoleScript.instance, $"ANTICHEAT DISABLED: {AnticheatPatches.AnticheatDisabled}");
                },
                null
                );

            ConsoleCommandRegistry.Register(
                "reset_cheat_flag",
                "Clear cheat flag",
                args =>
                {
                    RunIntegrity.Reset();
                    CUCoreUtils.ConsoleLog(ConsoleScript.instance, "Cheating flag reset!");
                },
                null
                );

            ConsoleCommandRegistry.Register(
              "unlock_achievement",
              "Unlock achievement",
              args =>
              {
                  if (args.Length < 2) return;
                  if (string.IsNullOrEmpty(args[1]))
                      return;
                  if (AchievementRegistry.TryGetFull(args[1], out RegisteredAchievement ach))
                  {
                      if (ach.Definition.Type == AchievementType.Counter)
                          ach.ProgressData.Progress = ach.Definition.MaxProgress;
                      ach.Unlock();
                  }
                  else
                      CUCoreUtils.ConsoleLog(ConsoleScript.instance, $"Achievement with id {args[1]} not found!");
              },
              new Dictionary<int, List<string>>
              {
                  { 0, AchievementRegistry.GetAll().Keys.ToList() }
              },
              ("id", "Achievement ID")
              );

            ConsoleCommandRegistry.Register(
                "popup_achievement",
                "Force popup for achievement",
                args =>
                {
                    if (args.Length < 2) return;
                    if (string.IsNullOrEmpty(args[1]))
                        return;

                    if (AchievementRegistry.TryGetFull(args[1], out RegisteredAchievement ach))
                        AchievementPopupManager.Show(ach, ach.Definition?.Type == AchievementType.Counter);
                    else
                        CUCoreUtils.ConsoleLog(ConsoleScript.instance, $"Achievement with id {args[1]} not found!");
                },
                new Dictionary<int, List<string>>
                {
                    { 0, AchievementRegistry.GetAll().Keys.ToList() }
                },
                ("id", "Achievement ID")
                );
        }
#endif
    }
}
