using System;

namespace UnityTK
{
    /// <summary>
    /// Attribute that can be used for a special unity inspector field drawer of the type <see cref="RangedFloat"/>.
    /// 
    /// Taken from "Unite 2016 - Overthrowing the MonoBehaviour Tyranny in a Glorious Scriptable Object Revolution" - https://www.youtube.com/watch?v=6vmRwLYWNRo
    /// </summary>
    public class MinMaxRangeAttribute : Attribute
    {
        public MinMaxRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public float Min { get; private set; }
        public float Max { get; private set; }
    }
}