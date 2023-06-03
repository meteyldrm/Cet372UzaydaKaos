using System.Collections;
using System.Collections.Generic;
using Objects;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SceneManagers {
    public class LightAndChargeGuidance : MonoBehaviour
    {
        public GameObject BottomSpeech;
        private TextMeshProUGUI BottomSpeechTMP;
        public GameObject TopSpeech;
        private TextMeshProUGUI TopSpeechTMP;

        public GameObject APA;
        public GameObject APASpeech;
        private TextMeshProUGUI APASpeechTMP;

        public GameObject NavigationArrows;

        public GameObject Light;
        public GameObject Report;
        private bool reportSetInactive = false;
        public GameObject Materials;
        public GameObject RubPanel;
        public GameObject ChargePanel;

        private UnityEvent evt;
        private bool removeEvent = false;
        
        // Start is called before the first frame update
        void Start() {
            BottomSpeechTMP = BottomSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            TopSpeechTMP = TopSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            APASpeechTMP = APASpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            evt = new UnityEvent();
            NextDialogue();
        }
        
        private int dialogueIndex = -1;

        private readonly List<string> dialogues = new() {
            "A_Birkaç hafta önce gemide elektriksel problemler yaşamaya başlamıştık. Dışarıdan ışıkların kapalı olduğunu görmüşsündür, problemlerin neden meydana geldiğini çözmeye çalışıyordum.",
            "A_Etrafta yürürken veya camları silerken ışıkların yanıp söndüğünü fark ettim, buna sebep olan şey sürtünme olmalı fakat hangi malzemelerin yaptığını bilmiyorum.\n\nSorun çıkarabileceğini düşündüğüm 6 tane malzeme topladım ama hata yapmış olabilirim.",
            "A_Test etmek için ufak bir ampul düzeneği de kurdum. Objeleri ampule dokundurarak belki yakmayı başarırsın.\n\nObjeleri açılan panele yerleştirip birbirlerine sürterek hangi kombinasyonların elektriksel probleme yol açtığını bulabilir misin?",
            "A_Ampul yandı! Elektriksel bir etkileşim oldu.\n\nSağ üstteki rapor butonuna basarak açılan rapora bulduklarını işlemelisin.",
            "A_Ampulü yakan objeleri aynı satırın içindeki kutucuklara sürükle.\n\nHangi objelerin problem çıkardığını bu rapor sayesinde öğreneceğiz!",
            "A_İlk kombinasyonu doğru yazdın!\n\nRaporu tamamlamamız için ampulü yakan iki kombinasyon daha bul. Aynı objeleri tekrar kullanma hakkın var.\nRapor butonuna basarak raporu açıp kapatabilirsin.",
            //"ACTION Skip",
            "A_İkinci kombinasyonu doğru yazdın!\n\nRaporu tamamlamamız için ampulü yakan bir kombinasyon daha bul. Aynı objeleri tekrar kullanma hakkın var.\nRapor butonuna basarak raporu açıp kapatabilirsin.",
            "A_Tebrikler, rapor başarılı! Tüm kombinasyonları buldun.\n\nİşimi baya kolaylaşırdın $NAME$, hızlı çalışıyorsun.",
            "A_Sen raporu hazırlarken ben de özel bir mercek üzerinde uğraşıyordum. Elektriksel etkilerin nasıl açığa çıktığını öğrenmemiz gerekiyor.\n\nBu mercek objeleri biraz büyütecek, elektriksel yoğunluğu da göstermesini umuyorum.",
            "A_Hangi objelerin birbiriyle etkileşime geçtiğini bildiğimiz için topladığım yığından kurtulabiliriz.\n\nOnların yerine merceği koyalım, objeleri bu sefer merceğe koyacaksın. Raporu açar mısın?",
            "A_Objeleri, rapordan merceğe sürükleyerek görüntüleyebilirsin.\n\nİstemediğin bir şey yaparsan veya objeleri silmek istersen, merceğin sağ altındaki çöp butonuna tıklayabilirsin. İlk satırdaki objeleri görüntüler misin?",
            "A_Objelerin üzerinde bir şeyler görüyoruz. Bakalım bunlar, objeleri sürtünce nasıl değişecek.\n\n İki objeyi 3 saniye boyunca sürter misin?",
            "A_3 saniye doldu! Objelerin elektriksel dağılımları değişti.\n\nBiz bunlara kısaca “yük” diyelim.\n\nObjelerin net yükünü hesaplayıp raporda “? Yük” yazan alana yazar mısın? Örneğin, 6 eksi ve 4 artı yükü için -2 yazman gerekli.",
            "A_Yazdığın yükler doğru görünüyor! Merceğin sağ altındaki çöp butonuyla mercekteki objeleri sil.\n\nRapordaki diğer objeleri de bu şekilde sürterek raporu tamamlayalım.",
            "A_Tüm yükleri hesapladın, tebrikler! Hangi material kombinasyonlarının daha iyi yük yüklendiğini artık biliyoruz.\n\nAraştırmamızın 2. kısmında da farklı sürelerde sürtüp sonuçları yazalım. Raporunun altındaki oklarla geçiş yapıp diğer sürelerde de aynı şekilde raporlayalım.",
        };

        public void PrevDialogue() {
            if (dialogueIndex > 0) {
                dialogueIndex--;
                DisplayDialogue(dialogues[dialogueIndex]);
            }
        }
        public void NextDialogue() {
            if (removeEvent) {
                evt.RemoveAllListeners();
            }
            
            if (dialogueIndex < dialogues.Count - 1) {
                dialogueIndex++;
                DisplayDialogue(dialogues[dialogueIndex]);
            }

            if (dialogueIndex == dialogues.Count) {
                APASpeech.SetActive(false);
                BottomSpeech.SetActive(false);
                print("Light guidance finalized");
                //GeneralGuidance.Instance.LoadNextScenario();
            }
        }

        private void DisplayDialogue(string dialogue) {
            if (dialogue.StartsWith("ACTION ")) {
                dialogue = dialogue.Replace("ACTION ", "");

                switch (dialogue) {
                    case "Skip": {
                        Report.SetActive(false);
                        Materials.SetActive(false);
                        dialogueIndex = 7;
                        NextDialogue();
                        break;
                    }
                    case "Particles": {
                        foreach (Transform tr in Materials.transform) {
                            tr.gameObject.GetComponent<ElectricSpecs>().OnShowVisualParticles();
                        }

                        break;
                    }
                }
                
            } else if (dialogue.StartsWith("PLAYER ") || dialogue.StartsWith("P_")) {
                dialogue = dialogue.Replace("PLAYER ", "").Replace("P_", "");

                if (dialogue.StartsWith("Merhaba! Benim adım")) {
                }
                
                APASpeech.SetActive(false);
                BottomSpeech.SetActive(true);
                BottomSpeechTMP.text = Interpolator(dialogue);
            } else if (dialogue.StartsWith("APA ") || dialogue.StartsWith("A_")) {
                dialogue = dialogue.Replace("APA ", "").Replace("A_", "");
                
                if (dialogue.StartsWith("Etrafta yürürken")) {
                    //Display draggables
                    Materials.SetActive(true);
                }

                if (dialogue.StartsWith("Test etmek için")) {
                    //Hide APA, display lightbulb
                    APA.SetActive(false);
                    Light.SetActive(true);
                    NavigationArrows.SetActive(false);
                    GeneralGuidance.Instance.allowDrag = true;
                    RubPanel.SetActive(true);
                }

                if (dialogue.StartsWith("Ampul yandı! ")) {
                    var index = GeneralGuidance.Instance.navbar.AddButton();
                    GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                    GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleReport);
                    evt.AddListener(NextDialogue);
                    removeEvent = true;
                }

                if (dialogue.StartsWith("İlk kombinasyonu doğru yazdın!")) {
                    // reportSetInactive = true;
                    APA.SetActive(false);
                    // NavigationArrows.SetActive(true);
                }

                if (dialogue.StartsWith("Tebrikler, rapor başarılı!")) {
                    NavigationArrows.SetActive(true);
                    Report.SetActive(false);
                    for (int i = 0; i < 3; i++) {
                        for (int j = 0; j < 2; j++) {
                            GeneralGuidance.Instance.materialReportArray[i, j, 1] = GeneralGuidance.Instance.materialReportArray[i, j, 0];
                            GeneralGuidance.Instance.materialReportArray[i, j, 2] = GeneralGuidance.Instance.materialReportArray[i, j, 0];
                        }
                    }
                }

                if (dialogue.StartsWith("Sen raporu hazırlarken")) {
                    APA.SetActive(true);
                    Light.SetActive(false);
                    RubPanel.SetActive(false);
                }
                
                if (dialogue.StartsWith("Hangi objelerin birbiriyle")) {
                    NavigationArrows.SetActive(false);
                    APA.SetActive(false);
                    ChargePanel.SetActive(true);
                    GeneralGuidance.Instance.rubbingMachine = ChargePanel.GetComponent<RubbingMachineManager>();
                    GeneralGuidance.Instance.rubbingMachine.slot1 = null;
                    GeneralGuidance.Instance.rubbingMachine.slot2 = null;
                    DeleteMaterials();
                    Materials.SetActive(true);
                    GeneralGuidance.Instance.report.ConvertToChargeAmounts();
                    GeneralGuidance.Instance.report.dualityConstraint = false;
                    GeneralGuidance.Instance.report.chargeObsConstraint = true;
                    for (int i = 0; i < Report.transform.childCount; i++) {
                        if (Report.transform.GetChild(i).TryGetComponent(out Draggable draggable)) {
                            draggable.canDrag = true;
                        }
                    }
                    evt.AddListener(NextDialogue);
                    removeEvent = true;
                }

                if (APA.activeInHierarchy) {
                    APASpeech.SetActive(true);
                    TopSpeech.SetActive(false);
                    BottomSpeech.SetActive(false);
                    APASpeechTMP.text = Interpolator(dialogue);
                } else {
                    APASpeech.SetActive(false);
                    TopSpeech.SetActive(true);
                    BottomSpeech.SetActive(false);
                    TopSpeechTMP.text = Interpolator(dialogue);
                }
            }
        }

        private void LateUpdate() {
            if (reportSetInactive) {
                Report.SetActive(false);
                reportSetInactive = false;
            }

            if (GeneralGuidance.Instance.skipDialogueChargeS2) {
                GeneralGuidance.Instance.skipDialogueChargeS2 = false;
                NextDialogue();
            }
        }

        private void ToggleReport() {
            if(!Report.activeSelf) APA.SetActive(Report.activeSelf);
            Report.SetActive(!Report.activeSelf);
            evt.Invoke();
        }

        private string Interpolator(string dialogue) {
            dialogue = dialogue.Replace("$NAME$", GeneralGuidance.Instance.playerName.Split(" ")[0].Trim(' '));
            dialogue = dialogue.Replace("$FULL_NAME$", GeneralGuidance.Instance.playerName);
            return dialogue;
        }

        private void DeleteMaterials() {
            foreach (Transform tr in Materials.transform) {
                Destroy(tr.gameObject);
            }
        }
    }
}
