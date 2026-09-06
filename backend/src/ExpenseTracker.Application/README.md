# Application
Feature servisleri, DTO'lar, FluentValidation ve dış servis sözleşmeleri.
Yalnızca Domain projesine referans verir; Infrastructure'a bağımlı değildir.
IAppDbContext, EF Core'un provider-independent DbSet/LINQ API'sini kullanır;
bu bilinçli paket bağımlılığı SQL sorgularını Application'da test edilebilir tutar.
PostgreSQL provider, somut context, migration ve DB transaction uygulaması Infrastructure'dadır.
