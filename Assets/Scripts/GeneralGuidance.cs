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
	private static readonly List<string> scenario = new() {
		"Start",
		"Intro",
		"LightAndCharge",
		"DoorUnlock",
		"Indoors",
		"Reactor",
		"Turbine"
	};

	private int scenarioIndex = -1;
	public ReportManager report;
	public NavbarManager navbar;
	public AlertController alert;

	public void LoadNextScenario() {
		scenarioIndex++;
		if (scenarioIndex <= scenario.Count) {
			ScenarioStartup(scenario[scenarioIndex]);
		}
	}

	private void ScenarioStartup(string objectName) {
		var scenarioObject = GetSceneGameObjectByName(objectName, 1);
		if (scenarioObject == null) {
			Debug.LogError($"Scenario {objectName} unable to be loaded.");
			return;
		};
		
		switch (objectName) {
			case "Start": {
				scenarioObject.SetActive(true);
				break;
			}
			
			case "Intro": {
				GetSceneGameObjectByName("Start", 1).SetActive(false);
				scenarioObject.SetActive(true);
				break;
			}

			case "LightAndCharge": {
				GetSceneGameObjectByName("Intro", 1).SetActive(false);
				scenarioObject.SetActive(true);
				break;
			}

			case "DoorUnlock": {
				GetSceneGameObjectByName("LightAndCharge", 1).SetActive(false);
				scenarioObject.SetActive(true);
				break;
			}
			
			case "Indoors": {
				GetSceneGameObjectByName("DoorUnlock", 1).SetActive(false);
				scenarioObject.SetActive(true);
				break;
			}

			case "Reactor": {
				GetSceneGameObjectByName("Indoors", 1).SetActive(false);
				scenarioObject.SetActive(true);
				break;
			}
			
			case "Turbine": {
				GetSceneGameObjectByName("Reactor", 1).SetActive(false);
				scenarioObject.SetActive(true);
				break;
			}
		}
	}

	public static List<T> GetAllSceneComponents<T>(int depthLimit = 0) {
		var all = new List<T>();
		foreach (var obj in GetAllSceneGameObjects(depthLimit, requireActive: true)) {
			if (obj.TryGetComponent(out T component)) {
				if (component != null) {
					all.Add(component);
				}
			}
		}

		return all;
	}
	
	public static GameObject GetSceneGameObjectByName(string name, int depthLimit = 0, bool requireActive = false) {
		return GetAllSceneGameObjectsByName(name, depthLimit, requireActive)[0];
	}

	public static List<GameObject> GetAllSceneGameObjectsByName(string name, int depthLimit = 0, bool requireActive = false) {
		List<GameObject> list = new List<GameObject>();

		foreach (var obj in GetAllSceneGameObjects(depthLimit, requireActive)) {
			if (obj.name.Equals(name)) {
				list.Add(obj);
			}
		}

		if (list.Count < 1) {
			list.Add(null);
		}

		return list;
	}
	
	public static List<GameObject> GetAllSceneGameObjects(int depthLimit = 0, bool requireActive = false) {
		var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
		var all = new List<GameObject>();
		foreach (var rootObject in rootObjects) {
			all.AddRange(GetChildGameObjects(rootObject, 0, depthLimit));
		}
		
		List<GameObject> GetChildGameObjects(GameObject obj, int currentDepth, int dl) {
			var objList = new List<GameObject>();

			if ((requireActive && obj.activeSelf) || !requireActive) {
				if (dl == 0 || currentDepth < dl) {
					for (var i = 0; i < obj.transform.childCount; i++) {
						objList.AddRange(GetChildGameObjects(obj.transform.GetChild(i).gameObject, currentDepth + 1, dl));
					}
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

	[SerializeField] public List<GameObject> MaterialPrefabList = new();
	#endregion

	private void Start() {
		#region Testing
		//Report initializer for testing prefab generation across scenes.
		if (materialReportInitializationList.Count > 0) {
			foreach (var i in materialReportInitializationList) {
				var x = i.Split(" ; ");
				var y = x[0].Split(",");
				materialReportArray[int.Parse(y[0]), int.Parse(y[1]), int.Parse(y[2])] = x[1];
			}
		}
		#endregion

		LoadNextScenario();
	}

	#region Report Functionality
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

	public bool HasMatchingConjugate(ElectricSpecs specs, int row = 0, int index = 0, int iteration = 0) {
		if (materialReportArray[row, 0 == index ? 1 : 0, iteration] != null && materialReportArray[row, 0 == index ? 1 : 0, iteration] != string.Empty) {
			var str = materialReportArray[row, 0 == index ? 1 : 0, iteration].Split("|")[5];
			if (!string.IsNullOrEmpty(str) && str == $"{specs.materialID}") {
				return true;
			}
		}

		return false;
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
		if (materialReportArray[row, 0 == index ? 1 : 0, iteration] != null && materialReportArray[row, 0 == index ? 1 : 0, iteration] != string.Empty) {
			var str = materialReportArray[row, 0 == index ? 1 : 0, iteration].Split("|")[5];
			if (str == "" || str != $"{specs.materialID}") {
				return false;
			}
		}
		
		if (wasFoundInCurrentIteration(specs.materialID, iteration)) {
			deleteFromCurrentIteration(specs.materialID, iteration);
		}
		
		materialReportArray[row, index, iteration] = $"{specs.getEffectiveCharge()}||{specs.accumulatedTime}||{specs.materialID}|{specs.rubbingMaterialID}";
		return true;
	}
	#endregion

	#region Report Control
	private void deleteFromCurrentIteration(int id, int iteration) {
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 2; j++) {
				if (materialReportArray[i, j, iteration] != null && materialReportArray[i, j, iteration] != string.Empty && materialReportArray[i, j, iteration].Split("|")[4] == $"{id}") {
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
					if(materialReportArray[i, j, iteration].Split("|")[4] == $"{id}") x = true;
				}
			}
		}

		return x;
	}
	#endregion
}