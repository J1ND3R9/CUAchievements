using CUCoreLib.Registries;

namespace CUAchievements
{
    /// <summary>
    /// A piece of text that is either a literal string or a reference to a localized string
    /// in <see cref="LocaleRegistry"/>, looked up lazily by <see cref="Resolve"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use it instead of calling <see cref="LocaleRegistry.Get(string, string)"/> yourself when you define achievements.
    /// Achievements are usually registered in <c>Awake</c>, when the language has not been loaded yet, so
    /// <see cref="LocaleRegistry.Get(string, string)"/> would return the fallback text and that text would stay in the
    /// definition forever. <see cref="LocalizedText"/> stores only the key and the fallback, and the lookup
    /// happens each time the text is displayed.
    /// </para>
    /// <para>
    /// A plain <see cref="string"/> converts implicitly and is never localized.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Localized: looked up in LocaleRegistry each time the text is shown.
    /// name: new LocalizedText("achievement_first_blood_name", "First blood"),
    ///
    /// // Literal: always shown as is.
    /// description: "Defeat your first enemy."
    /// </code>
    /// </example>
    public readonly struct LocalizedText
    {
        private readonly string key;
        private readonly string fallback;

        /// <summary>
        /// Creates a localized text that resolves <paramref name="key"/> through
        /// <see cref="LocaleRegistry"/>, or falls back to <paramref name="fallback"/>.
        /// </summary>
        /// <param name="key">
        /// Key in <see cref="LocaleRegistry"/>. Pass <see langword="null"/> to treat
        /// <paramref name="fallback"/> as a literal that is never localized.
        /// </param>
        /// <param name="fallback">
        /// Text returned when <paramref name="key"/> is <see langword="null"/> or is not found
        /// in the registry. May be <see langword="null"/>.
        /// </param>
        public LocalizedText(string key, string fallback)
        {
            this.key = key;
            this.fallback = fallback;
        }

        /// <summary>
        /// <see langword="true"/> if this instance carries no text at all: no key and no fallback.
        /// This is the case for <see langword="default"/>, which is how an omitted optional argument
        /// can be detected.
        /// </summary>
        public bool IsEmpty => key == null && fallback == null;

        /// <summary>
        /// Implicitly converts a literal string into a <see cref="LocalizedText"/> whose
        /// <see cref="Resolve"/> always returns that string. The string is
        /// <b>not</b> used as a locale key.
        /// </summary>
        /// <param name="literal">The text to show as is.</param>
        public static implicit operator LocalizedText(string literal) => new(null, literal);

        /// <summary>
        /// Resolves the text for the currently active locale. The result is not cached,
        /// so it follows language changes.
        /// </summary>
        /// <returns>
        /// The string registered under the key, or the fallback if the key is
        /// <see langword="null"/> or is not present in <see cref="LocaleRegistry"/>.
        /// </returns>
        public string Resolve() => key == null ? fallback : LocaleRegistry.Get(key, fallback);
    }
}
