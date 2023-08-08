using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reports {
    public class EngValueColorScript : MonoBehaviour {
        private TMP_Dropdown _dropdown;
        private Image _image;
        
        public bool OnUpdateColor() {
            if (_dropdown == null) {
                _dropdown = GetComponent<TMP_Dropdown>();
            }
            
            if (_image == null) {
                _image = GetComponent<Image>();
            }
            
            var text = _dropdown.options[_dropdown.value].text;
            if (text == gameObject.name) {
                _image.color = Color.HSVToRGB(115f / 360f, 35f / 100f, 95f / 100f);
                return true;
                // ReSharper disable once RedundantIfElseBlock
            } else if (text == "?") {
                _image.color = Color.HSVToRGB(0f / 360f, 0f / 100f, 100f / 100f);
                return false;
                // ReSharper disable once RedundantIfElseBlock
            } else {
                _image.color = Color.HSVToRGB(0f / 360f, 35f / 100f, 95f / 100f);
                return false;
            }
        }
    }
}
