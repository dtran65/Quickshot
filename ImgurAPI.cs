using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace io.github.charries96.quickshot
{
    /// <summary>
    /// Short and sweet image uploading API for Imgur,
    /// Uploads a single Image object.
    /// </summary>
    internal class ImgurAPI
    {
        /// <summary>
        /// Client ID
        /// </summary>
        private string clientId = "";

        /// <summary>
        /// Create a new ImgurAPI.
        /// </summary>
        /// <param name="clientId">Client ID in your Applications panel.</param>
        public ImgurAPI(String clientId)
        {
            this.clientId = clientId;
        }

        /// <summary>
        /// Upload the Image object.
        /// </summary>
        /// <param name="image">Image to upload</param>
        /// <returns>URL to image as String.</returns>
        public String UploadImage(Image image)
        {
            if (image == null)
            {
                return "Invalid image provided."; 
            }
            WebClient w = new WebClient();
            w.Headers.Add("Authorization", "Client-ID " + clientId);
            NameValueCollection Keys = new NameValueCollection();
            try
            {
                Keys.Add("image", ImageToBase64(image));
                byte[] responseArray = w.UploadValues("https://api.imgur.com/3/image", Keys);
                dynamic result = Encoding.ASCII.GetString(responseArray);
                Regex reg = new Regex("link\":\"(.*?)\"");
                Match match = reg.Match(result);
                return match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");
            }
            catch (Exception s)
            {
                return "Upload failed! Try again later.";
            }
        }

        /// <summary>
        /// Convert an image to the base64 equivalent.
        /// </summary>
        /// <param name="image">Image to convert</param>
        /// <param name="format">Image format to save as.</param>
        /// <returns>Base64 variant of the image.</returns>
        private string ImageToBase64(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}