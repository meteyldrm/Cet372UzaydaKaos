using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
// ReSharper disable StringLiteralTypo

namespace SceneManagers {
    public class IntroGuidance : MonoBehaviour {
        [FormerlySerializedAs("BottomSpeech")] public GameObject bottomSpeech;
        private TextMeshProUGUI _bottomSpeechTMP;

        [FormerlySerializedAs("APA")] public GameObject apa;
        [FormerlySerializedAs("APASpeech")] public GameObject apaSpeech;
        private TextMeshProUGUI _apaSpeechTMP;

        [FormerlySerializedAs("NameTag")] public GameObject nameTag;
        [FormerlySerializedAs("NameField1")] public GameObject nameField1;
        [FormerlySerializedAs("NameField2")] public GameObject nameField2;
        [FormerlySerializedAs("ShipBG")] public GameObject shipBg;
        [FormerlySerializedAs("AirlockBG")] public GameObject airlockBg;
        
        // Start is called before the first frame update
        private void Start() {
            _bottomSpeechTMP = bottomSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _apaSpeechTMP = apaSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            shipBg.SetActive(true);
            airlockBg.SetActive(false);
            NextDialogue();
        }
        
        private int _dialogueIndex = -1;

        private readonly List<string> _dialogues = new() {
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
            if (_dialogueIndex <= 0) return;
            _dialogueIndex--;
            DisplayDialogue(_dialogues[_dialogueIndex]);
        }
        public void NextDialogue() {
            if (_dialogueIndex < _dialogues.Count - 1) {
                _dialogueIndex++;
                DisplayDialogue(_dialogues[_dialogueIndex]);
            }

            // ReSharper disable once InvertIf
            if (_dialogueIndex == _dialogues.Count - 1) {
                shipBg.SetActive(false);
                airlockBg.SetActive(false);
                nameTag.SetActive(false);
                apaSpeech.SetActive(false);
                bottomSpeech.SetActive(false);
                GeneralGuidance.Instance.LoadNextScenario();
            }
        }

        private void DisplayDialogue(string dialogue) {
            if (dialogue.StartsWith("ACTION ")) {
                dialogue = dialogue.Replace("ACTION ", "");

                switch (dialogue) {
                    case "Name": {
                        nameTag.SetActive(true);
                        apaSpeech.SetActive(false);
                        bottomSpeech.SetActive(false);
                        break;
                    }
                }
                
            } else if (dialogue.StartsWith("PLAYER ") || dialogue.StartsWith("P_")) {
                dialogue = dialogue.Replace("PLAYER ", "").Replace("P_", "");

                if (dialogue.StartsWith("Uzay gemisine yaklaşıyorum")) {
                    //Old bg
                    shipBg.SetActive(true);
                    airlockBg.SetActive(false);
                }

                if (dialogue.StartsWith("Hava kilidinde")) {
                    //New bg
                    shipBg.SetActive(false);
                    airlockBg.SetActive(true);
                }

                if (dialogue.StartsWith("Merhaba! Benim adım")) {
                    nameTag.SetActive(false);
                }
                
                apaSpeech.SetActive(false);
                bottomSpeech.SetActive(true);
                _bottomSpeechTMP.text = Interpolator(dialogue);
            } else if (dialogue.StartsWith("APA ") || dialogue.StartsWith("A_")) {
                dialogue = dialogue.Replace("APA ", "").Replace("A_", "");
                
                if (dialogue.StartsWith("Merhaba! Beklettiğim")) {
                    //Show APA
                    apa.SetActive(true);
                }
                apaSpeech.SetActive(true);
                bottomSpeech.SetActive(false);
                _apaSpeechTMP.text = Interpolator(dialogue);
            }
        }

        private static string Interpolator(string dialogue) {
            dialogue = dialogue.Replace("$NAME$", GeneralGuidance.Instance.playerName.Split(" ")[0]);
            dialogue = dialogue.Replace("$FULL_NAME$", GeneralGuidance.Instance.playerName);
            return dialogue;
        }

        public void CheckName() {
            var c1 = nameField1.GetComponent<TMP_InputField>().text;
            var c2 = nameField2.GetComponent<TMP_InputField>().text;
            if (c1.Length <= 0 || c2.Length <= 0) return;
            GeneralGuidance.Instance.playerName = $"{c1} {c2}".Trim(' ');
            NextDialogue();
        }
    }
}
