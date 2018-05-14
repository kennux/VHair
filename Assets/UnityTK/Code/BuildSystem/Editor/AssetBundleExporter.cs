using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityTK
{
    /// <summary>
    /// Editor helper script that can be used to export all asset bundles to a specific folder.
    /// This script is standalone and can be used to export all asset bundles from any unity project.
    /// 
    /// Games using the UnityTK can use this script in their SDK to allow for easy asset bundle export from unity.
    /// </summary>
    public static class AssetBundleExporter
    {
        [MenuItem("UnityTK/Export Asset Bundles")]
        public static void Export()
        {
            BuildPipeline.BuildAssetBundles(EditorUtility.OpenFolderPanel("Bundles Path", "", ""), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}