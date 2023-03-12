// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using Unity.Mathematics;
using UnityEngine;

// Modified from https://github.com/meteyldrm/CET49A-BasketballAir/blob/master/Assets/Scripts/Draggable.cs

namespace Control {
    /// <summary>
    /// The class needs to perform draggable duties as well as drop-and-snap functionality.
    /// To facilitate this, a trigger overlap architecture will be used which will utilize snap targets.
    /// If the draggable is over a snappable field, inverse touch target guidance will animate the material to the snap target.
    /// Additionally, the script must calculate acceleration values and other temporal data for ElectricSpecs to bind with.
    ///
    /// InterceptTouch was called by BallHandler in BasketballAir. It was the script responsible for drag deadlocks and input vector intercept. We will need a similar implementation.
    /// GeneralGuidance will handle touch input for all Draggables.
    /// SetTransformGoal is the smooth movement implementation. screenVector is the variable responsible for target setting.
    ///
    /// Object pool will probably be modified to reproduce prefab instances. We will refer to constructors with material IDs. Consider how we might serialize charge.
    /// 
    /// Predicted Problems:
    /// The draggable target might behave weirdly when on target. Assume satisfactory epsilon distance and disable the movement checks, after instantiation for example.
    /// Is a circle collider appropriate for every material? Playtest with ElectricSpecs accumulation is necessary.
    /// </summary>
    public class Draggable : MonoBehaviour {
        private Rigidbody2D rb;
    
        // The report field will set this to false after it snaps into place so it can't be dragged or intercepted.
        [NonSerialized] public bool canDrag = true;
        private bool dragging;
        private Vector2 interceptOffset = Vector2.zero;

        private float lerpTime;
    
        private Vector2 screenVector;

        private const float interceptRadius = 0.75f;

        private CircleCollider2D sCollider;
    
        private void Start() {
            var _rb = gameObject.GetComponent<Rigidbody2D>();
            var _sCollider = GetComponent<CircleCollider2D>();

            if (_rb == null) {
                gameObject.AddComponent<Rigidbody2D>();
                _rb = GetComponent<Rigidbody2D>();
                _rb.gravityScale = 0f;
            }
            
            if (_sCollider == null) {
                gameObject.AddComponent<CircleCollider2D>();
                _sCollider = GetComponent<CircleCollider2D>();
                _sCollider.radius = 60f;
            }
            
            rb = _rb;
            sCollider = _sCollider;
        }

        private void OnDisable() {
            lerpTime = 0;
        }

        private void OnEnable() {
            if (rb == null) {
                rb = gameObject.GetComponent<Rigidbody2D>();
            }

            if (sCollider == null) {
                sCollider = GetComponent<CircleCollider2D>();
            }
        }

        private void FixedUpdate() {
            interceptOffset = Vector2.Lerp(interceptOffset, Vector2.zero, lerpTime);
            lerpTime += Time.fixedDeltaTime / 15f;
        }

        private void Update() {
            if (canDrag && dragging) {
                rb.velocity = (screenVector - rb.position) / (Time.fixedDeltaTime * 3);
            }
        }

        public bool interceptInput(Vector2 spaceVector) {
            if (canDrag) {
                return math.abs(Vector2.Distance(transform.position, spaceVector)) < interceptRadius;
            }

            return false;
        }

        /// <summary>
        /// Confirm input interception after interceptInput returns true (that this is the item being interacted with)
        /// </summary>
        /// <param name="state">start, end</param>
        /// <param name="spaceVector">I have no idea what this does. Investigate.</param>
        public void SetInteractionState(string state, Vector2 spaceVector) {
            switch (state) {
                case "start": {
                    var position = (Vector2) transform.position;
                    interceptOffset = spaceVector - position;
                    screenVector = position - interceptOffset;
                    doDrag(true);
                    break;
                }
                case "end": {
                    interceptOffset = spaceVector;
                    screenVector = spaceVector;
                    doDrag(false);
                    break;
                }
            }
        }

        public void SetTransformGoal(Vector2 goal) {
            screenVector = goal - interceptOffset;
        }

        private void doDrag(bool state) {
            if (state) {
                dragging = true;
            } else {
                try {
                    rb.velocity = Vector2.zero;
                    dragging = false;
                } catch (NullReferenceException) {
                }
            }
        }
    }
}