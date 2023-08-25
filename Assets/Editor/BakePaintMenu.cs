using System.IO;
using Src.Scripts;
using Src.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [ExecuteAlways]
    public class BakePaintMenu : MonoBehaviour
    {
        // Saves the selected object's splat texture as a png in Textures/BakedSplats
        [MenuItem("Tools/Paint/Bake Paint for Selected Object")]
        static void BakeSelected()
        {
            foreach (var gameObj in Selection.gameObjects)
            {
                Texture2D paintMap = gameObj.GetComponent<PaintTarget>().CreateBakedPaintMap();

                var objectName = gameObj.name;
                var bakedDir = "BakedPaint";
                var fullPath = Application.dataPath + "/Art/" + bakedDir;
                var assetPath = "Assets/Art/";

                if (!AssetDatabase.IsValidFolder(assetPath + bakedDir))
                    AssetDatabase.CreateFolder(assetPath, bakedDir);

                var num = 0;
                var name = $"Baked_{objectName}_{num}.png";
                while (File.Exists($"{fullPath}/{name}"))
                {
                    name = $"Baked_{objectName}_{++num}.png";
                }

                byte[] bytes = paintMap.EncodeToPNG();

                File.WriteAllBytes(fullPath + "/" + name, bytes);

                Debug.Log("Paintmap was saved as a " + bytes.Length / 1024 + "Kb file at: " + bakedDir + "/" + name +
                          "."
                          + " Remember to uncheck 'sRGB (Color Texture)' when importing.");
            }
        }

        [MenuItem("Tools/Paint/Bake Paint for Selected Object", true)]
        static bool ValidateBakePaintSelectedObject()
        {
            // Return false if no transform is selected.
            return Selection.activeTransform != null &&
                   Selection.activeTransform.TryGetComponent(out PaintTarget _) &&
                   Application.isPlaying;
        }
    }
}
