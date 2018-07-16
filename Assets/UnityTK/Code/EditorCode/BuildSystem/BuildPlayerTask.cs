using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace UnityTK.BuildSystem
{
    /// <summary>
    /// Build task that builds a player.
    /// </summary>
    [CreateAssetMenu(fileName = "Build Player Task", menuName = "UnityTK/BuildSystem/Build Player Task")]
    public class BuildPlayerTask : BuildTask
    {
        [Header("Task")]
        public string subfolder;

        /// <summary>
        /// The name of the compiled player with its extension.
        /// </summary>
        public string playerName = "player.exe";

        [Header("Build config")]
        [Tooltip("Relative to the build destination")]
        public string assetBundleManifestPath;
        public bool developmentBuild;
        public bool scriptDebugging;
        public BuildTarget buildTarget;
        public BuildTargetGroup buildTargetGroup;

        [Header("Scenes build")]
        public bool overrideScenes;
        public string[] overrideScenePaths;

        public override void Run(BuildJob job, BuildJobParameters parameters)
        {
            string targetFolder = Path.Combine(parameters.destination, this.subfolder);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            // Build build option
            BuildOptions buildOptions = BuildOptions.None;
            if (this.developmentBuild)
                buildOptions |= BuildOptions.Development;
            if (this.scriptDebugging)
                buildOptions |= BuildOptions.AllowDebugging;

            BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                assetBundleManifestPath = Path.Combine(parameters.destination, this.assetBundleManifestPath),
                options = buildOptions,
                locationPathName = Path.Combine(targetFolder, this.playerName),
                scenes = this.overrideScenes ? this.overrideScenePaths : EditorBuildSettings.scenes.Where((s) => s.enabled).Select((s) => s.path).ToArray(),
                target = this.buildTarget,
                targetGroup = this.buildTargetGroup
            });
        }
    }
}