using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STTCognitiveServicesTraitement.Models
{
    class Model
    {
    }

    public class SentimentKeyWords
    {
        public double? Score { get; set; }

        public string KeyWords { get; set; }
    }
    public class InterviewApiSource
    {
        public int InterviewID { get; set; }
        public int CampaignID { get; set; }
        public ApiSourceId SourceID { get; set; }
    }
    public class Media
    {
        public string Name { get; set; }

        public string Url { get; set; }
    }
    public class KeyWords
    {
        public string KeyWord { get; set; }
    }
    public class APIWAveNotProcessed
    {
        public ApiSourceId ApiSourceID { get; set; }
        public int QuestionOrder { get; set; }
        public int InterviewID { get; set; }
        public string MediaURL { get; set; }
        public string MediaShortName { get; set; }

        //Default Lang EN
        public string LanguageID { get; set; }
    }

    public class SpeechToText
    {
        public string Confidence { get; set; }
        public string Text { get; set; }
        public double? SentimentScore { get; set; }
        public int Order { get; set; }

        public string KeyWords { get; set; }
    }

    public enum MediaExtensions { MP4 = 1, MP3 = 2, JPG = 3, AVI = 4, WAV = 5 }

    public enum DBOperationType { Insert, Update }

    public enum TaskLabel { video, texte, smile, avsplit }

    public enum StatusId { NotYet = 1, OnGoing = 2, Finished = 3, Error = 4 }

    public enum ApiSourceId { Azure = 1, Google = 2 , Both = 3 }
}
