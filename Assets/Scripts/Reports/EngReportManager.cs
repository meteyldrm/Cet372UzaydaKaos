using System;
using System.Globalization;
using UnityEngine;

namespace Reports {
    public class EngReportManager : MonoBehaviour {

        
        
        private int correctCount = 0;
        private bool notified = false;
        private int part = 1;
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
            notified = false;
            if (part < 2) {
                transform.GetChild(0).GetChild(part - 1).gameObject.SetActive(true);
            }
        }

        public void UpdateColors() {
            correctCount = 0;
            foreach (Transform tr in transform.GetChild(0).GetChild(part - 1).GetChild(1).transform) {
                if(tr.gameObject.TryGetComponent(out EngValueColorScript cs)) {
                    var x = cs.OnUpdateColor();

                    if (x) {
                        correctCount++;
                    }
                } else {
                    Debug.LogError("Object had no colorscript");
                }
            }
        }
        
        private void LateUpdate() {
            if (part == 1 && correctCount == transform.GetChild(0).GetChild(part - 1).GetChild(1).childCount && !notified) {
                notified = true;
                part++;
                correctCount = 0;
                GeneralGuidance.Instance.skipDialogueEngReport = true;
            }
            
            if (part == 2 && correctCount == transform.GetChild(0).GetChild(part - 1).GetChild(1).childCount && !notified) {
                notified = true;
                part++;
                correctCount = 0;
                GeneralGuidance.Instance.skipDialogueEngReport = true;
            }
        }
    }
}
