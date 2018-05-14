using System;
using UnityEngine;

namespace UnityTK
{
    public static class FloatRemap
    {

        /// <summary>
        /// Scales the range between two values. Takes reverse input & output range.
        /// </summary>
        /// <returns>The range.</returns>
        /// <param name="value">Value.</param>
        /// <param name="oldMin">Old minimum.</param>
        /// <param name="oldMax">Old max.</param>
        /// <param name="newMin">New minimum.</param>
        /// <param name="newMax">New max.</param>
        public static float Remap(this float value, float oldMin, float oldMax, float newMin, float newMax)
        {

            //check reversed input range
            float oMin = Mathf.Min(oldMin, oldMax);
            float oMax = Mathf.Max(oldMin, oldMax);
            bool reverseInput = oMin != oldMin;

            //check reversed output range
            float nMin = Mathf.Min(newMin, newMax);
            float nMax = Mathf.Max(newMin, newMax);

            var reverseOutput = Math.Abs(nMin - newMin) > float.Epsilon;

            var portion = reverseInput
                ? (oMax - value) * (nMax - nMin) / (oMax - oMin)
                : (value - oMin) * (nMax - nMin) / (oMax - oMin);

            var result = reverseOutput ? nMax - portion : portion + nMin;

            return result;
        }
    }
}