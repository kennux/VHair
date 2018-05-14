using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Loads all <see cref="IManagedAsset"/> from the unity resources folder.
    /// </summary>
    public class ResourcesLoader : AssetLoader
    {
        protected override List<IManagedAsset> LoadAssets()
        {
            List<IManagedAsset> assets = new List<IManagedAsset>();
            assets.AddRange(Resources.LoadAll<ScriptableObject>("").Where((so) => so is IManagedAsset).Cast<IManagedAsset>());

            foreach (var go in Resources.LoadAll<GameObject>(""))
            {
                var asset = go.GetComponent<IManagedAsset>();
                if (!Essentials.UnityIsNull(asset))
                    assets.Add(asset);
            }

            return assets;
        }

        protected override void UnloadAssets(List<IManagedAsset> assets)
        {
            foreach (var asset in assets)
            {
                if (asset is ScriptableObject)
                    Resources.UnloadAsset((ScriptableObject)asset);
            }

            Resources.UnloadUnusedAssets();
        }
    }
}