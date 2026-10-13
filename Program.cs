using System;

namespace ConveyorAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            ConveyorSystem conveyor = new ConveyorSystem();
            conveyor.Run();
        }
    }

    // Uyarı Lambası Sınıfı (Andon / İkaz Lambası)
    public class WarningLamp
    {
        public string StatusColor { get; private set; } = "Kırmızı (Duruyor)";

        public void SetGreen()
        {
            StatusColor = "Yeşil (Normal Çalışma)";
        }

        public void SetRed()
        {
            StatusColor = "Kırmızı (Durdu / Beklemede)";
        }

        public void SetFlashingYellow()
        {
            StatusColor = "Sarı - Yanıp Sönüyor (ARIZA / E-STOP!)";
        }
    }

    // Motor Sınıfı: Bant hareketini ve termik korumayı yönetir
    public class Motor
    {
        public bool IsRunning { get; private set; }
        public bool IsOverheated { get; private set; }

        public void Start()
        {
            if (IsOverheated)
            {
                Console.WriteLine("[MOTOR] HATA: Motor termik arızalı! Önce arızayı sıfırlayın.");
                return;
            }

            IsRunning = true;
            Console.WriteLine("[MOTOR] Konveyör motoru çalıştırıldı.");
        }

        public void Stop()
        {
            IsRunning = false;
            Console.WriteLine("[MOTOR] Konveyör motoru durduruldu.");
        }

        public void TriggerOverheat()
        {
            IsRunning = false;
            IsOverheated = true;
            Console.WriteLine("[MOTOR] UYARI: Aşırı yüklenme! Termik attı, motor korumaya geçti.");
        }

        public void ResetFault()
        {
            IsOverheated = false;
            Console.WriteLine("[MOTOR] Arıza sıfırlandı (Termik resetlendi). Motor çalışmaya hazır.");
        }
    }

    // Sensör Sınıfı
    public class Sensor
    {
        public string Name { get; set; }

        public Sensor(string name)
        {
            Name = name;
        }

        public void Trigger()
        {
            Console.WriteLine($"[SENSÖR] {Name} tetiklendi! Ürün algılandı.");
        }
    }

    // Ana Otomasyon Sistemi
    public class ConveyorSystem
    {
        private Motor _motor;
        private WarningLamp _lamp;
        private Sensor _entrySensor;
        private Sensor _exitSensor;
        private int _itemCount;
        private int _targetLimit; // Hedef Parti Sayısı
        private bool _emergencyStop;

        public ConveyorSystem()
        {
            _motor = new Motor();
            _lamp = new WarningLamp();
            _entrySensor = new Sensor("Giriş Foto-sel Sensörü");
            _exitSensor = new Sensor("Çıkış Sayıcı Sensörü");
            _itemCount = 0;
            _targetLimit = 5; // Varsayılan hedef: 5 ürün işlenince bant durur
            _emergencyStop = false;
            _lamp.SetRed();
        }

        public void Run()
        {
            Console.WriteLine("=== Gelişmiş Endüstriyel Konveyör Bant Otomasyonu ===");
            bool exit = false;

            while (!exit)
            {
                // Duruma göre ikaz lambasını otomatik güncelle
                UpdateLampStatus();

                Console.WriteLine("\n--- Kontrol Paneli ---");
                Console.WriteLine("1: Sistemi Başlat (Start)");
                Console.WriteLine("2: Sistemi Durdur (Stop)");
                Console.WriteLine("3: Ürün Girişi Simüle Et");
                Console.WriteLine("4: Ürün Çıkışı Simüle Et");
                Console.WriteLine("5: Acil Durdurma (E-Stop)");
                Console.WriteLine("6: Motor Termik Arızası Oluştur (Simülasyon)");
                Console.WriteLine("7: Arızayı / E-Stop'u Sıfırla (Reset)");
                Console.WriteLine("8: Sayaç Değerini Sıfırla");
                Console.WriteLine("9: Sistem Durumunu Görüntüle");
                Console.WriteLine("0: Programdan Çık");
                Console.Write("Seçiminiz: ");

                string input = Console.ReadLine();
                Console.WriteLine();

                switch (input)
                {
                    case "1":
                        if (_emergencyStop || _motor.IsOverheated)
                        {
                            Console.WriteLine("HATA: Sistemde aktif arıza veya E-Stop var! Çalıştırılamaz.");
                        }
                        else if (_itemCount >= _targetLimit)
                        {
                            Console.WriteLine($"UYARI: Hedef parti sayısına ({_targetLimit} adet) ulaşıldı! Devam etmek için sayacı sıfırlayın.");
                        }
                        else
                        {
                            _motor.Start();
                        }
                        break;

                    case "2":
                        _motor.Stop();
                        break;

                    case "3":
                        if (_motor.IsRunning && !_emergencyStop && !_motor.IsOverheated)
                        {
                            if (_itemCount < _targetLimit)
                            {
                                _entrySensor.Trigger();
                                _itemCount++;
                                Console.WriteLine($"[BANT] Ürün bant üzerinde ilerliyor... Toplam Sayaç: {_itemCount}/{_targetLimit}");

                                // Hedefe ulaşıldıysa otomatik durdur
                                if (_itemCount >= _targetLimit)
                                {
                                    _motor.Stop();
                                    Console.WriteLine($"[OTOMASYON] Hedef sayıya ulaşıldı! Bant otomatik olarak durduruldu.");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("UYARI: Bant çalışmıyor, arıza var veya limit dolu, ürün alınamaz!");
                        }
                        break;

                    case "4":
                        if (_motor.IsRunning && !_emergencyStop && !_motor.IsOverheated)
                        {
                            _exitSensor.Trigger();
                            Console.WriteLine("[BANT] Ürün hattan başarıyla çıktı ve paketlendi.");
                        }
                        else
                        {
                            Console.WriteLine("UYARI: Bant çalışmıyor ya da sistem arızalı!");
                        }
                        break;

                    case "5":
                        _emergencyStop = true;
                        _motor.Stop();
                        Console.WriteLine("!!! ACİL DURDURMA (E-STOP) DEVREYE GİRDİ !!!");
                        break;

                    case "6":
                        _motor.TriggerOverheat();
                        break;

                    case "7":
                        _emergencyStop = false;
                        _motor.ResetFault();
                        Console.WriteLine("Sistem kilitleri ve arızalar temizlendi.");
                        break;

                    case "8":
                        _itemCount = 0;
                        Console.WriteLine("Ürün sayacı sıfırlandı. Yeni parti için üretime hazır.");
                        break;

                    case "9":
                        ShowStatus();
                        break;

                    case "0":
                        exit = true;
                        Console.WriteLine("Sistem kapatılıyor...");
                        break;

                    default:
                        Console.WriteLine("Geçersiz seçim, lütfen tekrar deneyin.");
                        break;
                }
            }
        }

        private void UpdateLampStatus()
        {
            if (_emergencyStop || _motor.IsOverheated)
            {
                _lamp.SetFlashingYellow();
            }
            else if (_motor.IsRunning)
            {
                _lamp.SetGreen();
            }
            else
            {
                _lamp.SetRed();
            }
        }

        private void ShowStatus()
        {
            Console.WriteLine("--- Sistem Durum Raporu ---");
            Console.WriteLine($"Motor Durumu : {(_motor.IsRunning ? "Çalışıyor (Aktif)" : "Duruyor")}");
            Console.WriteLine($"İkaz Lambası : {_lamp.StatusColor}");
            Console.WriteLine($"Termik Arıza : {(_motor.IsOverheated ? "VAR (Motor Korumada!)" : "Normal")}");
            Console.WriteLine($"Acil Durum   : {(_emergencyStop ? "Kilitli (E-Stop Basıldı)" : "Normal")}");
            Console.WriteLine($"Ürün Sayacı  : {_itemCount} / {_targetLimit} (Hedef)");
        }
    }
}
