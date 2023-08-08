using System.Collections.Generic;
using Objects;
using Reports;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;
// ReSharper disable RedundantDefaultMemberInitializer

/// <summary>
/// Every script has a reference to here. Do NOT try to replicate an event architecture. A more flexible, in-situ object manufacturing system can be implemented.
/// </summary>
public class GeneralGuidance : Singleton<GeneralGuidance> {
	#region Utility
	private static readonly List<string> Scenario = new() {
		"Start",
		"Intro",
		"LightAndCharge",
		"DoorUnlock",
		"Indoors",
		"Reactor",
		"Turbine"
	};

	public int activityIndex = -1;
	private int _scenarioIndex = -1;
	public ReportManager report;
	public EngReportManager engReport;
	public RubbingMachineManager rubbingMachine;
	public NavbarManager navbar;
	public AlertController alert;
	public GameObject helpPanel;
	public AudioSource audioSource;
	public void ToggleHelpPanel() {
		helpPanel.SetActive(!helpPanel.activeSelf);
	}

	public bool notifyOnSnap = true;
	
	public bool skipDialogueChargeS2 = false;
	public bool skipDialogueEngReport = false;
	public bool skipDialogueRoomNeutral = false;
	public bool dialogueRoomForceNegative = false;
	public bool dialogueRoomForcePositive = false;

	public bool allowDrag = false;

	public bool skipActivity = false;
	public void SkipActivity() {
		skipActivity = true;
	}

	public void LoadNextScenario() {
		_scenarioIndex++;
		if (_scenarioIndex <= Scenario.Count) {
			ScenarioStartup(Scenario[_scenarioIndex]);
		}
	}

	// ReSharper disable once MemberCanBeMadeStatic.Local
	private void ScenarioStartup(string objectName) {
		var scenarioObject = GetSceneGameObjectByName(objectName, 1);
		if (scenarioObject == null) {
			Debug.LogError($"Scenario {objectName} unable to be loaded.");
			return;
		}
		
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
			// ReSharper disable once InvertIf
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

	// ReSharper disable once MemberCanBePrivate.Global
	public static List<GameObject> GetAllSceneGameObjectsByName(string name, int depthLimit = 0, bool requireActive = false) {
		var list = new List<GameObject>();

		// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
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
	
	// ReSharper disable once MemberCanBePrivate.Global
	public static List<GameObject> GetAllSceneGameObjects(int depthLimit = 0, bool requireActive = false) {
		var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
		var all = new List<GameObject>();
		foreach (var rootObject in rootObjects) {
			all.AddRange(GetChildGameObjects(rootObject, 0, depthLimit));
		}

		return all;

		IEnumerable<GameObject> GetChildGameObjects(GameObject obj, int currentDepth, int dl) {
			var objList = new List<GameObject>();

			// ReSharper disable once InvertIf
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
	public string[,,] MaterialReportArray = new string[3,2,3];

	[SerializeField] public List<GameObject> materialPrefabList = new();
	[SerializeField] public GameObject positiveParticlePrefab;
	[SerializeField] public GameObject negativeParticlePrefab;
	#endregion

	private void Start() {
		#region Testing
		//Report initializer for testing prefab generation across scenes.
		if (materialReportInitializationList.Count > 0) {
			foreach (var i in materialReportInitializationList) {
				var x = i.Split(" ; ");
				var y = x[0].Split(",");
				MaterialReportArray[int.Parse(y[0]), int.Parse(y[1]), int.Parse(y[2])] = x[1];
			}
		}
		#endregion

		audioSource = GetComponent<AudioSource>();

		//Skip chapter
		_scenarioIndex = -1;
		LoadNextScenario();
	}

	private void LateUpdate() {
		// ReSharper disable once InvertIf
		if (skipActivity) {
			skipActivity = false;
			switch (_scenarioIndex)
			{
				case > 1:
					playerName = "Admin Test";
					return;
				case 1:
					playerName = "Admin Test";
					break;
			}

			LoadNextScenario();
		}
	}

	private void Update() {
		if (Input.GetKeyUp(KeyCode.LeftControl)) {
			ToggleAdminPanel();
		}
	}

	public GameObject adminPanel;
	public void ToggleAdminPanel() {
		adminPanel.SetActive(!adminPanel.activeSelf);
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
		// ReSharper disable once InvertIf
		if (MaterialReportArray[row, 0 == index ? 1 : 0, iteration] != null && MaterialReportArray[row, 0 == index ? 1 : 0, iteration] != string.Empty) {
			var str = MaterialReportArray[row, 0 == index ? 1 : 0, iteration].Split("|")[5];
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
		if (MaterialReportArray[row, 0 == index ? 1 : 0, iteration] != null && MaterialReportArray[row, 0 == index ? 1 : 0, iteration] != string.Empty) {
			var str = MaterialReportArray[row, 0 == index ? 1 : 0, iteration].Split("|")[5];
			if (str == "" || str != $"{specs.materialID}") {
				return false;
			}
		}
		
		// if (wasFoundInCurrentIteration(specs.materialID, iteration) && !report.snapList.Contains(specs.materialID)) {
		// 	print("Iteration deleted");
		// 	deleteFromCurrentIteration(specs.materialID, iteration);
		// }
		
		MaterialReportArray[row, index, iteration] = $"{specs.GetEffectiveCharge()}||{specs.accumulatedTime}||{specs.materialID}|{specs.rubbingMaterialID}";
		return true;
	}
	#endregion

	#region Report Control
	private void DeleteFromCurrentIteration(int id, int iteration) {
		for (var i = 0; i < 3; i++) {
			for (var j = 0; j < 2; j++) {
				if (MaterialReportArray[i, j, iteration] != null && MaterialReportArray[i, j, iteration] != string.Empty && MaterialReportArray[i, j, iteration].Split("|")[4] == $"{id}") {
					MaterialReportArray[i, j, iteration] = string.Empty;
				}
			}
		}
	}

	private bool WasFoundInCurrentIteration(int id, int iteration) {
		var x = false;
		for (var i = 0; i < 3; i++) {
			for (var j = 0; j < 2; j++) {
				// ReSharper disable once InvertIf
				if (MaterialReportArray[i, j, iteration] != null && MaterialReportArray[i, j, iteration] != string.Empty) {
					if(MaterialReportArray[i, j, iteration].Split("|")[4] == $"{id}") x = true;
				}
			}
		}

		return x;
	}
	#endregion
}