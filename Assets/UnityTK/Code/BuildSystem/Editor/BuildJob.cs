using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnityTK.BuildSystem
{
    /// <summary>
    /// Build job implementation.
    /// A build job is composed of multiple <see cref="BuildTask"/>s.
    /// 
    /// When the job is being executed by the <see cref="BuildSystem"/>, all tasks are being executed in order.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildJob", menuName = "UnityTK/BuildSystem/Job")]
    public class BuildJob : ScriptableObject
    {
        /// <summary>
        /// All tasks of this job.
        /// </summary>
        public BuildTask[] tasks;

        public string destination;
        public bool deleteExistingDestination;

        /// <summary>
        /// Executes all tasks
        /// </summary>
        public void Run(BuildJobParameters parameters)
        {
            if (parameters.deleteExistingDestination && Directory.Exists(parameters.destination))
                Directory.Delete(parameters.destination, true);

            if (!Directory.Exists(parameters.destination))
                Directory.CreateDirectory(parameters.destination);

            for (int i = 0; i < this.tasks.Length; i++)
                this.tasks[i].Run(this, parameters);
        }
    }
}