using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTK.DataBinding
{
    /// <summary>
    /// Interface providing API for implementing databindings.
    /// </summary>
    public interface IDataBinding
    {
        /// <summary>
        /// The object this databinding is binding to.
        /// </summary>
        object boundObject
        {
            get;
        }

        /// <summary>
        /// The type this binding was bound to (the type of <see cref="boundObject"/>).
        /// </summary>
        System.Type boundType
        {
            get;
        }

        /// <summary>
        /// The parent binding node.
        /// Roots might not have a parent (null).
        /// </summary>
        DataBinding parent
        {
            get;
        }

        /// <summary>
        /// Called whenever this binding needs to be updated.
        /// </summary>
        void UpdateBinding();

        /// <summary>
        /// Returns the target type this databinding is accepting.
        /// This is not necessarily the actual type of the <see cref="boundObject"/>.
        /// 
        /// It is used to determine which types can be assigned to this databinding.
        /// In most cases this will be the upper most type in the inheritance tree this binding can accept.
        /// For example a root that can bind to any object will return object as target type, while a text leaf will return string for example.
        /// </summary>
        Type GetBindTargetType();
    }
}
