using System.Collections.Generic;
using UnityEngine;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Objects {
	/// <summary>
	/// A couple approaches are viable:
	///
	/// 1: Rigidbody particle simulation with minimum distance spawning and built-in anti collision simulation. Can add force this way.
	///
	/// 2: Instantiate prefabs on pre-determined positions. No simulation is necessary, move the particles with simple lerp.
	///
	/// 1 pro: The induced forces can be simulated with ease. Constraints have to be implemented which will be a hassle.
	///
	/// 2 pro: The particles have to be interchanged. Set parent when particle starts to move, lerp to determined position. For induced charges, move the entirety of negative charges away from the charge source.
	/// </summary>
	public class ParticleManager: MonoBehaviour {
		private List<GameObject> _positiveParticles = new();
		private List<GameObject> _negativeParticles = new();

		[SerializeField] private List<Vector3> positivePositions;
		[SerializeField] private List<Vector3> negativePositions;

		private void Start() {
			if (gameObject.TryGetComponent(out ElectricSpecs specs)) {
				for (var i = 0; i < specs.protonDensity; i++) {
					var positiveParticle = Instantiate(GeneralGuidance.Instance.positiveParticlePrefab, gameObject.transform, true);
					_positiveParticles.Add(positiveParticle);
				}

				for (var i = 0; i < specs.protonDensity + 6; i++) {
					var negativeParticle = Instantiate(GeneralGuidance.Instance.negativeParticlePrefab, gameObject.transform, true);
					_negativeParticles.Add(negativeParticle);
				}
			}

			foreach (Transform tf in transform) {
				tf.gameObject.SetActive(false);
			}
		}

		private void SetPositiveParticlePosition() {
			for (var i = 0; i < _positiveParticles.Count; i++) {
				_positiveParticles[i].transform.localPosition = positivePositions[i];
			}
		}
		
		private void SetNegativeParticlePosition() {
			for (var i = 0; i < _negativeParticles.Count; i++) {
				_negativeParticles[i].transform.localPosition = negativePositions[i];
			}
		}

		public void SetDeltaNegativeParticlePosition(float count) {
			var delta = 0;
			foreach (var particle in _negativeParticles) {
				if (delta < count) {
					particle.SetActive(true);
					delta++;
				} else {
					break;
				}
			}
		}
	}
}