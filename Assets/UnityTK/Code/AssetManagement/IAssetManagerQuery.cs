using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTK.AssetManagement
{
    public interface IAssetManagerQuery
    {
        /// <summary>
        /// Called in order to reset the asset manager query.
        /// </summary>
        void Reset();

        /// <summary>
        /// Called in order to determine whether the specified asset fits the criterias of this query.
        /// </summary>
        bool MatchesCriterias(IManagedAsset asset);
    }
}