using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Cameras
{
    public static class CameraUtil
    {
        public static Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time, float firstHalfAdjust = 1.0f)
        {
            float secondHalfAdjust = 2.0f - firstHalfAdjust;
            if (time <= .5)
                time = time * firstHalfAdjust;
            else
                time = 0.5f * (firstHalfAdjust) + (time - .5f) * (secondHalfAdjust);


            Matrix4x4 ret = new Matrix4x4();
            for (int i = 0; i < 16; i++)
                ret[i] = Mathf.Lerp(from[i], to[i], time);
            return ret;
        }
    }
}
