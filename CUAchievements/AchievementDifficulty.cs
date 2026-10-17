namespace CUAchievements
{
    /// <summary>
    /// Subjective assessment of how hard an achievement is.
    /// </summary>
    /// <remarks>
    /// The difficulty selects the default frame drawn around the achievement and is used to order
    /// achievements in the menu. A frame set through <see cref="AchievementDefinition.CustomFrame"/>
    /// overrides the default one.
    /// </remarks>
    public enum AchievementDifficulty
    {
        /// <summary>
        /// A regular achievement.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// A demanding achievement.
        /// </summary>
        Difficult = 1,
        /// <summary>
        /// The hardest achievements.
        /// </summary>
        Hard = 2,
    }
}
