using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

namespace UnityTK
{
    /// <summary>
    /// Implements a timer system that can run arbitrary tasks in intervals or after a fixed period of time.
    /// </summary>
    public class Timer : MonoBehaviour
    {
        /// <summary>
        /// Data structure for timer entries.
        /// </summary>
        private abstract class TimerTask
        {
            public TimerHandle handle;
            public Action action;
            public object state;

            public void Initialize(TimerHandle handle, Action action, object state)
            {
                this.handle = handle;
                this.action = action;
                this.state = state;
            }

            /// <summary>
            /// Determines whether or not this task should be executed now.
            /// </summary>
            /// <returns></returns>
            public abstract bool ShouldExecute();

            /// <summary>
            /// Runs <see cref="action"/>.
            /// Standard implementation returns always true.
            /// </summary>
            /// <returns>Whether or not this task is done now and can be removed from the task list.</returns>
            public virtual bool Execute()
            {
                if (!object.ReferenceEquals(this.action, null))
                    this.action(this.state);
                return true;
            }

            public override int GetHashCode()
            {
                return Essentials.CombineHashCodes(this.action.GetHashCode(), this.state.GetHashCode());
            }
        }

        /// <summary>
        /// Task that is being executed only once.
        /// </summary>
        private class OneTimeTask : TimerTask
        {
            public float invokeTime;

            public override bool ShouldExecute()
            {
                return Time.time >= this.invokeTime;
            }
        }

        /// <summary>
        /// Task implementation for tasks invoked in regular intervals.
        /// </summary>
        private class IntervalTasks : TimerTask
        {
            public float invokeNext;
            public float interval;

            public override bool ShouldExecute()
            {
                return Time.time >= this.invokeNext;
            }

            public override bool Execute()
            {
                this.invokeNext = Time.time + this.interval;
                base.Execute();
                return false;
            }
        }

        public class Handle
        {
            public TimerEvent taskEvent { get { return this._taskEvent; } }
            private object taskObj;
            private TimerEvent _taskEvent;

            public bool isInvalidated { get { return this._isInvalidated; } }
            private bool _isInvalidated;

            /// <summary>
            /// Called when the <see cref="Timer"/> creates a new handle.
            /// Sets the appropriate fields and sets the invalidation flag to false.
            /// </summary>
            public void SetData(object taskObj, TimerEvent taskEvent)
            {
                if (!(taskObj is TimerTask))
                {
                    Debug.Log("Unauthorized timer handle data set call!");
                    return;
                }

                this.taskObj = taskObj;
                this._taskEvent = taskEvent;
                this._isInvalidated = false;
            }

            /// <summary>
            /// Stops this timer handle.
            /// </summary>
            public void Stop()
            {
                if (this.isInvalidated)
                {
                    Debug.LogError("Tried stopping invalidated timer handle");
                    return;
                }

                Timer.instance.ForceStopFromHandle(this.taskObj, this._taskEvent);
                this._isInvalidated = true;
            }

            public override int GetHashCode()
            {
                return Essentials.CombineHashCodes(this.taskEvent.GetHashCode(), this.taskObj.GetHashCode());
            }
        }

        /// <summary>
        /// Delegate used to pass in arbitrary tasks.
        /// </summary>
        /// <param name="state">The state this action is being invoked with.</param>
        public delegate void Action(object state);

        /// <summary>
        /// Singleton pattern JIT instantiating a timer manager.
        /// </summary>
        public static Timer instance
        {
            get
            {
                if (Essentials.UnityIsNull(_instance))
                {
                    var go = new GameObject("Timer");
                    _instance = go.AddComponent<Timer>();
                }
                return _instance;
            }
        }
        private static Timer _instance;

        /// <summary>
        /// Object pool for timer handles returned by the api.
        /// </summary>
        private ObjectPool<TimerHandle> handlesPool = new ObjectPool<TimerHandle>(() => new TimerHandle());
        private ObjectPool<OneTimeTask> oneTimeTaskPool = new ObjectPool<OneTimeTask>(() => new OneTimeTask());
        private ObjectPool<IntervalTasks> intervalTaskPool = new ObjectPool<IntervalTasks>(() => new IntervalTasks());

        /// <summary>
        /// All currently open tasks.
        /// Index maps directly to <see cref="TimerEvent"/>
        /// </summary>
        private List<TimerTask>[] tasks = new List<TimerTask>[3];

        /// <summary>
        /// Maps timer handles to their respective timer task (from <see cref="tasks"/>).
        /// </summary>
        private Dictionary<TimerHandle, TimerTask> taskHandleMapping = new Dictionary<TimerHandle, TimerTask>();

        public void Awake()
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i] = new List<TimerTask>();
        }

        /// <summary>
        /// Recycles a previously returned timer handle in order to lower GC pressure (by pooling done internally).
        /// </summary>
        public void ReturnTimerHandle(TimerHandle handle)
        {
            this.handlesPool.Return(handle);
        }

        /// <summary>
        /// Called from <see cref="Handle.Stop"/> to remove a task from the queue.
        /// </summary>
        /// <param name="taskObj"></param>
        /// <param name="evt"></param>
        private void ForceStopFromHandle(object taskObj, TimerEvent evt)
        {
            TimerTask task = (taskObj as TimerTask);

            this.tasks[(int)evt].Remove(task);
            this.taskHandleMapping.Remove(task.handle);
        }

        /// <summary>
        /// Invokes the specified action after waiting for seconds.
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TimerHandle InvokeIn(float seconds, Action action, TimerEvent evt, object state = null)
        {
            // Get objects
            var task = this.oneTimeTaskPool.Get();
            var handle = this.handlesPool.Get();

            // Setup task
            task.invokeTime = Time.time + seconds;
            task.Initialize(handle, action, state);

            // Setup handle
            handle.SetData(task, evt);

            // Register
            this.tasks[(int)evt].Add(task);
            this.taskHandleMapping.Add(handle, task);
            return handle;
        }

        /// <summary>
        /// Invokes the specified action in the specified interval until the handle is being destroyed.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="startInvokingIn"></param>
        /// <returns></returns>
        public TimerHandle InvokeInInterval(float interval, Action action, TimerEvent evt, object state = null, float startInvokingIn = 0)
        {
            // Get objects
            var task = this.intervalTaskPool.Get();
            var handle = this.handlesPool.Get();

            // Setup task
            task.interval = interval;
            task.invokeNext = Time.time + startInvokingIn;
            task.Initialize(handle, action, state);

            // Setup handle
            handle.SetData(task, evt);

            // Register
            this.tasks[(int)evt].Add(task);
            this.taskHandleMapping.Add(handle, task);
            return handle;
        }

        /// <summary>
        /// Checks all tasks in the specified list and runs them if needed.
        /// This is a method called from all <see cref="TimerEvent"/> method implementations.
        /// 
        /// </summary>
        /// <param name="tasks"></param>
        private void RunTasks(List<TimerTask> tasks)
        {
            List<TimerTask> removeTasks = ListPool<TimerTask>.Get();

            // Check & exec all tasks
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                if (task.ShouldExecute() && task.Execute())
                    removeTasks.Add(task);
            }

            // Remove tasks
            for (int i = 0; i < removeTasks.Count; i++)
            {
                var task = removeTasks[i];
                tasks.Remove(task);
                this.taskHandleMapping.Remove(task.handle);
            }

            ListPool<TimerTask>.Return(removeTasks);
        }

        /// <summary>
        /// <see cref="TimerEvent.LATE_UPDATE"/>
        /// </summary>
        public void LateUpdate()
        {
            RunTasks(this.tasks[(int)TimerEvent.LATE_UPDATE]);
        }

        /// <summary>
        /// <see cref="TimerEvent.FIXED_UPDATE"/>
        /// </summary>
        public void FixedUpdate()
        {
            RunTasks(this.tasks[(int)TimerEvent.FIXED_UPDATE]);
        }

        /// <summary>
        /// <see cref="TimerEvent.UPDATE"/>
        /// </summary>
        public void Update()
        {
            RunTasks(this.tasks[(int)TimerEvent.UPDATE]);
        }
    }
}