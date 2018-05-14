using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Simple asset bundle loader implementation that can be used to load bundles to <see cref="AssetManager.LoadAssetBundle(AssetBundle)"/>.
    /// This will load all files in the specified folder as asset bundles.
    /// Will filter for files without or .bundle extension.
    /// 
    /// In the editor this loader will simulate asset bundle loading by registering all assets in any bundle.
    /// <see cref="AssetManager.EditorLoadAndRegisterAssetsInBundles"/>
    /// </summary>
    public class AssetBundleLoader : AssetLoader
    {
        /// <summary>
        /// The path from where asset bundles are being loaded at runtime (in player).
        /// </summary>
        public string loadPath;

        /// <summary>
        /// All bundles loaded by this loader.
        /// </summary>
        [HideInInspector]
        public List<AssetBundle> loadedBundles = new List<AssetBundle>();
        
        protected override List<IManagedAsset> LoadAssets()
        {
            List<IManagedAsset> assets = new List<IManagedAsset>();

#if UNITY_EDITOR
            foreach (var bundle in UnityEditor.AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (var path in UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
                {
                    var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(path);

                    // Scriptable object / gameobject registering
                    if (obj is IManagedAsset)
                        assets.Add((IManagedAsset)obj);
                    else if (obj is GameObject)
                    {
                        var ma = (obj as GameObject).GetComponent<IManagedAsset>();
                        if (!Essentials.UnityIsNull(ma))
                            assets.Add(ma);
                    }
                }
            }
#else
            foreach (var file in Directory.GetFiles(loadPath))
            {
                string ext = Path.GetExtension(file);
                if (string.IsNullOrEmpty(ext) || ext.Equals(".bundle"))
                {
                    Debug.Log("Loading Asset Bundle " + file);

                    var bundle = AssetBundle.LoadFromFile(file);
                    LoadAssetBundle(bundle, assets);
                    this.loadedBundles.Add(bundle);
                }
            }
#endif

            return assets;
        }

        private void LoadAssetBundle(AssetBundle bundle, List<IManagedAsset> assets)
        {
            // Load assets
            assets.AddRange(bundle.LoadAllAssets<ManagedScriptableObject>());
            var gos = bundle.LoadAllAssets<GameObject>();
            for (int i = 0; i < gos.Length; i++)
            {
                // Get and validate gameobject
                var go = gos[i];
                if (Essentials.UnityIsNull(go))
                {
                    Debug.LogWarning("Asset bundle contained a gameobject that failed null equality check!");
                    continue;
                }

                // Try getting the component
                var managedAsset = go.GetComponent<IManagedAsset>();
                if (Essentials.UnityIsNull(managedAsset))
                {
                    continue;
                }

                assets.Add(managedAsset);
            }

            bundle.Unload(false);
            Resources.UnloadUnusedAssets();
        }

        protected override void UnloadAssets(List<IManagedAsset> assets)
        {
            foreach (var bundle in this.loadedBundles)
                if (!Essentials.UnityIsNull(bundle))
                    bundle.Unload(true);
        }
    }
}
