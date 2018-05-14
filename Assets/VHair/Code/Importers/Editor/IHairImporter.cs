using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair.Editor
{
    public interface IHairAssetImporter
    {
        string displayName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hairAsset"></param>
        /// <returns>Whether or not the importer can import data with current settings.</returns>
        bool OnInspectorGUI(HairAsset hairAsset);

        /// <summary>
        /// <see cref="IHairAsset.SetData(Vector3[], HairStrand[])"/>
        /// </summary>
        void Import(out Vector3[] vertices, out HairStrand[] strands);
    }
}