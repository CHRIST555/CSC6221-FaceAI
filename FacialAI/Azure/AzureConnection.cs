using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FacialAI.Azure
{
    class AzureConnection
    {
        private const string SUBSCRIPTION_KEY = "87f845c1b6f845c383164289c8ae42fb";
        private const string ENDPOINT = "https://csci6221.cognitiveservices.azure.com/";

        public static IFaceClient Authenticate()
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(SUBSCRIPTION_KEY)) { Endpoint = ENDPOINT };
        }
    }
}
