using System;
using System.Collections.Generic;
using Objects;
using Reports;
using UI;
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
	public ReportManager report;
	public AlertController alert;

	public void LoadNextScene() {
		sceneIndex++;
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
	/// [Row, Index, Iteration] = "AccumulatedCharge | ReportedCharge | IntSeconds | ReportedSeconds | MaterialID | ConjugateMaterialID". 
	/// Iteration != 0 is used for the expanded report. Copy values, leave seconds empty.
	/// </summary>
	[SerializeField] private List<string> materialReportInitializationList = new();
	public string[,,] materialReportArray = new string[3,2,2];
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

	/// <summary>
	/// Returns false if the conjugate ID doesn't match
	/// </summary>
	/// <param name="specs"></param>
	/// <param name="row"></param>
	/// <param name="index"></param>
	/// <param name="iteration"></param>
	public bool AddSpecsToReport(ElectricSpecs specs, int row = 0, int index = 0, int iteration = 0) {
		return SerializeSpecsToReport(specs, row, index, iteration);
	}
	
	/// <summary>
	/// [Row, Index, Iteration] = "AccumulatedCharge | ReportedCharge | IntSeconds | ReportedSeconds | MaterialID | ConjugateMaterialID".
	/// The ternary checks for conjugate mismatch.
	/// </summary>
	/// <param name="specs"></param>
	/// <param name="row"></param>
	/// <param name="index"></param>
	/// <param name="iteration"></param>
	/// <returns></returns>
	private bool SerializeSpecsToReport(ElectricSpecs specs, int row = 0, int index = 0, int iteration = 0) {
		if (wasFoundInCurrentIteration(specs.materialID, iteration)) { // What if the func returns false
			deleteFromCurrentIteration(specs.materialID, iteration);
		}
		if (materialReportArray[row, 0 == index ? 1 : 0, iteration] != null && materialReportArray[row, 0 == index ? 1 : 0, iteration] != string.Empty) {
			var str = materialReportArray[row, 0 == index ? 1 : 0, iteration].Split("|")[5];
			if (str != "" && str == $"{specs.materialID}") {
				materialReportArray[row, index, iteration] = $"{specs.getEffectiveCharge()}||{specs.accumulatedTime}||{specs.materialID}|{specs.rubbingMaterialID}";
				return true;
			}

			return false;
		}
		
		materialReportArray[row, index, iteration] = $"{specs.getEffectiveCharge()}||{specs.accumulatedTime}||{specs.materialID}|{specs.rubbingMaterialID}";
		return true;
	}

	public GameObject SpawnObjectFromReport(int row = 0, int index = 0, int iteration = 0) {
		return null;
	}

	#region Report Control
	private void deleteFromCurrentIteration(int id, int iteration) {
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 2; j++) {
				if (materialReportArray[i, j, iteration] != null && materialReportArray[i, j, iteration] != string.Empty && materialReportArray[i, j, iteration].Split("|")[5] == $"{id}") {
					materialReportArray[i, j, iteration] = string.Empty;
				}
			}
		}
	}

	private bool wasFoundInCurrentIteration(int id, int iteration) {
		bool x = false;
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 2; j++) {
				if (materialReportArray[i, j, iteration] != null && materialReportArray[i, j, iteration] != string.Empty) {
					if(materialReportArray[i, j, iteration].Split("|")[5] == $"{id}") x = true;
				}
			}
		}

		return x;
	}
	#endregion
}