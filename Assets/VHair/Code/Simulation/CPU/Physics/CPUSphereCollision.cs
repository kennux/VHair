using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

namespace VHair
{
    public class CPUSphereCollision : HairSimulationPass<CPUPhysicsSimulation>
    {
        public float pushForce = 50;
        public SphereCollider[] colliders;

        private struct CollisionSphere
        {
            public Vector3 center;
            public float radius;
        }

        public override void InitializeSimulation()
        {

        }

        public override void SimulationStep(float timestep)
        {
            List<CollisionSphere> colliders = ListPool<CollisionSphere>.Get();

            for (int i = 0; i < this.colliders.Length; i++)
            {
                colliders.Add(new CollisionSphere()
                {
                    center = this.colliders[i].transform.TransformPoint(this.colliders[i].center),
                    radius = this.colliders[i].radius
                });
            }
            int colliderCount = colliders.Count;

            Vector3[] vertices = this.instance.vertices.cpuReference;
            uint[] movability = this.instance.movability.cpuReference;

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
                        if (Vector3.Distance(v, c.center) <= c.radius)
                        {
                            // Intersection! push the vertex out
                            Vector3 dir = (v - c.center).normalized;
                            v += dir * this.pushForce * timestep;
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