using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Grpc.Auth;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using STTCognitiveServicesTraitement.Shared;
using STTCognitiveServicesTraitement.Utils.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace STTCognitiveServicesTraitement.Google_Helper
{
   public static class SpeechRecognitionGoogle
    {
        public static LongRunningRecognizeResponse stt_google(string mediaShortName, string languageId)
        {
            try
            {
                GoogleCredential googleCredential;
                var req = WebRequest.Create(WebConfiguration.googlecredentialsfile);
                using (Stream m = req.GetResponse().GetResponseStream())
                    googleCredential = GoogleCredential.FromStream(m);

                var channel = new Grpc.Core.Channel(SpeechClient.DefaultEndpoint.Host, googleCredential.ToChannelCredentials());
                var speech = SpeechClient.Create(channel);
                Console.WriteLine("***GoogleRecognizeSST SpeechClient init ");
                //Init Google Object

                string bucketName = WebConfiguration.bucketName;
                var longOperation = speech.LongRunningRecognize(new RecognitionConfig()
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 48000,
                    LanguageCode = String.IsNullOrEmpty(languageId) ? System.Configuration.ConfigurationManager.AppSettings["LanguageCode"] : languageId,

                }, RecognitionAudio.FromStorageUri("gs://" + bucketName + "/" + mediaShortName));

                Console.WriteLine("***GoogleRecognizeSST processing");
                longOperation = longOperation.PollUntilCompleted();

                return longOperation.Result;
            }
            catch(Exception ex)
            {
                SharedHelper.GetFullException(ex);
                return null;
            }

        }



    }
}
