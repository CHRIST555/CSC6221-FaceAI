using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialAI.Azure
{
    class BlobHandler
    {
        static string CONNECTION = "BlobEndpoint=https://6221faces.blob.core.windows.net/;QueueEndpoint=https://6221faces.queue.core.windows.net/;FileEndpoint=https://6221faces.file.core.windows.net/;TableEndpoint=https://6221faces.table.core.windows.net/;SharedAccessSignature=sv=2020-08-04&ss=bfqt&srt=sco&sp=rwdlacuptfx&se=2021-11-06T04:53:02Z&st=2021-10-11T20:53:02Z&spr=https&sig=HxT%2FmuUf9UuZVqqEHowyf34Q72fhZ5kS2XEN8%2Bx4%2B1Y%3D";
        static string CONTAINER = "faces";
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
                catch (Exception _)
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
