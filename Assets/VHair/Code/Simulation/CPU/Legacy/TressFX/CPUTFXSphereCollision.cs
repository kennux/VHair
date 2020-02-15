using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityTK;

namespace VHair
{
    public class CPUTFXSphereCollision : HairSimulationPass<CPUTFXPhysicsSimulation>
    {
        public SphereCollider[] colliders;

        private struct CollisionSphere
        {
            public Vector3 center;
            public float radius;
            public float radiusSq;
        }

        public override void InitializeSimulation()
        {

        }

        protected override void _SimulationStep(float timestep)
        {
            List<CollisionSphere> colliders = ListPool<CollisionSphere>.Get();

            for (int i = 0; i < this.colliders.Length; i++)
            {
                colliders.Add(new CollisionSphere()
                {
                    center = this.colliders[i].transform.TransformPoint(this.colliders[i].center),
                    radius = this.colliders[i].radius,
                    radiusSq = this.colliders[i].radius * this.colliders[i].radius
                });
            }
            int colliderCount = colliders.Count;

            NativeArray<float3> vertices = this.instance.vertices.CpuReference;
            NativeArray<uint> movability = this.instance.movability.CpuReference;

            bool wasAnythingModified = false;
            for (int i = 0; i < vertices.Length; i++)
            {
                // Is hair movable?
                if (HairMovability.IsMovable(i, movability))
                {
                    bool wasModified = false;
                    Vector3 v = vertices[i];
                    for (int j = 0; j < colliderCount; j++)
                    {
                        // Read collider
                        var c = colliders[j];

                        // Intersection?
                        Vector3 dir = (v - c.center);
                        float sqrMag = dir.sqrMagnitude;
                        if (sqrMag <= c.radiusSq) // if (Vector3.Distance(v, c.center) <= c.radius)
                        {
                            // Intersection! push the vertex out
                            float d = Mathf.Sqrt(sqrMag); // dir.magnitude;
                            v += (dir / d) * (c.radius - d);
                            wasModified = true;
                        }
                    }

                    if (wasModified)
                        vertices[i] = v; // Sync
                    wasAnythingModified |= wasModified;
                }
            }

            if (wasAnythingModified)
                this.instance.vertices.SetGPUDirty();

            ListPool<CollisionSphere>.Return(colliders);
        }
    }
}