using UnityEngine;

namespace Reports {
    public class EngReportManager : MonoBehaviour {
        private int _correctCount = 0;
        private bool _notified = false;
        private int _part = 1;
        //
        // public bool OnSnap(string value, GameObject parent) {
        //     if (value == parent.name.ToLower(CultureInfo.InvariantCulture)) {
        //         snapCount++;
        //         return true;
        //     }
        //
        //     return false;
        // }
        //
        private void OnEnable() {
            _notified = false;
            if (_part < 2) {
                transform.GetChild(0).GetChild(_part - 1).gameObject.SetActive(true);
            }
        }

        public void UpdateColors() {
            _correctCount = 0;
            foreach (Transform tr in transform.GetChild(0).GetChild(_part - 1).GetChild(1).transform) {
                if(tr.gameObject.TryGetComponent(out EngValueColorScript cs)) {
                    var x = cs.OnUpdateColor();

                    if (x) {
                        _correctCount++;
                    }
                } else {
                    Debug.LogError("Object had no color script");
                }
            }
        }
        
        private void LateUpdate() {
            if (_part == 1 && _correctCount == transform.GetChild(0).GetChild(_part - 1).GetChild(1).childCount && !_notified) {
                _notified = true;
                _part++;
                _correctCount = 0;
                GeneralGuidance.Instance.skipDialogueEngReport = true;
            }
            
            // ReSharper disable once InvertIf
            if (_part == 2 && _correctCount == transform.GetChild(0).GetChild(_part - 1).GetChild(1).childCount && !_notified) {
                _notified = true;
                _part++;
                _correctCount = 0;
                GeneralGuidance.Instance.skipDialogueEngReport = true;
            }
        }
    }
}
