using System.Collections.Generic;
using MemoQ.MTInterfaces;

namespace MtHubPlugin
{
    /// <summary>
    ///     Singleton helper class to be able to localize the plugin's textual information.
    /// </summary>
    internal class LocalizationHelper
    {
        /// <summary>
        ///     The singleton instance of the localization helper.
        /// </summary>
        private static readonly LocalizationHelper instance = new LocalizationHelper();

        /// <summary>
        ///     The default text to be used when the IEnvironment.GetResourceString returns with null.
        /// </summary>
        private readonly Dictionary<string, string> defaultTexts = new Dictionary<string, string>
        {
            {"ConnectorName", "MT-Hub Plugin"},
            {"OptionsForm.Text", "MT-Hub Plugin Settings"},
            {"OptionsForm.ApiTokenLabel", "API Token"},
            {"OptionsForm.OKButton", "OK"},
            {"MissingTokenErrorText", "Missing Token"},
            {"CommunicationErrorText", "There was an error during the communication with the service.\n\n{0}"},
            {"NetworkError", "A network error occurred ({0})"},
            {"GenericError", "An error occurred ({0})"}
        };

        /// <summary>
        ///     The environment to be used to get localized texts from memoQ.
        /// </summary>
        private IEnvironment environment;

        /// <summary>
        ///     Private constructor to avoid multiple instances.
        /// </summary>
        private LocalizationHelper()
        {
        }

        /// <summary>
        ///     The singleton instance of the localization helper.
        /// </summary>
        public static LocalizationHelper Instance
        {
            get { return instance; }
        }

        /// <summary>
        ///     Sets the environment to be able to get localized texts.
        /// </summary>
        /// <param name="environment"></param>
        public void SetEnvironment(IEnvironment environment)
        {
            this.environment = environment;
        }

        /// <summary>
        ///     Gets the localized text belonging to the specified key.
        /// </summary>
        public string GetResourceString(string key)
        {
            // try to get the localized text from the environment
            var localizedText = environment.GetResourceString(MtHubPluginDirector.PluginId, key);

            // use the default texts if the environment returns with null
            if (string.IsNullOrEmpty(localizedText))
                localizedText = defaultTexts[key];

            return localizedText;
        }
    }
}