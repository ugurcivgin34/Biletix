# Biletix - Geliştirme İlerleme Dosyası

## Proje Hakkında
.NET 8 Clean Architecture + Minimal API ile yazılmış Biletix/TicketMaster benzeri bilet satış backend sistemi.

Backend kapsamı 7 faz halinde tamamlandı.

## Backend Durumu
Tamamlandı.

- Domain, Application, Infrastructure ve API katmanları tamamlandı.
- Auth, RBAC, event/venue yönetimi, arama, booking, queue, payment, saga, outbox, notification, QR ve kapı doğrulama akışları tamamlandı.
- Unit test ve integration test altyapısı tamamlandı.
- OpenTelemetry, Prometheus/Grafana monitoring, rate limiting ve security headers tamamlandı.
- Son doğrulama: `dotnet build Biletix.sln` ve `dotnet test Biletix.sln` başarılı.

## Teknoloji Stack
- .NET 8 Minimal API, Clean Architecture, CQRS, MediatR 12.x
- PostgreSQL 16 - EF Core 8, migrations, soft delete, audit
- Redis 7 - distributed lock, cache, idempotency, waiting queue
- Apache Kafka + Debezium - CDC pipeline
- Elasticsearch 8 - fuzzy search, filtreleme, index sync
- Stripe - PaymentIntent, webhook handler
- BCrypt, JWT, refresh token rotation
- FluentValidation, Serilog, StackExchange.Redis
- OpenTelemetry, Prometheus, Grafana
- xUnit, FluentAssertions, NSubstitute, Testcontainers
- Docker Compose ile local altyapı

## Solution Yapısı
```text
src/
  Biletix.Domain/          -> Entity, AggregateRoot, DomainEvent, Exception
  Biletix.Application/     -> CQRS, Handler, Validator, Interface, DTO
  Biletix.Infrastructure/  -> EF Core, Redis, Kafka, ES, Stripe, Jobs, Observability
  Biletix.API/             -> Minimal API, Endpoint, Middleware

tests/
  Biletix.Domain.Tests/
  Biletix.Application.Tests/
  Biletix.Integration.Tests/
```

## Domain Entity'leri
- User: Admin, Organizer, Customer rolleri
- Venue
- Performer
- Event: Draft -> Published -> Cancelled -> Completed state machine
- TicketType: kapasite, fiyat, stok
- Booking: Pending -> Confirmed -> Cancelled -> Expired state machine
- BookingItem
- OutboxMessage
- TicketScan

## Tamamlanan Fazlar

### Faz 1 - Altyapı
- P-01: Clean Architecture solution, Minimal API iskeleti, IEndpoint pattern, GlobalExceptionHandler, logging/validation pipeline.
- P-02: Docker Compose ile PostgreSQL, Redis, Kafka, Elasticsearch, Debezium ve ilgili local servisler.
- P-03: EF Core domain entity'leri, migrations, audit, soft delete, domain event dispatch.
- P-04: CorrelationIdMiddleware, RequestLoggingMiddleware, health checks.

### Faz 2 - Auth
- P-05: User entity, BCrypt password hash, JWT access token, Redis refresh token storage ve rotation.
- P-06: AdminOnly, OrganizerOrAdmin, AuthenticatedUser policy'leri, CurrentUserService, ResourceAuthorizationService, admin seed.

### Faz 3 - Event & Venue
- P-07: Venue CRUD, arama, pagination, soft delete.
- P-08: Event CRUD, publish/cancel domain metotları, ticket type ekleme, CreatedBy alanı.
- P-09: Elasticsearch search, fuzzy query, filtreleme, sorting.
- P-10: Debezium CDC pipeline ile PostgreSQL -> Kafka -> Elasticsearch sync.

### Faz 4 - Booking & Ticket
- P-11/P-12/P-13: Ticket reservation domain logic, Redis distributed lock, Idempotency-Key, booking reserve/detail/my endpoints.
- P-14: Virtual waiting queue, Redis SortedSet, join/status/leave/SSE stream, reserve öncesi queue kontrolü.

### Faz 5 - Ödeme & Saga
- P-15: Stripe PaymentIntent, PaymentService, OutboxMessage, payment endpoints.
- P-16: Stripe webhook handler, booking confirm/payment failed flows.
- P-17: Outbox Relay Worker, KafkaEventPublisher, notification topic publish.
- P-18: BookingSaga, compensation, ExpireBookingsJob, checkout endpoint.

### Faz 6 - Bildirim & QR
- P-19: Notification Kafka consumer, email service, email templates.
- P-20: QR bilet üretimi, JWT imzalı payload, PNG generate.
- P-21: Kapı doğrulama endpoint'i, QR scan, JWT verify, scan history.

### Faz 7 - Test & Observability
- P-22: Domain ve Application unit testleri.
- P-23: Testcontainers ile PostgreSQL/Redis integration testleri.
- P-24: OpenTelemetry tracing/metrics, Prometheus endpoint, Grafana dashboard.
- P-25: Rate limiting ve security headers.
- Not: YARP Gateway eklenmedi; monolitik mimari için gerekli değil.

## Önemli Mimari Kararlar
1. ITokenService Application katmanında tutuldu; dependency yönü korundu.
2. EventSearchDocument Application katmanında tutuldu.
3. Minimal API + IEndpoint pattern kullanıldı.
4. EF Core soft delete global query filter ile uygulandı.
5. Transactional outbox ile notification publish güvenli hale getirildi.
6. Integration testlerde gerçek PostgreSQL ve Redis Testcontainers ile çalışıyor.
7. OpenTelemetry custom meter Application katmanında tutuldu; Infrastructure tarafında compatibility wrapper var.
8. appsettings.json secret içermeyecek şekilde temizlendi; gizli değerler environment/user-secrets ile verilmeli.

## Servis Portları
| Servis        | Port | URL                   |
|---------------|------|-----------------------|
| API           | 5157 | http://localhost:5157 |
| PostgreSQL    | 5432 | localhost:5432        |
| Redis         | 6379 | localhost:6379        |
| Kafka         | 9092 | localhost:9092        |
| Kafka UI      | 8080 | http://localhost:8080 |
| Elasticsearch | 9200 | http://localhost:9200 |
| Kibana        | 5601 | http://localhost:5601 |
| Debezium      | 8083 | http://localhost:8083 |
| Prometheus    | 9090 | http://localhost:9090 |
| Grafana       | 3000 | http://localhost:3000 |

## Kafka Topic'leri
| Topic                  | Kullanım                    |
|------------------------|-----------------------------|
| biletix.public.Events  | CDC - PostgreSQL -> ES sync |
| biletix.notifications  | Outbox -> Notification      |
| biletix.outbox         | Diğer eventler              |

## Önemli Notlar
- Stripe, SMTP ve benzeri gizli değerler Git'e yazılmamalı.
- `stripe listen` her testte yeniden başlatılmalı; webhook secret değişebilir.
- Debezium connector docker-compose restart sonrası yeniden register edilmeli.
- Test verileri integration test cleanup akışıyla temizleniyor.
- Admin seed uygulama başlangıcında otomatik çalışıyor.
- Monitoring stack için: `docker compose up -d prometheus grafana`.

## Kalan İş
Backend için kalan iş yok. Proje backend kapsamı tamamlandı.
