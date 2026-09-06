# Supabase bağlantısı

2026-09-06: Uygulama Supabase PostgreSQL veritabanına geçirildi. Development user-secrets içindeki `ConnectionStrings:DefaultConnection` artık Session pooler bağlantısını kullanır. Aynı bağlantı `ConnectionStrings:SupabaseConnection` altında da kayıtlıdır. Parola repository'ye yazılmadı.

| Ayar | Değer |
| --- | --- |
| Host | aws-0-ap-south-1.pooler.supabase.com |
| Port | 5432 |
| Database | postgres |
| Username | postgres.cntiylneocfuybnfkloi |
| Search Path | expense_tracker |
| SSL Mode | Require |
| Maximum Pool Size | 20 |

Altı EF migration hedefte uygulandı. Tablolar Supabase Table Editor içindeki `expense_tracker` şemasında görülebilir. PUBLIC, anon, authenticated ve service_role rollerinin bu şemaya ve mevcut tablolarına erişimi kapatıldı; uygulama ASP.NET Core API üzerinden erişir. Schema usage kontrollerinde anon/authenticated için false sonucu doğrulandı.

API yeniden başlatıldı. Gerçek Supabase üzerinde health/ready 200, kullanıcı kaydı, giriş, me, 15 varsayılan kategori ve sıfır bakiyeli dashboard doğrulandı. Kontrol için oluşturulan geçici kullanıcı ve kategorileri temizlendi. Yerel geliştirme veritabanındaki kullanıcılar ve finansal kayıtlar taşınmadı.

Normal başlatma:

```powershell
dotnet run --project backend/src/ExpenseTracker.Api --launch-profile http
```

Ortamda `ConnectionStrings__DefaultConnection` tanımlıysa user-secrets ayarını ezer. Testleri Supabase bağlantısıyla çalıştırmayın; `TEST_POSTGRES_CONNECTION` ayrı yerel PostgreSQL'i göstermelidir. Deployment ortamına secret ayrıca verilmelidir; Development user-secrets yalnızca bu makinede geçerlidir.

SSL Mode=Require bağlantıyı şifreler; tam sunucu sertifikası doğrulaması sağlamaz. Sistem CA deposuyla verify-full denemesi başarısız oldu. VerifyFull için projenin güvenilir CA sertifikası ayrıca yapılandırılmalıdır. Canlı uygulama hosting/deployment işlemi yapılmadı.
