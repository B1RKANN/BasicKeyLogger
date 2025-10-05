# KeyLogger - Klavye Giriş Kayıt Sistemi

## 📋 Proje Açıklaması

Bu proje, **Nesne Tabanlı Programlama Dersi Ödevi** kapsamında geliştirilmiş profesyonel bir klavye giriş kayıt sistemidir. Uygulama, kullanıcının klavye girişlerini gerçek zamanlı olarak kaydeder ve bu verileri güvenli bir şekilde AWS S3 bulut depolama servisine yükler.

### 🎯 Proje Özellikleri

- **Gerçek Zamanlı Klavye Takibi**: Tüm klavye girişlerini anlık olarak yakalar
- **Türkçe Karakter Desteği**: Ş, Ğ, Ü, Ö, İ, Ç karakterlerini doğru şekilde kaydeder
- **Özel Tuş Desteği**: ENTER, BACKSPACE, TAB, DELETE gibi özel tuşları tanır
- **Caps Lock ve Shift Desteği**: Büyük/küçük harf durumlarını doğru algılar
- **AWS S3 Entegrasyonu**: Log dosyalarını otomatik olarak bulut depolamaya yükler
- **Gizli Çalışma**: Konsol penceresi gizlenerek arka planda çalışır
- **Otomatik Yükleme**: Her dakika log dosyalarını S3'e yükler
- **Hata Yönetimi**: Kapsamlı hata yakalama ve loglama sistemi

## 🛠️ Kurulum Talimatları

### Gereksinimler

- **.NET 8.0 SDK** veya daha yeni sürüm
- **Windows İşletim Sistemi** (Windows API kullanımı nedeniyle)
- **AWS Hesabı** (S3 bucket için)

### AWS Konfigürasyonu

1. `.env` dosyasını oluşturarak AWS bilgilerinizi girin:
```
AWS_ACCESS_KEY=your_aws_access_key_here
AWS_SECRET_KEY=your_aws_secret_key_here
AWS_BUCKET_NAME=your_bucket_name_here
AWS_REGION=eu-central-1
```

## 📖 Kullanım Kılavuzu

> **Not**: Bu proje **Nesne Tabanlı Programlama Dersi Ödevi** olarak geliştirilmiştir.

### Temel Kullanım

1. **Uygulamayı Başlatma**:
   ```bash
   dotnet run
   ```
   
2. **Arka Plan Çalışması**: 
   - Uygulama başladıktan sonra konsol penceresi otomatik olarak gizlenir
   - Sistem tepsisinde çalışmaya devam eder

3. **Log Dosyası Konumu**:
   - Yerel log dosyası: `%APPDATA%\system_log.txt`

4. **Otomatik Yükleme**:
   - Her dakika log dosyası S3'e yüklenir
   - Başarılı yükleme sonrası yerel dosya temizlenir

### Güvenlik Uyarıları

⚠️ **Önemli Güvenlik Notları**:
- Bu uygulama eğitim amaçlıdır
- Sadece kendi bilgisayarınızda test edin
- Başkalarının bilgisayarında izinsiz kullanmayın
- AWS anahtarlarınızı güvenli tutun
- `.env` dosyasını asla paylaşmayın

## 🎥 Eğitim Videosu

[![KeyLogger Videosu](https://img.youtube.com/vi/ZFDa-nxahFs/0.jpg)](https://www.youtube.com/watch?v=ZFDa-nxahFs)

**Video Linki**: https://www.youtube.com/watch?v=ZFDa-nxahFs

### Kullanılan Teknolojiler

- **C# .NET 8.0**: Ana programlama dili ve framework
- **Windows Forms**: UI framework (gizli çalışma için)
- **Windows API**: Klavye hook işlemleri
- **AWS SDK**: S3 entegrasyonu
- **System.Threading.Timer**: Otomatik yükleme zamanlayıcısı

⚠️ **Yasal Uyarı**: Bu yazılım yalnızca eğitim amaçlıdır. Kullanıcılar, yazılımı kullanırken yerel yasalara ve gizlilik düzenlemelerine uygun davranmakla yükümlüdür.