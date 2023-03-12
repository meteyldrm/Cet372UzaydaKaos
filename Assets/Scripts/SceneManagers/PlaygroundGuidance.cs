using System.Collections.Generic;
using UnityEngine;

namespace SceneManagers {
    public class PlaygroundGuidance : MonoBehaviour, GeneralGuidance.IDraggableController {
        public GameObject draggable;
        
        // Start is called before the first frame update
        void Start() {
            GeneralGuidance.Instance.DraggableController = this;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public List<GameObject> getDraggables() {
            return new List<GameObject> { draggable };
        }
    }
}
