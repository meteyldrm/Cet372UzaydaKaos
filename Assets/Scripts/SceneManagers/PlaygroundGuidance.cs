using System.Collections.Generic;
using Control;
using TMPro;
using UnityEngine;

namespace SceneManagers {
    public class PlaygroundGuidance : MonoBehaviour, GeneralGuidance.IDraggableController {
        public Camera cam;
        public GameObject draggable;
        public GameObject chargeText;
        
        // Start is called before the first frame update
        private void Start() {
            GeneralGuidance.Instance.DraggableController = this;
            GeneralGuidance.Instance.registerCamera(cam);
        }

        // Update is called once per frame
        private void Update() {
            chargeText.GetComponent<TMP_Text>().text = $"Boot charge: {draggable.GetComponent<ElectricSpecs>().accumulatedCharge}";
        }

        public List<GameObject> getDraggables() {
            return new List<GameObject> { draggable };
        }

        public void ResetDraggableCharges() {
            foreach (var obj in GeneralGuidance.GetAllSceneComponents<ElectricSpecs>()) {
                obj.OnResetRubbing();
            }
            chargeText.GetComponent<TMP_Text>().text = "Boot charge: 0";
        }
    }
}
