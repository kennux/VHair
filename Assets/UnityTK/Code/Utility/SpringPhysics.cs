using UnityEngine;
using System.Collections;

namespace UnityTK
{
    /// <summary>
    /// Simple implementation of spring physics used for procedural animation.
    /// The spring is defined by its dampening and stiffness.
    /// </summary>
    public class SpringPhysics
    {
        /// <summary>
        /// The current state of the spring.
        /// </summary>
        private Vector3 state = Vector3.zero;
        
        /// <summary>
        /// The stiffness of the spring.
        /// </summary>
        public Vector3 stiffness;

        /// <summary>
        /// The damping of the spring.
        /// </summary>
        public Vector3 damping;

        /// <summary>
        /// The current velocity of the spring.
        /// This velocity will be applied to <see cref="state"/>
        /// </summary>
        private Vector3 velocity;

        public SpringPhysics(Vector3 stiffness, Vector3 damping)
        {
            this.stiffness = stiffness;
            this.damping = damping;
        }

        public Vector3 Get()
        {
            return state;
        }

        /// <summary>
        /// Simulation stepping using fixed delta time.
        /// </summary>
        public void FixedUpdate()
        {
            this.velocity += Vector3.Scale(-this.state, this.stiffness);
            this.velocity = Vector3.Scale(this.velocity, this.damping);

            this.state += this.velocity * Time.fixedDeltaTime;
        }

        public void AddForce(Vector3 force)
        {
            this.velocity += force;
        }
    }
}