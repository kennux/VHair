using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Datastructure holding 2 floats for min / max value.
    /// 
    /// Taken from "Unite 2016 - Overthrowing the MonoBehaviour Tyranny in a Glorious Scriptable Object Revolution" - https://www.youtube.com/watch?v=6vmRwLYWNRo
    /// </summary>
    [System.Serializable]
    public struct RangedFloat
    {
        public float minValue;
        public float maxValue;

        public RangedFloat(float min, float max)
        {
            this.minValue = min;
            this.maxValue = max;
        }

        /// <summary>
        /// Returns a random float inbetween <see cref="minValue"/> and <see cref="maxValue"/>.
        /// </summary>
        /// <returns>Random float in range.</returns>
        public float GetRandomInRange()
        {
            return Random.Range(this.minValue, this.maxValue);
        }
    }
}