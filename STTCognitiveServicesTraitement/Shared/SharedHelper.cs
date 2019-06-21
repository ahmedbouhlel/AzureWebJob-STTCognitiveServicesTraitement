using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Rest;
using STTCognitiveServicesTraitement.Models;
using STTCognitiveServicesTraitement.Utils.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STTCognitiveServicesTraitement.Shared
{
   public static class SharedHelper
    {

        public static void GetFullException(Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendLine(e.Message);
            var inner = e.InnerException;
            while (inner != null)
            {
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }
            Console.WriteLine(sb.ToString());
        }

        public static string CreateFileLocaly (Stream myBlob, string fileName,string filePath)
        {
            byte[] bytes = null;

            string tempFile = Path.Combine(filePath,fileName);

            // Determine whether the directory exists.
            if (Directory.Exists(filePath))
            {
                Console.WriteLine("That path exists already.");
            }
            // Try to create the directory.
            DirectoryInfo di = Directory.CreateDirectory(filePath);

            using (var ms = new MemoryStream())
            {
                myBlob.CopyTo(ms);
                bytes = ms.ToArray();
            }

            using (var myFile = File.Create(tempFile))
            {
                foreach (var b in bytes)
                {
                    myFile.WriteByte(b);
                }
            }

            return tempFile;
        }

        //Insert your Text Anaytics subscription key
        private static string SubscriptionKey =  WebConfiguration.TextAnalyticsKey;
        private class ApiKeyServiceClientCredentials : ServiceClientCredentials
        {
            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
        }
        public static async Task<SentimentKeyWords> BuildSentimentKeyWordsScoreAsync(string text, string language)
        {
            try
            {
                // Create a client.
                ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
                {
                    Endpoint = WebConfiguration.TextAnalyticsEndPoint,
                }; //Replace 'westus' with the correct region for your Text Analytics subscription


                SentimentBatchResult resultSentiment = await client.SentimentAsync(false,
                        new MultiLanguageBatchInput(
                            new List<MultiLanguageInput>()
                            {
                             new MultiLanguageInput(language, "0", text),
                            }));

                KeyPhraseBatchResult resultKeyWords = await client.KeyPhrasesAsync(false,
                        new MultiLanguageBatchInput(
                            new List<MultiLanguageInput>()
                            {
                          new MultiLanguageInput(language, "3", text),
                            }));

                var scoreResult = resultSentiment.Documents.FirstOrDefault();
                string keywordsResult = string.Empty;
                StringBuilder builder = new StringBuilder();
                var keyWords = resultKeyWords.Documents;

                foreach (var document in keyWords)
                {
                    foreach (string keyphrase in document.KeyPhrases)
                    {
                        builder.Append(keyphrase).Append(" | ");
                    }
                }

                var result = new SentimentKeyWords()
                {
                    Score = scoreResult != null ? scoreResult.Score : 0,
                    KeyWords = builder != null ? builder.ToString() : string.Empty
                };

                return result;
            }
            catch (Exception ex)
            {
                SharedHelper.GetFullException(ex);
                return new SentimentKeyWords();
            }
        }

    }
}
