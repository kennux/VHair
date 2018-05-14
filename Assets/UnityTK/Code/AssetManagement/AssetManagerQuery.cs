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

            return true;
        }
    }
}
