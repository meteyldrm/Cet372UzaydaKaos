using System;
using System.Collections;
using Objects;
using TMPro;
using UnityEngine;

namespace UI {
    public class RubbingMachineManager : MonoBehaviour {
        private RectTransform rt;
        private bool shown = false;
        private GameObject img;
        public TMP_Text Text;
        private bool updateText;

        public bool canToggle = false;
        private bool doOnce = false;

        public ElectricSpecs slot1;
        public ElectricSpecs slot2;

        private void Awake() {
            rt = GetComponent<RectTransform>();
            img = transform.GetChild(0).GetChild(0).gameObject;
        }

        private void OnEnable() {
            if (gameObject.name == "RubbingMachine") {
                if (Math.Abs(rt.anchoredPosition.x - (-215)) < 0.01f) {
                    shown = false;
                } else if (Math.Abs(rt.anchoredPosition.x - 100) < 0.01f) {
                    shown = true;
                } else {
                    rt.anchoredPosition = new Vector2(-215, rt.anchoredPosition.y);
                    shown = false;
                }
            }

            updateText = Text != null;
            DeleteChildrenMaterials();
        }

        public void OnToggle() {
            img.transform.Rotate(Vector3.forward, 180);
            if (shown) {
                rt.anchoredPosition = new Vector2(-215, rt.anchoredPosition.y);
                shown = false;
            } else {
                rt.anchoredPosition = new Vector2(100, rt.anchoredPosition.y);
                shown = true;
            }
        }

        public void OnRub() {
            if (slot1 != null && slot2 != null) {
                print($"Rubbing {slot1.name} and {slot2.name}");
                slot1.RubForOneSecond(slot2);
                DisableRubbingForOneSecond();
            }
        }
        private Coroutine crt;

        private void DisableRubbingForOneSecond() {
            if (crt == null) {
                crt = StartCoroutine(enumerator());
            }

            IEnumerator enumerator() {
                Draggable dr1 = slot1.GetComponent<Draggable>();
                Draggable dr2 = slot2.GetComponent<Draggable>();
                
                dr1.canDrag = false;
                dr2.canDrag = false;
                var delta = 0f;
                while (delta < 1f) {
                    if(updateText) Text.text = $"{slot1.accumulatedTime:N1}s";
                    delta += Time.deltaTime;
                    yield return null;
                }
                
                if(updateText) Text.text = $"{slot1.accumulatedTime:N1}s";
                crt = null;
                dr1.canDrag = true;
                dr2.canDrag = true;
                if (slot1.accumulatedTime > 2.99f) {
                    if (!doOnce) {
                        doOnce = true;
                        GeneralGuidance.Instance.skipDialogueChargeS2 = true;
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
