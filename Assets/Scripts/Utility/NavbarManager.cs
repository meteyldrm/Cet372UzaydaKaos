using UnityEngine;

namespace Utility {
    public class NavbarManager : MonoBehaviour {
        public int displayCount = 2;
        private RectTransform _rt;
        private readonly int[] _sizeMap = {
            190,
            280,
            370,
            460
        };

        private void Start() {
            _rt = GetComponent<RectTransform>();
            SetSize(displayCount);
        }

        public int AddButton() {
            SetSize(++displayCount);
            return displayCount - 1;
        }

        private void SetSize(int size) {
            _rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _sizeMap[size - 2]);
            foreach (Transform tr in transform) {
                tr.gameObject.SetActive(false);
            }
            for (var i = 0; i < size; i++) {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}
