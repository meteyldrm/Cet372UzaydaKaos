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
    public class DraggableV2 : MonoBehaviour {
        private Rigidbody2D rb;
        private Camera cam;
    
        private bool canDrag = true;

        public bool dragging {
            get;
            private set;
        }
        private Vector2 interceptOffset = Vector2.zero;

        private float lerpTime;
        private Vector2 screenVector;
        
        private const float smoothingStrength = 0.3f;
        private Collider2D selfCollider;
        public colliderTypeV2 colliderType;
    
        private void Awake() {
            var _rb = gameObject.GetComponent<Rigidbody2D>();
            var _selfCollider = GetComponent<Collider2D>();

            if (_rb == null) {
                gameObject.AddComponent<Rigidbody2D>();
                _rb = GetComponent<Rigidbody2D>();
                _rb.gravityScale = 0f;
            }
            
            if (_selfCollider == null && colliderType == colliderTypeV2.Circle) {
                _selfCollider = gameObject.AddComponent<CircleCollider2D>();
                ((CircleCollider2D)_selfCollider).radius = 60f;
                _selfCollider.isTrigger = true;
            } else if (_selfCollider == null && colliderType == colliderTypeV2.Box) {
                _selfCollider = gameObject.AddComponent<BoxCollider2D>();
                ((BoxCollider2D)_selfCollider).size = new Vector2(70, 70);
                ((BoxCollider2D)_selfCollider).edgeRadius = 0.15f;
                _selfCollider.isTrigger = true;
            }
            
            rb = _rb;
            selfCollider = _selfCollider;
            cam = Camera.main;
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

        /// <summary>
        /// Confirm input interception after interceptInput returns true (that this is the item being interacted with)
        /// </summary>
        /// <param name="state">start, end</param>
        /// <param name="spaceVector">I have no idea what this does. Investigate.</param>
        public void SetInteractionState(string state, Vector2 spaceVector) {
            if (canDrag) {
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
        }

        private void OnMouseDown() {
            SetInteractionState("start", cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, math.abs(cam.transform.position.z))));
        }

        private void OnMouseDrag() {
            if (dragging) {
                interceptOffset = Vector2.Lerp(interceptOffset, Vector2.zero, lerpTime);
                lerpTime += Time.fixedDeltaTime / (smoothingStrength * 50);
                rb.velocity = (screenVector - rb.position) / (Time.fixedDeltaTime * smoothingStrength * 10);
            }
        }

        private void OnMouseUpAsButton() {
            SetInteractionState("end", Vector2.zero);
        }

        public void SetTransformGoal(Vector2 goal) {
            screenVector = goal - interceptOffset;
        }

        //This is used when snapping in place
        private void doDrag(bool state) {
            if (state) {
                dragging = true;
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
    }

    public enum colliderTypeV2 {
        Circle,
        Box
    }
}