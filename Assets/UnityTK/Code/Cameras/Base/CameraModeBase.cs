using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTK.Cameras
{
    /// <summary>
    /// Generic base class for implementing <see cref="CameraMode"/>s.
    /// 
    /// This class implements integration of <see cref="CameraModeInput{T}"/>.
    /// </summary>
    /// <typeparam name="TInputData">The input data for this camera mode. <see cref="CameraModeInput{T}"/></typeparam>
    public abstract class CameraModeBase<TInputData> : CameraMode
    {
        /// <summary>
        /// All input implementations this camera has available.
        /// </summary>
        public ReadOnlyList<CameraModeInput<TInputData>> inputs
        {
            get { return this._inputs; }
        }
        private List<CameraModeInput<TInputData>> _inputs = new List<CameraModeInput<TInputData>>();

        /// <summary>
        /// The input data, not merged and only used as a temporary storage.
        /// Collected from all inputs in <see cref="_inputs"/>.
        /// </summary>
        private Dictionary<CameraModeInput<TInputData>, TInputData> unmergedInputData = new Dictionary<CameraModeInput<TInputData>, TInputData>();

        /// <summary>
        /// The merged input data.
        /// <seealso cref="unmergedInputData"/>
        /// <seealso cref="MergeInputData(Dictionary{CameraModeInput{TInputData}, TInputData})"/>
        /// </summary>
        protected TInputData inputData;

        /// <summary>
        /// Sets up <see cref="_inputs"/>
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            this.GetComponentsInChildren<CameraModeInput<TInputData>>(this._inputs);
            if (this._inputs.Count == 0)
            {
                Debug.LogError("UnityTK Camera without inputs! Disabling camera!", this);
                this.enabled = false;
                return;
            }
        }

        public override void UpdateMode(ref CameraState cameraState)
        {
            _UpdateInput();
            _UpdateMode(ref cameraState);
        }

        /// <summary>
        /// Updates the camera mode, called after <see cref="_UpdateInput"/>
        /// </summary>
        /// <param name="camera">The camera state update.</param>
        protected abstract void _UpdateMode(ref CameraState cameraState);

        /// <summary>
        /// Called right before <see cref="_UpdateMode(Camera)"/> in order to update camera input.
        /// </summary>
        protected virtual void _UpdateInput()
        {
            this.unmergedInputData.Clear();
            foreach (var i in this._inputs)
            {
                this.unmergedInputData.Add(i, i.GetData());
            }

            this.inputData = MergeInputData(this.unmergedInputData);
        }

        /// <summary>
        /// Merges input data from multiple inputs.
        /// </summary>
        /// <param name="data">The data to be merged.</param>
        /// <returns>Merge input data for this camera mode.</returns>
        protected abstract TInputData MergeInputData(Dictionary<CameraModeInput<TInputData>, TInputData> data);
    }
}
