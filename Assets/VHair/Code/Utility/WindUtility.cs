using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace VHair
{
	public static class WindUtility
	{
		private static float Pulse(float time, float frequency)
		{
			return .5f * (1 + Mathf.Sin(2 * Mathf.PI * frequency * time));
		}

		public static Vector3 EvaluateWindForce(WindZone zone, Vector3 point)
		{
			float pulse = Pulse(Time.time, zone.windPulseFrequency) * zone.windPulseMagnitude;
			var turbulence = zone.windTurbulence * pulse * zone.transform.TransformDirection(new Vector3(Mathf.PerlinNoise(Time.time, 0)*.1f, Mathf.PerlinNoise(0, Time.time)*.1f,1)).normalized;
			var windMain = zone.transform.forward * zone.windMain;

			return (windMain + turbulence) * .5f;
		}
	}
}
