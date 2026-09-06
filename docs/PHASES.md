# Aşama doğrulama kaydı

- Phase 2: User, JWT, FluentValidation, hashleme, register/login/me; migration uygulandı. Backend 0 uyarı/hata, 8 test geçti; frontend build geçti.
- Phase 3: Kategoriler, kullanıcıyla atomic default seed, sistem kategorisi koruması ve sahiplik kontrolleri. Migration uygulandı; 9 test, backend/frontend build geçti.
- Phase 4: Hesap CRUD, pasife alma, currency ve decimal kuralları. Migration uygulandı; 10 test, backend/frontend build geçti.
- Phase 5: Sadece gelir/gider transaction modeli, SQL filtre/sıralama/sayfalama ve tarih bazlı bakiyeler. 13 test ve build geçti. Bu kontrol tamamlanmadan transfer/dashboard eklenmedi.
- Phase 6: Ayrı immutable Transfer entity; iki hesabı tek kayıtta bağlayan serializable DB transaction. INSERT sonrası hata ile rollback doğrulandı. 15 test, backend/frontend build geçti.
- Phase 7: Aylık bütçeler ve SQL harcama hesapları; future işlemler/transferler hariç. Migration uygulandı; 16 test, backend/frontend build geçti.
- Phase 8: Ayrı dashboard aggregate servisleri; transferler gelir/gider dışında. PostgreSQL üzerinde 20 test, backend/frontend build geçti. Sorgular aggregate/projection kullanıyor; aylık boşluk doldurma en fazla 12 sonuç üzerinde.
- Phase 9: Typed API istemcisi, memory-only token taşıma, auth store, route guard, responsive layout ve ortak UI bileşenleri. Frontend build ve 20 backend testi geçti.
- Phase 10: Login/register/me/logout ve hata akışları gerçek Chromium tarayıcısında API/PostgreSQL ile doğrulandı. Frontend build, 20 backend testi, 1 E2E testi geçti.
- Phase 11: Dashboard KPI, SVG grafikler, son işlemler, bütçe kullanımı ve empty states. Build, 20 backend testi ve dashboard assertion içeren E2E geçti. Grafiklerde yalnızca görsel ölçek/segment yerleşimi hesaplanır.
- Phase 12: Server-side işlem listesi, tüm filtreler, sıralama, pagination ve ortak create/edit formu. Build, 20 backend testi ve 2 E2E testi geçti.
- Phase 13: Hesap/kategori/bütçe yönetimi ve ayrı transfer geçmişi. 20 backend testi ve 3 E2E testi geçti. Masaüstü/390px mobil görüntüler incelendi; yatay taşma ve JS runtime hatası yok.
- Phase 14: Sahiplik/JWT/overposting, composite FK, eşzamanlı budget tekilliği ve SQL sorgu sayısı testleri eklendi. Typed OpenAPI bearer sözleşmesi, eski oturuma ait 401 koruması, DTO alan sınırları ve modal Escape sonrası odak dönüşü düzeltildi. 43 backend testi, 3 E2E, build/type-check/format kontrolü geçti; NuGet/npm taramalarında bildirilen açık yok.
- Phase 15: Final restore/build/test yeniden çalıştırıldı: backend 0 uyarı/0 hata, 43/43 test; npm install/build/type-check/format ve 3/3 Chromium E2E başarılı. Migration model farkı yok. Katman bağımlılıkları, DTO/enum/tarih/pagination sözleşmeleri, sahiplik ve DB constraint'leri gözden geçirildi. README final kurulum/test/Supabase bağlantı adımları ve sınırlamalarla yenilendi. Canlı deployment, Docker runtime ve gerçek Supabase bağlantısı doğrulanmadı.
