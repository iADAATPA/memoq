using MemoQ.MTInterfaces;
using System;
using System.Drawing;
using System.Reflection;

namespace MtHubPlugin
{
    internal class MtHubEngine : EngineBase
    {
        /// <summary>
        ///     Plugin options
        /// </summary>
        private readonly MtHubOptions options;

        /// <summary>
        ///     The source language.
        /// </summary>
        private readonly string srcLangCode;

        /// <summary>
        ///     The target language.
        /// </summary>
        private readonly string trgLangCode;

        public MtHubEngine(string srcLangCode, string trgLangCode, MtHubOptions options)
        {
            this.srcLangCode = srcLangCode;
            this.trgLangCode = trgLangCode;
            this.options = options;
        }

        #region IDisposable Members

        public override void Dispose()
        {
            // dispose your resources if needed
        }

        #endregion

        #region IEngine Members

        /// <summary>
        ///     Creates a session for translating segments. Session will not be used in a multi-threaded way.
        /// </summary>
        public override ISession CreateLookupSession()
        {
            return new MtHubSession(srcLangCode, trgLangCode, options);
        }

        /// <summary>
        ///     Set an engine-specific custom property, e.g., subject matter area.
        /// </summary>
        public override void SetProperty(string name, string value)
        {
            // there are no properties to set
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns a small icon to be displayed under translation results when an MT hit is selected from this plugin.
        /// </summary>
        public override Image SmallIcon
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("MtHubPlugin.MTHub.png"));
            }
        }

        /// <summary>
        ///     Indicates whether the engine supports the adjustment of fuzzy TM hits through machine translation.
        /// </summary>
        public override bool SupportsFuzzyCorrection
        {
            get { return false; }
        }

        /// <summary>
        ///     Creates a session for translating segments. Session will not be used in a multi-threaded way.
        /// </summary>
        public override ISessionForStoringTranslations CreateStoreTranslationSession()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}