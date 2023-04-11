// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using System.Collections;
using UnityEngine;

namespace Objects {
	/// <summary>
	/// Charge interactions will take place according to proton density (how many + charges the object holds).
	/// The rubbing charge calculations happen based on charge_1 = time * (affinity_1 - affinity2) * (proton_density_1 / (proton_density_1 + proton_density_2))
	/// The contact charge calculations happen based on charge_1 = (electron_density_1 + electron_density_2) * (proton_density_1 / (proton_density_1 + proton_density_2))
	/// electron_density = proton_density - accumulatedCharge
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
		[Tooltip("Divide Exp1 value by 5 (seconds)")] public float chargeAffinity;
		[Tooltip("The number of protons the object will display")] public float protonDensity = 3;
		[NonSerialized] public float electronDensity;
		
		public float accumulatedTime;
		private bool rubbing;
		private int rubbingMaterialID;
		[NonSerialized] public ElectricSpecs conjugateItem;
		[NonSerialized] public bool isActiveObject;
		
		public bool canCharge = true;
		public bool canRub;
		public bool canContact;
		public bool hasElectrostaticForce;

		private Rigidbody2D rb;
		private Draggable drag;

		private Coroutine rubbingTriggerCoroutine = null;
		private Coroutine chargingTriggerCoroutine = null;
		private bool triggerIntercept = false;
		private bool triggerInterceptLate = false;

		private void Start() {
			rb = GetComponent<Rigidbody2D>();
			drag = GetComponent<Draggable>();
			electronDensity = protonDensity;
		}

		private void Update() {
			if (rubbing) {
				doOnceForRubbing();
				if (canCharge && triggerInterceptLate && isActiveObject && limit > 0) {
					if (!x()) {
						return; //Low velocity rubbing unresponsive
					}
					
					if(timeDelta < timeLimit) {
						timeDelta += Time.deltaTime;
					} else {
						timeDelta = 0f;
						limit--;
						electronDensity += Mathf.Sign(chargeAffinity);
						conjugateItem.electronDensity += Mathf.Sign(conjugateItem.chargeAffinity);
					}

					accumulatedTime += Time.deltaTime;
				}

				bool x() {
					return rb.velocity.sqrMagnitude > 0.16f;
				}
			}
		}

		/// <summary>
		/// Predicted problems:
		/// canCharge might change while inside the trigger, leading to the object not getting charged as expected.
		/// </summary>
		/// <param name="col"></param>
		private void OnTriggerEnter2D(Collider2D col) {
			isActiveObject = drag.dragging;
			if (isActiveObject) {
				if (col.gameObject.TryGetComponent(out ElectricSpecs specs)) {
					if (canCharge) {
						#region Debug
						if (!(canRub ^ canContact)) {
							Debug.LogWarning("Both canRub and canContact share the same state for this object, indeterminate behavior!", gameObject);
						}
					
						if (!(specs.canRub ^ specs.canContact)) {
							Debug.LogWarning("Both canRub and canContact share the same state for this object, indeterminate behavior!", specs.gameObject);
						}
						#endregion

						triggerIntercept = true;
						triggerInterceptLate = true;
						if (canContact && specs.canContact) {
							chargingTriggerCoroutine = StartCoroutine(invokeChargingWithDelay(specs));
						}
					
						if (canRub && specs.canRub && (Math.Sign(chargeAffinity * specs.chargeAffinity) == -1)) {
							conjugateItem = specs;
							rubbingTriggerCoroutine = StartCoroutine(invokeRubbingWithDelay(specs));
						}
					} else if (hasElectrostaticForce) {
						Debug.Log("Configure this object to have reactive physics or push animation", gameObject);
					} else if (specs.hasElectrostaticForce) {
						Debug.Log("Configure this object to have reactive physics or push animation", specs.gameObject);
					}
				}
			}
		}

		private void OnTriggerExit2D(Collider2D other) {
			if (isActiveObject) {
				triggerIntercept = false;
				StartCoroutine(lateTriggerRemove(0.5f));
			}

			IEnumerator lateTriggerRemove(float time) {
				yield return new WaitForSecondsRealtime(time);
				if (!triggerIntercept) {
					triggerInterceptLate = false;
				} else {
					other.TryGetComponent(out ElectricSpecs specs);
					if (chargingTriggerCoroutine != null) {
						StopCoroutine(chargingTriggerCoroutine);
						chargingTriggerCoroutine = null;
					}
					if (rubbing && specs.materialID == rubbingMaterialID) {
						OnStopRubbing(specs);
					} else {
						if (rubbingTriggerCoroutine != null) {
							StopCoroutine(rubbingTriggerCoroutine);
							rubbingTriggerCoroutine = null;
						}
					}
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
		private void OnStartRubbing(ElectricSpecs specs) {
			rubbing = true;
			if (canCharge) {
				if (conjugateItem != specs) {
					conjugateItem = specs;
					rubbingMaterialID = specs.materialID;
					accumulatedTime = 0;
				}
			}
		}

		private void OnStopRubbing(ElectricSpecs specs) {
			rubbing = false;
			isActiveObject = false;
			rubbingMaterialID = -1;
			didOnceForRubbing = false;
		}
		
		public void OnResetRubbing() {
			isActiveObject = false;
			rubbing = false;
			electronDensity = protonDensity;
			accumulatedTime = 0;
			rubbingMaterialID = -1;
			didOnceForRubbing = false;
		}

		/// <summary>
		/// If the trigger overlap has been performed and the object has been stationary for some time, assume that the objects have been touched.
		/// The distribution is based on proton counts. This should be coupled more closely with the charge particles. Introduce "proton" and "electron" fields.
		/// </summary>
		/// <param name="specs"></param>
		/// <param name="chargeOverride"></param>
		private void DoContactCharging(ElectricSpecs specs, bool chargeOverride = false) {
			if (isActiveObject && canCharge && specs.canCharge && ((canContact && specs.canContact) || chargeOverride)){
				//Individual proton count does not affect accumulated charge. It's only to determine the neutral point of the material.
				//Delta = (1-(total ED/total PD)) * sign * (avgAffinity)
				electronDensity = (1 - (electronDensity + specs.electronDensity) / (protonDensity + specs.protonDensity)) * Mathf.Sign(chargeAffinity) * (Mathf.Abs(chargeAffinity) + Mathf.Abs(specs.chargeAffinity));
				specs.electronDensity = specs.protonDensity - (protonDensity - electronDensity);
			}
		}
		
		private IEnumerator invokeChargingWithDelay(ElectricSpecs material) {
			yield return new WaitForSecondsRealtime(1f);
			if (triggerIntercept) DoContactCharging(material);
		}

		private IEnumerator invokeRubbingWithDelay(ElectricSpecs material) {
			yield return new WaitForSecondsRealtime(1f);
			if (triggerIntercept) OnStartRubbing(material);
		}

		private bool didOnceForRubbing;
		private float limit;
		private float timeDelta;
		private float timeLimit;
		private void doOnceForRubbing() {
			if (!didOnceForRubbing) {
				didOnceForRubbing = true;
				if (chargeAffinity > 0) {
					limit = electronDensity - 1;
				} else {
					limit = conjugateItem.electronDensity - 1;
				}
				timeDelta = 0f;
				timeLimit = 10f / (Mathf.Abs(chargeAffinity) + Mathf.Abs(conjugateItem.chargeAffinity));
			}
		}

		public float getEffectiveCharge() {
			return protonDensity - electronDensity;
		}
	}
}