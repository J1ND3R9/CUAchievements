using CUAchievements.Registry;
using CUAchievements.Services;
using System.Collections.Generic;
using System.Linq;

namespace CUAchievements
{
    /// <summary>
    /// The entry point of the CUAchievements API for plugins. Each instance is bound to the GUID
    /// of one plugin and works with that plugin's achievements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Create one instance with <see cref="Create(string)"/> (for example in <c>Awake</c>), keep it in a field and
    /// use it to register achievements and to change their progress.
    /// </para>
    /// <para>
    /// Not thread-safe: use it from the Unity main thread only.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// private AchievementsAPI achievements;
    ///
    /// private void Awake()
    /// {
    ///     achievements = AchievementsAPI.Create(MyPluginInfo.PLUGIN_GUID);
    ///
    ///     achievements.Register(AchievementDefinition.Instant(
    ///         id: "first_blood",
    ///         name: new LocalizedText("achievement_first_blood_name", "First blood"),
    ///         description: "Defeat your first enemy.",
    ///         icon: firstBloodIcon));
    /// }
    ///
    /// private void OnEnemyKilled()
    /// {
    ///     achievements.Unlock("first_blood");
    /// }
    /// </code>
    /// </example>
    public sealed class AchievementsAPI
    {
        private AchievementRegistry Registry { get; }
        private AchievementService Service { get; }
        private string PluginGUID { get; }

        private AchievementsAPI(string pluginGuid)
        {
            Registry = AchievementRegistry.Create(pluginGuid);
            Service = AchievementService.Create(pluginGuid);
            PluginGUID = pluginGuid;
        }

        /// <summary>
        /// Creates an <see cref="AchievementsAPI"/> bound to your plugin.
        /// </summary>
        /// <param name="pluginGuid">
        /// The GUID of your plugin. Use your actual GUID and do not change it after release:
        /// the player's saved progress is stored under it.
        /// </param>
        /// <returns>
        /// A new <see cref="AchievementsAPI"/>, or <see langword="null"/> if <paramref name="pluginGuid"/>
        /// is <see langword="null"/> or empty.
        /// </returns>
        public static AchievementsAPI Create(string pluginGuid)
        {
            if (string.IsNullOrEmpty(pluginGuid)) return null;
            return new AchievementsAPI(pluginGuid);
        }

        /// <summary>
        /// Registers an achievement. Saved progress of the player, if there is any, is restored.
        /// </summary>
        /// <param name="achievement">The definition to register. See <see cref="AchievementDefinition"/>.</param>
        /// <returns>
        /// The <see cref="RegisteredAchievement"/> if the registration succeeded; otherwise
        /// <see langword="null"/> (the definition is invalid or an achievement with the same ID is already
        /// registered). The reason is written to the log.
        /// </returns>
        /// <remarks>
        /// Achievements can be registered at any time and the menu updates automatically. Registering in
        /// <c>Awake</c> is recommended so that the list of achievements does not change while the player is using it.
        /// A duplicate ID is ignored with a warning. Texts that depend on the language should be passed as
        /// <see cref="LocalizedText"/>.
        /// </remarks>
        public RegisteredAchievement Register(AchievementDefinition achievement) => Registry.Register(achievement);

        /// <summary>
        /// Tries to get one of your achievements by its ID.
        /// </summary>
        /// <param name="id">The <see cref="AchievementDefinition.Id"/> you registered the achievement with.</param>
        /// <param name="achievement">The achievement if it is registered; otherwise <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the achievement is registered; otherwise <see langword="false"/>.
        /// </returns>
        public bool TryGet(string id, out RegisteredAchievement achievement) => Registry.TryGet(id, out achievement);

        /// <summary>
        /// Adds progress to one of your achievements.
        /// </summary>
        /// <param name="id">The <see cref="AchievementDefinition.Id"/> of the achievement.</param>
        /// <param name="amount">Amount of progress to add. The default is 1.</param>
        /// <returns>
        /// <see langword="true"/> if progress was added; <see langword="false"/> if there is no such achievement
        /// or the call was a no-op (see <see cref="RegisteredAchievement.AddProgress(float)"/>).
        /// </returns>
        public bool AddProgress(string id, float amount = 1) => Service.AddProgress(id, amount);

        /// <summary>
        /// Unlocks one of your achievements.
        /// </summary>
        /// <param name="id">The <see cref="AchievementDefinition.Id"/> of the achievement.</param>
        /// <returns>
        /// <see langword="true"/> if the achievement became unlocked because of this call;
        /// <see langword="false"/> if there is no such achievement or it was already unlocked
        /// (see <see cref="RegisteredAchievement.Unlock"/>).
        /// </returns>
        public bool Unlock(string id) => Service.Unlock(id);

        /// <summary>
        /// Gets all achievements registered through this instance.
        /// </summary>
        /// <returns>
        /// A snapshot list taken at the time of the call. The order is not defined, and achievements
        /// registered later are not added to the returned list.
        /// </returns>
        public IReadOnlyList<RegisteredAchievement> GetMyAchievements()
        {
            return AchievementRegistry.GetAll()
                .Where(kv => kv.Value.PluginGuid == PluginGUID)
                .Select(kv => kv.Value)
                .ToList();
        }
    }
}
