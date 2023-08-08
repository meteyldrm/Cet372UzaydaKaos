// ReSharper disable file CommentTypo
// ReSharper disable file IdentifierTypo

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Objects {
	/// <summary>
	/// Charge interactions will take place according to proton density (how many + charges the object holds).
	/// The rubbing charge calculations happen based on charge_1 = time * (affinity_1 - affinity2) * (proton_density_1 / (proton_density_1 + proton_density_2))
	/// The contact charge calculations happen based on charge_1 = (electron_density_1 + electron_density_2) * (proton_density_1 / (proton_density_1 + proton_density_2))
	/// electron_density = proton_density - accumulatedCharge
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
		private bool _rubbing;
		public int rubbingMaterialID;
		[NonSerialized] public ElectricSpecs ConjugateItem = null;
		[NonSerialized] public bool IsActiveObject;
		
		public bool canCharge = true;
		public bool canRub;
		public bool canContact;
		public bool hasElectrostaticForce;

		private Rigidbody2D _rb;
		private Draggable _drag;
		private bool _configured;

		private Coroutine _chargingTriggerCoroutine = null;
		private bool _triggerIntercept = false;

		private bool _showParticles;
		public bool snapped = false;

		private bool _wentLeft = false;
		private bool _wentRight = false;

		private void Start() {
			if (_configured) return;
			_rb = GetComponent<Rigidbody2D>();
			_drag = GetComponent<Draggable>();
			if (electronDensity == 0) {
				electronDensity = protonDensity;
			}

			_configured = true;
		}

		private void OnEnable() {
			if (_configured) return;
			_rb = GetComponent<Rigidbody2D>();
			_drag = GetComponent<Draggable>();
			if (electronDensity == 0) {
				electronDensity = protonDensity;
			}

			_configured = true;
		}

		private void Update() {
			// if (rubbing) {
			// 	if (accumulatedTime < 0.1) {
			// 		doOnceForRubbing();
			// 	}
			// 	if (canCharge && triggerInterceptLate && isActiveObject && limit > 0) {
			// 		if (!x()) {
			// 			return; //Low velocity rubbing unresponsive
			// 		}
			// 		
			// 		if(timeDelta < timeLimit) {
			// 			timeDelta += Time.deltaTime;
			// 		} else {
			// 			timeDelta = 0f;
			// 			limit--;
			// 			electronDensity -= Mathf.Sign(chargeAffinity);
			// 			conjugateItem.electronDensity -= Mathf.Sign(conjugateItem.chargeAffinity);
			// 		}
			//
			// 		accumulatedTime += Time.deltaTime;
			// 		conjugateItem.accumulatedTime += Time.deltaTime;
			// 	}
			//
			// 	bool x() {
			// 		return rb.velocity.sqrMagnitude > 0.16f;
			// 	}
			// }
		}

		/// <summary>
		/// Predicted problems:
		/// canCharge might change while inside the trigger, leading to the object not getting charged as expected.
		/// </summary>
		/// <param name="col"></param>
		private void OnTriggerEnter2D(Collider2D col) {
			if (col.CompareTag("ReportCollider")) return;
			if (col.CompareTag("RubMachineCollider")) return;
			IsActiveObject = _drag != null && _drag.dragging;
			if (IsActiveObject) {
				// ReSharper disable once InvertIf
				if (col.gameObject.TryGetComponent(out ElectricSpecs specs) && canCharge) {
					_triggerIntercept = true;
					if (canContact && specs.canContact) {
						_chargingTriggerCoroutine = StartCoroutine(InvokeChargingWithDelay(specs));
					}
					
					// if (canRub && specs.canRub && (Math.Sign(chargeAffinity * specs.chargeAffinity) == -1)) {
					// 	rubbingTriggerCoroutine = StartCoroutine(invokeRubbingWithDelay(specs));
					// }
				}
			} else if (hasElectrostaticForce) {
				// ReSharper disable once InvertIf
				if(TryGetComponent(out RectTransform rt) && col.gameObject.TryGetComponent(out ElectricSpecs specs)) {
					var rp = rt.position;

					if (specs.GetEffectiveCharge() == 0) {
						return;
					}

					if (Mathf.FloorToInt(Mathf.Sign(specs.GetEffectiveCharge())) == Mathf.FloorToInt(Mathf.Sign(GetEffectiveCharge()))) {
						rp.x += 0.8f;
						rt.position = rp;
						_wentRight = true;
						GeneralGuidance.Instance.dialogueRoomForcePositive = true;
						hasElectrostaticForce = false;
						_wentRight = false; //Make the object appear on the right permanently
					} else {
						rp.x -= 0.8f;
						rt.position = rp;
						_wentLeft = true;
						GeneralGuidance.Instance.dialogueRoomForceNegative = true;
					}
				}
			}
		}

		private void OnTriggerExit2D(Collider2D other) {
			if (other.CompareTag("ReportCollider") && gameObject.activeSelf && _drag.dragging) {
				// ReSharper disable once InvertIf
				if (GeneralGuidance.Instance.report.OnLeaveReport(materialID) && snapped) { //If the object was snapped before
					accumulatedTime = 0;
					snapped = false;
				}
			} else if (other.CompareTag("RubMachineCollider")) {
				switch (other.gameObject.name) {
					case "Slut1":
						GeneralGuidance.Instance.rubbingMachine.slot1 = null;
						break;
					case "Slut2":
						GeneralGuidance.Instance.rubbingMachine.slot2 = null;
						break;
				}

				// if (other.gameObject.name == "ChargePanel") {
				// 	GeneralGuidance.Instance.rubbingMachine.slot2 = null;
				// }
			} else if(TryGetComponent(out RectTransform rt)) {
				if (_wentLeft) {
					var rp = rt.position;
					rp.x += 0.8f;
					rt.position = rp;
					_wentLeft = false;
				} else if(_wentRight) {
					var rp = rt.position;
					rp.x -= 0.8f;
					rt.position = rp;
					_wentRight = false;
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
			_rubbing = true;
			var doOnce = false;
			if (canCharge) {
				if (ConjugateItem != specs) {
					ConjugateItem = specs;
					doOnce = true;
					specs.ConjugateItem = this;
					specs.rubbingMaterialID = materialID;
					rubbingMaterialID = specs.materialID;
					accumulatedTime = 0;
					ConjugateItem.accumulatedTime = 0;
				}
			} else {
				return;
			}
			if (accumulatedTime < 0.1f) {
				doOnceForRubbing(doOnce);
			}
		}

		private IEnumerator _rubForOneSecond(ElectricSpecs specs) {
			var delta = 0f;
			// ReSharper disable once InvertIf
			if (!_rubbing) {
				OnStartRubbing(specs);
				while (delta < 1f) {
					if (canCharge && _limit > 0) {
						if(_timeDelta < _timeLimit) {
							_timeDelta += Time.deltaTime;
						} else {
							_timeDelta = 0f;
							_limit--;
							electronDensity -= Mathf.Sign(chargeAffinity);
							ConjugateItem.electronDensity -= Mathf.Sign(ConjugateItem.chargeAffinity);
						}
				
						accumulatedTime += Time.deltaTime;
						ConjugateItem.accumulatedTime += Time.deltaTime;
					}
					delta += Time.deltaTime;
					OnChangeVisualParticles();
					specs.OnChangeVisualParticles();
					yield return null;
				}
				delta = 0f;
				OnStopRubbing(specs);
			}
		}

		public void RubForOneSecond(ElectricSpecs specs) {
			StartCoroutine(_rubForOneSecond(specs));
		}

		private void OnStopRubbing(ElectricSpecs specs) {
			_rubbing = false;
			_didOnceForRubbing = false;
			specs.accumulatedTime = accumulatedTime;
			OnChangeVisualParticles();
			specs.OnChangeVisualParticles();
		}
		
		public void OnResetRubbing() {
			IsActiveObject = false;
			_rubbing = false;
			electronDensity = protonDensity;
			accumulatedTime = 0;
			rubbingMaterialID = -1;
			_didOnceForRubbing = false;
			OnChangeVisualParticles();
		}

		/// <summary>
		/// If the trigger overlap has been performed and the object has been stationary for some time, assume that the objects have been touched.
		/// The distribution is based on proton counts. This should be coupled more closely with the charge particles. Introduce "proton" and "electron" fields.
		/// </summary>
		/// <param name="specs"></param>
		/// <param name="chargeOverride"></param>
		private void DoContactCharging(ElectricSpecs specs, bool chargeOverride = false) {
			if (IsActiveObject && canCharge && specs.canCharge && ((canContact && specs.canContact) || chargeOverride)) {
				//Individual proton count does not affect accumulated charge. It's only to determine the neutral point of the material.
				//Delta = (1-(total ED/total PD)) * sign * (avgAffinity)
				var delta = Mathf.Floor((GetEffectiveCharge() + specs.GetEffectiveCharge())/2);
				var conjugateDelta = Mathf.Ceil((GetEffectiveCharge() + specs.GetEffectiveCharge())/2);
				electronDensity = protonDensity - delta;
				specs.electronDensity = specs.protonDensity - conjugateDelta;
				if (specs.TryGetComponent(out LightBulb bulb)) {
					if (conjugateDelta is > 0 or < 0 || delta is > 0 or < 0) {
						bulb.LightUp();
					}
				}

				if (Math.Abs(specs.protonDensity - specs.electronDensity) < 0.1f && specs.gameObject.CompareTag("ContactNeutral")) {
					print("Neutral skip");
					GeneralGuidance.Instance.skipDialogueRoomNeutral = true;
				}
			}
			OnChangeVisualParticles();
			specs.OnChangeVisualParticles();
		}
		
		private IEnumerator InvokeChargingWithDelay(ElectricSpecs material) {
			if (material.TryGetComponent(out LightBulb bulb)) {
				yield return new WaitForSeconds(0.6f);
			} else {
				yield return new WaitForSeconds(1.2f);
			}
			if (_triggerIntercept) DoContactCharging(material);
		}

		private IEnumerator InvokeRubbingWithDelay(ElectricSpecs material) {
			if (accumulatedTime > 0) {
				yield return new WaitForSeconds(2f);
			} else {
				yield return new WaitForSeconds(1f);
			}
			if (_triggerIntercept) OnStartRubbing(material);
		}

		private bool _didOnceForRubbing;
		private float _limit;
		private float _timeDelta;
		private float _timeLimit;
		private void doOnceForRubbing(bool doOnce) {
			if (!_didOnceForRubbing) {
				rubbingMaterialID = ConjugateItem.materialID;
				ConjugateItem.rubbingMaterialID = materialID;
				if(doOnce) DoContactCharging(ConjugateItem, true);
				_didOnceForRubbing = true;
				if (chargeAffinity > 0) {
					_limit = electronDensity - 1;
				} else if (ConjugateItem.chargeAffinity > 0) {
					_limit = ConjugateItem.electronDensity - 1;
				}
				_timeDelta = 0f;
				_timeLimit = 5f / (Mathf.Abs(chargeAffinity) + Mathf.Abs(ConjugateItem.chargeAffinity));
			}
		}

		public void ResetRubbingPosition() {
			if (!_configured) {
				_rb = GetComponent<Rigidbody2D>();
				_drag = GetComponent<Draggable>();
				if (electronDensity == 0) {
					electronDensity = protonDensity;
				}

				_configured = true;
			}
			
			_rb.position = _drag.dragStartPosition;
			
			//TODO: Check if this triggers report colliders. We need to instantiate from the report, not drag from it.
		}

		public float GetEffectiveCharge() {
			return protonDensity - electronDensity;
		}

		public void OnShowVisualParticles() {
			_showParticles = true;
			if (TryGetComponent(out Image img)) {
				Color.RGBToHSV(img.color, out var h, out var s, out var v);
				img.color = Color.HSVToRGB(h, s, 65);
			}
			foreach (Transform tr in transform) {
				if (tr.gameObject.name.ToLowerInvariant() is "positives" or "negatives") {
					tr.gameObject.SetActive(true);
				}
			}
			OnChangeVisualParticles();
		}
		
		public void OnHideVisualParticles() {
			_showParticles = false;
			var img = GetComponent<Image>();
			Color.RGBToHSV(img.color, out var h, out var s, out var v);
			img.color = Color.HSVToRGB(h, s, 100);
			foreach (Transform tr in transform) {
				if (tr.gameObject.name.ToLowerInvariant() is "positives" or "negatives") {
					tr.gameObject.SetActive(false);
				}
			}
		}

		private void OnChangeVisualParticles() {
			if (!_showParticles) return;
			foreach (Transform tr in transform) {
				for (var i = 0; i < tr.childCount; i++) {
					switch (tr.gameObject.name.ToLowerInvariant()) {
						case "positives":
							tr.GetChild(i).gameObject.SetActive(i < protonDensity);
							break;
						case "negatives":
							tr.GetChild(i).gameObject.SetActive(i < electronDensity);
							break;
					}
				}
			}
		}
	}
}