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
        public GameObject visualRepresentation;

        #region Render cache

        /// <summary>
        /// Creates a rendering cache for the specified visual representation.
        /// </summary>
        /// <param name="preAlloc">Pre-allocated list for the result.</param>
        /// <returns>preAlloc if set. If not a new list is created. The returned list contains the render cache info.</returns>
        public static List<RenderNode> CreateRenderCache(GameObject visualRepresentation,  List<RenderNode> preAlloc = null)
        {
            ListPool<RenderNode>.GetIfNull(ref preAlloc);

            // Retrieve all mesh renderers
            List<MeshRenderer> renderers = ListPool<MeshRenderer>.Get();
            visualRepresentation.GetComponentsInChildren(renderers);
			var wtl = visualRepresentation.transform.worldToLocalMatrix;

            // Construct render nodes
            for (int i = 0; i < renderers.Count; i++)
            {
                // Grad renderer and filter
                var renderer = renderers[i];
                var filter = renderer.GetComponent<MeshFilter>();

                if (Essentials.UnityIsNull(filter))
                    continue;

				// Calculate TRS matrix for rendering
				var trs = wtl * renderer.transform.localToWorldMatrix;
				trs = Matrix4x4.TRS(trs.MultiplyPoint3x4(Vector3.zero), trs.rotation, renderer.transform.lossyScale);

				// Write render node
                preAlloc.Add(new RenderNode()
                {
                    matrix = trs,
                    materials = renderer.sharedMaterials,
                    mesh = filter.sharedMesh,
                    shadowMode = renderer.shadowCastingMode,
                    probeAnchor = renderer.probeAnchor,
                    recieveShadows = renderer.receiveShadows,
                    lightProbeUsage = renderer.lightProbeUsage
                });
            }
            ListPool<MeshRenderer>.Return(renderers);

            return preAlloc;
        }

        private void OnValidate()
        {
            if (Essentials.UnityIsNull(this.visualRepresentation))
                return;

            if (this.renderNodes.Count != this.visualRepresentation.GetComponentsInChildren<MeshRenderer>().Length)
                this.UpdateRenderCache();
        }

        public struct RenderNode
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
        /// Updates if needed and then returns the internal render cache.
        /// This render cache should only be read from, tho it can also be manipulated but will be rebuilt if <see cref="visualRepresentation"/> reference changes.
        /// </summary>
        public List<RenderNode> GetInternalRenderCache()
        {
            if (!object.ReferenceEquals(this.visualRepresentation, this._visualRepresentation))
                this.UpdateRenderCache();

            return this.renderNodes;
        }

        /// <summary>
        /// Updates the rendering cache of this visual representation.
        /// </summary>
        [ContextMenu("Update render cache")]
        private void UpdateRenderCache()
        {
            if (this.renderNodes.Count > 0)
                this.renderNodes.Clear();
            
            CreateRenderCache(this.visualRepresentation, this.renderNodes);
            this._visualRepresentation = this.visualRepresentation;
        }

        #endregion

        #region Material overrides

        /// <summary>
        /// Material overrides currently set to this vis rep.
        /// Key = Material to replace
        /// Value = Material to replace it with
        /// </summary>
        private Dictionary<Material, Material> materialOverrides;

        /// <summary>
        /// Material overrides currently set to this vis rep.
        /// Key = Mesh which gets its material overridden
        /// Value = Material to replace it with
        /// </summary>
        private Dictionary<Mesh, Material> meshMaterialOverrides;

        /// <summary>
        /// Overrides a material in this visual representation.
        /// <seealso cref="ClearMaterialOverrides"/>
        /// </summary>
        /// <param name="toOverride">The material to override.</param>
        /// <param name="replacement">The material to override it with.</param>
        public void OverrideMaterial(Material toOverride, Material replacement)
        {
            if (ReferenceEquals(this.materialOverrides, null))
                this.materialOverrides = DictionaryPool<Material, Material>.Get();

            this.materialOverrides.Add(toOverride, replacement);
        }

        /// <summary>
        /// Overrides a material in this visual representation by its mesh.
        /// This will cause the specified mesh to be always rendered with the specified material until the override is cleared.
        /// </summary>
        /// <param name="toOverride">The material to override.</param>
        /// <param name="replacement">The material to override it with.</param>
        public void OverrideMaterial(Mesh mesh, Material replacement)
        {
            if (ReferenceEquals(this.meshMaterialOverrides, null))
                this.meshMaterialOverrides = DictionaryPool<Mesh, Material>.Get();

            this.meshMaterialOverrides.Add(mesh, replacement);
        }

        /// <summary>
        /// Clears all previously submitted material replacements (<see cref="OverrideMaterial(Material, Material)"/>).
        /// </summary>
        public void ClearMaterialOverrides()
        {
            // Return replacement mats dict
            DestroyMatOverrides();
        }

        /// <summary>
        /// Clears all previously submitted material replacements (<see cref="OverrideMaterial(Mesh, Material)"/>).
        /// </summary>
        public void ClearMeshMaterialOverrides()
        {
            // Return replacement mats dict
            DestroyMeshOverrides();
        }

        /// <summary>
        /// Destroys <see cref="materialOverrides"/> by returning it to the dictionary pool.
        /// </summary>
        private void DestroyMatOverrides()
        {
            if (!ReferenceEquals(this.materialOverrides, null))
                DictionaryPool<Material, Material>.Return(this.materialOverrides);
            this.materialOverrides = null;
        }
        
        /// <summary>
        /// Destroys <see cref="meshMaterialOverrides"/> by returning it to the dictionary pool.
        /// </summary>
        private void DestroyMeshOverrides()
        {
            if (!ReferenceEquals(this.meshMaterialOverrides, null))
                DictionaryPool<Mesh, Material>.Return(this.meshMaterialOverrides);
            this.meshMaterialOverrides = null;
        }

        /// <summary>
        /// Frees up <see cref="materialOverrides"/>
        /// </summary>
        private void OnDestroy()
        {
            DestroyMatOverrides();
            DestroyMeshOverrides();
        }

        #endregion

        #region Rendering

        public void Update()
        {
            if (Essentials.UnityIsNull(this.visualRepresentation))
                return;

            var renderCache = GetInternalRenderCache();
            bool hasMaterialReplacements = !ReferenceEquals(this.materialOverrides, null);
            bool hasMeshMaterialReplacements = !ReferenceEquals(this.meshMaterialOverrides, null);
            // Render nodes
            for (int i = 0; i < renderCache.Count; i++)
            {
                var node = renderCache[i];

                // Execute all draw calls
                for (int j = 0; j < node.materials.Length; j++)
                {
                    // Material replacement
                    Material material = node.materials[j], replacementMat = null;
                    if (hasMaterialReplacements && this.materialOverrides.TryGetValue(material, out replacementMat))
                        material = replacementMat;

                    // Mesh material replacement
                    if (hasMeshMaterialReplacements && this.meshMaterialOverrides.TryGetValue(node.mesh, out replacementMat))
                        material = replacementMat;

                    Graphics.DrawMesh(node.mesh, this.transform.localToWorldMatrix * node.matrix, material, 8, null, j, null, node.shadowMode, node.recieveShadows, node.probeAnchor, node.lightProbeUsage != LightProbeUsage.Off);
                }
            }
        }

        #endregion
    }
}