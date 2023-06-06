using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reports {
    public class EngValueColorScript : MonoBehaviour {
        private TMP_Dropdown dropdown;
        private Image image;
        
        public bool OnUpdateColor() {
            if (dropdown == null) {
                dropdown = GetComponent<TMP_Dropdown>();
            }
            
            if (image == null) {
                image = GetComponent<Image>();
            }
            
            var text = dropdown.options[dropdown.value].text;
            if (text == gameObject.name) {
                image.color = Color.HSVToRGB(115f / 360f, 35f / 100f, 95f / 100f);
                return true;
            } else if (text == "?") {
                image.color = Color.HSVToRGB(0f / 360f, 0f / 100f, 100f / 100f);
                return false;
            } else {
                image.color = Color.HSVToRGB(0f / 360f, 35f / 100f, 95f / 100f);
                return false;
            }
        }
    }
}
