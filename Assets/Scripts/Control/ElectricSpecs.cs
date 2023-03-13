// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using System.Collections;
using UnityEngine;

namespace Control {
	/// <summary>
	/// Manages the electrical interactivity between objects. Implement friction logic here.
	/// TODO: Introduce a canCharge field.
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
		private int rubbingInstanceID;

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
			if (rubbing && other.gameObject.GetInstanceID() == rubbingInstanceID) {
				OnStopRubbing();
				other.gameObject.GetInstanceID();
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
			rubbingInstanceID = material.gameObject.GetInstanceID();
			accumulatedTime = 0;
			accumulationCoroutine = StartCoroutine(accumulateCharge());
		}

		private void OnStopRubbing() {
			rubbing = false;
			if (accumulationCoroutine != null) {
				StopCoroutine(accumulationCoroutine);
			}
			accumulationCoroutine = null;
			accumulatedCharge = Mathf.Floor(chargePerUnitTime * accumulatedTime);
			rubbingInstanceID = -1;
		}
		
		public void OnResetRubbing() {
			rubbing = false;
			if (accumulationCoroutine != null) {
				StopCoroutine(accumulationCoroutine);
			}
			accumulationCoroutine = null;
			accumulatedCharge = 0;
			accumulatedTime = 0;
			rubbingInstanceID = -1;
		}

		/// <summary>
		/// If the trigger overlap has been performed and the object has been stationary for some time, assume that the objects have been touched.
		/// The distribution is based on proton counts. This should be coupled more closely with the charge particles. Introduce "proton" and "electron" fields.
		/// </summary>
		/// <param name="material"></param>
		private void DoContactCharging(GameObject material) {
			
		}

		private IEnumerator invokeRubbingWithDelay(GameObject material) {
			yield return new WaitForSeconds(1f);
			rubbing = true;
			OnStartRubbing(material);
		}

		//TODO: Consider whether rubbing will invoke DoContactCharging, this affects accumulatedCharge.
		private IEnumerator accumulateCharge() {
			accumulatedTime += Time.deltaTime;
			accumulatedCharge = Mathf.Floor(chargePerUnitTime * accumulatedTime);
			yield return null;
		}
	}
}