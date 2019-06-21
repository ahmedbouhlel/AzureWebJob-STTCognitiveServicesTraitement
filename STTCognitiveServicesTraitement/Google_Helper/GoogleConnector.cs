using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
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
    public class GoogleConnector
    {

        public bool DeleteObjects(IEnumerable<string> objectNames)
        {
            try
            {
                string bucketName = WebConfiguration.bucketName;

                GoogleCredential googleCredential;
                var req = WebRequest.Create(WebConfiguration.googlecredentialsfile);
                using (Stream m = req.GetResponse().GetResponseStream())
                    googleCredential = GoogleCredential.FromStream(m);

                var storage = StorageClient.Create(googleCredential);
                foreach (string objectName in objectNames)
                {
                    storage.DeleteObject(bucketName, objectName);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteObject(string objectName)
        {
            try
            {
                string bucketName = WebConfiguration.bucketName;

                GoogleCredential googleCredential;
                var req = WebRequest.Create(WebConfiguration.googlecredentialsfile);
                using (Stream m = req.GetResponse().GetResponseStream())
                    googleCredential = GoogleCredential.FromStream(m).CreateScoped(new string[] { StorageService.Scope.DevstorageReadWrite });

                var storage = StorageClient.Create(googleCredential);
                storage.DeleteObject(bucketName, objectName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
