// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using System.Collections;
using UnityEngine;

namespace Control {
	/// <summary>
	/// Charge interactions will take place according to proton density (how many + charges the object holds).
	/// The rubbing charge calculations happen based on charge_1 = time * (affinity_1 - affinity2) * (proton_density_1 / (proton_density_1 + proton_density_2))
	/// The contact charge calculations happen based on charge_1 = (electron_density_1 + electron_density_2) * (proton_density_1 / (proton_density_1 + proton_density_2))
	/// electron_density = proton_density - accumulatedCharge
	/// TODO: Introduce a canCharge field.
	/// </summary>
	public class ElectricSpecsV2 : MonoBehaviour {
		public int materialID; //The ID of the material. This is the pseudo-serialization script for cross-consumer compatibility so bind it to a prefab for init.
		public string materialName; //Name to be displayed on screen

		/// <summary>
		/// Non-physical constant derived from the triboelectric series, always floored. The constant is to be multiplied by 5 in the first experiment.
		///
		/// The capacitance function should not allow for f = ~0 to be charged at all.
		/// Since a numerical mapping for the capacitance does not exist, we simply check if chargePerUnitTime == 0
		/// </summary>
		[Tooltip("Divide Exp1 value by 5 (seconds)")] public float chargeAffinity;
		[Tooltip("The number of protons the object will display")] public float protonDensity = 3;
		private float electronDensity => protonDensity - accumulatedCharge;

		public bool canCharge;
		public float accumulatedTime;
		public float accumulatedCharge;
		private bool rubbing;
		private int rubbingInstanceID;
		private ElectricSpecsV2 contactItem;
		
		[NonSerialized] public bool canRub;
		[NonSerialized] public bool canContact;

		private Rigidbody2D rb;
		private DraggableV2 drag;

		private Coroutine accumulationCoroutine = null;
		private Coroutine rubbingTriggerCoroutine = null;
		private Coroutine chargingTriggerCoroutine = null;

		private void Start() {
			rb = GetComponent<Rigidbody2D>();
			drag = GetComponent<DraggableV2>();
		}

		/// <summary>
		/// Predicted problems:
		/// canCharge might change while inside the trigger, leading to the object not getting charged as expected.
		/// </summary>
		/// <param name="col"></param>
		private void OnTriggerEnter2D(Collider2D col) {
			if (canCharge) {
				if (col.gameObject.TryGetComponent(out ElectricSpecsV2 specs)) {
					if (specs.canContact) {
						chargingTriggerCoroutine = StartCoroutine(invokeChargingWithDelay(specs));
					}
					
					if (specs.canRub) {
						rubbingTriggerCoroutine = StartCoroutine(invokeRubbingWithDelay(specs));
					}
				}
			}
		}

		private void OnTriggerExit2D(Collider2D other) {
			chargingTriggerCoroutine = null;
			if (rubbing && other.gameObject.GetInstanceID() == rubbingInstanceID) {
				OnStopRubbing();
			} else {
				if (rubbingTriggerCoroutine != null) {
					StopCoroutine(rubbingTriggerCoroutine);
					rubbingTriggerCoroutine = null;
				}
			}
		}

		/// <summary>
		/// If the trigger overlap has been performed and the threshold velocity checks are passing, assume that the materials are being rubbed. Use the magnitude of 2D velocity.
		/// Temporal heading differences are indicative of acceleration. Use this as the primary method of checking for rubbing. Might require close binding with a Draggable component.
		/// 
		/// Reduce the problem to the 1D case. Variations in the delta positions between objects are defined as rubbing.
		/// One object would be stationary, the other would be moving. Ensure that both are executing the same functions. Make the one with active Draggable initiate.
		/// </summary>
		/// <param name="specs"></param>
		private void OnStartRubbing(ElectricSpecsV2 specs) {
			if (canCharge) {
				contactItem = specs;
				rubbingInstanceID = specs.gameObject.GetInstanceID();
				accumulatedTime = 0;
				accumulationCoroutine = StartCoroutine(accumulateCharge());
			}
		}

		private void OnStopRubbing() {
			rubbing = false;
			if (accumulationCoroutine != null) {
				StopCoroutine(accumulationCoroutine);
			}
			accumulationCoroutine = null;
			accumulatedCharge = Mathf.Floor(chargeAffinity * accumulatedTime);
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
		/// <param name="specs"></param>
		private void DoContactCharging(ElectricSpecsV2 specs) {
			if (canCharge && canContact && specs.canCharge && specs.canContact) {
				accumulatedCharge = (electronDensity + specs.electronDensity) / (protonDensity / (protonDensity + specs.protonDensity));
				specs.accumulatedCharge = (electronDensity + specs.electronDensity) / (specs.protonDensity / (protonDensity + specs.protonDensity));
			}
		}
		
		private IEnumerator invokeChargingWithDelay(ElectricSpecsV2 material) {
			yield return new WaitForSeconds(1f);
			DoContactCharging(material);
		}

		private IEnumerator invokeRubbingWithDelay(ElectricSpecsV2 material) {
			yield return new WaitForSeconds(1f);
			rubbing = true;
			OnStartRubbing(material);
		}

		//TODO: Consider whether rubbing will invoke DoContactCharging, this affects accumulatedCharge.
		private IEnumerator accumulateCharge() {
			while (canCharge && rubbing) {
				accumulatedTime += Time.deltaTime;
				accumulatedCharge = Mathf.Floor(chargeAffinity * accumulatedTime * (protonDensity / (protonDensity + contactItem.protonDensity)));
				yield return null;
			}
		}
	}
}