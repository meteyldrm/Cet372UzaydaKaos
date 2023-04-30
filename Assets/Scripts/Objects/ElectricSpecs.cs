// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using System.Collections;
using Reports;
using Unity.VisualScripting;
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
		public float electronDensity;
		
		public float accumulatedTime;
		private bool rubbing;
		public int rubbingMaterialID;
		[NonSerialized] public ElectricSpecs conjugateItem = null;
		[NonSerialized] public bool isActiveObject;
		
		public bool canCharge = true;
		public bool canRub;
		public bool canContact;
		public bool hasElectrostaticForce;

		private Rigidbody2D rb;
		private Draggable drag;
		private bool configured;

		private Coroutine rubbingTriggerCoroutine = null;
		private Coroutine chargingTriggerCoroutine = null;
		private bool triggerIntercept = false;
		private bool triggerInterceptLate = false;

		private void Start() {
			if (configured) return;
			rb = GetComponent<Rigidbody2D>();
			drag = GetComponent<Draggable>();
			if (electronDensity == 0) {
				electronDensity = protonDensity;
			}

			configured = true;
		}

		private void OnEnable() {
			if (configured) return;
			rb = GetComponent<Rigidbody2D>();
			drag = GetComponent<Draggable>();
			if (electronDensity == 0) {
				electronDensity = protonDensity;
			}

			configured = true;
		}

		private void Update() {
			if (rubbing) {
				if (accumulatedTime < 0.1) {
					doOnceForRubbing();
				}
				if (canCharge && triggerInterceptLate && isActiveObject && limit > 0) {
					if (!x()) {
						return; //Low velocity rubbing unresponsive
					}
					
					if(timeDelta < timeLimit) {
						timeDelta += Time.deltaTime;
					} else {
						timeDelta = 0f;
						limit--;
						electronDensity -= Mathf.Sign(chargeAffinity);
						conjugateItem.electronDensity -= Mathf.Sign(conjugateItem.chargeAffinity);
					}

					accumulatedTime += Time.deltaTime;
					conjugateItem.accumulatedTime += Time.deltaTime;
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
			if (col.CompareTag("ReportCollider")) return;
			isActiveObject = drag != null && drag.dragging;
			if (isActiveObject) {
				if (col.gameObject.TryGetComponent(out ElectricSpecs specs)) {
					if (canCharge) {
						triggerIntercept = true;
						triggerInterceptLate = true;
						if (canContact && specs.canContact) {
							chargingTriggerCoroutine = StartCoroutine(invokeChargingWithDelay(specs));
						}
					
						if (canRub && specs.canRub && (Math.Sign(chargeAffinity * specs.chargeAffinity) == -1)) {
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
			if (other.CompareTag("ReportCollider")) {
				GeneralGuidance.Instance.report.OnLeaveReport(materialID);
			}
			if (isActiveObject) {
				triggerIntercept = false;
				StartCoroutine(lateTriggerRemove(0.2f));
			}

			IEnumerator lateTriggerRemove(float time) {
				yield return new WaitForSeconds(time);
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
					specs.conjugateItem = this;
					specs.rubbingMaterialID = materialID;
					rubbingMaterialID = specs.materialID;
					accumulatedTime = 0;
					conjugateItem.accumulatedTime = 0;
				}
			}
		}

		private void OnStopRubbing(ElectricSpecs specs) {
			rubbing = false;
			didOnceForRubbing = false;
			specs.accumulatedTime = accumulatedTime;
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
			if (isActiveObject && canCharge && specs.canCharge && ((canContact && specs.canContact) || chargeOverride)) {
				//Individual proton count does not affect accumulated charge. It's only to determine the neutral point of the material.
				//Delta = (1-(total ED/total PD)) * sign * (avgAffinity)
				var delta = Mathf.Floor((getEffectiveCharge() + specs.getEffectiveCharge())/2);
				var conjugateDelta = Mathf.Ceil((getEffectiveCharge() + specs.getEffectiveCharge())/2);
				electronDensity = protonDensity - delta;
				specs.electronDensity = specs.protonDensity - conjugateDelta;
				if (specs.TryGetComponent(out Lightbulb bulb)) {
					if (conjugateDelta is > 0 or < 0 || delta is > 0 or < 0) {
						bulb.LightUp();
					}
				}
			}
		}
		
		private IEnumerator invokeChargingWithDelay(ElectricSpecs material) {
			if (material.TryGetComponent(out Lightbulb bulb)) {
				yield return new WaitForSeconds(0.6f);
			} else {
				yield return new WaitForSeconds(1.2f);
			}
			if (triggerIntercept) DoContactCharging(material);
		}

		private IEnumerator invokeRubbingWithDelay(ElectricSpecs material) {
			if (accumulatedTime > 0) {
				yield return new WaitForSeconds(2f);
			} else {
				yield return new WaitForSeconds(1f);
			}
			if (triggerIntercept) OnStartRubbing(material);
		}

		private bool didOnceForRubbing;
		private float limit;
		private float timeDelta;
		private float timeLimit;
		private void doOnceForRubbing() {
			if (!didOnceForRubbing) {
				rubbingMaterialID = conjugateItem.materialID;
				conjugateItem.rubbingMaterialID = materialID;
				
				DoContactCharging(conjugateItem, true);
				didOnceForRubbing = true;
				if (chargeAffinity > 0) {
					limit = electronDensity - 1;
				} else if (conjugateItem.chargeAffinity > 0) {
					limit = conjugateItem.electronDensity - 1;
				}
				timeDelta = 0f;
				timeLimit = 5f / (Mathf.Abs(chargeAffinity) + Mathf.Abs(conjugateItem.chargeAffinity));
			}
		}

		public void resetRubbingPosition() {
			if (!configured) {
				rb = GetComponent<Rigidbody2D>();
				drag = GetComponent<Draggable>();
				if (electronDensity == 0) {
					electronDensity = protonDensity;
				}

				configured = true;
			}
			
			rb.position = drag.dragStartPosition;
		}

		public float getEffectiveCharge() {
			return protonDensity - electronDensity;
		}
	}
}