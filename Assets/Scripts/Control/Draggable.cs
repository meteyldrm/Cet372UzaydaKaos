// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
        private bool canDrag = true;

        public bool dragging {
            get;
            private set;
        }
        private Vector2 interceptOffset = Vector2.zero;

        private float lerpTime;
        private Vector2 screenVector;
        private bool doAverageMomentum = true;
        private Stack<float> averageVelocity = new Stack<float>();
        private const float averagingWindow = 2f;

        private const float interceptRadius = 0.75f;
        private const float smoothingStrength = 0.3f;
        private Collider2D selfCollider;
        public colliderType colliderType;
    
        private void Awake() {
            var _rb = gameObject.GetComponent<Rigidbody2D>();
            var _selfCollider = GetComponent<Collider2D>();

            if (_rb == null) {
                gameObject.AddComponent<Rigidbody2D>();
                _rb = GetComponent<Rigidbody2D>();
                _rb.gravityScale = 0f;
            }
            
            if (_selfCollider == null && colliderType == colliderType.Circle) {
                _selfCollider = gameObject.AddComponent<CircleCollider2D>();
                ((CircleCollider2D)_selfCollider).radius = 60f;
                _selfCollider.isTrigger = true;
            } else if (_selfCollider == null && colliderType == colliderType.Box) {
                _selfCollider = gameObject.AddComponent<BoxCollider2D>();
                ((BoxCollider2D)_selfCollider).size = new Vector2(70, 70);
                ((BoxCollider2D)_selfCollider).edgeRadius = 0.15f;
                _selfCollider.isTrigger = true;
            }
            
            rb = _rb;
            selfCollider = _selfCollider;
        }

        private void Start() {
        }

        private void OnDisable() {
            lerpTime = 0;
        }

        private void OnEnable() {
            if (rb == null) {
                rb = gameObject.GetComponent<Rigidbody2D>();
            }

            if (selfCollider == null) {
                selfCollider = GetComponent<Collider2D>();
            }
        }

        private void FixedUpdate() {
            interceptOffset = Vector2.Lerp(interceptOffset, Vector2.zero, lerpTime);
            lerpTime += Time.fixedDeltaTime / (smoothingStrength * 50);
        }

        private void Update() {
            if (canDrag && dragging) {
                rb.velocity = (screenVector - rb.position) / (Time.fixedDeltaTime * smoothingStrength * 10);
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

        //This is used when snapping in place
        private void doDrag(bool state) {
            if (state) {
                dragging = true;
                StartCoroutine(slidingWindowAverageVelocity());
            } else {
                try {
                    rb.velocity = Vector2.zero;
                    dragging = false;
                    interceptOffset = Vector2.zero;
                    screenVector = Vector2.zero;
                } catch (NullReferenceException) {
                }
            }
        }

        // blankFrame was an attempt at ignoring the first N frames, since the draggable moves to reduce touch offset each frame.
        private IEnumerator slidingWindowAverageVelocity() {
            var time = 0f;
            var blankFrame = 30;
            while (time < averagingWindow) {
                time += Time.deltaTime;
                if (blankFrame <= 0) {
                    averageVelocity.Push(rb.velocity.sqrMagnitude);
                } else {
                    blankFrame--;
                    averageVelocity.Push(0);
                }
                yield return null;
            }

            while (doAverageMomentum && time >= averagingWindow) {
                time += Time.deltaTime;
                averageVelocity.Pop();
                averageVelocity.Push(rb.velocity.sqrMagnitude);
                yield return null;
            }
            yield return null;
        }

        public float calculateAverageVelocity() {
            // slidingWindowAverageVelocity is responsible for generating the averageVelocity data.
            // Reports 0 for all values under 0.5f. This was an attempt at distinguishing rubbing vs contact. The value might be too high.
            return averageVelocity.Select(i => i > 0.5f ? i : 0f).Sum() / averageVelocity.Count;
        }
    }

    public enum colliderType {
        Circle,
        Box
    }
}