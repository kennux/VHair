using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Static utility class providing helper methods for several tasks.
    /// </summary>
    public static class Essentials
    {
        /// <summary>
        /// Returns a random item from the specified list.
        /// </summary>
        public static T RandomItem<T>(this List<T> lst)
        {
            if (lst.Count == 0)
                return default(T);

            return lst[UnityEngine.Random.Range(0, lst.Count)];
        }

        /// <summary>
        /// Returns a random item from the specified array.
        /// </summary>
        public static T RandomItem<T>(this T[] array)
        {
            if (array.Length == 0)
                return default(T);

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Combines 2 hash codes (<see cref="object.GetHashCode"/>).
        /// </summary>
        /// <param name="h">Hashcode 1</param>
        /// <param name="h2">Hashcode 2</param>
        /// <returns>A new hashcode that combines h1 and h2</returns>
        public static int CombineHashCodes(int h, int h2)
        {
            return ((h << 5) + h) ^ h2;
        }

        /// <summary>
        /// Creates an enumerator for a coroutine being called in delay seconds.
        /// </summary>
        public static IEnumerator DelayedInvokeRoutine(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

        /// <summary>
        /// Sends the specified message to all gameobjects recursively.
        /// </summary>
        public static void SendMessageRecursively(this GameObject gameObject, string methodName, object parameter = null, SendMessageOptions sendMessageOptions = SendMessageOptions.DontRequireReceiver)
        {
            foreach (Transform transform in gameObject.transform)
            {
                SendMessageRecursively(transform.gameObject, methodName, parameter, sendMessageOptions);
            }

            if (parameter == null)
                gameObject.SendMessage(methodName, sendMessageOptions);
            else
                gameObject.SendMessage(methodName, parameter, sendMessageOptions);
        }

        /// <summary>
        /// Helper method that does "unity" null checks.
        /// Since fields which are referencing components which are null arent actually null in unity, this is a specialized null equality check to deal with this situation.
        /// </summary>
        public static bool UnityIsNull(object obj)
        {
            return object.ReferenceEquals(obj, null) || obj.Equals(null);
        }
        
        /// <summary>
        /// Tries getting the value for the specified key from the dictionary.
        /// If the value is not found, its created via new TValue().
        /// </summary>
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue val;
            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        /// <summary>
        /// Sets the specified key in the dictionary to the specified value.
        /// 
        /// If the key is not existing yet, a new entry is created in the dictionary.
        /// If it already exists, its being overwritten.
        /// </summary>
        public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
        /// <summary>
        /// Gets the first component in parents.
        /// Returns default(T) if there was no component found.
        /// </summary>
        /// <returns>The first component in parents.</returns>
        /// <param name="transform">Transform.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetFirstComponentInParents<T>(Transform transform)
        {
            if (transform.GetComponent<T>() != null)
                return transform.GetComponent<T>();
            if (transform.parent != null)
                return GetFirstComponentInParents<T>(transform.parent);
            return default(T);
        }

        /// <summary>
        /// Calculates direction vector (unnormalized) from me to other.
        /// </summary>
        /// <returns>The to.</returns>
        /// <param name="me">Me.</param>
        /// <param name="other">Other.</param>
        public static Vector3 DirectionTo(this Vector3 me, Vector3 other)
        {
            return other - me;
        }

        public static Vector2 ToVector2XZ(this Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.z);
        }


        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        /// <summary>
        /// Gets the recursive bounds.
        /// This iterates through all gameobject childs, gets their renderers and composes one bounds box which includes all.
        /// </summary>
        /// <returns>The recursive bounds.</returns>
        /// <param name="go">Go.</param>
        public static Bounds GetRecursiveBounds(GameObject go)
        {
            // TODO: This whole method yields very unstable results
            // Most likely due to no real transformations used to determine points
            // Research why this sometimes yields completely incorrect results

            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();

            Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
            foreach (MeshRenderer renderer in renderers)
            {
                var p = renderer.transform.position;
                p = (go.transform.position - p);

                var c = renderer.GetComponent<Collider>();
                if (c != null)
                    bounds.Encapsulate(c.bounds);
                else
                    bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        public static Bounds TransformBounds(this Transform _transform, Bounds _localBounds)
        {
            var center = _transform.TransformPoint(_localBounds.center);

            // transform the local extents' axes
            var extents = _localBounds.extents;
            var axisX = _transform.TransformVector(extents.x, 0, 0);
            var axisY = _transform.TransformVector(0, extents.y, 0);
            var axisZ = _transform.TransformVector(0, 0, extents.z);

            // sum their absolute value to get the world extents
            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds { center = center, extents = extents };
        }

        /// <summary>
        /// Sets the layer of the given gameobject recursively (to all its children aswell).
        /// </summary>
        /// <param name="go">Go.</param>
        /// <param name="layerNumber">Layer number.</param>
        public static void SetLayerRecursively(GameObject go, int layerNumber)
        {
            foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = layerNumber;
            }
        }

        public static bool HasParameter(this Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        private static BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static T DeserializeBinaryFormatted<T>(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return (T)binaryFormatter.Deserialize(ms);
            }
        }

        public static byte[] SerializeBinaryFormatted<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
