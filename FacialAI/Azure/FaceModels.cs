using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FacialAI.Azure
{
    /// <summary>
    /// Class <c>FaceModels</c> holdes and manages all client side operations and API calls to the azure base.
    /// </summary>
    class FaceModels
    {
        // The client for connecting to azure
        private static IFaceClient client;
        // Recognition model
        private const string RECOGNITION_MODEL4 = RecognitionModel.Recognition04;

        // Link to image base
        private readonly string IMAGE_BASE_URL = ConfigurationManager.AppSettings.Get("IMAGE_BASE_URL");
        private readonly string FACE_URL = ConfigurationManager.AppSettings.Get("FACE_URL");
        private readonly string PATH_TO_TEMP = Path.GetTempPath() + "FaceAI\\";
        private readonly double CONFIDENCE_THREASHOLD = 75.0;

        /// <summary>
        /// The constructor, which also generates a path to the temp folder.
        /// </summary>
        public FaceModels()
        {
            client = AzureConnection.Authenticate();
            System.IO.Directory.CreateDirectory(PATH_TO_TEMP);
        }

        /// <summary>
        /// Generate a list of IDs related to faces
        /// </summary>
        /// <param name="faceClient">The client used to detect the faces (Azure)</param>
        /// <param name="url">THe link to the face</param>
        /// <param name="recognition_model">The model used to find a face</param>
        /// <returns>Returns a list of face IDs</returns>
        private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string url, string recognition_model)
        {
            // Detect faces from image URL. Since only recognizing, use the recognition model 1.
            // We use detection model 3 because we are not retrieving attributes.
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithUrlAsync(url, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03);
            return detectedFaces.ToList();
        }

        /// <summary>
        /// Detects to see if the image handed is a face,
        /// </summary>
        /// <param name="faceClient">The client used to detect the faces (Azure)</param>
        /// <param name="url">THe link to the face</param>
        /// <param name="recognition_model">The model used to find a face</param>
        /// <returns>Returns if a face was detected</returns>
        private static async Task<bool> ImageisFace(IFaceClient faceClient, string url, string recognition_model)
        {
            // Detect faces from image URL. Since only recognizing, use the recognition model 1.
            // We use detection model 3 because we are not retrieving attributes.
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithUrlAsync(url, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03);
            Console.WriteLine(detectedFaces.Count);
            return detectedFaces.Count > 0;
        }

        /// <summary>
        /// Find similar faces to the one presented, also upload the face to the database.
        /// </summary>
        /// <param name="image">The bitmap image</param>
        /// <param name="to_save">If the image should be saved to the database or just checked</param>
        /// <returns>Returns if the task compleated successfully</returns>
        public async Task<bool> FindSimilar(Bitmap image, bool to_save)
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
            List<SimilarFace> confidentResults = new List<SimilarFace>();
            bool found_similar = false;
                foreach (var similarResult in similarResults)
                {
                    if (similarResult.Confidence >= CONFIDENCE_THREASHOLD/100)
                    {
                        Console.WriteLine($"Faces from {file_name} & ID:{similarResult.FaceId} are similar with confidence: {similarResult.Confidence}.");
                        confidentResults.Add(similarResult);
                        found_similar = true;
                    }
                }

            if (!found_similar)
            {
                MessageBox.Show($"No face was found with a similarity above {CONFIDENCE_THREASHOLD}%", "No similar faces");
            }

            // This will remove the picture from the azure blob
            if(!to_save)
            {
                BlobHandler.DeleteItem(file_name);
            }

            return true;
        }

        /// <summary>
        /// Gets the image codec info to save to the temp space
        /// </summary>
        /// <param name="mimeType">n/a</param>
        /// <returns>returns codec info</returns>
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
