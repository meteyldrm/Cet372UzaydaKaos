﻿// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using Unity.Mathematics;
using UnityEngine;

// Modified from https://github.com/meteyldrm/CET49A-BasketballAir/blob/master/Assets/Scripts/Draggable.cs

namespace Objects {
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
        private Camera cam;
        private GameObject reportCollider;
        private GameObject rubbingCollider;
    
        public bool canDrag = true;

        public bool dragging;
        private Vector2 guidanceStartPosition;
        public Vector2 dragStartPosition;
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
            guidanceStartPosition = rb.position;
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
        private void SetInteractionState(bool state, Vector2 spaceVector) {
            if (canDrag && GeneralGuidance.Instance.allowDrag) {
                switch (state) {
                    case true: {
                        var position = (Vector2) transform.position;
                        interceptOffset = spaceVector - position;
                        screenVector = position - interceptOffset;
                        doDrag(true);
                        break;
                    }
                    case false: {
                        interceptOffset = spaceVector;
                        screenVector = spaceVector;
                        doDrag(false);
                        break;
                    }
                }
            }

            if (dragging && !state) {
                interceptOffset = spaceVector;
                screenVector = spaceVector;
                doDrag(false);
            }
        }

        private void OnMouseDown() {
            SetInteractionState(true, cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, math.abs(cam.transform.position.z))));
        }

        private void OnMouseDrag() {
            if (dragging) {
                SetTransformGoal(cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, math.abs(cam.transform.position.z))));
                interceptOffset = Vector2.Lerp(interceptOffset, Vector2.zero, lerpTime);
                lerpTime += Time.fixedDeltaTime / (smoothingStrength * 50);
                rb.velocity = (screenVector - rb.position) / (Time.fixedDeltaTime * smoothingStrength * 10);
            }
        }

        private void OnMouseUp() {
            if (dragging) {
                if (reportCollider != null) {
                    if (gameObject.TryGetComponent(out ElectricSpecs specs)) {
                        if (!GeneralGuidance.Instance.report.OnSnap(specs, reportCollider.name, reportCollider.transform.parent.name)) {
                            SetInteractionState(false, Vector2.zero);
                            specs.resetRubbingPosition();
                            return;
                        }
                        SetInteractionState(false, Vector2.zero);
                        transform.SetParent(GeneralGuidance.Instance.report.gameObject.transform, true);
                        transform.position = reportCollider.transform.position;
                        canDrag = false;
                        var obj = Instantiate(GeneralGuidance.Instance.MaterialPrefabList[specs.materialID], GeneralGuidance.GetSceneGameObjectByName("Materials", 2).transform);
                        obj.transform.position = guidanceStartPosition;
                        var componentSpecs = obj.GetComponent<ElectricSpecs>();
                        componentSpecs.canCharge = true;
                        componentSpecs.canRub = true;
                        componentSpecs.canContact = true;
                    }
                } else {
                    SetInteractionState(false, Vector2.zero);
                }
                
                if (rubbingCollider != null) {
                    if (gameObject.TryGetComponent(out ElectricSpecs specs)) {
                        if (rubbingCollider.gameObject.name == "Slut1") {
                            GeneralGuidance.Instance.rubbingMachine.slot1 = specs;
                        } else if (rubbingCollider.gameObject.name == "Slut2") {
                            GeneralGuidance.Instance.rubbingMachine.slot2 = specs;
                        } 
                        SetInteractionState(false, Vector2.zero);
                        transform.position = rubbingCollider.transform.position;
                    }
                } else {
                    SetInteractionState(false, Vector2.zero);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.CompareTag("ReportCollider")) {
                reportCollider = col.gameObject;
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
            
            if (col.CompareTag("RubMachineCollider")) {
                rubbingCollider = col.gameObject;
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        private void OnTriggerExit2D(Collider2D col) {
            if (col.CompareTag("ReportCollider")) {
                reportCollider = null;
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
            
            if (col.CompareTag("RubMachineCollider")) {
                rubbingCollider = null;
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        private void SetTransformGoal(Vector2 goal) {
            screenVector = goal - interceptOffset;
        }

        //This is used when snapping in place
        private void doDrag(bool state) {
            if (state) {
                dragging = true;
                dragStartPosition = rb.position;
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