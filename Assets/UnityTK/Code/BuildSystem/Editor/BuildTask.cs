using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BuildSystem
{
    /// <summary>
    /// Abstract implementation of a build task that can be used on a <see cref="BuildJob"/>.
    /// </summary>
    public abstract class BuildTask : ScriptableObject
    {
        public abstract void Run(BuildJob job, BuildJobParameters parameters);
    }
}