using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialAI.Azure
{
    class FaceModels
    {
        // The client for connecting to azure
        private static IFaceClient client;
        // Recognition model
        private const string RECOGNITION_MODEL4 = RecognitionModel.Recognition04;

        // Link to test images
        const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";

        public FaceModels()
        {
            client = AzureConnection.Authenticate();
        }

        public async Task DetectFaceExtract()
        {
            Console.WriteLine("========DETECT FACES========\n");

            List<string> imageFileNames = new List<string>
                    {
                        "detection1.jpg",    // single female with glasses
                        // "detection2.jpg", // (optional: single man)
                        // "detection3.jpg", // (optional: single male construction worker)
                        // "detection4.jpg", // (optional: 3 people at cafe, 1 is blurred)
                        "detection5.jpg",    // family, woman child man
                        "detection6.jpg"     // elderly couple, male female
                    };

            foreach (var imageFileName in imageFileNames)
            {
                IList<DetectedFace> detectedFaces;

                // Detect faces with all attributes from image url.
                detectedFaces = await client.Face.DetectWithUrlAsync($"{IMAGE_BASE_URL}{imageFileName}",
                        returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
                FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
                FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile },
                        // We specify detection model 1 because we are retrieving attributes.
                        detectionModel: DetectionModel.Detection01,
                        recognitionModel: RECOGNITION_MODEL4);

                Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{imageFileName}`.");
            }

            Console.WriteLine("========TEST Complete========\n");
        }
    }
}
