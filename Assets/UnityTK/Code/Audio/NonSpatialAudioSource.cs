using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTK.Audio
{
    /// <summary>
    /// Specialized audio source for UnityTK.
    /// This audio source completely ignores spatial parameters and plays back a sound at a constant volume level.
    /// </summary>
    public class NonSpatialAudioSource : UTKAudioSource
    {
        // Disable setting properties of spatial audio
        public override float minDistance
        {
            get
            {
                return base.minDistance;
            }

            set
            {
            }
        }

        public override float maxDistance
        {
            get
            {
                return base.maxDistance;
            }

            set
            {
            }
        }

        public override AudioRolloffMode rolloffMode
        {
            get
            {
                return base.rolloffMode;
            }

            set
            {
            }
        }

        public override void ResetConfig()
        {
            base.ResetConfig();

            this.underlying.spatialBlend = 0;
            this.underlying.panStereo = 0;
        }
    }
}
