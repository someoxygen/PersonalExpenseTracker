# Personal Expense Tracker

## Overview

Kişisel gelir/gider, hesap, transfer ve aylık bütçe yönetimi. Vue arayüzü gerçek ASP.NET Core API ve PostgreSQL ile çalışır; dashboard örnek veri kullanmaz. Phase 2–15 sırasıyla uygulanmıştır. Transaction (5), transfer (6) ve dashboard (8) ayrı aşamalarda geliştirilip doğrulanmıştır. Kontrol kaydı: [docs/PHASES.md](docs/PHASES.md).

Kayıt/giriş/çıkış, profil görüntüleme, gelir/gider CRUD, filtreleme/sıralama/sayfalama, hesap ve kategori yönetimi, hesaplar arası transfer, aylık kategori bütçeleri ve responsive dashboard bulunur. Formlar yükleme, hata, boş durum, onay ve işlem geri bildirimi içerir.

## Architecture

```text
Domain          -> bağımlılık yok
Application     -> Domain
Infrastructure  -> Application -> Domain (transitive)
Api             -> Application + Infrastructure
```

Domain iş kurallarını; Application kullanım senaryolarını, DTO, validation ve abstraction'ları; Infrastructure PostgreSQL/EF mapping, migration, hashleme ve JWT üretimini; API HTTP/authentication/composition işlemlerini içerir. Controller'lar Application servislerini çağırır.

Application, provider bağımsız EF Core DbSet/LINQ API'sini IAppDbContext üzerinden bilinçli olarak kullanır; ORM bağımsız değildir. Concrete DbContext, Npgsql ve veritabanı yapılandırması Infrastructure'dadır. Domain framework bağımlılığı taşımaz. Gereksiz repository veya MediatR katmanı eklenmedi.

Finansal toplamlar backend SQL aggregate/projection sorgularında hesaplanır. Hesap listesi tek SQL sorgusu, dashboard summary en fazla beş sorgudur; sorgu sayıları test edilir. Aylık grafikte eksik aylar, SQL'den gelen en fazla 12 özet satır üzerinde doldurulur. Frontend yalnızca grafik geometrisini ve gösterim formatını hesaplar.

### Finansal kurallar

- Para alanları PostgreSQL `numeric(18,2)` / C# decimal; gelir/gider, transfer ve bütçe tutarları pozitif ve en fazla iki ondalıklıdır. Açılış bakiyesi negatif olabilir; eksi bakiye engellenmez.
- Kullanıcı para birimi MVP'de TRY'dir. Hesap aynı para biriminde olmalıdır ve para birimi değiştirilemez. Model USD/EUR kodlarını da destekler; döviz dönüşümü veya kullanıcı para birimi değiştirme ekranı yoktur.
- Güncel bakiye = açılış + gelir − gider + gelen transfer − giden transfer. Veritabanında güncel bakiye saklanmaz. Pasif hesaplar toplam bakiyeye dahildir, yeni/yenilenen işlemlerde kullanılamaz. Hesap DELETE fiziksel silme yerine pasife alır.
- Transfer, gelir/giderden ayrı immutable bir kayıttır. Kendi aktif, farklı ve aynı para birimli iki hesabını serializable DB transaction içinde bağlar. Yarım transfer oluşmaz. Transfer toplam gelir/gider ve bütçe harcamasına girmez. Transfer düzenleme/silme endpoint'i yoktur.
- Finansal tarih DateOnly, audit zamanları UTC'dir. Gelecek tarihli kayıt tutulabilir ve listede görünür; tarih gelene kadar bakiye, dashboard ve bütçe harcamasına katılmaz. Varsayılan finansal saat dilimi Europe/Istanbul'dur.
- Bütçe yalnızca gider kategorisine, kullanıcı/kategori/ay/yıl başına bir kez tanımlanır. Remaining negatif, percentage 100 üzerinde olabilir. Dashboard netBalance, bu ayın geliri eksi gideridir.
- Kayıt sırasında 15 varsayılan kategori kullanıcıyla atomik oluşturulur. Sistem kategorileri değiştirilemez/silinemez. Özel kategorinin tipi sonradan değişmez; ilişkili kategori silme 409 döner.

## Tech Stack

.NET 10, ASP.NET Core, EF Core 10, Npgsql, PostgreSQL 17, FluentValidation, JWT; Vue 3 Composition API, strict TypeScript, Vite, Pinia, Vue Router, Axios, sade CSS ve SVG grafikler. xUnit, gerçek PostgreSQL entegrasyon testleri ve Playwright Chromium E2E testleri. Kesin sürümler proje dosyaları ve lock dosyalarındadır.

## Requirements

.NET 10 SDK, Node.js 24 ve npm, PostgreSQL 17. Docker isteğe bağlıdır. Komutlar aksi belirtilmedikçe repository kökünde PowerShell içindir. API geliştirme portu 5080, frontend 5173'tür.

## Local PostgreSQL

Mevcut PostgreSQL kullanıcınızı ve parolanızı connection string'e yazın. Yerel development varsayılanı:

```text
Host=localhost;Port=5432;Database=ExpenseTracker;Username=postgres;Password=postgres
```

Gerekirse PostgreSQL'de `CREATE DATABASE "ExpenseTracker";` çalıştırın. Yerel varsayılan parolayı production'da kullanmayın. Test kullanıcısı ayrı test veritabanları oluşturabilmek için CREATEDB yetkisine ihtiyaç duyar.

## Docker PostgreSQL

```powershell
docker compose up -d postgres
docker compose ps
```

Yalnızca PostgreSQL konteynerde çalışır; port 127.0.0.1:5432'ye açılır, veriler named volume'dadır. Yerel PostgreSQL bu portu kullanıyorsa iki seçeneği birlikte başlatmayın. `docker compose down` durdurur; `down -v` verileri siler. Compose yapılandırması sağlandı; bu ortamda Docker çalıştırılarak doğrulanmadı.

## Backend Setup

```powershell
dotnet tool restore
dotnet restore backend/ExpenseTracker.sln
$env:ASPNETCORE_ENVIRONMENT = 'Development'

# Bir defa: yerel JWT imza anahtarını üretip user-secrets içine kaydet.
$jwtBytes = New-Object byte[] 48
$jwtRng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$jwtRng.GetBytes($jwtBytes)
$jwtRng.Dispose()
$jwtSecret = [Convert]::ToBase64String($jwtBytes)
dotnet user-secrets set 'Jwt:Key' $jwtSecret --project backend/src/ExpenseTracker.Api

# Bağlantınız varsayılandan farklıysa gerçek yerel bağlantınızla ayarlayın:
# dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'YOUR_CONNECTION_STRING' --project backend/src/ExpenseTracker.Api

dotnet ef database update --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api
dotnet build backend/ExpenseTracker.sln --no-restore
dotnet run --project backend/src/ExpenseTracker.Api --launch-profile http
```

JWT anahtarı en az 32 UTF-8 byte olmalı; repository'de anahtar yoktur. Geçersiz/eksik zorunlu ayarlar başlangıçta hata verir. Backend `.env` dosyasını otomatik okumaz.

## Frontend Setup

Ayrı terminalde:

```powershell
cd frontend
Copy-Item .env.example .env
npm install
npm run dev
```

Frontend: http://localhost:5173. Yeni hesap oluşturun; hazır demo parolası yoktur. `.env` değişikliği için Vite'ı yeniden başlatın. `npm run build` çıktısı `frontend/dist` dizinindedir. Statik hosting'de Vue Router history adreslerini `index.html` dosyasına yönlendirin.

## Environment Variables

| Ayar | Kullanım |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL bağlantısı |
| `Jwt__Key` | Gizli imza anahtarı; development için user-secrets tercih edilir |
| `Jwt__Issuer` / `Jwt__Audience` | Varsayılan ExpenseTracker / ExpenseTracker.Web |
| `Jwt__ExpirationMinutes` | Varsayılan 15, geçerli aralık 1–60 |
| `Finance__TimeZone` | Varsayılan Europe/Istanbul |
| `Cors__AllowedOrigins__0` | Frontend origin'i; development http://localhost:5173 |
| `AllowedHosts` | API hostname; birden fazla değer `;` ile ayrılır |
| `ASPNETCORE_ENVIRONMENT` | Development / Production |
| `ASPNETCORE_URLS` | HTTP sunucu dinleme adresleri |
| `VITE_API_BASE_URL` | http://localhost:5080; sonuna /api eklenmez |
| `VITE_FINANCIAL_TIME_ZONE` | Backend Finance:TimeZone ile aynı olmalı |
| `TEST_POSTGRES_CONNECTION` | Entegrasyon testleri için yönetici DB bağlantısı |

`VITE_` değerleri tarayıcıya açıktır, secret içeremez. Production connection string ve CORS listesi varsayılan olarak boştur. Production TLS, secret yönetimi, AllowedHosts ve güvenilen reverse proxy yapılandırması hosting ortamında yapılmalıdır.

JWT tarayıcı belleğinde tutulur; localStorage/sessionStorage kullanılmaz. Sayfa yenileme veya token süresinin dolması yeniden giriş gerektirir. Çıkış istemci oturumunu temizler; sunucuda token iptali ve refresh token yoktur. Şifreler PBKDF2 ile hashlenir. API fallback authorization, sahiplik filtreleri, owner-qualified foreign key'ler, auth rate limit ve sınırlandırılmış CORS uygular. Hatalar ProblemDetails döner; stack trace/parola/token uygulama loglarına yazılmaz. Paket taramaları kapsamlı güvenlik denetiminin yerini tutmaz.

## EF Core Migration Commands

Altı migration mevcuttur: InitialCreate, AddCategories, AddAccounts, AddTransactions, AddTransfers, AddBudgets. Yeniden InitialCreate eklemeyin.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api
dotnet ef migrations has-pending-model-changes --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api
# Yalnızca gelecekte model değiştiğinde:
# dotnet ef migrations add DescribeYourChange --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api --output-dir Persistence/Migrations
```

Başlangıçta EnsureCreated veya otomatik Migrate çağrısı yapılmaz. Migration'ları production'a kontrollü deployment adımı olarak uygulayın. FK delete davranışları finansal geçmişi korur; kullanıcıyla birlikte hesap/kategori sahipliği composite FK üzerinden korunur. Normalize email, kategori adı/tipi ve aylık bütçe için unique constraint; sahiplik, tarih ve ilişki sorguları için index'ler vardır.

## Running

API http://localhost:5080, uygulama http://localhost:5173. `/health/live` süreç durumunu, `/health/ready` DB bağlantısını kontrol eder; readiness şema güncelliğini doğrulamaz. HTTPS geliştirme için `dotnet dev-certs https --trust`, ardından `--launch-profile https`; frontend API adresini https://localhost:7080 yapın. `npm run preview` portu 4173 için ayrıca CORS origin'i gerekir.

| API | İşlev |
| --- | --- |
| POST /api/auth/register, /api/auth/login | Anonim kayıt/giriş |
| GET /api/auth/me | Oturumdaki profil |
| /api/accounts, /api/categories, /api/transactions, /api/budgets | GET liste/id, POST, PUT id, DELETE id |
| /api/transfers | POST, GET liste/id |
| GET /api/dashboard/summary | Bakiye, aylık gelir/gider, bütçe özeti |
| GET /api/dashboard/monthly?months=6 | 6 veya 12 aylık özet |
| GET /api/dashboard/category-expenses | Bu ay kategori dağılımı |
| GET /api/dashboard/recent-transactions | Son 10 gerçekleşmiş gelir/gider |
| GET /api/dashboard/budgets | Bu ayın bütçeleri |

DTO alanları camelCase, enum değerleri Income/Expense ve Cash/Bank/CreditCard/Savings/Other string'leridir. Finansal tarihler YYYY-MM-DD, audit zamanları UTC ISO 8601. Sayfalama `items/page/pageSize/totalCount/totalPages` döner. İşlem filtreleri `accountId`, `categoryId`, `type`, `startDate`, `endDate`, `minAmount`, `maxAmount`, `search`; sıralama `sortBy=transactionDate|amount|createdAt`, `sortDirection=asc|desc`. `pageSize` en fazla 100'dür. Bütçe listesi `month/year` alır.

Başarılı oluşturma 201, silme/pasife alma 204; validation 400, kimlik doğrulama 401, bulunamayan veya başka kullanıcıya ait kayıt 404, ilişki/tekillik çakışması 409, rate limit 429 döner. İstemci UserId göndererek sahipliği değiştiremez.

## Swagger

Yalnızca Development: http://localhost:5080/swagger ve `/openapi/v1.json`. Kayıt/giriş cevabındaki token'ı Swagger Authorize alanına girin. Production'da dokümantasyon kapalıdır.

## Testing

```powershell
dotnet restore backend/ExpenseTracker.sln --locked-mode
dotnet build backend/ExpenseTracker.sln --no-restore
# Gerekiyorsa kendi test PostgreSQL bağlantınız:
$env:TEST_POSTGRES_CONNECTION = 'Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres'
dotnet test backend/ExpenseTracker.sln --no-build --no-restore
```

43 backend testi: 6 Domain, 7 Application, 30 entegrasyon. Entegrasyon testleri gerçek PostgreSQL'de kendilerine ait rastgele isimli `expense_tests_*` veritabanları oluşturur, migration uygular, sonunda yalnızca oluşturduklarını siler. EF InMemory/SQLite kullanılmaz. PostgreSQL olmadan yalnızca Domain.Tests ve Application.Tests projelerini çalıştırabilirsiniz.

Test kapsamı: kullanıcı izolasyonu, anonim erişim/JWT imza-issuer-audience-expiry, overposting, transaction filtre/bakiye, transfer INSERT sonrası hata ile rollback, bütçe tekilliği ve eşzamanlı oluşturma, cross-owner FK reddi, gelecek tarih ve dashboard aggregate doğruluğu, sorgu sayıları.

```powershell
cd frontend
npm install
npx playwright install chromium
npm run build
npm run type-check
npm run format:check
# API development veritabanıyla çalışıyor olmalı; test Vite'ı başlatır veya kullanır.
npm run test:e2e
```

Üç Chromium E2E senaryosu gerçek API üzerinde auth/oturum, transaction CRUD/filtre ve hesap/transfer/kategori/bütçe/dashboard akışlarını doğrular. Testler development DB'sinde benzersiz test kullanıcıları/verileri oluşturur; production bağlantısı kullanmayın. Masaüstü ve 390px mobil ekran, taşma, runtime hatası ve modal Escape/odak davranışı kontrol edilir. Görseller `frontend/test-results` içindedir. Ayrı lint script'i yoktur; strict TypeScript ve Prettier kontrolleri çalıştırılır.

Son yerel doğrulama: backend 0 uyarı/hata, 43/43 test; frontend build/type-check/format ve 3/3 E2E başarılı. NuGet ve npm taramalarında bilinen güvenlik açığı bildirilmedi. Gerçek PostgreSQL 17.11 test sunucusu bu çalışma sırasında 127.0.0.1:55432'de kullanıldı; normal kurulum varsayılanı 5432'dir.

## Supabase Migration

Supabase SDK veya Supabase'e bağlı domain kodu yoktur. Aynı Npgsql provider ve migration'lar kullanılır; uygulamanın hedef PostgreSQL sunucusunu değiştirmek için `ConnectionStrings__DefaultConnection` yeterlidir. Hedefin sağladığı SSL/pooling parametrelerini bağlantıya ekleyin; migration için DDL destekleyen bağlantı ve yetkiler kullanın. Migration'ları hedefe uygulama ve mevcut veriyi taşıma ayrı operasyonel adımlardır; connection string veri taşımaz.

2026-09-06: Gerçek Supabase Session pooler bağlantısı etkinleştirildi; altı migration, API sağlık kontrolü, kayıt/giriş ve dashboard doğrulandı. Tablolar private `expense_tracker` şemasındadır. Yerel veriler taşınmadı; tam TLS sertifika doğrulaması ve deployment tamamlanmadı. Ayrıntılar: [Supabase bağlantısı](docs/SUPABASE.md).

## Project Structure

```text
backend/
  ExpenseTracker.sln
  src/
    ExpenseTracker.Domain/{Common,Entities}
    ExpenseTracker.Application/{Abstractions,Common,Features}
    ExpenseTracker.Infrastructure/{Authentication,Persistence}
    ExpenseTracker.Api/{Authentication,Configuration,Controllers,Middleware}
  tests/
    ExpenseTracker.Domain.Tests/
    ExpenseTracker.Application.Tests/
    ExpenseTracker.IntegrationTests/
frontend/
  src/{api,components,composables,features,layouts,router,stores,types,utils,views}
  tests/
  playwright.config.ts
  .env.example
docs/PHASES.md
docker-compose.yml
dotnet-tools.json
global.json
```

## Known Limitations

Canlı deployment, Docker çalıştırması ve yük testi yapılmadı. Gerçek Supabase bağlantısı ayrıca doğrulandı; bkz. docs/SUPABASE.md. Profil salt okunur; MVP tek kullanıcı para birimi TRY ile çalışır. Refresh token, sunucu tarafı token iptali, parola sıfırlama, email doğrulama, 2FA ve transfer değiştirme/iptal akışı yoktur. Eksi bakiye mümkündür. Auth rate limit süreç belleğindedir; çok instance deployment için dağıtık limit/proxy değerlendirmesi gerekir. Bu doğrulamalar production operasyonlarının veya bağımsız güvenlik testinin yerine geçmez.

## Future Improvements

Recurring transactions, subscriptions, savings goals, multi-currency ve exchange rates, receipt upload/OCR, CSV import/export, Excel reports, advanced analytics, notifications, dark mode, PWA/mobile, Google authentication, 2FA, refresh/session yönetimi ve transfer düzeltme akışı.


