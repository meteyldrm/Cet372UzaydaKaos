using System.Collections.Generic;
using Objects;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility;
// ReSharper disable StringLiteralTypo

namespace SceneManagers {
    public class LightAndChargeGuidance : MonoBehaviour
    {
        [FormerlySerializedAs("BottomSpeech")] public GameObject bottomSpeech;
        private TextMeshProUGUI _bottomSpeechTMP;
        [FormerlySerializedAs("TopSpeech")] public GameObject topSpeech;
        private TextMeshProUGUI _topSpeechTMP;

        [FormerlySerializedAs("APA")] public GameObject apa;
        [FormerlySerializedAs("APASpeech")] public GameObject apaSpeech;
        private TextMeshProUGUI _apaSpeechTMP;

        [FormerlySerializedAs("NavigationArrows")] public GameObject navigationArrows;

        [FormerlySerializedAs("Light")] public GameObject lightBulb;
        [FormerlySerializedAs("Report")] public GameObject report;
        [FormerlySerializedAs("EngReport")] public GameObject engReport;
        [FormerlySerializedAs("Materials")] public GameObject materials;
        [FormerlySerializedAs("RubPanel")] public GameObject rubPanel;
        [FormerlySerializedAs("ChargePanel")] public GameObject chargePanel;
        [FormerlySerializedAs("GuidanceBackground")] public GameObject guidanceBackground;
        
        [FormerlySerializedAs("RoomExterior")] public Sprite roomExterior;
        [FormerlySerializedAs("RoomInterior")] public Sprite roomInterior;
        [FormerlySerializedAs("ReactorExterior")] public Sprite reactorExterior;
        [FormerlySerializedAs("ReactorInterior")] public Sprite reactorInterior;
        [FormerlySerializedAs("TurbineInterior")] public Sprite turbineInterior;

        private UnityEvent _evt;
        private bool _removeEvent = false;
        
        // Start is called before the first frame update
        private void Start() {
            _bottomSpeechTMP = bottomSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _topSpeechTMP = topSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _apaSpeechTMP = apaSpeech.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _evt = new UnityEvent();
            NextDialogue();
        }
        
        private int _dialogueIndex = -1;

        private readonly List<string> _dialogues = new() {
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
            "A_Tam güce kavuştuk! Yardımın için teşekkür ederim $NAME$, uzay gemisini kurtardın!.. Tatil yapmaya gelmiştin, değil mi?"
        };

        private static string _staticStartsWith = null;

        private void SetDialogue(string startsWith = "") {
            if (_staticStartsWith != null) {
                for (var i = 0; i < _dialogues.Count; i++) {
                    var dlg = _dialogues[i].TrimStart("A_");
                    dlg = dlg.TrimStart("P_");
                    // ReSharper disable once InvertIf
                    if (dlg.StartsWith(_staticStartsWith)) {
                        _dialogueIndex = i;
                        DisplayDialogue(_dialogues[_dialogueIndex]);
                        break;
                    }
                }

                _staticStartsWith = null;
                return;
            }
            for (var i = 0; i < _dialogues.Count; i++) {
                var dlg = _dialogues[i].TrimStart("A_");
                dlg = dlg.TrimStart("P_");
                // ReSharper disable once InvertIf
                if (dlg.StartsWith(startsWith)) {
                    _dialogueIndex = i;
                    DisplayDialogue(_dialogues[_dialogueIndex]);
                    break;
                }
            }
        }

        public void PrevDialogue() {
            // ReSharper disable once InvertIf
            if (_dialogueIndex > 0) {
                _dialogueIndex--;
                DisplayDialogue(_dialogues[_dialogueIndex]);
            }
        }
        public void NextDialogue() {
            if (_removeEvent) {
                _evt.RemoveAllListeners();
            }
            
            if (_dialogueIndex < _dialogues.Count - 1) {
                _dialogueIndex++;
                DisplayDialogue(_dialogues[_dialogueIndex]);
            }

            // ReSharper disable once InvertIf
            if (_dialogueIndex == _dialogues.Count) {
                apaSpeech.SetActive(false);
                bottomSpeech.SetActive(false);
                print("Light guidance finalized");
                //GeneralGuidance.Instance.LoadNextScenario();
            }
        }

        private void DisplayDialogue(string dialogue) {
            var apaTurn = false;
            var playerTurn = false;
            if (dialogue.StartsWith("ACTION ")) {
                dialogue = dialogue.Replace("ACTION ", "");

                switch (dialogue) {
                    case "Skip": {
                        report.SetActive(false);
                        materials.SetActive(false);
                        _dialogueIndex = 7;
                        NextDialogue();
                        break;
                    }
                    case "Particles": {
                        foreach (Transform tr in materials.transform) {
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
                materials.SetActive(true);
            }

            if (dialogue.StartsWith("Test etmek için")) {
                GeneralGuidance.Instance.activityIndex = 1;
                //Hide APA, display lightBulb
                apa.SetActive(false);
                lightBulb.SetActive(true);
                navigationArrows.SetActive(false);
                GeneralGuidance.Instance.allowDrag = true;
                rubPanel.SetActive(true);
            }

            if (dialogue.StartsWith("Ampul yandı! ")) {
                var index = GeneralGuidance.Instance.navbar.AddButton();
                GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleReport);
                _evt.AddListener(NextDialogue);
                _removeEvent = true;
            }

            if (dialogue.StartsWith("İlk kombinasyonu doğru yazdın!")) {
                // reportSetInactive = true;
                apa.SetActive(false);
                // NavigationArrows.SetActive(true);
            }

            if (dialogue.StartsWith("Tebrikler, rapor başarılı!")) {
                navigationArrows.SetActive(true);
                report.SetActive(false);
                for (var i = 0; i < 3; i++) {
                    for (var j = 0; j < 2; j++) {
                        GeneralGuidance.Instance.MaterialReportArray[i, j, 1] = GeneralGuidance.Instance.MaterialReportArray[i, j, 0];
                        GeneralGuidance.Instance.MaterialReportArray[i, j, 2] = GeneralGuidance.Instance.MaterialReportArray[i, j, 0];
                    }
                }
            }

            if (dialogue.StartsWith("Sen raporu hazırlarken")) {
                GeneralGuidance.Instance.activityIndex = 2;
                apa.SetActive(true);
                lightBulb.SetActive(false);
                rubPanel.SetActive(false);
            }

            if (dialogue.StartsWith("Objeleri, rapordan merceğe ")) {
                GeneralGuidance.Instance.notifyOnSnap = true;
            }
            
            if (dialogue.StartsWith("Hangi objelerin birbiriyle")) {
                navigationArrows.SetActive(false);
                apa.SetActive(false);
                chargePanel.SetActive(true);
                GeneralGuidance.Instance.rubbingMachine = chargePanel.GetComponent<RubbingMachineManager>();
                GeneralGuidance.Instance.rubbingMachine.slot1 = null;
                GeneralGuidance.Instance.rubbingMachine.slot2 = null;
                DeleteMaterials();
                materials.SetActive(true);
                GeneralGuidance.Instance.report.ConvertToChargeAmounts();
                GeneralGuidance.Instance.report.dualityConstraint = false;
                GeneralGuidance.Instance.report.chargeObsConstraint = true;
                for (var i = 0; i < report.transform.childCount; i++) {
                    if (report.transform.GetChild(i).TryGetComponent(out Draggable draggable)) {
                        draggable.canDrag = true;
                    }
                }
                _evt.AddListener(NextDialogue);
                _removeEvent = true;
            }

            if (dialogue.StartsWith("Yazdığın yükler doğru görünüyor!")) {
                GeneralGuidance.Instance.notifyOnSnap = false;
            }

            if (dialogue.StartsWith("Raporu bitirmişin")) {
                report.SetActive(false);
                apa.SetActive(true);
                rubPanel.SetActive(false);
                navigationArrows.SetActive(true);
                GeneralGuidance.Instance.notifyOnSnap = false;
            }
            
            if (dialogue.StartsWith("Önem sırasına")) {
                GeneralGuidance.Instance.activityIndex = 3;
                guidanceBackground.GetComponent<SpriteRenderer>().sprite = roomExterior;
                guidanceBackground.transform.GetChild(0).gameObject.SetActive(false);
                guidanceBackground.transform.GetChild(1).gameObject.SetActive(true);
                _shouldChangeApaVisibility = false;
            }

            if (dialogue.StartsWith("Kapıda biraz")) {
                chargePanel.SetActive(true);
                navigationArrows.SetActive(true);
                guidanceBackground.transform.GetChild(1).GetComponent<ElectricSpecs>().OnShowVisualParticles();
            }

            if (dialogue.StartsWith("Kapıyı fazla yükünden")) {
                navigationArrows.SetActive(false);
                apa.SetActive(false);
            }

            if (dialogue.StartsWith("Kapının net yükünü sıfırladın!")) {
                chargePanel.SetActive(false);
                report.SetActive(false);
                apa.SetActive(true);
                _shouldChangeApaVisibility = true;
                DeleteMaterials();
                navigationArrows.SetActive(true);
                guidanceBackground.transform.GetChild(1).gameObject.SetActive(false);
            }

            if (dialogue.StartsWith("Burası yaşam destek odası.")) {
                GeneralGuidance.Instance.activityIndex = 4;
                GeneralGuidance.Instance.rubbingMachine.doOnce = true;
                guidanceBackground.GetComponent<SpriteRenderer>().sprite = roomInterior;
                guidanceBackground.transform.GetChild(1).gameObject.SetActive(false);
                guidanceBackground.transform.GetChild(2).gameObject.SetActive(true);
                guidanceBackground.transform.GetChild(2).GetComponent<ElectricSpecs>().OnShowVisualParticles();
            }

            if (dialogue.StartsWith("Yüklü tahtaya ancak")) {
                apa.SetActive(false);
                _shouldChangeApaVisibility = false;
                navigationArrows.SetActive(false);
                chargePanel.SetActive(true);
            }

            if (dialogue.StartsWith("Paneli açığa çıkarmayı")) {
                navigationArrows.SetActive(true);
                apa.SetActive(true);
                report.SetActive(false);
                DeleteMaterials();
                chargePanel.SetActive(false);
            }
            
            if (dialogue.StartsWith("Teknik rapora erişmen için")) {
                navigationArrows.SetActive(false);
                var index = GeneralGuidance.Instance.navbar.AddButton();
                GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleEngReport);
                _evt.AddListener(NextDialogue);
                _removeEvent = true;
            }

            if (dialogue.StartsWith("Yardımın için teşekkür")) {
                navigationArrows.SetActive(true);
            }

            if (dialogue.StartsWith("Raporu oluşturup kısmen")) {
                navigationArrows.SetActive(false);
            }

            if (dialogue.StartsWith("Sistem raporu onayladı,")) {
                navigationArrows.SetActive(true);
            }

            if (dialogue.StartsWith("Nükleer reaktör bu kapının")) {
                GeneralGuidance.Instance.activityIndex = 5;
                guidanceBackground.GetComponent<SpriteRenderer>().sprite = reactorExterior;
                guidanceBackground.transform.GetChild(1).gameObject.SetActive(false);
                guidanceBackground.transform.GetChild(2).gameObject.SetActive(false);
            }

            if (dialogue.StartsWith("Nasıl görünüyor")) {
                guidanceBackground.GetComponent<SpriteRenderer>().sprite = reactorInterior;
            }
            
            if (dialogue.StartsWith("Reaktörün üç yanında")) {
                chargePanel.SetActive(true);
            }


            if (apaTurn) {
                if (apa.activeInHierarchy) {
                    apaSpeech.SetActive(true);
                    topSpeech.SetActive(false);
                    bottomSpeech.SetActive(false);
                    _apaSpeechTMP.text = Interpolator(dialogue);
                } else {
                    apaSpeech.SetActive(false);
                    topSpeech.SetActive(true);
                    bottomSpeech.SetActive(false);
                    _topSpeechTMP.text = Interpolator(dialogue);
                }
            } else if (playerTurn) {
                apaSpeech.SetActive(false);
                bottomSpeech.SetActive(true);
                _bottomSpeechTMP.text = Interpolator(dialogue);
            }
        }

        private void Update() {
            // ReSharper disable once InvertIf
            if (GeneralGuidance.Instance.skipActivity) {
                //Ac2-7
                //Ac3-20
                //Ac3-20
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (_dialogueIndex < 7) {
                    GeneralGuidance.Instance.allowDrag = true;
                    navigationArrows.SetActive(true);
                    if (GeneralGuidance.Instance.navbar.GetComponent<NavbarManager>().displayCount == 2) {
                        var index = GeneralGuidance.Instance.navbar.AddButton();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleReport);
                    }
                    GeneralGuidance.Instance.MaterialReportArray = new string[3, 2, 3];
                    GeneralGuidance.Instance.MaterialReportArray[0, 0, 0] = "1||3,010964||1|0";
                    GeneralGuidance.Instance.MaterialReportArray[0, 1, 0] = "-1||3,010964||0|1";
                    
                    GeneralGuidance.Instance.MaterialReportArray[1, 0, 0] = "1||3,010964||3|5";
                    GeneralGuidance.Instance.MaterialReportArray[1, 1, 0] = "-1||3,010964||5|3";
                    
                    GeneralGuidance.Instance.MaterialReportArray[2, 0, 0] = "-1||3,010964||3|1";
                    GeneralGuidance.Instance.MaterialReportArray[2, 1, 0] = "1||3,010964||1|3";
                    GeneralGuidance.Instance.notifyOnSnap = true;
                    _dialogueIndex = 6;
                    NextDialogue();
                    return;
                }

                if (_dialogueIndex < 20) {
                    report.SetActive(false);
                    GeneralGuidance.Instance.notifyOnSnap = false;
                    
                    GeneralGuidance.Instance.MaterialReportArray[0, 0, 0] = "1|1|3,010964||1|0";
                    GeneralGuidance.Instance.MaterialReportArray[0, 1, 0] = "-1|-1|3,010964||0|1";
                    
                    GeneralGuidance.Instance.MaterialReportArray[1, 0, 0] = "1|1|3,010964||3|5";
                    GeneralGuidance.Instance.MaterialReportArray[1, 1, 0] = "-1|-1|3,010964||5|3";
                    
                    GeneralGuidance.Instance.MaterialReportArray[2, 0, 0] = "-1|-1|3,010964||3|1";
                    GeneralGuidance.Instance.MaterialReportArray[2, 1, 0] = "1|1|3,010964||1|3";
                    for (var i = 0; i < 3; i++) {
                        for (var j = 0; j < 2; j++) {
                            GeneralGuidance.Instance.MaterialReportArray[i, j, 1] = GeneralGuidance.Instance.MaterialReportArray[i, j, 0];
                            GeneralGuidance.Instance.MaterialReportArray[i, j, 2] = GeneralGuidance.Instance.MaterialReportArray[i, j, 0];
                        }
                    }
                    
                    GeneralGuidance.Instance.MaterialReportArray[0, 0, 1] = "2|2|5,010964||1|0";
                    GeneralGuidance.Instance.MaterialReportArray[0, 1, 1] = "-2|-2|5,010964||0|1";
                    GeneralGuidance.Instance.MaterialReportArray[0, 0, 2] = "4|4|7,010964||1|0";
                    GeneralGuidance.Instance.MaterialReportArray[0, 1, 2] = "-4|-4|7,010964||0|1";
                    apa.SetActive(true);
                    lightBulb.SetActive(false);
                    rubPanel.SetActive(false);
                    navigationArrows.SetActive(true);
                    chargePanel.SetActive(true);
                    GeneralGuidance.Instance.rubbingMachine = chargePanel.GetComponent<RubbingMachineManager>();
                    GeneralGuidance.Instance.rubbingMachine.slot1 = null;
                    GeneralGuidance.Instance.rubbingMachine.slot2 = null;
                    DeleteMaterials();
                    materials.SetActive(true);
                    GeneralGuidance.Instance.report.ConvertToChargeAmounts();
                    GeneralGuidance.Instance.report.dualityConstraint = false;
                    GeneralGuidance.Instance.report.chargeObsConstraint = true;
                    for (var i = 0; i < report.transform.childCount; i++) {
                        if (report.transform.GetChild(i).TryGetComponent(out Draggable draggable)) {
                            draggable.canDrag = true;
                        }
                    }
                    chargePanel.SetActive(false);
                    
                    _dialogueIndex = 19;
                    NextDialogue();
                    return;
                }

                if (_dialogueIndex < 25) {
                    _dialogueIndex = 26;
                    NextDialogue();
                    return;
                }
                
                // ReSharper disable once InvertIf
                if (_dialogueIndex < 36) {
                    if (GeneralGuidance.Instance.navbar.GetComponent<NavbarManager>().displayCount == 3) {
                        var index = GeneralGuidance.Instance.navbar.AddButton();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.RemoveAllListeners();
                        GeneralGuidance.Instance.navbar.transform.GetChild(index).GetComponent<Button>().onClick.AddListener(ToggleEngReport);
                    }
                    _dialogueIndex = 37;
                    NextDialogue();
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

            // ReSharper disable once InvertIf
            if (GeneralGuidance.Instance.skipDialogueEngReport) {
                GeneralGuidance.Instance.skipDialogueEngReport = false;
                engReport.SetActive(false);
                NextDialogue();
            }
        }

        private bool _shouldChangeApaVisibility = true;

        private void ToggleReport() {
            if (_shouldChangeApaVisibility) {
                apa.SetActive(report.activeSelf);
            }
            report.SetActive(!report.activeSelf);
            
            if (apa.activeInHierarchy) {
                apaSpeech.SetActive(true);
                topSpeech.SetActive(false);
                bottomSpeech.SetActive(false);
                _apaSpeechTMP.text = Interpolator(_dialogues[_dialogueIndex]);
            } else {
                apaSpeech.SetActive(false);
                topSpeech.SetActive(true);
                bottomSpeech.SetActive(false);
                _topSpeechTMP.text = Interpolator(_dialogues[_dialogueIndex]);
            }
            
            _evt.Invoke();
        }
        
        private void ToggleEngReport() {
            if (_shouldChangeApaVisibility) {
                apa.SetActive(engReport.activeSelf);
            }
            engReport.SetActive(!engReport.activeSelf);
            
            if (apa.activeInHierarchy) {
                apaSpeech.SetActive(true);
                topSpeech.SetActive(false);
                bottomSpeech.SetActive(false);
                _apaSpeechTMP.text = Interpolator(_dialogues[_dialogueIndex]);
            } else {
                apaSpeech.SetActive(false);
                topSpeech.SetActive(true);
                bottomSpeech.SetActive(false);
                _topSpeechTMP.text = Interpolator(_dialogues[_dialogueIndex]);
            }
            
            _evt.Invoke();
        }

        private static string Interpolator(string dialogue) {
            dialogue = dialogue.TrimStart("A_");
            dialogue = dialogue.TrimStart("P_");
            dialogue = dialogue.Replace("$NAME$", GeneralGuidance.Instance.playerName.Split(" ")[0].Trim(' '));
            dialogue = dialogue.Replace("$FULL_NAME$", GeneralGuidance.Instance.playerName);
            return dialogue;
        }

        private void DeleteMaterials() {
            foreach (Transform tr in materials.transform) {
                Destroy(tr.gameObject);
            }
        }
    }
}
