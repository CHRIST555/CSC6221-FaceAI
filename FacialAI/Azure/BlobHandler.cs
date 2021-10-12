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
        static string NAME = "6221faces";
        static string KEY = "PdpA+IDe5XkRQ/1HYx8CtaPtbMUa+JkydAbrJbv8eKosVuouW6YFARct+QzyhpobHaCjhFzA8RtCA+fyi8tJfw==";
        static string CONNECTION = "BlobEndpoint=https://6221faces.blob.core.windows.net/;QueueEndpoint=https://6221faces.queue.core.windows.net/;FileEndpoint=https://6221faces.file.core.windows.net/;TableEndpoint=https://6221faces.table.core.windows.net/;SharedAccessSignature=sv=2020-08-04&ss=bfqt&srt=sco&sp=rwdlacuptfx&se=2021-11-06T04:53:02Z&st=2021-10-11T20:53:02Z&spr=https&sig=HxT%2FmuUf9UuZVqqEHowyf34Q72fhZ5kS2XEN8%2Bx4%2B1Y%3D";
        static string CONTAINER = "faces";
        public static async Task UploadToStorage(String _file, string fileName)
        {
            CloudStorageAccount storageacc = CloudStorageAccount.Parse(CONNECTION);

            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(CONTAINER);
            container.CreateIfNotExists();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.Properties.ContentType = "image/jpg";
            using (var filestream = File.OpenRead(_file))
            {
                blockBlob.UploadFromStream(filestream);
            }
        }

        public static List<string> get_files()
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
    }
}
