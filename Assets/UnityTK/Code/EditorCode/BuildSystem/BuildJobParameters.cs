using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTK.BuildSystem
{
    /// <summary>
    /// Data model for <see cref="BuildJob"/> parameters.
    /// </summary>
    public class BuildJobParameters
    {
        /// <summary>
        /// The build destination
        /// </summary>
        public string destination
        {
            get
            {
                return this._destination;
            }
        }
        private string _destination;

        /// <summary>
        /// Whether or not the job should be deleting an already existing <see cref="destination"/>.
        /// </summary>
        public bool deleteExistingDestination
        {
            get
            {
                return this._deleteExistingDestination;
            }
        }
        private bool _deleteExistingDestination;


        public BuildJobParameters(string destination, bool deleteExistingDestination = true)
        {
            this._destination = destination;
            this._deleteExistingDestination = deleteExistingDestination;
        }
    }
}
