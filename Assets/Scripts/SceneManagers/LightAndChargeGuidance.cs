using System.Collections.Generic;
using Objects;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;

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
        public GameObject EngReport;
        public GameObject Materials;
        public GameObject RubPanel;
        public GameObject ChargePanel;
        public GameObject GuidanceBackground;
        
        public Sprite RoomExterior;
        public Sprite RoomInterior;
        public Sprite ReactorExterior;
        public Sprite ReactorInterior;
        public Sprite TurbineInterior;

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
            "A_Test etmek için ufak bir ampul düzeneği de kurdum. Objeleri ampule dokundurarak belki yakmayı başarırsın.\n\nObjeleri açılan panele yerleştirip birbirlerine sürterek hangi kombinasyonların elektriksel probleme yol açtığını bulabilir misin? Emin olmak için 2-3 kere sürt.",
            "A_Ampul yandı! Elektriksel bir etkileşim oldu.\n\nSağ üstteki rapor butonuna basarak açılan rapora bulduklarını işlemelisin.",
            "A_Ampulü yakan objeleri aynı satırın içindeki kutucuklara sürükle.\n\nHangi objelerin problem çıkardığını bu rapor sayesinde öğreneceğiz!",
            "A_İlk kombinasyonu doğru yazdın!\n\nRaporu tamamlamamız için ampulü yakan iki kombinasyon daha bul. Aynı objeleri tekrar kullanma hakkın var.\nRapor butonuna basarak raporu açıp kapatabilirsin.",
            "A_İkinci kombinasyonu doğru yazdın!\n\nRaporu tamamlamamız için ampulü yakan bir kombinasyon daha bul. Aynı objeleri tekrar kullanma hakkın var.\nRapor butonuna basarak raporu açıp kapatabilirsin.",
            "A_Tebrikler, rapor başarılı! Tüm kombinasyonları buldun.\n\nİşimi baya kolaylaşırdın $NAME$, hızlı çalışıyorsun.",
            //Activity 2
            "A_Sen raporu hazırlarken ben de özel bir mercek üzerinde uğraşıyordum. Elektriksel etkilerin nasıl açığa çıktığını öğrenmemiz gerekiyor.\n\nBu mercek objeleri biraz büyütecek, elektriksel yoğunluğu da göstermesini umuyorum.",
            "A_Hangi objelerin birbiriyle etkileşime geçtiğini bildiğimiz için topladığım yığından kurtulabiliriz.\n\nOnların yerine merceği koyalım, objeleri bu sefer merceğe koyacaksın. Raporu açar mısın?",
            "A_Objeleri, rapordan merceğe sürükleyerek görüntüleyebilirsin.\n\nİstemediğin bir şey yaparsan veya objeleri silmek istersen, merceğin sağ altındaki çöp butonuna tıklayabilirsin. İlk satırdaki objeleri görüntüler misin?",
            "A_Objelerin üzerinde bir şeyler görüyoruz. Bakalım bunlar, objeleri sürtünce nasıl değişecek.\n\n İki objeyi 3 saniye boyunca sürter misin?",
            "A_3 saniye doldu! Objelerin elektriksel dağılımları değişti.\n\nBiz bunlara kısaca “yük” diyelim.\n\nObjelerin net yükünü hesaplayıp raporda “? Yük” yazan alana yazar mısın? Örneğin, 6 eksi ve 4 artı yükü için -2 yazman gerekli.",
            "A_Yazdığın yükler doğru görünüyor! Merceğin sağ altındaki çöp butonuyla mercekteki objeleri sil.\n\nRapordaki diğer objeleri de bu şekilde sürterek raporu tamamlayalım.",
            "A_Tüm yükleri hesapladın, tebrikler! Hangi material kombinasyonlarının daha iyi yük yüklendiğini artık biliyoruz.\n\nAraştırmamızın 2. kısmında da farklı sürelerde sürtüp sonuçları yazalım. Raporunun altındaki oklarla geçiş yapıp diğer sürelerde de aynı şekilde raporlayalım.",
            "A_5 saniye boyunca sürttüklerini de hesapladın!\n\nSon olarak 7 saniyeye de geçip raporlayalım.",
            "A_Raporu bitirmişin, harika!\n\nYazdıkların doğru görünüyor.",
            "A_Artık etkileşime giren materyalleri tespit ettiğimize göre ileride sorunları önlememiz daha kolay olacak.\n\nÖnlemimizi alabiliriz fakat istasyonun elektrik devreleri düzgün çalışmamaya devam ediyor. Sorunları teşhis edip çözmeliyiz.",
            "A_Elektriksel arızası olan birkaç tane oda var.\n\nBu odalara girip elekrik panelinden elektrik akışını kapatmamız gerekiyor, ancak ben odalara girmeye çalıştığımda kapıları açılmamıştı. Belki sorunu beraber çözeriz. Hazır mısın?",
            "P_Evet! Öğrendiklerimizi uygulamaya koymamızın vakti geldi.",
            //Activity 3
            "A_Önem sırasına göre problemleri çözmemiz gerek ve oksijenimiz giderek azalıyor.\n\nBu yüzden öncelikle yaşam destek odasıyla ilgilenmeliyiz. Bu kapıyı bir şekilde açmamız gerek.",
            "P_Mercekle kapıya bakarsak belki problem görebiliriz.",
            "A_İyi fikir, $NAME$. Merceği biraz daha yakına koyacağım ki daha geniş bir alana bakalım.",
            "A_Kapıda biraz yük birikmiş gibi görünüyor. Güvenlik sistemi, birinin izinsiz giriş yapmaya çalıştığını düşünüp kilitlemiş olmalı.\n\nKapının fazla yükünden kurtulabilirsek kapının kilidi açılacak.",
            "A_Görünüşe göre raporundaki objelerle yük üretmemiz gerekiyor.\n\nRaporundan iki tane objeyi yere koyar mısın?",
            "A_Kapıyı fazla yükünden arındırmalısın. Objeleri sürttükten sonra kapıya dokundurabilirsin.\n\nGüvenlik taramasını “Kontrol Et” butonuna basarak çalıştırmayı unutma.",
            "A_Kapının net yükünü sıfırladın! Kapı nötr bir yüke sahip, güvenlik sistemi kapıyı açtı.\n\nArtık içeri girebiliriz.",
            //Activity 4
            "A_Burası yaşam destek odası. Oksijen üreten sistemler burada çalışıyor.\n\nElektrik paneli arkamda bulunuyor fakat panele erişimimiz tahtalarla engellenmiş, onları kaldırmamız mümkün değil. Kapağı açmamızı engelleyen tahtayı sağa doğru ittirmeyi denemeliyiz.",
            "A_Raporundan yine iki obje seçmelisin.\n\nYüklü gördüğümüz tahtayı, uzaktan ittirmeyi deneyelim. Sağa doğru itilirse kapağı açabilirim.",
            "A_Yüklü tahtaya ancak uzaktan etki edebiliriz, objeleri birbirine sürtüp nasıl etki ediyor diye bak.",
            "A_Olamaz, tahta sola kaydı! Net yüklerle alakalı bir problem olmalı.\n\nTahtayı sağa doğru ittirmezsek bulunduğu yere sıkışabilir!",
            "A_Paneli açığa çıkarmayı başardın! Kapağı da yere düştü, panelin içine erişebiliyorum.",
            "A_Panele yalıtkan köpük sıktım, artık burada elektrik sorunları yaşamayacağız.\n\nDiğer odaları da açmadan önce geminin teknik raporunu doldurmam gerekiyor. Yardımcı olur musun?",
            "A_Teknik rapora erişmen için yukarıda yeni bir buton var.\n\nTeknik raporu açar mısın?",
            "A_Yardımın için teşekkür ederim.\n\nBu geminin teknik raporu; normalde gemi üzerinde yapılan işlemlerin sonuçlarını buraya not alıyor.\n\nYaşadığımız enerji kaybından dolayı raporu tamamlayacak işlemci gücü yok.",
            "A_Raporu oluşturup kısmen analiz etmeyi başarmış, odalarda gözlem yaptıkça burayı tamamlamamız gerekecek. Boşluklardan uygun olan seçenekleri seçerek cümleleri tamamlar mısın?",
            "A_Sistem raporu onayladı, doğru doldurmuşsun.\n\nAçılmayan başka odalar varsa onları da açmalıyız.",
            "A_Geminin nükleer reaktörü bozulmuş olmalı.\n\nBunu düzeltmemiz gerek, yoksa reaktör eriyebilir!",
            //Activity 5
            "A_Nükleer reaktör bu kapının arkasında. İçeri girince fazla vaktimiz olmayacak, hızlı bir şekilde problemleri çözmeliyiz.\n\nHazır mısın?",
            "P_Evet! Reaktör erimediği sürece odanın nispeten güvenli olması lazım, değil mi?",
            "A_Erimemişse muhtemelen güvenlidir, fakat o riski almak istediğine emin misin? Reaktörden uzakta durmamız gerekiyor.\n\nHaydi, gecikmeden içeri göz atalım.",
            "P_Nasıl görünüyor?",
            "A_Reaktörün koruyucu panelleri yerinden çıkmış. Daha fazla yaklaşmadan bu panelleri nasıl yerleştirebiliriz ki?",
            "P_Tekrar mercekten bakmayı deneyebiliriz!",
            "A_Reaktörün üç yanında bu iletken panellerden var, ilki nötr görünüyor. Diğerlerinin de nötr olduğunu varsayabiliriz.\n\nNe yapalım?",
            "P_Yük yaratıp ne olduğunu görmekten başka şansımız yok sanırım.",
            "A_Pekala. Bu panelle başlayalım. Objeleri sürterek yük oluşturmayı deneyeblirsin ama bu sefer daha uzun süre sürtmen gerekebilir.",
            "A_$NAME$, bak! Paneli çekmeye başladın!\n\nÖbür objeyi yakınlaştırırsan ne oluyor?",
            "A_Panel tamamen yerine oturdu!",
            "P_İlk panel yerine oturdu! Ayrıca, iki obje de paneli çekti! Diğer panelleri çekerken daha dikkatli gözlemleyip raporlayalım.",
            "A_Tüm panelleri yerleştirdin! Artık reaktör güvenli.\n\nTeknik rapora bir göz atalım.",
            "A_Yaptığımız gözlemlere göre boşlukları doldur.\n\nBazı boşlukları tahmin edip denemen gerekebilir.",
            "A_Rapor doğru. Şimdi elektrik üretiyor muyuz diye buhar türbinine gidip bakabiliriz.",
            //Activity 6
            "A_Görünüşe göre hiç elektrik üretmiyoruz. Buhar olduğunu görüyorum ama türbinlerin dönmeye başlaması lazım. Döndürme kolunu çevirir misin?",
            "A_Sen kolu çevirdikçe elektrik üretmeye başlıyoruz!\n\nÇevirmeye devam et!",
            "A_Tam güce kavuştuk! Yardımın için teşekkür ederim $NAME$, uzay gemisini kurtardın!.. Tatil yapmaya gelmiştin, değil mi?",
        };

        public static string staticstartswith = null;
        public void SetDialogue(string startswith = "") {
            if (staticstartswith != null) {
                for (int i = 0; i < dialogues.Count; i++) {
                    var dlg = dialogues[i].TrimStart("A_");
                    dlg = dlg.TrimStart("P_");
                    if (dlg.StartsWith(staticstartswith)) {
                        dialogueIndex = i;
                        DisplayDialogue(dialogues[dialogueIndex]);
                        break;
                    }
                }

                staticstartswith = null;
                return;
            }
            for (int i = 0; i < dialogues.Count; i++) {
                var dlg = dialogues[i].TrimStart("A_");
                dlg = dlg.TrimStart("P_");
                if (dlg.StartsWith(startswith)) {
                    dialogueIndex = i;
                    DisplayDialogue(dialogues[dialogueIndex]);
                    break;
                }
            }
        }

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
            bool apaTurn = false;
            bool playerTurn = false;
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
                playerTurn = true;
            } else if (dialogue.StartsWith("APA ") || dialogue.StartsWith("A_")) {
                apaTurn = true;
            }

            dialogue = dialogue.Replace("APA ", "").Replace("A_", "");
            dialogue = dialogue.Replace("PLAYER ", "").Replace("P_", "");
                
            if (dialogue.StartsWith("Etrafta yürürken")) {
                //Display draggables
                Materials.SetActive(true);
            }

            if (dialogue.StartsWith("Test etmek için")) {
                GeneralGuidance.Instance.activityIndex = 1;
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
                GeneralGuidance.Instance.activityIndex = 2;
                APA.SetActive(true);
                Light.SetActive(false);
                RubPanel.SetActive(false);
            }

            if (dialogue.StartsWith("Objeleri, rapordan merceğe ")) {
                GeneralGuidance.Instance.notifyOnSnap = true;
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

            if (dialogue.StartsWith("Yazdığın yükler doğru görünüyor!")) {
                GeneralGuidance.Instance.notifyOnSnap = false;
            }

            if (dialogue.StartsWith("Raporu bitirmişin")) {
                Report.SetActive(false);
                APA.SetActive(true);
                RubPanel.SetActive(false);
                NavigationArrows.SetActive(true);
                GeneralGuidance.Instance.notifyOnSnap = false;
            }
            
            if (dialogue.StartsWith("Önem sırasına")) {
                GeneralGuidance.Instance.activityIndex = 3;
                GuidanceBackground.GetComponent<SpriteRenderer>().sprite = RoomExterior;
                GuidanceBackground.transform.GetChild(0).gameObject.SetActive(false);
                GuidanceBackground.transform.GetChild(1).gameObject.SetActive(true);
                shouldChangeApaVisibility = false;
            }

            if (dialogue.StartsWith("Kapıda biraz")) {
                ChargePanel.SetActive(true);
                NavigationArrows.SetActive(true);
                GuidanceBackground.transform.GetChild(1).GetComponent<ElectricSpecs>().OnShowVisualParticles();
            }

            if (dialogue.StartsWith("Kapıyı fazla yükünden")) {
                NavigationArrows.SetActive(false);
                APA.SetActive(false);
            }

            if (dialogue.StartsWith("Kapının net yükünü sıfırladın!")) {
                ChargePanel.SetActive(false);
                Report.SetActive(false);
                APA.SetActive(true);
                shouldChangeApaVisibility = true;
                DeleteMaterials();
                NavigationArrows.SetActive(true);
                GuidanceBackground.transform.GetChild(1).gameObject.SetActive(false);
            }

            if (dialogue.StartsWith("Burası yaşam destek odası.")) {
                GeneralGuidance.Instance.activityIndex = 4;
                GeneralGuidance.Instance.rubbingMachine.doOnce = true;
                GuidanceBackground.GetComponent<SpriteRenderer>().sprite = RoomInterior;
                GuidanceBackground.transform.GetChild(1).gameObject.SetActive(false);
                GuidanceBackground.transform.GetChild(2).gameObject.SetActive(true);
                GuidanceBackground.transform.GetChild(2).GetComponent<ElectricSpecs>().OnShowVisualParticles();
            }

            if (dialogue.StartsWith("Yüklü tahtaya ancak")) {
                APA.SetActive(false);
                shouldChangeApaVisibility = false;
                NavigationArrows.SetActive(false);
                ChargePanel.SetActive(true);
            }

            if (dialogue.StartsWith("Paneli açığa çıkarmayı")) {
                NavigationArrows.SetActive(true);
                APA.SetActive(true);
                Report.SetActive(false);
                DeleteMaterials();
                ChargePanel.SetActive(false);
            }
            
            if (dialogue.StartsWith("Teknik rapora erişmen için")) {
                NavigationArrows.SetActive(false);
                var index = GeneralGuidance.Instance.navbar.AddButton();
                GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleEngReport);
                evt.AddListener(NextDialogue);
                removeEvent = true;
            }

            if (dialogue.StartsWith("Yardımın için teşekkür")) {
                NavigationArrows.SetActive(true);
            }

            if (dialogue.StartsWith("Raporu oluşturup kısmen")) {
                NavigationArrows.SetActive(false);
            }

            if (dialogue.StartsWith("Sistem raporu onayladı,")) {
                NavigationArrows.SetActive(true);
            }

            if (dialogue.StartsWith("Nükleer reaktör bu kapının")) {
                GeneralGuidance.Instance.activityIndex = 5;
                GuidanceBackground.GetComponent<SpriteRenderer>().sprite = ReactorExterior;
                GuidanceBackground.transform.GetChild(1).gameObject.SetActive(false);
                GuidanceBackground.transform.GetChild(2).gameObject.SetActive(false);
            }

            if (dialogue.StartsWith("Nasıl görünüyor")) {
                GuidanceBackground.GetComponent<SpriteRenderer>().sprite = ReactorInterior;
            }
            
            if (dialogue.StartsWith("Reaktörün üç yanında")) {
                ChargePanel.SetActive(true);
            }


            if (apaTurn) {
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
            } else if (playerTurn) {
                APASpeech.SetActive(false);
                BottomSpeech.SetActive(true);
                BottomSpeechTMP.text = Interpolator(dialogue);
            }
        }

        private void Update() {
            if (GeneralGuidance.Instance.skipActivity) {
                //Ac2-7
                //Ac3-20
                //Ac3-20
                if (dialogueIndex < 7) {
                    GeneralGuidance.Instance.allowDrag = true;
                    NavigationArrows.SetActive(true);
                    if (GeneralGuidance.Instance.navbar.GetComponent<NavbarManager>().displayCount == 2) {
                        var index = GeneralGuidance.Instance.navbar.AddButton();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleReport);
                    }
                    GeneralGuidance.Instance.materialReportArray = new string[3, 2, 3];
                    GeneralGuidance.Instance.materialReportArray[0, 0, 0] = "1||3,010964||1|0";
                    GeneralGuidance.Instance.materialReportArray[0, 1, 0] = "-1||3,010964||0|1";
                    
                    GeneralGuidance.Instance.materialReportArray[1, 0, 0] = "1||3,010964||3|5";
                    GeneralGuidance.Instance.materialReportArray[1, 1, 0] = "-1||3,010964||5|3";
                    
                    GeneralGuidance.Instance.materialReportArray[2, 0, 0] = "-1||3,010964||3|1";
                    GeneralGuidance.Instance.materialReportArray[2, 1, 0] = "1||3,010964||1|3";
                    GeneralGuidance.Instance.notifyOnSnap = true;
                    dialogueIndex = 6;
                    NextDialogue();
                    return;
                }

                if (dialogueIndex < 20) {
                    Report.SetActive(false);
                    GeneralGuidance.Instance.notifyOnSnap = false;
                    
                    GeneralGuidance.Instance.materialReportArray[0, 0, 0] = "1|1|3,010964||1|0";
                    GeneralGuidance.Instance.materialReportArray[0, 1, 0] = "-1|-1|3,010964||0|1";
                    
                    GeneralGuidance.Instance.materialReportArray[1, 0, 0] = "1|1|3,010964||3|5";
                    GeneralGuidance.Instance.materialReportArray[1, 1, 0] = "-1|-1|3,010964||5|3";
                    
                    GeneralGuidance.Instance.materialReportArray[2, 0, 0] = "-1|-1|3,010964||3|1";
                    GeneralGuidance.Instance.materialReportArray[2, 1, 0] = "1|1|3,010964||1|3";
                    for (int i = 0; i < 3; i++) {
                        for (int j = 0; j < 2; j++) {
                            GeneralGuidance.Instance.materialReportArray[i, j, 1] = GeneralGuidance.Instance.materialReportArray[i, j, 0];
                            GeneralGuidance.Instance.materialReportArray[i, j, 2] = GeneralGuidance.Instance.materialReportArray[i, j, 0];
                        }
                    }
                    
                    GeneralGuidance.Instance.materialReportArray[0, 0, 1] = "2|2|5,010964||1|0";
                    GeneralGuidance.Instance.materialReportArray[0, 1, 1] = "-2|-2|5,010964||0|1";
                    GeneralGuidance.Instance.materialReportArray[0, 0, 2] = "4|4|7,010964||1|0";
                    GeneralGuidance.Instance.materialReportArray[0, 1, 2] = "-4|-4|7,010964||0|1";
                    APA.SetActive(true);
                    Light.SetActive(false);
                    RubPanel.SetActive(false);
                    NavigationArrows.SetActive(true);
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
                    ChargePanel.SetActive(false);
                    
                    dialogueIndex = 19;
                    NextDialogue();
                    return;
                }

                if (dialogueIndex < 25) {
                    dialogueIndex = 26;
                    NextDialogue();
                    return;
                }
                
                if (dialogueIndex < 36) {
                    if (GeneralGuidance.Instance.navbar.GetComponent<NavbarManager>().displayCount == 3) {
                        var index = GeneralGuidance.Instance.navbar.AddButton();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleEngReport);
                    }
                    dialogueIndex = 37;
                    NextDialogue();
                    return;
                }
            }
        }

        private void LateUpdate() {
            if (GeneralGuidance.Instance.skipDialogueChargeS2) {
                GeneralGuidance.Instance.skipDialogueChargeS2 = false;
                NextDialogue();
            }
            
            if (GeneralGuidance.Instance.skipDialogueRoomNeutral) {
                GeneralGuidance.Instance.skipDialogueRoomNeutral = false;
                NextDialogue();
            }
            
            if (GeneralGuidance.Instance.dialogueRoomForceNegative) {
                GeneralGuidance.Instance.dialogueRoomForceNegative = false;
                SetDialogue("Olamaz, tahta sola kaydı!");
            }
            
            if (GeneralGuidance.Instance.dialogueRoomForcePositive) {
                GeneralGuidance.Instance.dialogueRoomForcePositive = false;
                SetDialogue("Paneli açığa çıkarmayı başardın!");
            }

            if (GeneralGuidance.Instance.skipDialogueEngReport) {
                GeneralGuidance.Instance.skipDialogueEngReport = false;
                EngReport.SetActive(false);
                NextDialogue();
            }
        }

        private bool shouldChangeApaVisibility = true;

        private void ToggleReport() {
            if (shouldChangeApaVisibility) {
                APA.SetActive(Report.activeSelf);
            }
            Report.SetActive(!Report.activeSelf);
            
            if (APA.activeInHierarchy) {
                APASpeech.SetActive(true);
                TopSpeech.SetActive(false);
                BottomSpeech.SetActive(false);
                APASpeechTMP.text = Interpolator(dialogues[dialogueIndex]);
            } else {
                APASpeech.SetActive(false);
                TopSpeech.SetActive(true);
                BottomSpeech.SetActive(false);
                TopSpeechTMP.text = Interpolator(dialogues[dialogueIndex]);
            }
            
            evt.Invoke();
        }
        
        private void ToggleEngReport() {
            if (shouldChangeApaVisibility) {
                APA.SetActive(EngReport.activeSelf);
            }
            EngReport.SetActive(!EngReport.activeSelf);
            
            if (APA.activeInHierarchy) {
                APASpeech.SetActive(true);
                TopSpeech.SetActive(false);
                BottomSpeech.SetActive(false);
                APASpeechTMP.text = Interpolator(dialogues[dialogueIndex]);
            } else {
                APASpeech.SetActive(false);
                TopSpeech.SetActive(true);
                BottomSpeech.SetActive(false);
                TopSpeechTMP.text = Interpolator(dialogues[dialogueIndex]);
            }
            
            evt.Invoke();
        }

        private string Interpolator(string dialogue) {
            dialogue = dialogue.TrimStart("A_");
            dialogue = dialogue.TrimStart("P_");
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
