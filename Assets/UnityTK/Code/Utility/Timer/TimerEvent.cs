using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// This enum specifies all possible unity events where a task can be ran.
    /// </summary>
    public enum TimerEvent : int
    {
        UPDATE = 0,
        FIXED_UPDATE = 1,
        LATE_UPDATE = 2
    }
}