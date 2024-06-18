using UnityEngine;

namespace VARLab.Sandbox.Samples
{

    /// <summary>
    ///     A pickup object which floats in the air and waits for a player to roll into it. 
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        public Renderer Renderer;
        public Collider Collider;
        public ParticleSystem Particles;
        public Animator Animator;


        /// <summary>
        ///     The public fields for this object are all obtained in the OnValidate() callback,
        ///     which is called when the component is created or any serialized field on the 
        ///     component is changed. 
        ///     All of these fields are intended to be automatically determined by the components 
        ///     on the attached GameObject. 
        ///     If any are missing, they can be manually serialized, or ignored.
        /// </summary>
        public void OnValidate()
        {
            if (!Renderer) { Renderer = GetComponent<Renderer>(); }
            if (!Collider) { Collider = GetComponent<Collider>(); }
            if (!Particles) { Particles = GetComponent<ParticleSystem>(); }
            if (!Animator) { Animator = GetComponent<Animator>(); }
        }

        /// <summary>
        ///     Unity callback which is called when the "Reset" option is selected in the Editor.
        ///     This can also be called at runtime to reset the object in the scene.
        /// </summary>
        public void Reset()
        {
            enabled = true;

            UpdateComponentState();
        }

        /// <summary>
        ///     Indicates that the Pickup should be marked as "collected". This disables
        ///     the visuals and plays the pickup particles.
        /// </summary>
        public void Collect()
        {
            if (!enabled) { return; }

            // Play particle effect
            if (Particles) { Particles.Play(); }

            enabled = false;

            UpdateComponentState();
        }


        /// <summary>
        ///     Updates the enabled state of relevant components on the GameObject
        ///     based on this component's enabled state.
        /// </summary>
        public void UpdateComponentState()
        {
            // Set mesh state
            if (Renderer) { Renderer.enabled = enabled; }

            // Set animator state
            if (Animator) { Animator.enabled = enabled; }

            // Set collider state
            if (Collider) { Collider.enabled = enabled; }
        }
    }
}