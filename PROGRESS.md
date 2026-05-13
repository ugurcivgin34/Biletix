# Biletix - Geliştirme İlerleme Dosyası

## Proje Hakkında
.NET 8 Clean Architecture + Minimal API ile yazılmış Biletix/TicketMaster benzeri bilet satış backend sistemi.

Backend kapsamı tamamlandı. Frontend geliştirme fazı başlıyor.

## Backend Durumu
TAMAMLANDI.

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

## Backend API Endpoint'leri

### Auth
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh
- POST /api/auth/logout
- GET  /api/auth/me

### Venues
- GET    /api/venues
- GET    /api/venues/{id}
- POST   /api/venues (Admin)
- PUT    /api/venues/{id} (Admin)
- DELETE /api/venues/{id} (Admin)

### Events
- GET    /api/events
- GET    /api/events/{id}
- GET    /api/events/my (Organizer/Admin)
- POST   /api/events (Organizer/Admin)
- PUT    /api/events/{id} (Organizer/Admin)
- POST   /api/events/{id}/publish
- POST   /api/events/{id}/cancel
- POST   /api/events/{id}/ticket-types

### Search
- GET /api/search/events

### Bookings
- POST /api/bookings/reserve
- POST /api/bookings/checkout (Saga)
- GET  /api/bookings/{id}
- GET  /api/bookings/my

### Queue
- POST   /api/queue/{eventId}/join
- GET    /api/queue/{eventId}/status
- DELETE /api/queue/{eventId}/leave
- GET    /api/queue/{eventId}/stream (SSE)

### Payments
- POST /api/payments/create-intent
- POST /api/payments/cancel
- GET  /api/payments/booking/{bookingId}

### Tickets
- GET  /api/tickets/{bookingId}/qr
- GET  /api/tickets/{bookingId}/token
- POST /api/tickets/validate (Organizer/Admin)
- GET  /api/tickets/scans/{eventId} (Organizer/Admin)

### Webhooks
- POST /api/webhooks/stripe

### System
- GET /health
- GET /health/live
- GET /metrics

## Servis Portları
| Servis        | Port | URL                        |
|---------------|------|----------------------------|
| API           | 5157 | http://localhost:5157      |
| PostgreSQL    | 5432 | localhost:5432             |
| Redis         | 6379 | localhost:6379             |
| Kafka         | 9092 | localhost:9092             |
| Kafka UI      | 8080 | http://localhost:8080      |
| Elasticsearch | 9200 | http://localhost:9200      |
| Kibana        | 5601 | http://localhost:5601      |
| Debezium      | 8083 | http://localhost:8083      |
| Prometheus    | 9090 | http://localhost:9090      |
| Grafana       | 3000 | http://localhost:3000      |

## Kafka Topic'leri
| Topic                  | Kullanım             |
|------------------------|----------------------|
| biletix.public.Events  | CDC - PG -> ES sync  |
| biletix.notifications  | Outbox -> Email      |
| biletix.outbox         | Diğer eventler       |

## Önemli Teknik Kararlar
1. ITokenService -> Application katmanında
2. EventSearchDocument -> Application katmanında
3. Minimal API + IEndpoint pattern, controller yok
4. EF local tool: dotnet-ef 8.0.11
5. SDK: .NET 9.0.313, preview uyumu
6. Primary constructor kullanılmadı
7. Debezium -> PowerShell ile register, Windows/WSL yok
8. Testing env'de rate limit yüksek tutuldu

## Önemli Notlar
- appsettings.json gizli değer içermemeli; Stripe, Gmail ve benzeri key'ler Git'e yazılmamalı.
- `stripe listen` her testte yeniden başlatılmalı.
- Debezium connector docker-compose restart'ta yeniden register edilmeli.
- Admin seed otomatik: `admin@biletix.com` / `Admin123!`
- Monitoring stack için: `docker compose up -d prometheus grafana`

## Test Sonuçları
- Domain Tests:      27 OK
- Application Tests:  9 OK
- Integration Tests: 20 OK
- Toplam:           56 OK - 0 Failed, 0 Skipped

## Backend - TAMAMLANDI

Backend için kalan iş yok. Proje backend kapsamı tamamlandı.

## Frontend - BAŞLIYOR

### Frontend Proje Planı (F-01 -> F-25)

#### Faz 1 - Proje İskeleti
- F-01: Next.js 14 kurulum, layout, Tailwind, shadcn/ui
- F-02: API client, Axios, TanStack Query setup, Zustand auth store
- F-03: Auth sayfaları - login, register

#### Faz 2 - Ana Kullanıcı Akışı
- F-04: Ana sayfa - öne çıkan etkinlikler, hero section
- F-05: Etkinlik arama ve listeleme
- F-06: Etkinlik detay sayfası
- F-07: Profil ve biletlerim sayfası

#### Faz 3 - Bilet Satın Alma
- F-08: Rezervasyon akışı - ticket type seçimi
- F-09: Bekleme sırası UI - SSE ile gerçek zamanlı
- F-10: Stripe Elements - ödeme formu
- F-11: Ödeme onay ve QR bilet sayfası

#### Faz 4 - Organizatör Paneli
- F-12: Organizatör dashboard
- F-13: Etkinlik oluşturma formu
- F-14: Etkinlik yönetimi - publish, cancel, ticket types
- F-15: Kapı doğrulama - QR okuyucu

#### Faz 5 - Admin Paneli
- F-16: Admin dashboard - istatistikler
- F-17: Kullanıcı yönetimi
- F-18: Venue yönetimi

#### Faz 6 - Polish & Production
- F-19: Responsive tasarım düzenlemeleri
- F-20: Loading states, error boundaries, toast notifications
- F-21: SEO - metadata, og tags, sitemap
- F-22: Environment config, deployment hazırlığı
