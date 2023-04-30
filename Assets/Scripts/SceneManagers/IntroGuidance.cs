using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SceneManagers {
    public class IntroGuidance : MonoBehaviour {
        public GameObject BottomSpeech;
        private TextMeshProUGUI BottomSpeechTMP;

        public GameObject APA;
        public GameObject APASpeech;
        private TextMeshProUGUI APASpeechTMP;

        public GameObject NameTag;
        public GameObject NameField1;
        public GameObject NameField2;
        public GameObject ShipBG;
        public GameObject AirlockBG;
        
        // Start is called before the first frame update
        void Start() {
            BottomSpeechTMP = BottomSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            APASpeechTMP = APASpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            ShipBG.SetActive(true);
            AirlockBG.SetActive(false);
            NextDialogue();
        }
        
        private int dialogueIndex = -1;

        private readonly List<string> dialogues = new() {
            "P_İşimin yorgunluğunu geride bırakmak için çok sabırsızım. Her şeyden uzakta tatil yapabilmenin mümkün olduğu kimin aklına gelirdi? Tek başıma uzaya çıktım!",
            "P_İşimin başında olmadan, rahatlayabileceğim bir tatil yapacağım. Hiç işimi düşünmem gerekmeyecek.",
            "P_Uzay gemisine yaklaşıyorum fakat ışıkları yanmıyor, bu çok tuhaf. Yine de kenetlenip gemiye binmeliyim, belki de herkes uyuyordur. Uzayda sürüklenerek kalmak istemem.",
            "P_Hava kilidinde de kimse beni karşılamaya gelmedi. Ne yapmalı acaba? Gemide biri varsa beni çoktan görmüş olmalı.",
            "A_Merhaba! Beklettiğim için üzgünüm.\n\nBen Mehmet Işık, geminin mühendisiyim. Çoğu kişi gemiyi terk ettiği için kimsenin gelmesini beklemiyordum. Kimliğini görebilir miyim?",
            "ACTION Name",
            "P_Merhaba! Benim adım $FULL_NAME$, tatil için buraya rezervasyon yapmıştım ama pek iyi bir zamanda varmadım galiba.",
            "A_Gemide beklenmedik problemler yaşadığımız için mürettebat ayrılmayı tercih etti, teknisyenlerin buraya gelmesi bir haftadan uzun sürecek.\n\nDünya ile iletişimimizin kopması çok kötü oldu.",
            "P_Ben de dünyada mühendis olarak çalışıyordum, belki problem çözmekte yardımcı olabilirim. İletişim ekipmanı da devredışı kaldığına göre sanırım bir süre boyunca alacağın tek yardım benden gelecek.",
            "A_Öyle görünüyor.\n\nGemide fazla vaktimizin olmadığını biliyorum, ama belki senin yardımınla problemleri çözüp gemiyi kurtarabiliriz. Her şey sana bağlı, $NAME$!",
            "P_Ne tatil ama. Elimden geleni yapacağım! Haydi başlayalım."
        };

        public void PrevDialogue() {
            if (dialogueIndex > 0) {
                dialogueIndex--;
                DisplayDialogue(dialogues[dialogueIndex]);
            }
        }
        public void NextDialogue() {
            if (dialogueIndex < dialogues.Count - 1) {
                dialogueIndex++;
                DisplayDialogue(dialogues[dialogueIndex]);
            }

            if (dialogueIndex == dialogues.Count - 1) {
                ShipBG.SetActive(false);
                AirlockBG.SetActive(false);
                NameTag.SetActive(false);
                APASpeech.SetActive(false);
                BottomSpeech.SetActive(false);
                GeneralGuidance.Instance.LoadNextScenario();
            }
        }

        private void DisplayDialogue(string dialogue) {
            if (dialogue.StartsWith("ACTION ")) {
                dialogue = dialogue.Replace("ACTION ", "");

                switch (dialogue) {
                    case "Name": {
                        NameTag.SetActive(true);
                        APASpeech.SetActive(false);
                        BottomSpeech.SetActive(false);
                        break;
                    }
                }
                
            } else if (dialogue.StartsWith("PLAYER ") || dialogue.StartsWith("P_")) {
                dialogue = dialogue.Replace("PLAYER ", "").Replace("P_", "");

                if (dialogue.StartsWith("Uzay gemisine yaklaşıyorum")) {
                    //Old bg
                    ShipBG.SetActive(true);
                    AirlockBG.SetActive(false);
                }

                if (dialogue.StartsWith("Hava kilidinde")) {
                    //New bg
                    ShipBG.SetActive(false);
                    AirlockBG.SetActive(true);
                }

                if (dialogue.StartsWith("Merhaba! Benim adım")) {
                    NameTag.SetActive(false);
                }
                
                APASpeech.SetActive(false);
                BottomSpeech.SetActive(true);
                BottomSpeechTMP.text = Interpolator(dialogue);
            } else if (dialogue.StartsWith("APA ") || dialogue.StartsWith("A_")) {
                dialogue = dialogue.Replace("APA ", "").Replace("A_", "");
                
                if (dialogue.StartsWith("Merhaba! Beklettiğim")) {
                    //Show APA
                    APA.SetActive(true);
                }
                APASpeech.SetActive(true);
                BottomSpeech.SetActive(false);
                APASpeechTMP.text = Interpolator(dialogue);
            }
        }

        private string Interpolator(string dialogue) {
            dialogue = dialogue.Replace("$NAME$", GeneralGuidance.Instance.playerName.Split(" ")[0]);
            dialogue = dialogue.Replace("$FULL_NAME$", GeneralGuidance.Instance.playerName);
            return dialogue;
        }

        public void CheckName() {
            var c1 = NameField1.GetComponent<TMP_InputField>().text;
            var c2 = NameField2.GetComponent<TMP_InputField>().text;
            if (c1.Length > 0 && c2.Length > 0) {
                GeneralGuidance.Instance.playerName = $"{c1} {c2}".Trim(' ');
                NextDialogue();
            }
        }
    }
}
