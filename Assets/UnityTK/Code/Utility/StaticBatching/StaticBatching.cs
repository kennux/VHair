using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System;
using Unity.Collections;

namespace UnityTK
{
    /// <summary>
    /// Static batching singleton class.
    /// This is the singleton behaviour thats handling statically batched objects.
    /// 
    /// You can either let the static batching singleton be constructed automatically or attach this to a gameobject in your scene.
    /// If its constructed automatically it will be constructed with DontDestroyOnLoad.
    /// 
    /// This static batching system can be used at runtime (unlike unity's built in system).
    /// This can be very helpful when procedurally generating maps as it can dramatically shrink down the amount of draw calls, culling, ...
    /// Its a major optimization in procedurally generated worlds!
    /// 
    /// It should only be used on actually static objects tho.
    /// Also it only supports mesh rendering, visual representations are passed into the batching manager with their TRS matrix and are then automatically baked.
    /// 
    /// TODO: 
    /// - Support for layers
    /// - Use 16bit indices if possible (optimization)
    /// - Burst Backend
    /// - Non-triangle topology support
    /// - Dynamic mesh layouts (meshes without tangents for ex.)
    /// </summary>
    public class StaticBatching : MonoBehaviour
    {
        /// <summary>
        /// Sigleton pattern.
        /// </summary>
        public static StaticBatching instance
        {
            get
            {
                if (Essentials.UnityIsNull(_instance))
                {
                    // Automatic creation
                    var go = new GameObject("__StaticBatching__");
                    _instance = go.AddComponent<StaticBatching>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static StaticBatching _instance;

        /// <summary>
        /// Specialized getter for usage inside the unity message OnDestroy.
        /// Will avoid gameobject creation.
        /// </summary>
        public static StaticBatching GetInstanceOnDestroy()
        {
            return _instance;
        }

        /// <summary>
        /// The chunk size for grouping representations together on 3 axis (chunk are cubic).
        /// <see cref="StaticBatchingGroupKey.chunk"/>
        /// 
        /// Do not set this at runtime after meshes have been inserted!
        /// TODO: Implement runtime update and rebuilds
        /// </summary>
        [Header("Configuration")]
        public int chunkSize = 128;

        /// <summary>
        /// Maximum amount of mesh rebuilds per frame.
        /// </summary>
        public int maxRebuildsPerFrame = 1;

        #region Accessors / API

        /// <summary>
        /// Finishes all ongoing rebuilds with optional filters.
        /// </summary>
        /// <param name="material">Optional material filter</param>
        /// <param name="layer">Optional layer filter.</param>
        public void FinishAllRebuilds(Material material = null, int? layer = null)
        {
            // Finish potential ongoing rebuilds
            foreach (var kvp in this.runningJobs)
            {
                var jobData = kvp.Value;
                var groupKey = kvp.Value.groupKey;
                if ((ReferenceEquals(material, null) || ReferenceEquals(groupKey.material, material)) &&
                    (!layer.HasValue || groupKey.layer == layer))
                {
                    jobData.jobHandle.Complete();
                    FinishMeshRebuild(jobData);
                }
            }
        }

        /// <summary>
        /// Returns the batched meshes for the specified material and layer.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="layer">!NOT IMPLEMENTED YET!</param>
        /// <returns>preAlloc, drawn from pool if passed in with a list object instance.</returns>
        public List<Mesh> GetBatchedMeshes(Material material, int layer, List<Mesh> preAlloc = null)
        {
            ListPool<Mesh>.GetIfNull(ref preAlloc);
            List<StaticBatchingRebuildJobData> rebuildJobs = ListPool<StaticBatchingRebuildJobData>.Get();

            foreach (var kvp in this.runningJobs)
                rebuildJobs.Add(kvp.Value);

            // Finish potential ongoing rebuilds
            foreach (var jobData in rebuildJobs)
            {
                var groupKey = jobData.groupKey;
                if (ReferenceEquals(groupKey.material, material) && groupKey.layer == layer)
                {
                    jobData.jobHandle.Complete();
                    FinishMeshRebuild(jobData);
                }
            }

            // Look up meshes
            foreach (var kvp in this.meshes)
            {
                var groupKey = kvp.Key;
                var mesh = kvp.Value;

                if (groupKey.layer == layer && ReferenceEquals(groupKey.material, material))
                {
                    preAlloc.Add(mesh);
                }
            }

            ListPool<StaticBatchingRebuildJobData>.Return(rebuildJobs);
            return preAlloc;
        }

        #endregion

        #region Command Queue

        /// <summary>
        /// Command for <see cref="InsertVisualRepresentation(GameObject, Matrix4x4, object)"/> on <see cref="insertQueue"/>
        /// </summary>
        private struct BatchingCommandInsert
        {
            /// <summary>
            /// The representation material.
            /// </summary>
            public Material material;

            /// <summary>
            /// The visual representation mesh.
            /// </summary>
            public Mesh mesh;

            /// <summary>
            /// The TRS matrix.
            /// </summary>
            public Matrix4x4 transform;

            /// <summary>
            /// The layer of the visual representation.
            /// </summary>
            public int layer;

            /// <summary>
            /// The owner of the visual representation.
            /// </summary>
            public object owner;
        }

        /// <summary>
        /// Command for <see cref="DestroyVisualRepresentations(object)"/> on <see cref="destroyQueue"/>
        /// </summary>
        private struct BatchingCommandDestroy
        {
            /// <summary>
            /// Destroy command for owners.
            /// </summary>
            public object owner;
        }

        /// <summary>
        /// Command queue for insert commands (<see cref="BatchingCommandInsert"/>).
        /// </summary>
        private List<BatchingCommandInsert> insertQueue = new List<BatchingCommandInsert>();

        /// <summary>
        /// Command queue for destroy commands (<see cref="BatchingCommandDestroy"/>).
        /// </summary>
        private List<BatchingCommandDestroy> destroyQueue = new List<BatchingCommandDestroy>();

        /// <summary>
        /// Command queue indices.
        /// Positive indices map to <see cref="insertQueue"/>,
        /// Negative indices map to <see cref="destroyQueue"/>
        /// </summary>
        private Queue<int> commandQueue = new Queue<int>();

        #endregion

        #region Management

        /// <summary>
        /// Visual representation cache.
        /// Mapping a game object of a visual representation to the representation cache.
        /// </summary>
        private Dictionary<Mesh, StaticVisualRepresentationCache> visCache = new Dictionary<Mesh, StaticVisualRepresentationCache>();

        /// <summary>
        /// Tries getting a static visual representation cache object for the specified gameobject.
        /// If there is none yet it will generate it.
        /// </summary>
        /// <param name="mesh">The mesh to get the cache for.</param>
        /// <returns>The representation cache information.</returns>
        private StaticVisualRepresentationCache GetVisCache(Mesh mesh)
        {
            StaticVisualRepresentationCache cache;
            if (!this.visCache.TryGetValue(mesh, out cache))
                cache = CreateVisCache(mesh);

            return cache;
        }

        /// <summary>
        /// Creator for <see cref="GetVisCache(GameObject)"/>.
        /// Will automatically register on <see cref="visCache"/>.
        /// </summary>
        /// <param name="mesh">The mesh to get the cache for.</param>
        private StaticVisualRepresentationCache CreateVisCache(Mesh mesh)
        {
            var cache = new StaticVisualRepresentationCache(mesh);

            this.visCache.Add(mesh, cache);
            return cache;
        }

        /// <summary>
        /// Inserts a new visual representation.
        /// This may cause a mesh rebake for other representations sharing the same <see cref="StaticBatchingGroupKey"/>.
        /// 
        /// The visual representation gameobject can be a prefab or a runtime object, note that its transform localToWorld matrices will be multipled by transform.
        /// </summary>
        /// <param name="visualRepresentation">Visual representation prefab</param>
        /// <param name="transform">The transform (TRS) matrix to be used for translating the visual representation into worldspace.</param>
        /// <param name="owner">An object reference that can be used to destroy the inserted representation again.</param>
        public void InsertVisualRepresentation(GameObject visualRepresentation, Matrix4x4 transform, object owner)
        {
            List<MeshRenderer> meshRenderers = ListPool<MeshRenderer>.Get();

            visualRepresentation.GetComponentsInChildren<MeshRenderer>(meshRenderers);
            Matrix4x4 transformInverse = transform.inverse;

            foreach (var mr in meshRenderers)
            {
                var mf = mr.GetComponent<MeshFilter>();

                int queueIndex = this.insertQueue.Count+1;
                this.insertQueue.Add(new BatchingCommandInsert()
                {
                    material = mr.sharedMaterial,
                    owner = owner,
                    transform = transform * mr.transform.localToWorldMatrix,
                    layer = visualRepresentation.layer,
                    mesh = mf.sharedMesh
                    
                });
                this.commandQueue.Enqueue(queueIndex);
            }

            ListPool<MeshRenderer>.Return(meshRenderers);
        }

        /// <summary>
        /// Inserts a new visual representation mesh into the static batching system.
        /// This may cause a mesh rebake for other representations sharing the same <see cref="StaticBatchingGroupKey"/>.
        /// </summary>
        /// <param name="mesh">The mesh to insert with specified transformation</param>
        /// <param name="material">The material to insert the mesh with.</param>
        /// <param name="layer"><see cref="GameObject.layer"/></param>
        /// <param name="transform">The transform (TRS) matrix to be used for translating the visual representation into worldspace.</param>
        /// <param name="owner">An object reference that can be used to destroy the inserted representation again.</param>
        public void InsertMesh(Mesh mesh, Material material, int layer, Matrix4x4 transform, object owner)
        {
            int queueIndex = this.insertQueue.Count + 1;
            this.insertQueue.Add(new BatchingCommandInsert()
            {
                material = material,
                owner = owner,
                transform = transform,
                layer = layer,
                mesh = mesh
            });
            this.commandQueue.Enqueue(queueIndex);
        }

        /// <summary>
        /// Will destroy all visual representations inserted with the specified owner.
        /// </summary>
        /// <param name="owner">The owner to destroy representations for.</param>
        public void DestroyVisualRepresentations(object owner)
        {
            int queueIndex = -(this.destroyQueue.Count+1);
            this.destroyQueue.Add(new BatchingCommandDestroy()
            {
                owner = owner
            });
            this.commandQueue.Enqueue(queueIndex);
        }

        /// <summary>
        /// Enforces the singleton pattern.
        /// </summary>
        private void Awake()
        {
            if (Essentials.UnityIsNull(_instance))
                _instance = this;
            else
            {
                Debug.LogError("Singleton pattern violated! Destroying StaticBatching!", this.gameObject);
                Destroy(this);
            }
        }

        /// <summary>
        /// Frees memory taken by <see cref="visCache"/>
        /// </summary>
        public void OnDestroy()
        {
            foreach (var cache in this.visCache.Values)
                cache.Dispose();

            foreach (var job in this.runningJobs)
            {
                job.Value.jobHandle.Complete();
                job.Value.Deallocate();
            }
        }

        #endregion

        #region Batching jobs / Mesh Build

        /// <summary>
        /// All mesh instance data.
        /// Grouped together by material.
        /// </summary>
        private Dictionary<StaticBatchingGroupKey, List<StaticMeshInstance>> meshInstances = new Dictionary<StaticBatchingGroupKey, List<StaticMeshInstance>>(EqualityComparer<StaticBatchingGroupKey>.Default);

        /// <summary>
        /// All meshes to be rendered every frame.
        /// Those meshes are the result of batched visual representations.
        /// </summary>
        private Dictionary<StaticBatchingGroupKey, Mesh> meshes = new Dictionary<StaticBatchingGroupKey, Mesh>(EqualityComparer<StaticBatchingGroupKey>.Default);
        
        /// <summary>
        /// Mapping unsigned integer ids to rebuild job datas.
        /// </summary>
        private Dictionary<uint, StaticBatchingRebuildJobData> runningJobs = new Dictionary<uint, StaticBatchingRebuildJobData>();
        private uint runningJobDataCounter = 0;

        /// <summary>
        /// Job transforming mesh data.
        /// </summary>
        private struct TransformJob : IJobParallelFor
        {
            public uint dataId;

            public void Execute(int index)
            {
                var data = StaticBatching.instance.runningJobs[this.dataId];
                int vertexOffset = data.vertexOffsets[index];
                int indexOffset = data.indexOffsets[index];
                StaticMeshInstance meshInstance = data.meshInstances[index];

                // Write transformed vertices
                var visCache = StaticBatching.instance.GetVisCache(meshInstance.mesh);
                for (int i = 0; i < visCache.vertices.Length; i++)
                {
                    data.nVertices[vertexOffset + i] = meshInstance.transform.MultiplyPoint3x4(visCache.vertices[i]);
                    data.nNormals[vertexOffset + i] = meshInstance.transform.MultiplyVector(visCache.normals[i]);
                    data.nTangents[vertexOffset + i] = meshInstance.transform.MultiplyVector(visCache.tangents[i]);
                    data.nUvs[vertexOffset + i] = visCache.uv0[i];
                }

                // Write indices
                for (int i = 0; i < visCache.indices.Length; i++)
                    data.nIndices[indexOffset + i] = visCache.indices[i] + vertexOffset;
            }
        }

        /// <summary>
        /// Job to be used to copy data from native arrays to lists.
        /// </summary>
        private struct CopyJob : IJob
        {
            public uint dataId;

            public void Execute()
            {
                var data = StaticBatching.instance.runningJobs[this.dataId];
                int vCount = data.nVertices.Length;
                int iCount = data.nIndices.Length;

                for (int i = 0; i < vCount; i++)
                {
                    data.vertices.Add(data.nVertices[i]);
                    data.normals.Add(data.nNormals[i]);
                    data.tangents.Add(data.nTangents[i]);
                    data.uvs.Add(data.nUvs[i]);
                }

                for (int i = 0; i < iCount; i++)
                    data.indices.Add(data.nIndices[i]);
            }
        }

        /// <summary>
        /// Queues new or rebuild of the specified group.
        /// </summary>
        /// <param name="group">The group to rebuild.</param>
        private void QueueRebuild(StaticBatchingGroupKey group)
        {
            // Read instances
            List<StaticMeshInstance> instances;
            if (!this.meshInstances.TryGetValue(group, out instances))
                return;

            // Prepare data arrays for job
            int vCounter = 0, iCounter = 0; // Vertex and index counter
            StaticBatchingRebuildJobData jobData = new StaticBatchingRebuildJobData()
            {
                groupKey = group
            };
            jobData.PreAllocate();
            
            foreach (var instance in instances)
            {
                var cache = GetVisCache(instance.mesh);
                jobData.meshInstances.Add(instance);
                jobData.vertexOffsets.Add(vCounter);
                jobData.indexOffsets.Add(iCounter);
                vCounter += cache.vertices.Length;
                iCounter += cache.indices.Length;
            }

            // Allocate job data memory
            jobData.Allocate(vCounter, iCounter);

            // Dispatch jobs
            var dep = new TransformJob()
            {
                dataId = this.runningJobDataCounter
            }.Schedule(jobData.meshInstances.Count, 4);
            dep = new CopyJob()
            {
                dataId = this.runningJobDataCounter
            }.Schedule(dep);
            jobData.jobHandle = dep;

            jobData.jobId = this.runningJobDataCounter++;
            this.runningJobs.Add(jobData.jobId, jobData);
            JobHandle.ScheduleBatchedJobs();
        }

        /// <summary>
        /// Finishes the mesh rebuild, should only be called if the running job was finished.
        /// </summary>
        private void FinishMeshRebuild(StaticBatchingRebuildJobData jobData)
        {
            var groupKey = jobData.groupKey;

            // Flush rebuild
            Mesh m;
            if (this.meshes.TryGetValue(groupKey, out m))
            {
                Destroy(m);
                this.meshes.Remove(groupKey);
            }

            // Create mesh
            m = new Mesh();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            this.meshes.Add(groupKey, m);
            m.RecalculateBounds();

            // Flush
            jobData.Flush(m);
            this.runningJobs.Remove(jobData.jobId);
        }

        /// <summary>
        /// Updates the rebuild system.
        /// </summary>
        private void RebuildUpdate()
        {
            if (this.runningJobs.Count == 0)
                return;

            // Get first finished job
            uint dataId = 0;
            StaticBatchingRebuildJobData jobData = default(StaticBatchingRebuildJobData);
            bool found = false;
            
            foreach (var kvp in this.runningJobs)
            {
                var val = kvp.Value;
                if (val.jobHandle.IsCompleted)
                {
                    dataId = kvp.Key;
                    jobData = val;
                    found = true;
                    break;
                }
            }

            if (!found)
                return;

            FinishMeshRebuild(jobData);
        }

        #endregion

        #region Rendering / Sync

        /// <summary>
        /// Will flush the <see cref="commandQueue"/>
        /// </summary>
        private void FlushCommandQueue()
        {
            int index;
            HashSet<StaticBatchingGroupKey> rebuilds = HashSetPool<StaticBatchingGroupKey>.Get();

            while (this.commandQueue.Count > 0)
            {
                // Get command queue index
                index = this.commandQueue.Dequeue();

                if (index < 0)
                {
                    // Destroy command
                    DestroyCommand(this.destroyQueue[(-index)-1], rebuilds);
                }
                else
                {
                    // Insert command
                    InsertCommand(this.insertQueue[index-1], rebuilds);
                }
            }

            // Queue rebuilds
            foreach (var rebuild in rebuilds)
                QueueRebuild(rebuild);

            // Clean up
            this.insertQueue.Clear();
            this.destroyQueue.Clear();
            HashSetPool<StaticBatchingGroupKey>.Return(rebuilds);
        }

        /// <summary>
        /// Executes a <see cref="BatchingCommandDestroy"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        private void DestroyCommand(BatchingCommandDestroy command, HashSet<StaticBatchingGroupKey> rebuilds)
        {
            List<int> removeIndices = ListPool<int>.Get();

            // Check all instances for owner
            foreach (var kvp in this.meshInstances)
            {
                // Collect remove indices
                var lst = kvp.Value;
                for (int i = 0; i < lst.Count; i++)
                {
                    var ele = lst[i];
                    if (ReferenceEquals(ele.owner, command.owner))
                        removeIndices.Add(i);
                }

                // Remove
                for (int i = 0; i < removeIndices.Count; i++)
                {
                    int index = removeIndices[i] - i;
                    var obj = lst[index];
                    
                    // Queue rebuild & remove from instance list
                    rebuilds.Add(obj.groupKey);
                    lst.RemoveAt(index);
                }

                removeIndices.Clear();
            }

            ListPool<int>.Return(removeIndices);
        }

        /// <summary>
        /// Executes a <see cref="BatchingCommandInsert"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="rebuilds">List containing all groups which require a rebuild.</param>
        private void InsertCommand(BatchingCommandInsert command, HashSet<StaticBatchingGroupKey> rebuilds)
        {
            // Build group key
            var groupKey = new StaticBatchingGroupKey()
            {
                chunk = GetChunk(command.transform.MultiplyPoint3x4(Vector3.zero)),
                layer = command.layer,
                material = command.material
            };

            // Insert instance
            this.meshInstances.GetOrCreate(groupKey).Add(new StaticMeshInstance()
            {
                groupKey = groupKey,
                owner = command.owner,
                transform = command.transform,
                mesh = command.mesh
            });

            // Queue rebuild
            rebuilds.Add(groupKey);
        }

        /// <summary>
        /// Gets the chunk coordinates for the specified worldspace.
        /// </summary>
        private Vector3Int GetChunk(Vector3 worldspace)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldspace.x / (float)this.chunkSize),
                Mathf.FloorToInt(worldspace.y / (float)this.chunkSize),
                Mathf.FloorToInt(worldspace.z / (float)this.chunkSize)
            );
        }

        /// <summary>
        /// Submits the draw calls and checks for finished jobs to synchronize meshes.
        /// </summary>
        public void Update()
        {
            // Flush the command queue generated by calls to the batching system
            FlushCommandQueue();

            // Update rebuild queue
            RebuildUpdate();

            // Render meshes
            foreach (var kvp in this.meshes)
            {
                Graphics.DrawMesh(kvp.Value, Matrix4x4.identity, kvp.Key.material, kvp.Key.layer);
            }
        }

        #endregion
    }
}