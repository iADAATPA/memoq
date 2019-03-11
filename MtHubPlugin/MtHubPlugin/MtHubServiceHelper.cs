using Kilgray.Utils;
using MemoQ.MTInterfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MtHubPlugin
{
    internal class MtHubServiceHelper
    {
        public const string BaseUrl = "https://app.mt-hub.eu/api/";

        private const int TimeOut = 120000;

        /// <summary>
        ///     The singleton instance of the MtHubServiceHelper helper.
        /// </summary>
        private static readonly MtHubServiceHelper _instance = new MtHubServiceHelper();

        /// <summary>
        ///     Private constructor to avoid multiple instances.
        /// </summary>
        private MtHubServiceHelper()
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
        }

        /// <summary>
        ///     The singleton instance of the localization helper.
        /// </summary>
        public static MtHubServiceHelper Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     Translates a single string with the help of the dummy MT service.
        /// </summary>
        /// <param name="options">The MT-Hub options.</param>
        /// <param name="input">The string to translate.</param>
        /// <param name="srcLangCode">The source language code.</param>
        /// <param name="trgLangCode">The target language code.</param>
        /// <returns>The translated string.</returns>
        public static string Translate(MtHubOptions options, string input, string srcLangCode, string trgLangCode)
        {
            var inputList = new List<string> { input };

            var translations = BatchTranslate(options, inputList, srcLangCode, trgLangCode);
            var translation = translations.First();

            return translation;
        }

        /// <summary>
        ///     Batch translates segments.
        /// </summary>
        /// <param name="options">The name of the MT engine or alias.</param>
        /// <param name="input">The segments that is to be translated.</param>
        /// <param name="srcLangCode">If an alias is being used, the source language code to use.</param>
        /// <param name="trgLangCode">If an alias is being used, the target language code to use.</param>
        /// <returns>The translations for the segments.</returns>
        public static List<string> BatchTranslate(MtHubOptions options, List<string> input,
            string srcLangCode = null, string trgLangCode = null)
        {
            var inputDictionary = new Dictionary<string, string>();
            for (var i = 0; i < input.Count; i++)
            {
                inputDictionary.Add(i.ToString(), HttpUtility.UrlEncode(HttpUtility.UrlEncode(input[i])));
            }

            const string sUrl = BaseUrl + "translate";
            var json = string.Empty;

            json = DoPostRequest(sUrl, inputDictionary, options, srcLangCode, trgLangCode);

            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                var test = reader.ToString();

                var translations = new List<string>();
                while (reader.Read())
                    if (reader.TokenType == JsonToken.PropertyName)
                        if (reader.Value.ToString() == "translation")
                        {
                            translations.Add(reader.ReadAsString());
                        }
                        else if (reader.Value.ToString() == "message")
                        {

                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                throw new Exception(reader.ReadAsString());
                            }

                            reader.Read();
                            if (reader.TokenType == JsonToken.StartObject)
                            {

                                JObject obj = JObject.Load(reader);

                                throw new Exception(obj.ToString());
                            }
                        }

                return translations;
            }
        }

        /// <summary>
        ///     Check if the language pair is supported by MT-Hub by checking if its an engine on the account of the api token
        ///     supplied
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool IsLanguagePairSupported(LanguagePairSupportedParams args)
        {
            var options = new MtHubOptions(args.PluginSettings);
            var apiToken = options.SecureSettings.ApiToken;
            var src = LanguageData.GetIsoCode2LetterFromIsoCode3Letter(args.SourceLangCode);
            var trg = LanguageData.GetIsoCode2LetterFromIsoCode3Letter(args.TargetLangCode);

            var url = BaseUrl + "describesuppliers/" + apiToken;
            var json = DoGetRequest(url);


            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                while (reader.Read())
                    if (reader.TokenType == JsonToken.PropertyName)
                        if (reader.Value.ToString() == "engines")
                            while (reader.Read())
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    var engine = JObject.Load(reader);

                                    if (engine["source"].ToString().Equals(src) && engine["target"].ToString().Equals(trg))
                                    {
                                        return true;
                                    }
                                }
            }

            return false;
        }

        /// <summary>
        ///     Sends a get request using the specified url.
        /// </summary>
        /// <param name="url">The URL get request.</param>
        /// <returns>The response from the get request.</returns>
        private static string DoGetRequest(string url)
        {
            var request = WebRequest.Create(url);
            request.Timeout = TimeOut;

            using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                var json = string.Empty;
                var line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                    json += line;

                return json;
            }
        }

        /// <summary>
        ///     Sends string(s) in a post request to the specified engine.
        /// </summary>
        /// <param name="url">The url to send the post request to.</param>
        /// <param name="input">The string(s) being sent in the post request.</param>
        /// <param name="options">The MT-Hub options.</param>
        /// <param name="srcLangCode">If an alias is being used, the source language code to use.</param>
        /// <param name="trgLangCode">If an alias is being used, the target language code to use.</param>
        /// <returns>The response from the post request.</returns>
        private static string DoPostRequest(string url, Dictionary<string, string> input, MtHubOptions options,
            string srcLangCode, string trgLangCode)
        {
            var apiToken = options.SecureSettings.ApiToken;

            var request = WebRequest.Create(url);
            request.Timeout = TimeOut;
            request.Method = "POST";
            var postData = "token=" + apiToken;

            srcLangCode = LanguageData.GetIsoCode2LetterFromIsoCode3Letter(srcLangCode);
            trgLangCode = LanguageData.GetIsoCode2LetterFromIsoCode3Letter(trgLangCode);
            postData += "&source=" + srcLangCode + "&target=" + trgLangCode;

            // if the input is not empty add the parameters (count())
            if (input != null)
                foreach (var parameterKeyValuePair in input)
                {
                    var parameterValue = parameterKeyValuePair.Value.Replace("&", "%2526");
                    postData += "&segments[" + parameterKeyValuePair.Key + "]=" + parameterValue;
                }

            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream.
            using (var dataStream = request.GetRequestStream())
            {
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {
                using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    var json = string.Empty;
                    var line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                        json += line;

                    return json;
                }
            }
            catch (WebException ex)
            {
                string exMessage = ex.Message;

                if (ex.Response != null)
                {
                    using (var responseReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        exMessage = responseReader.ReadToEnd();

                    }
                }

                return exMessage;
            }
        }
    }
}