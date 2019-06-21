using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using STTCognitiveServicesTraitement.Models;
using STTCognitiveServicesTraitement.Utils.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STTCognitiveServicesTraitement.Connectors
{
    public class AzureConnector
    {
        private string storageConnectionString = string.Empty;
        private CloudStorageAccount cloudStorageAccount = null;

        public AzureConnector()
        {
            storageConnectionString = WebConfiguration.BlobStorageConnectionString;
        }

        public Stream GetMediaBlob(string mediaUrl)
        {
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlob = cloudBlobClient.GetBlobReferenceFromServer(new Uri(mediaUrl));


            var cloudBlobExists = cloudBlob.Exists();

            if (!cloudBlobExists)
                return null;
            Stream blobStream = cloudBlob.OpenRead();

            return blobStream;
        }

        public Media StoreBlob(string path, string name)
        {
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("media");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);

            using (var fileStream = System.IO.File.OpenRead(path))
            {
                blockBlob.UploadFromStream(fileStream);
                Media media = new Media()
                {
                    Name = String.Empty,
                    Url = blockBlob.Uri.ToString()
                };
                return media;
            }
        }

        public string ChangeExtension(string mediaName, string newExtension)
        {
            if (!mediaName.Contains(".mp4"))
            {
                mediaName = mediaName + ".mp4";
            }

            String[] array = mediaName.Split('.');
            return mediaName.Replace(array[array.Length - 1], newExtension).ToString();
        }
    }

}
