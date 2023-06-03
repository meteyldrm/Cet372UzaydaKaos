using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Utility {
	public class RubSliderManager: MonoBehaviour {
		private Slider _slider;
		private Coroutine _coroutine;
		
		private void Start() {
			_slider = GetComponent<Slider>();
		}

		public void SlideForOneSecond() {
			if (_coroutine == null) {
				_coroutine = StartCoroutine(_slideForOneSecond());
			}
		}

		private IEnumerator _slideForOneSecond() {
			var delta = 0f;
			while (delta < 1f) {
				delta += Time.deltaTime;
				yield return null;
				_slider.value = delta / 1f;
			}

			_slider.value = 0f;
			_coroutine = null;
		}
	}
}