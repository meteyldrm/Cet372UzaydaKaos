using System.Collections;
using SceneManagers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Objects {
	public class LightBulb : MonoBehaviour {
		[FormerlySerializedAs("DimBulb")] public Sprite dimBulb;
		[FormerlySerializedAs("LitBulb")] public Sprite litBulb;
		public LightAndChargeGuidance guidance;
		private bool _hasTriggered = false;
		
		public void LightUp() {
			StartCoroutine(Light());
		}

		private IEnumerator Light() {
			var img = GetComponent<Image>();
			img.sprite = litBulb;
			transform.GetChild(0).gameObject.SetActive(true);
			yield return new WaitForSeconds(0.7f);
			img.sprite = dimBulb;
			transform.GetChild(0).gameObject.SetActive(false);
			var s = GetComponent<ElectricSpecs>();
			s.electronDensity = s.protonDensity;
			// ReSharper disable once InvertIf
			if (!_hasTriggered) {
				guidance.NextDialogue();
				_hasTriggered = true;
			}
		}
	}
}
