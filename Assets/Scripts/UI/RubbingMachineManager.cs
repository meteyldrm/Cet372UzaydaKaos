using System;
using Objects;
using UnityEngine;

namespace UI {
    public class RubbingMachineManager : MonoBehaviour {
        private RectTransform rt;
        private bool shown = false;
        private GameObject img;

        public ElectricSpecs slot1;
        public ElectricSpecs slot2;

        private void Awake() {
            rt = GetComponent<RectTransform>();
            img = transform.GetChild(0).GetChild(0).gameObject;
        }

        private void OnEnable() {
            if (Math.Abs(rt.anchoredPosition.x - (-215)) < 0.01f) {
                shown = false;
            } else if (Math.Abs(rt.anchoredPosition.x - 100) < 0.01f) {
                shown = true;
            } else {
                rt.anchoredPosition = new Vector2(-215, rt.anchoredPosition.y);
                shown = false;
            }
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
                slot1.RubForOneSecond(slot2);
            }
        }
    }
}
