using System;
using System.Reflection;

namespace CUAchievements.UI
{
    internal static class UIText
    {
        public static readonly LocalizedText ModalSearchPlaceholder = new("achievements_modal_search_placeholder", "Search...");
        public static readonly LocalizedText ModalStatsButton = new("achievements_modal_stats_button", "Stats");

        public static readonly LocalizedText ListNoResultsTitle = new("achievements_list_no_results_title", "No achievements found!");
        public static readonly LocalizedText ListNoResultsDescription = new("achievements_list_no_results_desc", "We didn't find any achievements with those filters.");

        public static readonly LocalizedText ListNoAchievementsTitle = new("achievements_list_no_achievements_title", "No achievements :(");
        public static readonly LocalizedText ListNoAchievementsDescription = new("achievements_list_no_achievements_desc", "You don't have any mods installed that add achievements.\nFix this by installing, for example, {0}!");

        public static readonly LocalizedText CardRunScope = new("achievements_card_scope_run", "FOR RUN");

        public static readonly LocalizedText CardHiddenTitle = new("achievements_card_hidden_title", "Hidden achievements remaining: {0}");
        public static readonly LocalizedText CardHiddenDescription = new("achievements_card_hidden_desc", "Information about each achievement will be available after it is obtained");
        public static readonly LocalizedText CardHiddenRevealDefault = new("achievements_card_hidden_reveal_default", "Reveal");
        public static readonly LocalizedText CardHiddenRevealFirst = new("achievements_card_hidden_reveal_first", "Are you sure?");
        public static readonly LocalizedText CardHiddenRevealSecond = new("achievements_card_hidden_reveal_second", "Are you ABSOLUTELY sure?");
        public static readonly LocalizedText CardHiddenRevealThird = new("achievements_card_hidden_reveal_third", "This decision cannot be changed!");

        public static readonly LocalizedText StatsByMod = new("achievements_stats_by_mod", "BY MOD");
        public static readonly LocalizedText StatsDangerSection = new("achievements_stats_danger_section", "DANGER SECTION");
        public static readonly LocalizedText StatsResetButtonDefault = new("achievements_stats_reset_progress_default", "RESET PROGRESS");
        public static readonly LocalizedText StatsResetButtonFirst = new("achievements_stats_reset_progress_first", "Are you sure?");
        public static readonly LocalizedText StatsResetButtonSecond = new("achievements_stats_reset_progress_second", "Are you ABSOLUTELY sure?");
        public static readonly LocalizedText StatsResetButtonThird = new("achievements_stats_reset_progress_third", "This decision cannot be changed!");

        public static readonly LocalizedText DetailsLastProgress = new("achievements_details_last_progress", "LAST RUN PROGRESS:");
        public static readonly LocalizedText DetailsRunScope = new("achievements_details_scope_run", "MUST BE COMPLETED DURING A SINGLE RUN — PROGRESS RESETS ON DEATH");
        public static readonly LocalizedText DetailsLifetimeScope = new("achievements_details_scope_lifetime", "PROGRESS IS SAVED BETWEEN RUNS");

        public static readonly LocalizedText PauseButtonDefaultTipDescription = new("achievements_pause_button_default_tip_desc", "Show my achievements!");
        public static readonly LocalizedText PauseButtonTutorialTipDescription = new("achievements_pause_button_tutorial_tip_desc", "<color=\"orange\">ATTENTION!</color> Achievements will not be unlocked in the tutorial or sandbox!");
        public static readonly LocalizedText PauseButtonCheatedTipDescription = new("achievements_pause_button_cheated_tip_desc", "<color=\"orange\">ATTENTION!</color> Achievements will not be unlocked because this run is marked as \"cheated\"!");

        public static readonly LocalizedText Achieved = new("achievements_achieved", "ACHIEVED");
        public static readonly LocalizedText Earned = new("achievements_earned", "EARNED");
        public static readonly LocalizedText Total = new("achievements_total", "TOTAL");
        public static readonly LocalizedText Unlocked = new("achievements_unlocked", "UNLOCKED");
        public static readonly LocalizedText Locked = new("achievements_locked", "LOCKED");

        public static readonly LocalizedText Achievements = new("achievements", "Achievements");

        public static readonly LocalizedText GreetingsAchievementTitle = new("achievements_popup_greetings_title", "Thanks for using CUAchievements!");
        public static readonly LocalizedText GreetingsAchievementDescription = new("achievements_popup_greetings_desc", "oh my god! Hello!! Hi!!! hii!!! HEEELlOOO!!!");

        public static readonly LocalizedText SettingsSeconds = new("achievements_settings_sec", "s");

        public static readonly LocalizedText AlertUnlock = new("achievements_alert_unlocked", "Achievement \"{0}\" unlocked!");

        public static void Register(string ownerId)
        {
            Type type = typeof(UIText);

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(LocalizedText))
                {
                    LocalizedText localizedText = (LocalizedText)field.GetValue(null);

                    localizedText.Register(ownerId);
                }
            }
        }
    }
}
