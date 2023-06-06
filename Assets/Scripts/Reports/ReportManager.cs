using System;
using System.Collections.Generic;
using Objects;
using SceneManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reports {
    public class ReportManager : MonoBehaviour {
        public bool dualityConstraint = true; //Couple rubbed materials in scene
        public bool chargeObsConstraint = false; // Match visual charges with objects
        public bool secondObsConstraint = false; // Match visual charges for different rubbing times
        public List<int> snapList = new();

        public int snapCount;
        public int matchedSnapCount;
        public int currentPage = 0;

        public bool page0Done;
        public bool page1Done;
        public bool page2Done;

        private int valueCount;

        #region PageControl
        public void NextPage() {
            if (currentPage == 2) return;
            currentPage++;
            DeleteElectricChildren();
            InstantiateElectricChildren(currentPage);
            transform.GetChild(3).GetChild(2).GetComponent<TMP_Text>().text = $"Sürtme süresi:\n{3 + 2 * currentPage} saniye";
        }

        public void PreviousPage() {
            if (currentPage == 0) return;
            currentPage--;
            DeleteElectricChildren();
            InstantiateElectricChildren(currentPage);
            transform.GetChild(3).GetChild(2).GetComponent<TMP_Text>().text = $"Sürtme süresi:\n{3 + 2 * currentPage} saniye";
        }

        private void OnEnable() {
            DeleteElectricChildren();
            InstantiateElectricChildren(currentPage);
            transform.GetChild(3).GetChild(2).GetComponent<TMP_Text>().text = $"Sürtme süresi:\n{3 + 2 * currentPage} saniye";
        }
        #endregion

        private bool callMouseUp;

        public bool OnSnap(ElectricSpecs specs, GameObject parent, GameObject grandparent) {
            int col = int.Parse(parent.name.Split(".")[1]) - 1;
            int row = int.Parse(grandparent.name.Split(".")[1]) - 1;

            if (dualityConstraint) { //Disables onSnap behavior for non-duality activities
                if (specs.isActiveObject) {
                    if (specs.conjugateItem == null) {
                        GeneralGuidance.Instance.alert.alert("Hata!","Görevin tamamlanması için bu objeyi bir başka objeyle sürtmen gerek.","Tamam");
                        // Print the parameters of specs.conjugateItem 
                        return false;
                    }
                }
                
                if (specs.accumulatedTime < 0.1f) {
                    print($"Error with {specs.gameObject.name}");
                    print($"Interaction with {specs.conjugateItem}, {specs.accumulatedTime} {specs.getEffectiveCharge()}");
                    GeneralGuidance.Instance.alert.alert("Hata!", "Görevin tamamlanması için objeleri birbirlerine sürtüp rapora sürüklemelisin.", "Tamam");
                    return false;
                }

                if (!GeneralGuidance.Instance.AddSpecsToReport(specs, row, col)) {
                    GeneralGuidance.Instance.alert.alert("Hata!", "Obje eşleştirmesi hatalı, rapordaki objeyi bu objeye sürtmemişsin.", "Tamam");
                    return false;
                }

                snapCount++;

                if (snapCount == 1) {
                    var x = GeneralGuidance.GetSceneGameObjectByName("LightAndCharge", 0, true);
                    if (x != null) {
                        if (snapCount is 2) {
                            x.GetComponent<LightAndChargeGuidance>().NextDialogue();
                        }
                        if (snapCount is 4) {
                            x.GetComponent<LightAndChargeGuidance>().NextDialogue();
                        }
                    }
                }
                
                if (GeneralGuidance.Instance.HasMatchingConjugate(specs, row, col)) {
                    matchedSnapCount++;
                    var x = GeneralGuidance.GetSceneGameObjectByName("LightAndCharge");
                    if (x != null) {
                        if (matchedSnapCount is 1 or 2 or 3) {
                            x.GetComponent<LightAndChargeGuidance>().NextDialogue();
                        }
                    }
                }
                
                snapList.Add(specs.materialID);
                specs.snapped = true;
                GeneralGuidance.Instance.audio.PlayOneShot(snapClip);
                //GeneralGuidance.Instance.report.SmartInstantiateElectricChild(specs.gameObject.GetComponent<Draggable>().FakeParent);
                return true;
            }
            
            if (chargeObsConstraint) {
                
            }
            return false;
        }

        public AudioClip snapClip;

        public bool OnLeaveReport(int materialID) {
            if (dualityConstraint) {
                if (snapList.Contains(materialID)) {
                    snapCount--;
                    snapList.Remove(materialID);
                    return true;
                }
            }
            
            // else {
            //     print("Reset children");
            //     // DeleteElectricChildren();
            //     // InstantiateElectricChildren(currentPage);
            //     // int col = int.Parse(parent.name.Split(".")[1]) - 1;
            //     // int row = int.Parse(parent.transform.parent.name.Split(".")[1]) - 1;
            //     // var obj = InstantiateElectricChild(row, col, currentPage);
            //     return true;
            // }

            return false;
        }

        public bool OnChargeUpdate() { //TODO: Couple specs charges with report entry
            if (chargeObsConstraint){}
            return false;
        }

        public bool OnSecondsUpdate() { //TODO: Couple specs time with report entry
            if (secondObsConstraint){}
            return false;
        }

        public Sprite NeutralChargeField;
        public Sprite PositiveChargeField;
        public Sprite NegativeChargeField;

        public void ConvertToChargeAmounts() {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 2; j++) {
                    var tr = transform.GetChild(0).GetChild(i).GetChild(j);
                    tr.GetComponent<Image>().sprite = NeutralChargeField;
                    tr.GetChild(0).gameObject.SetActive(true);
                }
            }
        }

        public int GetUserCharge(int row, int column) {
            var tr = transform.GetChild(0).GetChild(row).GetChild(column);
            var txt = tr.GetChild(0).GetComponent<TMP_InputField>().text;
            txt = txt.Trim('+');
            txt = txt.Replace(" ", "");
            if (txt != "") {
                return int.Parse(txt);
            }
            return 0;
        }

        public void UpdateAllMaterialFieldBackgrounds() {
            valueCount = 0;
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 2; j++) {
                    SetMaterialFieldBackground(i, j);
                }
            }
            if (valueCount == 2 && currentPage == 0) {
                GeneralGuidance.Instance.skipDialogueChargeS2 = true;
            }
            if (valueCount == 6) {
                if (currentPage == 0 && !page0Done) {
                    GeneralGuidance.Instance.skipDialogueChargeS2 = true;
                    print("Page 0 done");
                    page0Done = true;
                }
                if (currentPage == 1 && !page1Done) {
                    GeneralGuidance.Instance.skipDialogueChargeS2 = true;
                    print("Page 1 done");
                    page1Done = true;
                }
                if (currentPage == 2 && !page2Done) {
                    GeneralGuidance.Instance.skipDialogueChargeS2 = true;
                    print("Page 2 done");
                    page2Done = true;
                }
            }
        }

        public void SetMaterialFieldBackground(int row, int column) {
            var tr = transform.GetChild(0).GetChild(row).GetChild(column);
            var userCharge = GetUserCharge(row, column);
            if (userCharge != 0) valueCount++;
            tr.GetComponent<Image>().sprite = (userCharge) switch {
                > 0 => PositiveChargeField,
                0 => NeutralChargeField,
                < 0 => NegativeChargeField
            };
            if (userCharge != 0) {
                var arr = GeneralGuidance.Instance.materialReportArray[row, column, currentPage].Split("|");
                arr[1] = userCharge.ToString();
                GeneralGuidance.Instance.materialReportArray[row, column, currentPage] = string.Join("|", arr);
            }
        }

        public void DeleteElectricChildren() {
            foreach (Transform tr in transform) {
                if (tr.TryGetComponent(out ElectricSpecs specs)) {
                    Destroy(specs.gameObject);
                }
            }
        }

        public void InstantiateElectricChildren(int iteration = 0) {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 2; j++) {
                    if (GeneralGuidance.Instance.materialReportArray == null) continue;
                    CommonObjectSpawner(i, j, iteration);
                }
            }
        }
        
        public void InstantiateElectricChild(int row, int column, int iteration = 0) {
            if (GeneralGuidance.Instance.materialReportArray == null) return;
            CommonObjectSpawner(row, column, iteration);
        }
        
        public void SmartInstantiateElectricChild(GameObject parent) {
            if (GeneralGuidance.Instance.materialReportArray == null) return;
            int col = int.Parse(parent.name.Split(".")[1]) - 1;
            int row = int.Parse(parent.transform.parent.name.Split(".")[1]) - 1;
            CommonObjectSpawner(row, col, currentPage);
        }

        private void CommonObjectSpawner(int i, int j, int iteration = 0) {
            var virtual_obj = GeneralGuidance.Instance.materialReportArray[i, j, iteration];
            if (string.IsNullOrEmpty(virtual_obj)) {
                print($"Virtual object empty for {i}, {j}, {iteration}");
                return;
            }
            var x = virtual_obj.Split("|");
            var id = x[4];
            if (string.IsNullOrEmpty(id)) {
                print($"Material ID empty for {i}, {j}, {iteration}");
                return;
            }
            if (!GeneralGuidance.Instance.report.dualityConstraint) {
                var tr = transform.GetChild(0).GetChild(i).GetChild(j);
                tr.GetChild(0).GetComponent<TMP_InputField>().text = x[1];
                SetMaterialFieldBackground(i, j);
            }
            var subParent = transform.GetChild(0).GetChild(i).GetChild(j);
            var obj = Instantiate(GeneralGuidance.Instance.MaterialPrefabList[int.Parse(id)], GeneralGuidance.Instance.report.gameObject.transform, true);
            obj.transform.position = subParent.GetComponent<BoxCollider2D>().bounds.center;
            obj.transform.localScale = Vector3.one;
            var drg = obj.GetComponent<Draggable>();
            drg.SetInterceptColliderSize(true);
            drg.FakeParent = subParent.gameObject;
            var spc = obj.GetComponent<ElectricSpecs>();
            spc.canCharge = true;
            spc.canRub = true;
            spc.canContact = true;
        }
    }
}
