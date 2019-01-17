using System;
using MemoQ.MTInterfaces;

namespace MtHubPlugin
{
    /// <summary>
    ///     Class for storing the MT-Hub plugin settings.
    /// </summary>
    public class MtHubOptions : PluginSettingsObject<MtHubGeneralSettings, MtHubSecureSetting>
    {
        private MtHubOptions()
        {
        }

        /// <summary>
        ///     Create instance by deserializing from provided serialized settings.
        /// </summary>
        public MtHubOptions(PluginSettings serializedSettings)
            : base(serializedSettings)
        {
        }

        /// <summary>
        ///     Create instance by providing the settings objects.
        /// </summary>
        public MtHubOptions(MtHubGeneralSettings hubGeneralSettings, MtHubSecureSetting hubSecureSettings)
            : base(hubGeneralSettings, hubSecureSettings)
        {
        }
    }

    /// <summary>
    ///     General settings, content preserved when settings are exported.
    /// </summary>
    public class MtHubGeneralSettings
    {
    }

    /// <summary>
    ///     Secure settings, content not preserved when settings leave the machine.
    /// </summary>
    public class MtHubSecureSetting
    {
        /// <summary>
        ///     The token used to be able to use the MT service.
        /// </summary>
        public string ApiToken = string.Empty;
    }
}