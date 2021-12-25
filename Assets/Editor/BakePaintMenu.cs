using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BakePaintMenu : MonoBehaviour
{
    // Saves the selected object's splat texture as a png in Textures/BakedSplats
    [MenuItem("Tools/Paint/Bake Paint for Selected Object")]
    static void BakeSelected()
    {
        Texture2D splatTex = Selection.activeTransform.GetComponent<PaintTarget>().CreateBakedTex();

        var splatDir = "BakedSplats";
        var dirPath = Application.dataPath + "/Textures/" + splatDir;

        if(!AssetDatabase.IsValidFolder("Assets/Textures/" + splatDir))
            AssetDatabase.CreateFolder("Assets/Textures", splatDir);

        int num = Directory.GetFiles(dirPath).Length + 1;
        var name = "Baked_" + Selection.activeObject.name + "_" + num + ".png";
        
        File.WriteAllBytes(dirPath + "/" + name, splatTex.EncodeToPNG());
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
