using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Rest;
using STTCognitiveServicesTraitement.Connectors;
using STTCognitiveServicesTraitement.Connectors.DB;
using STTCognitiveServicesTraitement.Google_Helper;
using STTCognitiveServicesTraitement.Microsoft_Helper;
using STTCognitiveServicesTraitement.Models;
using STTCognitiveServicesTraitement.Shared;

namespace STTCognitiveServicesTraitement
{
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            try
            {
                Console.WriteLine("*** STT stating ***");
                DBConnector dbConnector = new DBConnector();
                GoogleConnector googleConnector = new GoogleConnector();

                string AZURE_TEMP_FOLDER_PATH = ConfigurationManager.AppSettings["AZURE_TEMP_FOLDER_PATH"];
                var groups = dbConnector.AllBlobNotProcessed((int)MediaExtensions.WAV, ApiSourceId.Both).GroupBy(i => i.InterviewID).ToList();
                Console.WriteLine("Not processed wavs groups count : " + groups.Count());
                if (groups.Count() > 0)
                {
                    AzureConnector azureConnector = new AzureConnector();
                    foreach (var group in groups)
                    {
                        if (group.Count() > 0)
                        {
                            var tempPath = String.Empty;
                            foreach (var item in group)
                            {
                                Console.WriteLine("-----------------------------------------------------");
                                Console.WriteLine(item.MediaURL);
                                List<Microsoft.CognitiveServices.Speech.SpeechRecognitionResult> recognizedItems = new List<Microsoft.CognitiveServices.Speech.SpeechRecognitionResult>();

                                var tempFolderName = Guid.NewGuid().ToString();
                                 tempPath = System.IO.Path.Combine(AZURE_TEMP_FOLDER_PATH, item.InterviewID.ToString());

                                Console.WriteLine("Interviw LanguageID :" + item.LanguageID);
                                Console.WriteLine("InterviwID : " + item.InterviewID);
                                Console.WriteLine("MediaShortName : ", item.MediaShortName);

                                var apisource = dbConnector.GetInterviewApiSource(item.InterviewID, item.LanguageID);
                                if (apisource != null)
                                {
                                    List<SpeechToText> list = new List<SpeechToText>();
                                    if (apisource.SourceID.Equals(ApiSourceId.Azure))
                                    {
                                        if(item.ApiSourceID == ApiSourceId.Azure)
                                        {
                                            #region STT Microsoft
                                            //Only select Question Order
                                            var questionOrder = item.MediaShortName.Substring(item.MediaShortName.Length - 5, 1);

                                            Stream stream = azureConnector.GetMediaBlob(item.MediaURL);

                                            var temp_audio_file = SharedHelper.CreateFileLocaly(stream, tempFolderName, tempPath);

                                            SpeechRecognitionMS.ContinuousRecognitionWithFileAsync(temp_audio_file, item.LanguageID, recognizedItems).Wait();

                                            foreach (var recognitionResult in recognizedItems)
                                            {
                                                var text = recognitionResult.Text;
                                                var result = SharedHelper.BuildSentimentKeyWordsScoreAsync(text, item.LanguageID);
                                                var sTT = new SpeechToText()
                                                {
                                                    Confidence = "Microsoft",
                                                    Text = text,
                                                    SentimentScore = result.Result.Score,
                                                    KeyWords = result.Result.KeyWords,
                                                };
                                                list.Add(sTT);
                                            }

                                            var jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(list);

                                            int quest = 0;
                                            if (int.TryParse(questionOrder, out quest))
                                            {
                                                dbConnector.AddSpeechToText(item.InterviewID, quest, jsonResult);
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        if(item.ApiSourceID == ApiSourceId.Google)
                                        {
                                            #region STT Google
                                            //STT Google to be tested , then delete the old web Job
                                            //Only select Question Order

                                            var questionOrder = item.MediaShortName.Substring(item.MediaShortName.Length - 5, 1);
                                            Console.WriteLine("QuestionOrder : " + questionOrder);
                                            Console.WriteLine("Media Url : " + item.MediaURL);

                                            var response = SpeechRecognitionGoogle.stt_google(item.MediaShortName, item.LanguageID);
                                            if (response != null)
                                            {
                                                foreach (var result in response.Results)
                                                {
                                                    foreach (var alternative in result.Alternatives)
                                                    {
                                                        var text = alternative.Transcript;
                                                        var result_ms = SharedHelper.BuildSentimentKeyWordsScoreAsync(text, String.IsNullOrEmpty(item.LanguageID) ? System.Configuration.ConfigurationManager.AppSettings["LanguageCode"] : item.LanguageID);
                                                        Console.WriteLine($"currrent google speech to text : {alternative.Transcript}");
                                                        var sTT = new SpeechToText()
                                                        {
                                                            Confidence = "google",
                                                            Text = text,
                                                            SentimentScore = result_ms.Result.Score,
                                                            Order = int.Parse(questionOrder),
                                                            KeyWords = result_ms.Result.KeyWords,
                                                        };
                                                        list.Add(sTT);
                                                    }
                                                }
                                            }

                                            var jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                                            Console.WriteLine($"Json google speech: {jsonResult}");
                                            dbConnector.AddSpeechToText(item.InterviewID, int.Parse(questionOrder), jsonResult, (int)ApiSourceId.Google);
                                            #endregion
                                        }
                                    }

                                }

                                dbConnector.UpdateInterviewMediaStatus(item.MediaURL, 1);
                                //TODO::Delete  wavs from google storage at the end of the process
                                googleConnector.DeleteObject(item.MediaShortName);

                                var _result = dbConnector.AllBlobProcessed(item.InterviewID, (int)MediaExtensions.WAV);
                                if (_result == "TRUE")
                                {
                                  dbConnector.InsertOrUpdateAVTaskTracker(DBOperationType.Update.ToString(),"", item.InterviewID, TaskLabel.texte.ToString(), (int)StatusId.Finished, 0);
                                }
                            }
                            DeleteTempFiles_If_All_Files_Are_treated(dbConnector, ApiSourceId.Azure, group.FirstOrDefault().InterviewID, tempPath);

                        }
                    }
                }
            }
            catch (Exception e)
            {
               SharedHelper.GetFullException(e);
            }
            Console.WriteLine("***GoogleRecognizeSST Finished ***");
        }

        private static void DeleteTempFiles_If_All_Files_Are_treated(DBConnector dBConnector , ApiSourceId apiSourceId , int interviewId , string tempPath)
        {
            try
            {
                // TODO : Check if all Azure wave  blobs have been treated
                var wavs = dBConnector.AllBlobNotProcessedByInterviewID((int)MediaExtensions.WAV, apiSourceId, interviewId);

                Console.WriteLine($"Check if all wave Azure blobs have been treated - count :  wavs.Count");

                if (wavs.Count == 0)
                {
                    Console.WriteLine($"Check if Directory Existe :{Directory.Exists(tempPath)}");

                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }
                }
            }
            catch(Exception ex)
            {
                SharedHelper.GetFullException(ex);
            }
        }
    }
}
