using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityTK
{
    /// <summary>
    /// Implements visual representation behaviour that can be used to render a gameobject as a visual representation of a gameobject.
    /// 
    /// In order to render the assigned visual representation, the gameobject is scanned for every <see cref="MeshRenderer"/> and a cache is generated.
    /// When the representation is being rendered, it will render every mesh found in the previous scan step relatively to the gameobject (of the representation) transformation.
    /// 
    /// This comes in very handy if you want to render loads of objects very efficiently or when trying to implement game data where modders can easily change visualization, ...
    /// </summary>
    [ExecuteInEditMode]
    public class VisualRepresentation : MonoBehaviour
    {
        /// <summary>
        /// The gameobject that is being used to render the representation.
        /// </summary>
        [Header("Configuration")]
        [SerializeField]
        private GameObject visualRepresentation;
        
        /// <summary>
        /// Returns <see cref="visualRepresentation"/>-
        /// </summary>
        public GameObject GetVisualRepresentation()
        {
            return this.visualRepresentation;
        }

        /// <summary>
        /// Sets a visual representation and generates the rendering cache.
        /// After this was called the specified gameobject will be rendered as visual representation;
        /// </summary>
        public void SetRepresentation(GameObject visualRepresentation)
        {
            this.visualRepresentation = visualRepresentation;
        }

        #region Render cache

        private void OnValidate()
        {
            if (Essentials.UnityIsNull(this.visualRepresentation))
                return;

            if (this.renderNodes.Count != this.visualRepresentation.GetComponentsInChildren<MeshRenderer>().Length)
                this.UpdateRenderCache();
        }

        private struct RenderNode
        {
            public Matrix4x4 matrix;
            public Mesh mesh;
            public Material[] materials;

            public ShadowCastingMode shadowMode;
            public bool recieveShadows;
            public LightProbeUsage lightProbeUsage;

            public Transform probeAnchor;
        }

        private GameObject _visualRepresentation;
        private List<RenderNode> renderNodes = new List<RenderNode>();

        /// <summary>
        /// Updates the rendering cache of this visual representation.
        /// </summary>
        [ContextMenu("Update render cache")]
        private void UpdateRenderCache()
        {
            if (this.renderNodes.Count > 0)
                this.renderNodes.Clear();

            // Retrieve all mesh renderers
            List<MeshRenderer> renderers = ListPool<MeshRenderer>.Get();
            this.visualRepresentation.GetComponentsInChildren(renderers);

            // Construct render nodes
            for (int i = 0; i < renderers.Count; i++)
            {
                // Grad renderer and filter
                var renderer = renderers[i];
                var filter = renderer.GetComponent<MeshFilter>();

                if (Essentials.UnityIsNull(filter))
                    continue;
                
                // Write render node
                this.renderNodes.Add(new RenderNode()
                {
                    matrix = this.visualRepresentation.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix,
                    materials = renderer.sharedMaterials,
                    mesh = filter.sharedMesh,
                    shadowMode = renderer.shadowCastingMode,
                    probeAnchor = renderer.probeAnchor,
                    recieveShadows = renderer.receiveShadows,
                    lightProbeUsage = renderer.lightProbeUsage
                });
            }
            ListPool<MeshRenderer>.Return(renderers);

            this._visualRepresentation = this.visualRepresentation;
        }

        #endregion

        #region Rendering

        public void Update()
        {
            if (Essentials.UnityIsNull(this.visualRepresentation))
                return;

            if (!object.ReferenceEquals(this.visualRepresentation, this._visualRepresentation))
                this.UpdateRenderCache();

            // Render nodes
            for (int i = 0; i < this.renderNodes.Count; i++)
            {
                var node = this.renderNodes[i];

                // Execute all draw calls
                for (int j = 0; j < node.materials.Length; j++)
                {
                    Graphics.DrawMesh(node.mesh, this.transform.localToWorldMatrix * node.matrix, node.materials[j], 8, null, j, null, node.shadowMode, node.recieveShadows, node.probeAnchor, node.lightProbeUsage != LightProbeUsage.Off);
                }
            }
        }

        #endregion
    }
}