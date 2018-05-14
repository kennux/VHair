using UnityEngine;
using System.Collections.Generic;

namespace UnityTK
{
    /// <summary>
    /// Static helper class for handling scene folders.
    /// Scene folders are gameobjects on the highest level in the hierarchy used to pack groups of objects together.
    /// </summary>
    public static class SceneFolders
    {
        /// <summary>
        /// The folder cache is used to cache already created folders.
        /// </summary>
        private static Dictionary<string, Transform> folderCache = new Dictionary<string, Transform>();

        /// <summary>
        /// The folder cache is used to cache already created folders.
        /// </summary>
        private static Dictionary<Transform, Dictionary<string, Transform>> childFolderCache = new Dictionary<Transform, Dictionary<string, Transform>>();

        /// <summary>
        /// Tries to get the folder with the specified name.
        /// If the folder is not in the cache it will be first searched in the scene, and if not existing it will be created!
        /// </summary>
        public static Transform GetFolder(string name, Transform parent = null)
        {
            Transform folder = null;
            Dictionary<string, Transform> cache = folderCache;
            if (parent != null)
            {
                if (!childFolderCache.TryGetValue(parent, out cache))
                {
                    cache = new Dictionary<string, Transform>();
                    childFolderCache.Add(parent, cache);
                }
            }

            if (!folderCache.TryGetValue(name, out folder))
            {
                // Try to find existing
                var folderGo = GameObject.Find(name);
                if (folderGo == null)
                {
                    // Not found :'( create new
                    folderGo = new GameObject(name);
                    folder = folderGo.transform;
                    folder.position = Vector3.zero;
                    folder.rotation = Quaternion.identity;
                }
                else
                {
                    folder = folderGo.transform;
                }

                // Add to cache
                folderCache.Add(name, folder);
            }

            if (folder == null)
            {
                folderCache.Remove(name);
                folder = GetFolder(name);
            }

            return folder;
        }
    }
}