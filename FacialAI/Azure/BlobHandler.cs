using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace FacialAI.Azure
{
    class BlobHandler
    {
        static readonly string CONNECTION = ConfigurationManager.AppSettings.Get("BLOB_ENDPOINT");
        static readonly string CONTAINER = ConfigurationManager.AppSettings.Get("CONTAINER");
        public static bool UploadToStorage(String _file, string fileName)
        {
            CloudStorageAccount storageacc = CloudStorageAccount.Parse(CONNECTION);

            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(CONTAINER);
            container.CreateIfNotExists();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.Properties.ContentType = "image/jpg";
            using (var filestream = File.OpenRead(_file))
            {
                try
                {
                    blockBlob.UploadFromStream(filestream);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public static List<string> Get_files()
        {
            CloudStorageAccount storage = CloudStorageAccount.Parse(CONNECTION);
            CloudBlobClient blobClient = storage.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(CONTAINER);

            var list = container.ListBlobs(useFlatBlobListing: true);

            List<string> files = new List<string>();

            foreach (var blob in list)
            {
                var blobFileName = blob.Uri.Segments.Last();
                files.Add(blobFileName);
            }

            return files;


        }

        public static void DeleteItem(string fileName)
        {
            CloudStorageAccount storage = CloudStorageAccount.Parse(CONNECTION);
            CloudBlobClient blobClient = storage.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(CONTAINER);

            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();
        }
    }
}
