using System.Collections.Generic;
using Objects;
using TMPro;
using UnityEngine;

namespace SceneManagers {
    public class PlaygroundGuidance : MonoBehaviour {
        public GameObject draggable;
        public ElectricSpecs draggableSpecs;
        public GameObject chargeText;
        private TMP_Text cText;
        
        // Start is called before the first frame update
        private void Start() {
            draggableSpecs = draggable.GetComponent<ElectricSpecs>();
            cText = chargeText.GetComponent<TMP_Text>();
        }

        // Update is called once per frame
        private void Update() {
            cText.text = $"Boot charge: {draggableSpecs.getEffectiveCharge()}";
        }

        public List<GameObject> getDraggables() {
            return new List<GameObject> { draggable };
        }

        public void ResetDraggableCharges() {
            foreach (var obj in GeneralGuidance.GetAllSceneComponents<ElectricSpecs>()) {
                obj.OnResetRubbing();
            }
            cText.text = "Boot charge: 0";
        }
    }
}
