using System;
using UnityEngine;

namespace Utility {
    public class NavbarManager : MonoBehaviour {
        public int displayCount = 2;
        private RectTransform rt;
        private readonly int[] sizeMap = {
            190,
            280,
            370,
            460
        };

        private void Start() {
            rt = GetComponent<RectTransform>();
            SetSize(displayCount);
        }

        public int AddButton() {
            SetSize(++displayCount);
            return displayCount - 1;
        }

        private void SetSize(int size) {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeMap[size - 2]);
            for (int i = 2; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            transform.GetChild(size - 1).gameObject.SetActive(true);
        }
    }
}
