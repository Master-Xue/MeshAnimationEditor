using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetsCreateEditor : Editor
{

    [MenuItem("Assets/Create/Mesh Animator Controller")]
    static void CreateEditorAnimatorController()
    {
        string[] guids = Selection.assetGUIDs;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        MeshAnimatorController controller = CreateInstance<MeshAnimatorController>();

        string filePath = FileSystem.GetFileName(path, "New MeshAnimatorController.asset");
        AssetDatabase.CreateAsset(controller, filePath);
    }


    public static void CreateAnimationAsset(MeshAnimationAsset asset)
    {
        string[] guids = Selection.assetGUIDs;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        
        string filePath = FileSystem.GetFileName(path, "New MeshAnimation.asset");
        AssetDatabase.CreateAsset(asset, filePath);

    }


}
