using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteAlways]
public class BakePaintMenu : MonoBehaviour
{
    // Saves the selected object's splat texture as a png in Textures/BakedSplats
    [MenuItem("Tools/Paint/Bake Paint for Selected Object")]
    static void BakeSelected()
    {
        Texture2D splatTex = Selection.activeTransform.GetComponent<PaintTarget>().CreateBakedTex();

        var objectName = Selection.activeObject.name;
        var splatDir = "BakedSplats";
        var fullPath = Application.dataPath + "/Materials/" + splatDir;
        var assetPath = "Assets/Materials/";

        if(!AssetDatabase.IsValidFolder(assetPath + splatDir))
            AssetDatabase.CreateFolder(assetPath, splatDir);

        int num = Directory.GetFiles(fullPath).Length + 1;
        var name = "Baked_" + objectName + "_" + num + ".png";

        byte[] bytes = splatTex.EncodeToPNG();
        
        File.WriteAllBytes(fullPath + "/" + name, bytes);
        
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + splatDir + "/" + name);
    }

    [MenuItem("Tools/Paint/Bake Paint for Selected Object", true)]
    static bool ValidateLogSelectedObject()
    {
        // Return false if no transform is selected.
        return Selection.activeTransform != null && 
        Selection.activeTransform.TryGetComponent<PaintTarget>(out PaintTarget component) &&
        Application.isPlaying;
    }
}
