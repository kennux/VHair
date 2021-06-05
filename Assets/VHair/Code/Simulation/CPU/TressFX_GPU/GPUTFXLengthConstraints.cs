using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
    public class GPUTFXLengthConstraints : HairSimulationPass<GPUTFXPhysicsSimulation>
    {
        public float stiffness = 1;

        private GraphicsBuffer lengths;

        public override void InitializeSimulation()
        {
            var lengths = new NativeArray<float>(this.instance.vertexCount, Allocator.Temp);
            try
            {
                NativeArray<HairStrand> strands = this.instance.strands.CpuReference;
                NativeArray<float3> vertices = this.instance.vertices.CpuReference;

                for (int s = 0; s < strands.Length; s++)
                {
                    HairStrand strand = strands[s];
                    for (int j = strand.firstVertex + 1; j < strand.lastVertex; j++)
                    {
                        lengths[j] = math.distance(vertices[j], vertices[j - 1]);
                    }
                }

                this.lengths = new GraphicsBuffer(GraphicsBuffer.Target.Structured, lengths.Length, 4);
                this.lengths.SetData(lengths);
            } finally { lengths.Dispose(); }
        }

        protected override void _SimulationStep(float timestep)
        {
            simulation.simulationShader.SetFloat("pStiffness", stiffness);
            int kernelLengthConstraintsGroup = simulation.simulationShader.FindKernel("LengthConstraintsGroup");
            int kernelLengthConstraintsSingle = simulation.simulationShader.FindKernel("LengthConstraintsSingle");

            simulation.simulationShader.SetBuffer(kernelLengthConstraintsGroup, "sLengths", lengths);
            simulation.simulationShader.SetBuffer(kernelLengthConstraintsSingle, "sLengths", lengths);

            simulation.DispatchShader(kernelLengthConstraintsGroup, kernelLengthConstraintsSingle, instance.strandCount);
        }

        public void OnDestroy()
        {
            lengths.Dispose();
        }
    }
}