using System;
using System.Collections;
using Objects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI {
    public class RubbingMachineManager : MonoBehaviour {
        private RectTransform _rt;
        private bool _shown = false;
        private GameObject _img;
        [FormerlySerializedAs("Text")] public TMP_Text text;
        private bool _updateText;

        public bool canToggle = false;
        public bool doOnce = false;

        public ElectricSpecs slot1;
        public ElectricSpecs slot2;

        private void Awake() {
            _rt = GetComponent<RectTransform>();
            _img = transform.GetChild(0).GetChild(0).gameObject;
        }

        private void OnEnable() {
            if (gameObject.name == "RubbingMachine") {
                // ReSharper disable once ArrangeRedundantParentheses
                if (Math.Abs(_rt.anchoredPosition.x - (-215)) < 0.01f) {
                    _shown = false;
                } else if (Math.Abs(_rt.anchoredPosition.x - 100) < 0.01f) {
                    _shown = true;
                } else {
                    _rt.anchoredPosition = new Vector2(-215, _rt.anchoredPosition.y);
                    _shown = false;
                }
            }

            _updateText = text != null;
            DeleteChildrenMaterials();
        }

        public void OnToggle() {
            _img.transform.Rotate(Vector3.forward, 180);
            if (_shown) {
                _rt.anchoredPosition = new Vector2(-215, _rt.anchoredPosition.y);
                _shown = false;
            } else {
                _rt.anchoredPosition = new Vector2(100, _rt.anchoredPosition.y);
                _shown = true;
            }
        }

        public void OnRub() {
            // ReSharper disable once InvertIf
            if (slot1 != null && slot2 != null) {
                print($"Rubbing {slot1.name} and {slot2.name}");
                slot1.RubForOneSecond(slot2);
                DisableRubbingForOneSecond();
            }
        }
        private Coroutine _crt;

        private void DisableRubbingForOneSecond() {
            _crt ??= StartCoroutine(Enumerator());

            return;

            IEnumerator Enumerator() {
                var dr1 = slot1.GetComponent<Draggable>();
                var dr2 = slot2.GetComponent<Draggable>();
                
                dr1.canDrag = false;
                dr2.canDrag = false;
                var delta = 0f;
                while (delta < 1f) {
                    if(_updateText) text.text = $"{slot1.accumulatedTime:N1}s";
                    delta += Time.deltaTime;
                    yield return null;
                }
                
                if(_updateText) text.text = $"{slot1.accumulatedTime:N1}s";
                _crt = null;
                dr1.canDrag = true;
                dr2.canDrag = true;
                if (slot1.accumulatedTime > 2.99f) {
                    if (!doOnce) {
                        doOnce = true;
                        if (GeneralGuidance.Instance.activityIndex == 1 || GeneralGuidance.Instance.activityIndex == 2) {
                            GeneralGuidance.Instance.skipDialogueChargeS2 = true;
                        }
                    }
                }
            }
        }

        public void DeleteChildrenMaterials() {
            if(slot1 != null) Destroy(slot1.gameObject);
            if(slot2 != null) Destroy(slot2.gameObject);
        }
    }
}
