using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTK.AssetManagement
{
    /// <summary>
    /// Basic implementation of <see cref="IAssetManagerQuery"/> offering tag / name filtering.
    /// </summary>
    public class AssetManagerQuery : IAssetManagerQuery
    {
        /// <summary>
        /// Returns a pooled asset manager query with a tag-criteria matching the specifiedd tag.
        /// This is a shorthand for retrieving a query from <see cref="AssetManagerQueryPool{T}"/> and manually calling <see cref="AddTagCriteria(string)"/>.
        /// </summary>
        /// <param name="tag">The tag the query should match for.</param>
        public static AssetManagerQuery GetPooledTagQuery(string tag)
        {
            var query = AssetManagerQueryPool<AssetManagerQuery>.Get();
            query.AddTagCriteria(tag);

            return query;
        }

        /// <summary>
        /// Returns a pooled asset manager query with a tag-criteria matching the specifiedd tag.
        /// This is a shorthand for retrieving a query from <see cref="AssetManagerQueryPool{T}"/> and manually calling <see cref="AddNameCriteria(string)"/>.
        /// </summary>
        /// <param name="name">The name the query should match for.</param>
        public static AssetManagerQuery GetPooledNameQuery(string name)
        {
            var query = AssetManagerQueryPool<AssetManagerQuery>.Get();
            query.AddTagCriteria(name);

            return query;
        }

        /// <summary>
        /// Returns a pooled asset manager query with a tag-criteria matching the specifiedd tag.
        /// This is a shorthand for retrieving a query from <see cref="AssetManagerQueryPool{T}"/> and manually calling <see cref="AddTypeCriteria{T}"/>.
        /// </summary>
        public static AssetManagerQuery GetPooledTypeQuery<T>() where T : UnityEngine.Object
        {
            var query = AssetManagerQueryPool<AssetManagerQuery>.Get();
            query.AddTypeCriteria<T>();

            return query;
        }

        /// <summary>
        /// Returns a pooled asset manager query with a tag-criteria matching the specifiedd tag.
        /// This is a shorthand for retrieving a query from <see cref="AssetManagerQueryPool{T}"/> and manually calling <see cref="AddTypeCriteria(Type)"/>.
        /// </summary>
        public static AssetManagerQuery GetPooledTypeQuery(Type type)
        {
            var query = AssetManagerQueryPool<AssetManagerQuery>.Get();
            query.AddTypeCriteria(type);

            return query;
        }

        /// <summary>
        /// The tag filter criteria.
        /// Selected assets need to have all of those tags.
        /// </summary>
        protected List<string> tags = new List<string>();

        /// <summary>
        /// Names filter criteria.
        /// Selected assets will have one of those names.
        /// </summary>
        protected HashSet<string> names = new HashSet<string>();

        /// <summary>
        /// The types filter criteria.
        /// Selected assets will always be of atleast one of the types specified.
        /// </summary>
        protected HashSet<System.Type> types = new HashSet<Type>();
        
        /// <summary>
        /// Adds the specified tag to the list of required tags (<see cref="tags"/>).
        /// </summary>
        public void AddTagCriteria(string tag)
        {
            ListPool<string>.GetIfNull(ref this.tags);
            this.tags.Add(tag);
        }

        /// <summary>
        /// Adds the specified name to the list of names searching for (<see cref="names"/>).
        /// </summary>
        public void AddNameCriteria(string name)
        {
            HashSetPool<string>.GetIfNull(ref this.names);
            this.names.Add(name);
        }

        /// <summary>
        /// Adds the specified type as type-criteria to this query.
        /// </summary>
        public void AddTypeCriteria<T>() where T : UnityEngine.Object
        {
            AddTypeCriteria(typeof(T));
        }

        /// <summary>
        /// Non-generic version of <see cref="AddTypeCriteria{T}"/>.
        /// </summary>
        public void AddTypeCriteria(Type type)
        {
            HashSetPool<Type>.GetIfNull(ref this.types);
            this.types.Add(type);
        }

        /// <summary>
        /// Resets the query manager (all its criterias).
        /// </summary>
        public virtual void Reset()
        {
            if (!ReferenceEquals(this.tags, null))
                this.tags.Clear();
            if (!ReferenceEquals(this.names, null))
                this.names.Clear();
        }

        /// <summary>
        /// Called in order to determine whether or not the specified asset fits the criterias.
        /// </summary>
        public virtual bool MatchesCriterias(IManagedAsset asset)
        {
            if (!ReferenceEquals(this.tags, null) && this.tags.Count > 0)
            {
                var tags = asset.tags;
                for (int i = 0; i < this.tags.Count; i++)
                {
                    // Look for the tag
                    bool didntHaveTag = true;
                    for (int j = 0; j < tags.Length; j++)
                    {
                        if (tags[j].Equals(this.tags[i]))
                        {
                            didntHaveTag = false;
                            break;
                        }
                    }

                    // Didnt have the tag?
                    if (didntHaveTag)
                        return false;
                }
            }

            // Look for name
            if (!ReferenceEquals(this.names, null) && this.names.Count > 0)
            {
                if (!this.names.Contains(asset.name))
                    return false;
            }

            // Look for type
            foreach (var t in this.types)
            {
                if (ReferenceEquals(asset, null) || !t.IsAssignableFrom(asset.GetType()))
                    return false;
            }

            return true;
        }
    }
}
