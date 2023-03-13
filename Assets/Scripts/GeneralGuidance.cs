using System;
using System.Collections.Generic;
using System.Linq;
using Control;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

/// <summary>
/// Every script has a reference to here. Do NOT try to replicate an event architecture. A more flexible, in-situ object manufacturing system can be implemented.
/// </summary>
public class GeneralGuidance : Singleton<GeneralGuidance> {
	#region Utility
	private static readonly List<string> scenes = new() {
		"Assets/Scenes/Start.unity",
		"Assets/Scenes/Introduction.unity",
		"Assets/Scenes/LightActivity.unity", 
		"Assets/Scenes/ChargeActivity.unity",
		"Assets/Scenes/DoorUnlockActivity.unity",
		"Assets/Scenes/IndoorsActivity.unity",
		"Assets/Scenes/ReactorActivity.unity",
		"Assets/Scenes/TurbineActivity.unity"
	};

	private int sceneIndex;

	public void LoadNextScene() {
		sceneIndex++;
		cameraRegistered = false;
		if (scenes.Count <= sceneIndex) {
			SceneManager.LoadScene(scenes[sceneIndex]);
		}
	}

	public static List<T> GetAllSceneComponents<T>() {
		var all = new List<T>();
		foreach (var obj in GetAllSceneGameObjects(requireActive: true)) {
			if (obj.TryGetComponent(out T component)) {
				if (component != null) {
					all.Add(component);
				}
			}
		}

		return all;
	}

	public static List<GameObject> GetAllSceneGameObjects(bool requireActive = false) {
		var x = SceneManager.GetActiveScene().GetRootGameObjects();
		var all = new List<GameObject>();
		foreach (var roots in x) {
			all.AddRange(GetChildGameObjects(roots));
		}
		
		List<GameObject> GetChildGameObjects(GameObject obj) {
			var objList = new List<GameObject>();
			//Only return the active branch of objects, disabled objects return null
			if ((requireActive && obj.activeSelf) || !requireActive) {
				for (var i = 0; i < obj.transform.childCount; i++) {
					objList.AddRange(GetChildGameObjects(obj.transform.GetChild(i).gameObject));
				}
				
				objList.Add(obj);
			}
			return objList;
		}

		return all;
	}
	#endregion

	#region Player Variables
	public string playerName = "";

	/// <summary>
	/// 3 dimensional array supporting the material report. 
	/// [Row, Index, Iteration] = "IntSeconds | materialID". 
	/// Iteration != 0 is used for the expanded report. Copy values, leave seconds empty.
	/// </summary>
	[Tooltip("Utilize the string as \"Vector3 ; IntSeconds | materialID\"")]
	[SerializeField] private List<string> materialReportInitializationList;
	public string[,,] materialReportArray = new string[3,2,2];
	#endregion

	#region Draggable Intercept
	private bool dragPause;
	private bool dragDeadlock;
	private Draggable draggingObject;
	private Camera cam;
	private bool cameraRegistered;
	
	public IDraggableController DraggableController;
	private bool isClicking;

	/// <summary>
	/// Draggable intercept depends on the canDrag attribute of the draggable. Make sure they're bound to their respective controllers (report items' values will be set to false).
	/// If the report draggable intercepts the touch while 
	/// </summary>
	private void DoDraggableIntercept() {
		if (dragPause) return;

		var spaceVector = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, math.abs(cam.transform.position.z)));

		if(Input.GetKeyDown(KeyCode.Mouse0)) {
			isClicking = true;
			
			if (DraggableController != null) {
				var draggables = DraggableController.getDraggables();
				if (draggables != null && draggables.Count > 0) {
					foreach (var go in draggables) {
						if (!dragDeadlock) {
							if (!go.activeSelf) {
								continue;
							}

							draggingObject = go.GetComponent<Draggable>();
							dragDeadlock = draggingObject.interceptInput(spaceVector);
							if (dragDeadlock) {
								draggingObject.SetInteractionState("start", spaceVector);
							}
						} else {
							break;
						}
					}
				}
			}
		}
		
		if (Input.GetKey(KeyCode.Mouse0)) {
			if (isClicking) {
				try {
					draggingObject.SetTransformGoal(spaceVector);
				} catch(NullReferenceException) {
				}
			}
		}

		if (Input.GetKeyUp(KeyCode.Mouse0)) {
			isClicking = false;
			dragDeadlock = false;
			draggingObject.SetInteractionState("end", Vector2.zero);
			draggingObject = null;
		}
	}
	
	public void registerCamera(Camera _camera) {
		cam = _camera;
		cameraRegistered = true;
	}

	public interface IDraggableController {
		List<GameObject> getDraggables();
	}
	#endregion

	private void Start() {
		//Report initializer for testing prefab generation across scenes.
		if (materialReportInitializationList.Count > 0) {
			foreach (var i in materialReportInitializationList) {
				var x = i.Split(" ; ");
				var y = x[0].Split(",");
				materialReportArray[int.Parse(y[0]), int.Parse(y[1]), int.Parse(y[2])] = x[1];
			}
		}
	}

	private void Update() {
		if (cameraRegistered) {
			DoDraggableIntercept();
		}
	}
}