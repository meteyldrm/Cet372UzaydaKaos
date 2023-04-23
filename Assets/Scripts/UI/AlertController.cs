using System.Collections;
using TMPro;
using UnityEngine;

namespace UI {
    public class AlertController : MonoBehaviour {
        [SerializeField] private GameObject alertTitle;
        [SerializeField] private GameObject alertBody;
        [SerializeField] private GameObject alertAction;
        
        private TextMeshProUGUI _alertTitleTmp;
        private TextMeshProUGUI _alertBodyTmp;
        private TextMeshProUGUI _alertActionTmp;

        private bool _configured;

        private void OnEnable() {
            configure();
        }

        public void dismiss() {
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void configure() {
            if (!_configured) {
                if (_alertActionTmp == null) _alertActionTmp = alertAction.GetComponent<TextMeshProUGUI>();
                if (_alertBodyTmp == null) _alertBodyTmp = alertBody.GetComponent<TextMeshProUGUI>();
                if (_alertTitleTmp == null) _alertTitleTmp = alertTitle.GetComponent<TextMeshProUGUI>();
                
                _configured = true;
            }
        }

        public void alert(string title, string body, string action) {
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            configure();
            _alertTitleTmp.text = title;
            _alertBodyTmp.text = body;
            _alertActionTmp.text = action;
        }
    }
}
