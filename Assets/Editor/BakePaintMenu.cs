using System.IO;
using Src.Scripts;
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
            Texture2D paintMap = Selection.activeTransform.GetComponent<PaintTarget>().CreateBakedTex();

            var objectName = Selection.activeObject.name;
            var bakedDir = "BakedPaint";
            var fullPath = Application.dataPath + "/Art/" + bakedDir;
            var assetPath = "Assets/Art/";

            if(!AssetDatabase.IsValidFolder(assetPath + bakedDir))
                AssetDatabase.CreateFolder(assetPath, bakedDir);

            int num = Directory.GetFiles(fullPath).Length + 1;
            var name = "Baked_" + objectName + "_" + num + ".png";

            byte[] bytes = paintMap.EncodeToPNG();
        
            File.WriteAllBytes(fullPath + "/" + name, bytes);
        
            Debug.Log("Paintmap was saved as a " + bytes.Length / 1024 + "Kb file at: " + bakedDir + "/" + name + "."
            + " Remember to uncheck 'sRGB (Color Texture)' when importing.");
        }

        [MenuItem("Tools/Paint/Bake Paint for Selected Object", true)]
        static bool ValidateLogSelectedObject()
        {
            // Return false if no transform is selected.
            return Selection.activeTransform != null && 
                   Selection.activeTransform.TryGetComponent(out PaintTarget _) &&
                   Application.isPlaying;
        }
    }
}
