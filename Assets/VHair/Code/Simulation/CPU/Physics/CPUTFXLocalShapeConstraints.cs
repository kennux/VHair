using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
    public class CPUTFXLocalShapeConstraints : HairSimulationPass<CPUTFXPhysicsSimulation>
    {
        public float stiffness;

        private Quaternion[] initialLocalTransform;
        private Quaternion[] prevFrameLocalTransform;
        private Quaternion[] currentLocalTransform;

        public override void InitializeSimulation()
        {
            this.initialLocalTransform = new Quaternion[this.instance.vertexCount];
            this.prevFrameLocalTransform = new Quaternion[this.instance.vertexCount];
            this.currentLocalTransform = new Quaternion[this.instance.vertexCount];

            HairStrand[] strands = this.instance.strands.cpuReference;
            Vector3[] vertices = this.instance.vertices.cpuReference;
            this.CalculateTransforms(strands, vertices, this.initialLocalTransform);

            System.Array.Copy(this.initialLocalTransform, this.prevFrameLocalTransform, this.initialLocalTransform.Length);
            System.Array.Copy(this.initialLocalTransform, this.currentLocalTransform, this.initialLocalTransform.Length);
        }

        private void CalculateTransforms(HairStrand[] strands, Vector3[] vertices, Quaternion[] transforms)
        {
            for (int i = 0; i < strands.Length; i++)
            {
                Quaternion prev = Quaternion.identity;
                HairStrand strand = strands[i];
                int j;
                for (j = strand.firstVertex; j < strand.lastVertex; j++)
                {
                    Vector3 p1 = vertices[j], p2 = vertices[j + 1];
                    prev = Quaternion.Inverse(prev) * Quaternion.LookRotation((p2 - p1).normalized);

                    transforms[j] = prev;
                }

                transforms[j] = transforms[strand.firstVertex]; // Set last to transform before
            }
        }

        protected override void _SimulationStep(float timestep)
        {
            HairStrand[] strands = this.instance.strands.cpuReference;
            Vector3[] vertices = this.instance.vertices.cpuReference;
            for (int i = 0; i < strands.Length; i++)
            {
                HairStrand strand = strands[i];
                for (int j = strand.firstVertex; j < strand.lastVertex; j++)
                {
                }
            }
        }
    }
}