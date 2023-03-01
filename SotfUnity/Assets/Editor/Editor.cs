using System.IO;
using FMODUnity;
using UnityEditor;
using UnityEngine;

public static class EditorCustomization
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        var target = EditorUserBuildSettings.activeBuildTarget;
        EventManager.CopyToStreamingAssets(target);
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, target);
        EventManager.UpdateBankStubAssets(target);
    }
}
