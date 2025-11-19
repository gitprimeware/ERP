# ERP/MRP Sistemi

.NET Windows Forms ve SQL Server kullanılarak geliştirilen ERP/MRP (Enterprise Resource Planning / Manufacturing Resource Planning) sistemi.

## Teknolojiler
- .NET 8.0 Windows Forms
- SQL Server
- Microsoft.Data.SqlClient

## Proje Yapısı (Katmanlı Mimari)

### ERP.Core
**Amaç**: Ortak kullanılan sınıflar ve veri modelleri

- **Models/**: Veri modelleri ve temel sınıflar
  - `BaseModel.cs`: Tüm modeller için temel sınıf (Id, CreatedDate, ModifiedDate, IsActive)

### ERP.DAL (Data Access Layer)
**Amaç**: Veritabanı erişim katmanı

- **DatabaseHelper.cs**: SQL Server bağlantı yönetimi
  - Connection string yönetimi
  - Bağlantı test fonksiyonları
  - Veritabanı işlemleri için temel metodlar

### ERP.BLL (Business Logic Layer)
**Amaç**: İş mantığı katmanı

- Veri doğrulama ve iş kuralları
- Business logic işlemleri
- DAL ve UI katmanları arasında köprü

### ERP.UI (User Interface)
**Amaç**: Windows Forms arayüzü ve kullanıcı etkileşimi

#### Klasör Yapısı ve Açıklamaları

```
ERP.UI/
├── Models/                  # Veri modelleri (UI katmanına özel)
│   ├── MenuItem.cs         # Menü öğeleri için model (Text, Tag, Icon, Order)
│   └── FormMetadata.cs     # Form meta verileri (FormName, FormType, DisplayName)
│
├── Interfaces/              # Arayüz tanımlamaları
│   ├── IForm.cs            # Form kontrolleri için arayüz
│   └── IMenuProvider.cs    # Menü sağlayıcıları için arayüz
│
├── Services/               # İş mantığı servisleri
│   ├── MenuService.cs      # Menü öğelerini yöneten servis
│   └── FormResolverService.cs  # Form çözümleme ve kayıt servisi
│
├── Managers/               # UI yönetim sınıfları
│   ├── MenuManager.cs      # Menü paneli yönetimi (buton oluşturma, event handling)
│   └── ContentManager.cs   # İçerik paneli yönetimi (form açma/kapama)
│
├── Components/             # Yeniden kullanılabilir UI bileşenleri
│   ├── HeaderPanel.cs      # Üst başlık paneli (UserControl)
│   └── WelcomePanel.cs     # Hoş geldiniz sayfası (UserControl)
│
├── Factories/              # Nesne oluşturma fabrikaları
│   ├── PanelFactory.cs     # Panel oluşturma yardımcıları
│   └── ButtonFactory.cs    # Buton oluşturma yardımcıları
│
├── Forms/                  # Ana formlar
│   ├── MainForm.cs         # Ana pencere (koordinasyon)
│   └── OrderEntryForm.cs   # Sipariş giriş formu
│
└── UI/                     # UI yardımcı sınıfları
    ├── ThemeColors.cs      # Renk paleti tanımlamaları
    └── UIHelper.cs         # UI yardımcı metodları (kart stilleri, vb.)
```

#### Detaylı Açıklamalar

**Models/**
- `MenuItem`: Menü öğelerinin veri modeli (Text, Tag, Icon, Order, IsVisible)
- `FormMetadata`: Form kayıtları için metadata (FormName, FormType, DisplayName)

**Interfaces/**
- `IForm`: Tüm form kontrolleri için standart arayüz
- `IMenuProvider`: Menü öğelerini sağlayan servisler için arayüz

**Services/**
- `MenuService`: Menü öğelerini yönetir, sıralama ve görünürlük kontrolü yapar
- `FormResolverService`: Form adına göre doğru formu çözümler ve döndürür

**Managers/**
- `MenuManager`: Menü panelini yönetir, buton oluşturur, event'leri dinler
- `ContentManager`: İçerik panelinde form açma/kapama işlemlerini yönetir

**Components/**
- `HeaderPanel`: Üst başlık çubuğu (başlık ve kullanıcı bilgisi)
- `WelcomePanel`: Ana sayfa hoş geldiniz paneli (kartlar ve bilgilendirme)

**Factories/**
- `PanelFactory`: Standart panel oluşturma metodları
- `ButtonFactory`: Standart buton oluşturma metodları (Success, Cancel, vb.)

**Forms/**
- `MainForm`: Ana pencere, tüm bileşenleri koordine eder
- `OrderEntryForm`: Sipariş giriş işlemleri için form

**UI/**
- `ThemeColors`: Tüm renk tanımlamaları (Primary, Secondary, Accent, vb.)
- `UIHelper`: UI yardımcı metodları (kart stilleri, yuvarlatılmış köşeler, gölgeler)

## Mimari Prensipler

### 1. Single Responsibility Principle (SRP)
Her sınıf tek bir sorumluluğa sahiptir:
- `MenuManager` sadece menü yönetimi yapar
- `ContentManager` sadece içerik yönetimi yapar
- `FormResolverService` sadece form çözümleme yapar

### 2. Separation of Concerns
- **UI Mantığı** → Managers
- **İş Mantığı** → Services
- **Veri Modelleri** → Models
- **UI Bileşenleri** → Components

### 3. Dependency Injection Hazırlığı
Servisler interface'ler üzerinden çalışır, bağımlılıklar net tanımlanmıştır.

### 4. Factory Pattern
Standart nesne oluşturma işlemleri factory sınıfları üzerinden yapılır.

### 5. Component-Based Architecture
Yeniden kullanılabilir UI bileşenleri ayrı UserControl'ler olarak tasarlanmıştır.

## Özellikler

### UI/UX
- Modern Material Design benzeri renk paleti
- Responsive menü yapısı
- Hover efektleri ve animasyonlar
- Yuvarlatılmış köşeler ve gölge efektleri
- Profesyonel görünüm

### Modüller
- 🏠 Ana Sayfa
- 📝 Sipariş Girişi
- 📦 Stok Yönetimi
- 🏭 Üretim Planlama
- 📊 Satış Yönetimi
- 🛒 Satın Alma
- 👥 Müşteriler
- 🏢 Tedarikçiler
- 📈 Raporlar
- ⚙️ Ayarlar

## Yeni Form Ekleme

Yeni bir form eklemek için:

1. **Form'u oluştur**: `ERP.UI/Forms/` klasörüne yeni UserControl ekle
2. **FormResolverService'e kaydet**: 
   ```csharp
   RegisterForm("FormName", typeof(YourForm), "Görünen Ad");
   ```
3. **MenuService'e menü öğesi ekle**:
   ```csharp
   new MenuItem("📝 Form Adı", "FormName", "📝", orderNumber)
   ```

## Kurulum

1. SQL Server'ın çalıştığından emin olun
2. Projeyi Visual Studio'da açın
3. Connection string'i `ERP.UI/app.config` dosyasında düzenleyin:
   ```xml
   <connectionStrings>
       <add name="ERPConnection" 
            connectionString="..." 
            providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```
4. Solution'ı derleyin (Build Solution)
5. ERP.UI projesini başlangıç projesi olarak ayarlayın
6. Projeyi çalıştırın (F5)

## Geliştirme Notları

- **Modüler Yapı**: Her bileşen bağımsız çalışabilir, test edilebilir
- **Genişletilebilirlik**: Yeni modüller kolayca eklenebilir
- **Bakım Kolaylığı**: Değişiklikler izole edilmiş sınıflarda yapılır
- **Kod Kalitesi**: SOLID prensipleri uygulanmıştır
- **Yeniden Kullanılabilirlik**: Component'ler başka projelerde kullanılabilir

## Gelecek Geliştirmeler

- [ ] Veritabanı tabloları ve DAL metodları
- [ ] Business Logic Layer implementasyonu
- [ ] Kullanıcı yetkilendirme sistemi
- [ ] Raporlama modülü
- [ ] Loglama sistemi
- [ ] Unit testler
- [ ] Dependency Injection container (Microsoft.Extensions.DependencyInjection)

## Lisans

Bu proje özel bir projedir.
