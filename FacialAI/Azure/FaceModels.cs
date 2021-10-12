using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

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
        const string FACE_URL = "https://6221faces.blob.core.windows.net/faces/";
        string PATH_TO_TEMP = Path.GetTempPath() + "FaceAI\\";
        
        public FaceModels()
        {
            client = AzureConnection.Authenticate();
            System.IO.Directory.CreateDirectory(PATH_TO_TEMP);
        }

        private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string url, string recognition_model)
        {
            // Detect faces from image URL. Since only recognizing, use the recognition model 1.
            // We use detection model 3 because we are not retrieving attributes.
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithUrlAsync(url, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03);
            return detectedFaces.ToList();
        }

        private static async Task<bool> ImageisFace(IFaceClient faceClient, string url, string recognition_model)
        {
            // Detect faces from image URL. Since only recognizing, use the recognition model 1.
            // We use detection model 3 because we are not retrieving attributes.
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithUrlAsync(url, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03);
            Console.WriteLine(detectedFaces.Count);
            return detectedFaces.Count > 0;
        }

        public async Task<bool> FindSimilar(Bitmap image)
        {

            Encoder imageEncoder;
            ImageCodecInfo imageEncoderInfo;
            EncoderParameter imageEncoderParameter;
            EncoderParameters imageEncoderParameters;

            imageEncoderInfo = GetEncoderInfo("image/jpeg");
            imageEncoder = Encoder.Quality;
            imageEncoderParameters = new EncoderParameters(1);
            imageEncoderParameter = new EncoderParameter(imageEncoder, 75L);

            imageEncoderParameters.Param[0] = imageEncoderParameter;
            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            string file_name = unixTime.ToString() + ".jpg";
            string path = PATH_TO_TEMP + file_name;
         
            image.Save(path, imageEncoderInfo, imageEncoderParameters);
            

            List<string> targetImageFileNames = BlobHandler.Get_files();

            if (!(BlobHandler.UploadToStorage(path, file_name)))
            {
                MessageBox.Show("There was a error connecting to the blob.", "Could not upload file!");
                return false;
            }


            bool is_face = await ImageisFace(client, $"{FACE_URL}{file_name}", RECOGNITION_MODEL4);
            if (!is_face)
            {
                BlobHandler.DeleteItem(file_name);
                MessageBox.Show("There is no detected face in this image. The process has be aborted.", "No face detected!");
                return false;
            }

            Console.WriteLine("========FIND SIMILAR========");
            Console.WriteLine();
            


            IList<Guid?> targetFaceIds = new List<Guid?>();
            foreach (var targetImageFileName in targetImageFileNames)
            {
                // Detect faces from target image url.
                var faces = await DetectFaceRecognize(client, $"{FACE_URL}{targetImageFileName}", RECOGNITION_MODEL4);
                // Add detected faceId to list of GUIDs.
                targetFaceIds.Add(faces[0].FaceId.Value);
            }

            // Detect faces from source image url.
            IList<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{FACE_URL}{file_name}", RECOGNITION_MODEL4);
            Console.WriteLine();

            // Find a similar face(s) in the list of IDs. Comapring only the first in list for testing purposes.
            IList<SimilarFace> similarResults = await client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, null, null, targetFaceIds);

            foreach (var similarResult in similarResults)
            {
                Console.WriteLine($"Faces from {file_name} & ID:{similarResult.PersistedFaceId} are similar with confidence: {similarResult.Confidence}.");
            }
            Console.WriteLine("DONE");
            return true;
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


        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

    }
}
