using System.Collections;
using SceneManagers;
using UnityEngine;
using UnityEngine.UI;

namespace Objects {
	public class Lightbulb : MonoBehaviour {
		public Sprite DimBulb;
		public Sprite LitBulb;
		public LightAndChargeGuidance guidance;
		
		public void LightUp() {
			StartCoroutine(Light());
		}

		IEnumerator Light() {
			var img = GetComponent<Image>();
			img.sprite = LitBulb;
			transform.GetChild(0).gameObject.SetActive(true);
			yield return new WaitForSeconds(0.7f);
			img.sprite = DimBulb;
			transform.GetChild(0).gameObject.SetActive(false);
			var s = GetComponent<ElectricSpecs>();
			s.electronDensity = s.protonDensity;
			guidance.NextDialogue();
		}
	}
}
