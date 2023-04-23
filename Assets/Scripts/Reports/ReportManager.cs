using Objects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reports {
    public class ReportManager : MonoBehaviour {
        public bool dualityConstraint = true; //Couple rubbed materials in scene
        public bool chargeObsConstraint = false; // Match visual charges with objects
        public bool secondObsConstraint = false; // Match visual charges for different rubbing times
        
        public bool OnSnap(ElectricSpecs specs, string parent, string grandparent) {
            int col = int.Parse(parent.Split(".")[1]) - 1;
            int row = int.Parse(grandparent.Split(".")[1]) - 1;

            if (dualityConstraint) { //Disables onSnap behavior for non-duality activities
                if (specs.isActiveObject) {
                    if (specs.conjugateItem == null) {
                        GeneralGuidance.Instance.alert.alert("Hata!","Görevin tamamlanması için bu objeyi bir başka objeyle sürtmen gerek.","Tamam");
                        // Print the parameters of specs.conjugateItem 
                        return false;
                    }
                }
                
                if (specs.accumulatedTime < 0.1f) {
                    GeneralGuidance.Instance.alert.alert("Hata!", "Görevin tamamlanması için objeleri birbirlerine sürtüp rapora sürüklemelisin.", "Tamam");
                    return false;
                }

                if (!GeneralGuidance.Instance.AddSpecsToReport(specs, row, col)) {
                    GeneralGuidance.Instance.alert.alert("Hata!", "Obje eşleştirmesi hatalı, rapordaki objeyi bu objeye sürtmemişsin.", "Tamam");
                    return false;
                }
                return true;
            }

            Debug.LogError("ReportManager Duality | This should never trigger");
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
    }
}
