using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MemoQ.Addins.Common.DataStructures;
using MemoQ.MTInterfaces;

namespace MtHubPlugin
{
    /// <summary>
    ///     Session that perform actual translation. Created on a segment-by-segment basis, or once for batch operations.
    /// </summary>
    public class MtHubSession : ISession
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

        public MtHubSession(string srcLangCode, string trgLangCode, MtHubOptions options)
        {
            this.srcLangCode = srcLangCode;
            this.trgLangCode = trgLangCode;
            this.options = options;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // dispose your resources if needed
        }

        #endregion

        #region ISession Members

        /// <summary>
        ///     Translates a single segment, possibly using a fuzzy TM hit for improvement
        /// </summary>
        public TranslationResult TranslateCorrectSegment(Segment segm, Segment tmSource, Segment tmTarget)
        {
            // Call the TranslateCorrectSegment() method for multiple segments and return the first TranslationResult [0]
            return TranslateCorrectSegment(new[] {segm}, new[] {tmSource}, new[] {tmTarget})[0];
        }

        /// <summary>
        ///     Translates multiple segments, possibly using a fuzzy TM hit for improvement
        /// </summary>
        public TranslationResult[] TranslateCorrectSegment(Segment[] segs, Segment[] tmSources, Segment[] tmTargets)
        {
            var results = new TranslationResult[segs.Length];

            try
            {
                // The first key is the index of the segment, the second
                // key is the index of the tag in the source segment.
                // The value is null if the tag is a structural tag and
                // an InlineTag object if the current tag is an inline tag.
                var tags = new Dictionary<int, Dictionary<int, InlineTag>>();
                var segmentContents = new List<string>();
                for (var i = 0; i < segs.Length; i++)
                {
                    tags[i] = new Dictionary<int, InlineTag>();
                    var sb = new StringBuilder();
                    for (var j = 0; j < segs[i].Length; j++)
                    {
                        var ch = segs[i][j];
                        if (ch.IsStructuralTag)
                        {
                            // the current character is a structural tag
                            sb.Append(string.Format("<{0}>", tags[i].Count));
                            tags[i].Add(tags[i].Count, null);
                        }
                        else if (ch.IsInlineTag)
                        {
                            // the current character is an inline tag
                            sb.Append(string.Format("<{0}>", tags[i].Count));
                            tags[i].Add(tags[i].Count, segs[i].GetInlineTagAtPos(j));
                        }
                        else
                        {
                            // the current character is a regular character
                            sb.Append(ch.CharValue);
                        }
                    }

                    segmentContents.Add(sb.ToString());
                }

                List<string> translations;

                translations = MtHubServiceHelper.BatchTranslate(options, segmentContents, srcLangCode, trgLangCode);

                var placeholderRegex = new Regex(@"<(\d+)>");
                for (var i = 0; i < translations.Count; i++)
                {
                    // build the translated segment
                    var segmentBuilder = new SegmentBuilder();
                    if (placeholderRegex.IsMatch(translations[i]))
                    {
                        // the translated segment contains tag placeholders
                        var index = 0;
                        foreach (Match match in placeholderRegex.Matches(translations[i]))
                        {
                            if (match.Index > index)
                                segmentBuilder.AppendString(translations[i].Substring(index, match.Index - index));

                            var phId = int.Parse(match.Groups[1].Value);
                            if (tags[i][phId] == null)
                                segmentBuilder.AppendStructuralTag();
                            else
                                segmentBuilder.AppendInlineTag(tags[i][phId]);

                            index = match.Index + match.Length;
                        }

                        if (index < translations[i].Length)
                            segmentBuilder.AppendString(translations[i].Substring(index));
                    }
                    else
                    {
                        // the translated segment does not contain tag placeholders
                        segmentBuilder.AppendString(translations[i]);
                    }

                    results[i] = new TranslationResult();
                    results[i].Translation = segmentBuilder.ToSegment();
                }
            }
            catch (Exception e)
            {
                for (var i = 0; i < results.Length; i++)
                {
                    string localizedMessage;
                    string englishMessage;
                    if (e is WebException)
                    {
                        localizedMessage = string.Format(LocalizationHelper.Instance.GetResourceString("NetworkError"),
                            e.Message);
                        englishMessage = string.Format("A network error occurred ({0})", e.Message);
                    }
                    else
                    {
                        localizedMessage = string.Format(LocalizationHelper.Instance.GetResourceString("GenericError"),
                            e.Message);
                        englishMessage = string.Format("An error occurred ({0})", e.Message);
                    }

                    var result = results[i];

                    if (result == null)
                        result = new TranslationResult();

                    result.Exception = new MTException(localizedMessage, englishMessage, e);
                    results[i] = result;
                }
            }

            return results;
        }

        #endregion

    }
}