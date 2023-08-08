// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

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
        private Rigidbody2D _rb;
        private Camera _cam;
        private GameObject _reportCollider;
        private GameObject _engReportCollider;
        private GameObject _rubbingCollider;
    
        public bool canDrag = true;

        public bool dragging;
        private Vector2 _guidanceStartPosition;
        public Vector2 dragStartPosition;
        private Vector2 _interceptOffset = Vector2.zero;

        private float _lerpTime;
        private Vector2 _screenVector;
        
        private const float SmoothingStrength = 0.3f;
        private Collider2D _selfCollider;
        public ColliderTypeV2 colliderType;

        private bool _scaled;
        private bool _parented;
        private bool _snapped;

        private bool _skippedOnce = false;

        [FormerlySerializedAs("FakeParent")] public GameObject fakeParent;
    
        private void Awake() {
            var tempRb = gameObject.GetComponent<Rigidbody2D>();
            var tempCollider = GetComponent<Collider2D>();

            if (tempRb == null) {
                gameObject.AddComponent<Rigidbody2D>();
                tempRb = GetComponent<Rigidbody2D>();
                tempRb.gravityScale = 0f;
            }
            
            if (_selfCollider == null && colliderType == ColliderTypeV2.Circle) {
                _selfCollider = gameObject.AddComponent<CircleCollider2D>();
                ((CircleCollider2D)_selfCollider).radius = 60f;
                _selfCollider.isTrigger = true;
            } else if (_selfCollider == null && colliderType == ColliderTypeV2.Box) {
                _selfCollider = gameObject.AddComponent<BoxCollider2D>();
                ((BoxCollider2D)_selfCollider).size = new Vector2(70, 70);
                ((BoxCollider2D)_selfCollider).edgeRadius = 0.15f;
                _selfCollider.isTrigger = true;
            }
            
            _rb = tempRb;
            _selfCollider = tempCollider;
            _cam = Camera.main;
            _guidanceStartPosition = _rb.position;
        }

        private void OnDisable() {
            _lerpTime = 0;
        }

        private void OnEnable() {
            if (_rb == null) {
                _rb = gameObject.GetComponent<Rigidbody2D>();
            }

            if (_selfCollider == null) {
                _selfCollider = GetComponent<Collider2D>();
            }
        }

        /// <summary>
        /// Confirm input interception after interceptInput returns true (that this is the item being interacted with)
        /// </summary>
        /// <param name="state">start, end</param>
        /// <param name="spaceVector">I have no idea what this does. Investigate.</param>
        private void SetInteractionState(bool state, Vector2 spaceVector) {
            if (canDrag && GeneralGuidance.Instance.allowDrag) {
                SetInterceptColliderSize(!state);
                switch (state) {
                    case true: {
                        var position = (Vector2) transform.position;
                        _interceptOffset = spaceVector - position;
                        _screenVector = position - _interceptOffset;
                        DoDrag(true);
                        if (TryGetComponent(out ElectricSpecs _) && !GeneralGuidance.Instance.report.dualityConstraint && _reportCollider != null) {
                            GeneralGuidance.Instance.report.SmartInstantiateElectricChild(fakeParent);
                        }
                        break;
                    }
                    case false: {
                        _interceptOffset = spaceVector;
                        _screenVector = spaceVector;
                        DoDrag(false);
                        break;
                    }
                }
            }

            // ReSharper disable once InvertIf
            if (dragging && !state) {
                _interceptOffset = spaceVector;
                _screenVector = spaceVector;
                DoDrag(false);
            }
        }

        private void OnMouseDown() {
            SetInteractionState(true, _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, math.abs(_cam.transform.position.z))));
        }

        private void OnMouseDrag() {
            // ReSharper disable once InvertIf
            if (dragging) {
                SetTransformGoal(_cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, math.abs(_cam.transform.position.z))));
                _interceptOffset = Vector2.Lerp(_interceptOffset, Vector2.zero, _lerpTime);
                _lerpTime += Time.fixedDeltaTime / (SmoothingStrength * 50);
                _rb.velocity = (_screenVector - _rb.position) / (Time.fixedDeltaTime * SmoothingStrength * 10);
            }
        }

        private void OnMouseUp() {
            // ReSharper disable once InvertIf
            if (dragging) {
                if (_reportCollider != null) {
                    if (gameObject.TryGetComponent(out ElectricSpecs specs)) {
                        if (!GeneralGuidance.Instance.report.OnSnap(specs, _reportCollider, _reportCollider.transform.parent.gameObject)) {
                            SetInteractionState(false, Vector2.zero);
                            specs.ResetRubbingPosition();
                            return;
                        }
                        SetInteractionState(false, Vector2.zero);
                        transform.SetParent(GeneralGuidance.Instance.report.gameObject.transform, true);
                        try {
                            fakeParent = _reportCollider.gameObject;
                        } catch (NullReferenceException) {
                            fakeParent = GeneralGuidance.Instance.report.gameObject;
                        }
                        _parented = true;
                        try {
                            transform.position = _reportCollider.GetComponent<BoxCollider2D>().bounds.center;
                        } catch (NullReferenceException) {
                        }
                        canDrag = false;
                        var obj = Instantiate(GeneralGuidance.Instance.materialPrefabList[specs.materialID], GeneralGuidance.GetSceneGameObjectByName("Materials", 2).transform);
                        obj.transform.position = _guidanceStartPosition;
                        var componentSpecs = obj.GetComponent<ElectricSpecs>();
                        componentSpecs.canCharge = true;
                        componentSpecs.canRub = true;
                        componentSpecs.canContact = true;
                    }
                } else {
                    SetInteractionState(false, Vector2.zero);
                }
                
                if (_rubbingCollider != null) {
                    // ReSharper disable once InvertIf
                    if (gameObject.TryGetComponent(out ElectricSpecs specs)) {
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (_rubbingCollider.gameObject.name == "Slut1") {
                            GeneralGuidance.Instance.rubbingMachine.slot1 = specs;
                            SetInteractionState(false, Vector2.zero);
                            transform.position = _rubbingCollider.transform.position;
                        }
                        
                        else if (_rubbingCollider.gameObject.name == "Slut2") {
                            GeneralGuidance.Instance.rubbingMachine.slot2 = specs;
                            SetInteractionState(false, Vector2.zero);
                            transform.position = _rubbingCollider.transform.position;
                        }
                        
                        else if (_rubbingCollider.gameObject.name == "ChargePanel") {
                            specs.OnShowVisualParticles();
                            if (GeneralGuidance.Instance.rubbingMachine.slot1 == null) {
                                GeneralGuidance.Instance.rubbingMachine.slot1 = specs;
                                specs.OnResetRubbing();
                                if (!_scaled) {
                                    _scaled = true;
                                    gameObject.transform.localScale *= 2;
                                }
                                SetInteractionState(false, Vector2.zero);
                                transform.SetParent(_rubbingCollider.gameObject.transform, true);
                                _snapped = true;
                                gameObject.transform.GetChild(0).gameObject.SetActive(false);
                            } else if (GeneralGuidance.Instance.rubbingMachine.slot2 == null) {
                                GeneralGuidance.Instance.rubbingMachine.slot2 = specs;
                                specs.OnResetRubbing();
                                if (!_scaled) {
                                    _scaled = true;
                                    gameObject.transform.localScale *= 2;
                                }
                                SetInteractionState(false, Vector2.zero);
                                transform.SetParent(_rubbingCollider.gameObject.transform, true);
                                _snapped = true;
                                gameObject.transform.GetChild(0).gameObject.SetActive(false);
                                // ReSharper disable once InvertIf
                                if (!_skippedOnce && GeneralGuidance.Instance.notifyOnSnap) {
                                    GeneralGuidance.Instance.skipDialogueChargeS2 = true;
                                    _skippedOnce = true;
                                }
                            } else { //Add non-interactive temporal snapping, reset position causing issues when moving n=2
                                if(!_snapped) specs.ResetRubbingPosition();
                                SetInteractionState(false, Vector2.zero);
                            }
                        }
                    }
                } else {
                    SetInteractionState(false, Vector2.zero);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.CompareTag("ReportCollider")) {
                _reportCollider = col.gameObject;
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
            
            // ReSharper disable once InvertIf
            if (col.CompareTag("RubMachineCollider")) {
                _rubbingCollider = col.gameObject;
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        private void OnTriggerExit2D(Collider2D col) {
            if (col.CompareTag("ReportCollider")) {
                _reportCollider = null;
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                if (_parented && dragging) { //Current object parent is deactivating
                    var obj = GeneralGuidance.GetSceneGameObjectByName("Materials", 2);
                    gameObject.transform.SetParent(obj.transform, true);
                    fakeParent = null;
                    _parented = false;
                }
            }
            
            // ReSharper disable once InvertIf
            if (col.CompareTag("RubMachineCollider")) {
                _rubbingCollider = null;
                if (_scaled) {
                    _scaled = false;
                    gameObject.transform.localScale /= 2;
                    if (gameObject.TryGetComponent(out ElectricSpecs specs)) {
                        specs.OnHideVisualParticles();
                    }
                }
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                var obj = GeneralGuidance.GetSceneGameObjectByName("Materials", 2);
                gameObject.transform.SetParent(obj.transform, true);
                _snapped = false;
            }
        }

        private void SetTransformGoal(Vector2 goal) {
            _screenVector = goal - _interceptOffset;
        }

        //This is used when snapping in place
        private void DoDrag(bool state) {
            if (state) {
                dragging = true;
                dragStartPosition = _rb.position;
            } else {
                try {
                    _rb.velocity = Vector2.zero;
                    dragging = false;
                    _interceptOffset = Vector2.zero;
                    _screenVector = Vector2.zero;
                } catch (NullReferenceException) {
                }
            }
        }
        
        public void SetInterceptColliderSize(bool makeSmaller) {
            if (_selfCollider != null && colliderType == ColliderTypeV2.Circle) {
                ((CircleCollider2D)_selfCollider).radius = makeSmaller ? 40f : 60f;
            } else if (_selfCollider != null && colliderType == ColliderTypeV2.Box) {
                ((BoxCollider2D)_selfCollider).size = makeSmaller ? new Vector2(40, 40) : new Vector2(70, 70);
                ((BoxCollider2D)_selfCollider).edgeRadius = makeSmaller ? 0.1f : 0.15f;
            }
        }
    }

    public enum ColliderTypeV2 {
        Circle,
        Box
    }
}