using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class GPUTFXLocalShapeConstraints : HairSimulationPass<GPUTFXPhysicsSimulation>
    {
        public float stiffness = 0.3f;

        private GraphicsBuffer localTransforms;
        private GraphicsBuffer globalTransforms;
        private GraphicsBuffer referenceVectors;

        public override void InitializeSimulation()
        {
            this.localTransforms = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instance.vertexCount, 16);
            this.globalTransforms = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instance.vertexCount, 16);
            this.referenceVectors = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instance.vertexCount, 12);

            HairStrand[] strands = this.instance.asset.CreateStrandDataCopy();
            float3[] vertices = this.instance.asset.CreateVertexDataCopy();

            var localTransforms = new quaternion[instance.vertexCount];
            var globalTransforms = new quaternion[instance.vertexCount];
            var referenceVectors = new float3[instance.vertexCount];

            CalculateTransforms(strands, vertices, localTransforms, globalTransforms, referenceVectors);

            this.localTransforms.SetData(localTransforms);
            this.globalTransforms.SetData(globalTransforms);
            this.referenceVectors.SetData(referenceVectors);
        }

        private void CalculateTransforms(HairStrand[] strands, float3[] vertices, quaternion[] localTransforms, quaternion[] globalTransforms, float3[] referenceVectors)
        {
            for (int i = 0; i < strands.Length; i++)
            {
                HairStrand strand = strands[i];
                int j;

                // First vertex
                Quaternion local;
                local = globalTransforms[strand.firstVertex] = localTransforms[strand.firstVertex] = Quaternion.LookRotation(math.normalize(vertices[strand.firstVertex + 1] - vertices[strand.firstVertex]));

                for (j = strand.firstVertex + 1; j < strand.lastVertex; j++)
                {
                    Vector3 p1 = vertices[j - 1], p2 = vertices[j], d = (p2 - p1);
                    Vector3 vec = Quaternion.Inverse(globalTransforms[j - 1]) * d;
                    if (vec.magnitude < 0.001f)
                        local = Quaternion.identity;
                    else
                        local = Quaternion.LookRotation(vec.normalized);

                    referenceVectors[j] = vec;
                    globalTransforms[j] = globalTransforms[j - 1] * local;
                    localTransforms[j] = local;
                }
            }
        }

        protected override void _SimulationStep(float timestep)
        {
            float stiffness = .5f * Mathf.Min(this.stiffness * timestep, .95f);

            simulation.simulationShader.SetFloat("pStiffness", stiffness);
            simulation.simulationShader.SetVector("pRotation", new Vector4(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w));

            void SetBuffers(int k)
            {
                simulation.simulationShader.SetBuffer(k, "sGlobalTransforms", globalTransforms);
                simulation.simulationShader.SetBuffer(k, "sLocalTransforms", localTransforms);
                simulation.simulationShader.SetBuffer(k, "sRefVectors", referenceVectors);
            }

            var groupKernel = simulation.simulationShader.FindKernel("LocalShapeConstraintsGroup");
            var singleKernel = simulation.simulationShader.FindKernel("LocalShapeConstraintsSingle");
            SetBuffers(groupKernel);
            SetBuffers(singleKernel);

            simulation.DispatchShader(groupKernel, singleKernel, this.simulation.instance.strandCount);
        }

        public void OnDestroy()
        {
            this.localTransforms.Dispose();
            this.globalTransforms.Dispose();
            this.referenceVectors.Dispose();
        }
    }
}