using System.Collections.Generic;
using UnityEngine;

namespace SceneManagers {
    public class PlaygroundGuidance : MonoBehaviour, GeneralGuidance.IDraggableController {
        public GameObject draggable;
        public Camera cam;
        
        // Start is called before the first frame update
        private void Start() {
            GeneralGuidance.Instance.DraggableController = this;
            GeneralGuidance.Instance.registerCamera(cam);
        }

        // Update is called once per frame
        private void Update()
        {
        
        }

        public List<GameObject> getDraggables() {
            return new List<GameObject> { draggable };
        }
    }
}
