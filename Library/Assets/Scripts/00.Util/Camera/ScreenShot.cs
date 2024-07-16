using System.IO;
using UnityEngine;


namespace Util
{
    public class ScreenShot : MonoBehaviour
    {
        [SerializeField] RenderTexture renderTexture;

        /// <summary>
        /// Save objects captured by a specific camera as Texture2D.
        /// </summary>
        /// <param name="camera"> target camera </param>
        /// <param name="texW"> texture width </param>
        /// <param name="texH"> texture height </param>
        /// <returns> screen shot texture </returns>
        public Texture2D Shot(Camera camera, int texW, int texH)
        {
            Texture2D shot = new Texture2D(texW, texH, TextureFormat.RGB24, false);
            Rect shotRect = new Rect(0, 0, shot.width, shot.height);
            camera.Render();

            RenderTexture.active = renderTexture;
            shot.ReadPixels(shotRect, 0, 0);
            shot.Apply();

            return shot;
        }

        /// <summary>
        /// Capture objects through the camera, save them as an image, and return the path.
        /// </summary>
        /// <param name="camera"> target camera </param>
        /// <param name="texW"> texture width </param>
        /// <param name="texH"> texture height </param>
        /// <returns> path </returns>
        public string Save(Camera camera, int texW, int texH)
        {
            string path = Application.dataPath + "/ScreenShot/";

            if (Directory.Exists(path) == false)
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (System.Exception e)
                {
                    Debug.Log("Exception: " + e);
                }
            }

            string filePath = path + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";

            Texture2D shot = Shot(camera, texW, texH);
            byte[] bytes = shot.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }
        /// <summary>
        /// Save the Texture2D as an image file and return the path.
        /// </summary>
        /// <param name="texture"> texture </param>
        /// <returns> path </returns>
        public string Save(Texture2D texture)
        {
            string path = Application.dataPath + "/ScreenShot/";

            if (Directory.Exists(path) == false)
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (System.Exception e)
                {
                    Debug.Log("Exception: " + e);
                }
            }

            string filePath = path + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";

            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }
    }

}
