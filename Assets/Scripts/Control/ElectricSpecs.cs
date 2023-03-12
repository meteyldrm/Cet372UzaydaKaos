// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using System.Collections;
using UnityEngine;

namespace Control {
	/// <summary>
	/// Manages the electrical interactivity between objects. Implement friction logic here.
	/// </summary>
	public class ElectricSpecs : MonoBehaviour {
		public int materialID; //The ID of the material. This is the pseudo-serialization script for cross-consumer compatibility so bind it to a prefab for init.
		public string materialName; //Name to be displayed on screen

		/// <summary>
		/// Non-physical constant derived from the triboelectric series, always floored. The constant is to be multiplied by 5 in the first experiment.
		///
		/// The capacitance function should not allow for f = ~0 to be charged at all.
		/// Since a numerical mapping for the capacitance does not exist, we simply check if chargePerUnitTime == 0
		/// </summary>
		[Tooltip("Divide Exp1 value by 5 (seconds)")] public float chargePerUnitTime;

		public float accumulatedTime;
		public float accumulatedCharge;
		private bool rubbing;

		private Rigidbody2D rb;
		private Draggable drag;

		private Coroutine accumulationCoroutine = null;
		private Coroutine rubbingTriggerCoroutine = null;

		private void Start() {
			rb = GetComponent<Rigidbody2D>();
			drag = GetComponent<Draggable>();
		}

		private void OnTriggerEnter2D(Collider2D col) {
			rubbingTriggerCoroutine = StartCoroutine(invokeRubbingWithDelay(col.gameObject));
		}

		private void OnTriggerExit2D(Collider2D other) {
			if (rubbing) {
				OnStopRubbing(other.gameObject);
			} else {
				StopCoroutine(rubbingTriggerCoroutine);
				rubbingTriggerCoroutine = null;
			}
		}

		/// <summary>
		/// If the trigger overlap has been performed and the threshold velocity checks are passing, assume that the materials are being rubbed. Use the magnitude of 2D velocity.
		/// Temporal heading differences are indicative of acceleration. Use this as the primary method of checking for rubbing. Might require close binding with a Draggable component.
		/// </summary>
		/// <param name="material"></param>
		private void OnStartRubbing(GameObject material) {
			accumulatedTime = 0;
			accumulationCoroutine = StartCoroutine(accumulateCharge());
		}

		private void OnStopRubbing(GameObject material) {
			rubbing = false;
			StopCoroutine(accumulationCoroutine);
			accumulationCoroutine = null;
			accumulatedCharge = Mathf.Floor(chargePerUnitTime * accumulatedTime);
		}

		private IEnumerator invokeRubbingWithDelay(GameObject material) {
			yield return new WaitForSeconds(1f);
			rubbing = true;
			OnStartRubbing(material);
		}

		private IEnumerator accumulateCharge() {
			accumulatedTime += Time.deltaTime;
			yield return null;
		}
	}
}