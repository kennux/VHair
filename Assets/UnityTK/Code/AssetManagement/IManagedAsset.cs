using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// This interface is used to implement managable assets.
    /// 
    /// Manageable assets are assets which can be loaded and managed via <see cref="AssetManager"/>.
    /// They are the core concept of the <see cref="AssetManagement"/>.
    /// 
    /// Standard implementations of this are <see cref="ManagedScriptableObject"/> and <see cref="ManagedGameObject"/>.
    /// </summary>
    public interface IManagedAsset
    {
        /// <summary>
        /// An identifier that _must_ be unique.
        /// Can be used to unambigously identify an asset or in order to overwrite assets.
        /// </summary>
        string identifier { get; }

        /// <summary>
        /// The name this asset has.
        /// </summary>
        string name { get; }

        /// <summary>
        /// The tags this asset identifier belongs to.
        /// </summary>
        string[] tags { get; }

        /// <summary>
        /// Called in order to retrieve this object as an object of type T.
        /// 
        /// This is used on gameobjects to retrieve a component of type T.
        /// On scriptable objects this should perform a cast.
        /// 
        /// If null is returned, the object doesnt "match" type T.
        /// </summary>
        T GetAs<T>() where T : Object;
    }
}