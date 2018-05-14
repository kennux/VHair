using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityTK
{
    /// <summary>
    /// Utility script that can be used to load multiple scenes.
    /// The scenes are being loaded additively (and asynchronously) and after the load is completed, this object's gameobject is being destroyed.
    /// 
    /// This component can be very useful to implement "EntryPoints" which load your game that is divided in several seperate scens.
    /// </summary>
    public class MultiSceneLoader : MonoBehaviour
    {
        [Header("Config")]
        /// <summary>
        /// The scenes to load.
        /// </summary>
        public SceneField[] scenes;

        /// <summary>
        /// Destroys this object's gameobject after scene load
        /// </summary>
        public bool destroyAfterLoad;

        /// <summary>
        /// The loading progress of all scenes combined, range 0-1 (0-100%)
        /// </summary>
        public float loadPercentage
        {
            get
            {
                int c = this.sceneLoadOperations.Count;
                if (c == 0)
                    return 0;

                float v = 0;
                for (int i = 0; i < this.sceneLoadOperations.Count; i++)
                {
                    v += this.sceneLoadOperations[i].progress;
                }

                return v / (float)c;
            }
        }

        /// <summary>
        /// Internal list used to keep track of scene load ops
        /// </summary>
        private List<AsyncOperation> sceneLoadOperations = new List<AsyncOperation>();

        public void Awake()
        {
            for (int i = 0; i < this.scenes.Length; i++)
            {
                this.sceneLoadOperations.Add(SceneManager.LoadSceneAsync(this.scenes[i], LoadSceneMode.Additive));
            }
        }

        public void Update()
        {
            bool allFinished = true;
            for (int i = 0; i < this.sceneLoadOperations.Count; i++)
            {
                if (!this.sceneLoadOperations[i].isDone)
                {
                    allFinished = false;
                    return;
                }
            }

            if (allFinished)
            {
                if (this.destroyAfterLoad)
                    Destroy(this.gameObject);
            }
        }
    }
}