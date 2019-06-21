using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STTCognitiveServicesTraitement.Utils.Configuration
{
    public static class WebConfiguration
    {
        public static string DbConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["DbConnectionString"].ToString();
            }
        }

        public static string BlobStorageConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["BlobStorageConnectionString"].ToString();
            }
        }

        public static string googlecredentialsfile
        {
            get
            {
                return ConfigurationManager.AppSettings["googlecredentialsfile"].ToString();
            }
        }

        public static string bucketName
        {
            get
            {
                return ConfigurationManager.AppSettings["bucketName"].ToString();
            }
        }

        public static string TextAnalyticsKey
        {
            get
            {
                return ConfigurationManager.AppSettings["TextAnalyticsKey"].ToString();
            }
        }

        public static string TextAnalyticsEndPoint
        {
            get
            {
                return ConfigurationManager.AppSettings["TextAnalyticsEndPoint"].ToString();
            }
        }

        public static string SpeechRecognizerKey
        {
            get
            {
                return ConfigurationManager.AppSettings["SpeechRecognizerKey"].ToString();
            }
        }

        public static string SpeechRecognizerRegion
        {
            get
            {
                return ConfigurationManager.AppSettings["SpeechRecognizerRegion"].ToString();
            }
        }
        

    }

}
