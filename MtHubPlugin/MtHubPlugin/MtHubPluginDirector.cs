using MemoQ.Addins.Common.Framework;
using MemoQ.MTInterfaces;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MtHubPlugin
{
    public class MtHubPluginDirector : PluginDirectorBase, IModule
    {
        /// <summary>
        ///     The identifier of the plugin.
        /// </summary>
        public const string PluginId = "MtHub";

        /// <summary>
        ///     The memoQ's application environment; e.g., to provide UI language settings etc. to the plugin.
        /// </summary>
        private IEnvironment environment;

        public MtHubPluginDirector()
        {
        }

        #region IModule Members

        public void Cleanup()
        {
            // write your cleanup code here
        }

        public void Initialize(IModuleEnvironment env)
        {
            // write your initialization code here
        }

        public bool IsActivated
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region IPluginDirector Members

        /// <summary>
        ///     Indicates whether interactive lookup (in the translation grid) is supported or not.
        /// </summary>
        public override bool InteractiveSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Indicates whether batch lookup is supported or not.
        /// </summary>
        public override bool BatchSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Indicates whether storing translations is supported.
        /// </summary>
        public override bool StoringTranslationSupported
        {
            get { return false; }
        }

        /// <summary>
        ///     The plugin's non-localized name.
        /// </summary>
        public override string PluginID
        {
            get { return PluginId; }
        }

        /// <summary>
        ///     Returns the friendly name to show in memoQ's Tools / Options.
        /// </summary>
        public override string FriendlyName
        {
            get { return "MT-Hub Plugin"; }
        }

        /// <summary>
        ///     Return the copyright text to show in memoQ's Tools / Options.
        /// </summary>
        public override string CopyrightText
        {
            get { return DateTime.Now.Year.ToString(); }
        }

        /// <summary>
        ///     Return a 48x48 display icon to show in MemoQ's Tools / Options. Black is the transparent color.
        /// </summary>
        public override Image DisplayIcon
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("MtHubPlugin.MTHub.png"));
            }
        }

        /// <summary>
        ///     The memoQ's application environment; e.g., to provide UI language settings etc. to the plugin.
        /// </summary>
        public override IEnvironment Environment
        {
            set
            {
                environment = value;

                // initialize the localization helper
                LocalizationHelper.Instance.SetEnvironment(value);
            }
        }

        /// <summary>
        ///     Tells memoQ if the plugin supports the provided language combination. The strings provided are memoQ language
        ///     codes.
        /// </summary>
        public override bool IsLanguagePairSupported(LanguagePairSupportedParams args)
        {

            return MtHubServiceHelper.IsLanguagePairSupported(args);
        }

        /// <summary>
        ///     Creates an MT engine for the supplied language pair.
        /// </summary>
        public override IEngine2 CreateEngine(CreateEngineParams args)
        {

            return new MtHubEngine(args.SourceLangCode, args.TargetLangCode,
                new MtHubOptions(args.PluginSettings));
        }

        /// <summary>
        ///     Shows the plugin's options form.
        /// </summary>
        public override PluginSettings EditOptions(IWin32Window parentForm, PluginSettings settings)
        {

            using (var form = new MtHubOptionsForm { Options = new MtHubOptions(settings) })
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    environment.PluginAvailabilityChanged();
                return form.Options.GetSerializedSettings();
            }
        }

        #endregion
    }
}