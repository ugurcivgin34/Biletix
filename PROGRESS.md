# Biletix — Geliştirme İlerleme Dosyası

## Proje Hakkında
.NET 8 Clean Architecture + Minimal API ile yazılmış
Biletix/TicketMaster benzeri bilet satış backend sistemi.
25 prompt, 7 faz halinde geliştiriliyor.

## Teknoloji Stack
- .NET 8 Minimal API, Clean Architecture, CQRS, MediatR 12.x
- PostgreSQL 16 — EF Core 8, Migrations, Soft Delete, Audit
- Redis 7 — Distributed Lock, Cache, Idempotency, Waiting Queue
- Apache Kafka + Debezium — CDC Pipeline
- Elasticsearch 8 — Fuzzy Search, Filtre, Index Sync
- Stripe — PaymentIntent, Webhook Handler
- BCrypt, JWT, Refresh Token Rotation
- FluentValidation, Serilog, StackExchange.Redis
- Docker Compose — 8 servis tek komutla

## Solution Yapısı
src/
  Biletix.Domain/          → Entity, AggregateRoot, DomainEvent, Exception
  Biletix.Application/     → CQRS, Handler, Validator, Interface, DTO
  Biletix.Infrastructure/  → EF Core, Redis, Kafka, ES, Stripe, Jobs
  Biletix.API/             → Minimal API, Endpoint, Middleware

tests/
  Biletix.Domain.Tests/
  Biletix.Application.Tests/
  Biletix.Integration.Tests/

## Domain Entity'leri
- User (Admin/Organizer/Customer rolleri)
- Venue (mekan)
- Performer (sanatçı)
- Event (Draft→Published→Cancelled→Completed state machine)
- TicketType (kapasite, fiyat, stok)
- Booking (Pending→Confirmed→Cancelled→Expired state machine)
- BookingItem
- OutboxMessage

## Tamamlanan Adımlar

### ✅ Faz 1 — Altyapı (P-01 → P-04)
- P-01: Clean Architecture solution, Minimal API iskeleti
        IEndpoint pattern, GlobalExceptionHandler (RFC 7807)
        LoggingBehaviour, ValidationBehaviour pipeline
- P-02: Docker Compose — PostgreSQL, Redis, Kafka, Elasticsearch, Debezium
        8 servis, health check'li, biletix-network
- P-03: EF Core domain entity'leri, InitialCreate migration
        SaveChangesAsync: audit + soft delete + domain event dispatch
- P-04: CorrelationIdMiddleware, RequestLoggingMiddleware
        Health checks — postgresql/redis/elasticsearch ayrı ayrı

### ✅ Faz 2 — Auth (P-05 → P-06)
- P-05: User entity, BCrypt password hash
        JWT access token (15dk) + refresh token (7gün)
        Redis refresh token storage + rotation
        /api/auth/register, login, refresh, logout
- P-06: AdminOnly, OrganizerOrAdmin, AuthenticatedUser policy'leri
        ICurrentUserService, IResourceAuthorizationService
        Admin seed (admin@biletix.com / Admin123!)
        /api/auth/me, /admin-only, /organizer-panel

### ✅ Faz 3 — Event & Venue (P-07 → P-10)
- P-07: Venue CRUD — GET/POST/PUT/DELETE /api/venues
        ILike ile arama, pagination, soft delete
- P-08: Event CRUD — /api/events
        Event.Publish(), Event.Cancel() domain metotları
        TicketType ekleme, CreatedBy alanı
        Draft/Published/Cancelled state machine
- P-09: Elasticsearch search — /api/search/events
        Multi-match fuzzy, filtre (city/genre/price/date), sort
        Domain event handler ile publish sonrası ES index
- P-10: CDC Pipeline
        Debezium PostgreSQL connector (pgoutput, logical replication)
        EventCdcConsumer BackgroundService
        op=c/u/d → ES otomatik sync
        Kafka topic: biletix.public.Events

### ✅ Faz 4 — Booking & Ticket (P-11/12/13 → P-14)
- P-11/12/13: TicketType.Reserve/ReleaseReservation/ConfirmSale
              Redis SET NX distributed lock (TTL 10dk)
              Idempotency-Key header + Redis cache (24h)
              /api/bookings/reserve, /{id}, /my
- P-14: Virtual Waiting Queue — Redis SortedSet (score=timestamp)
        ActiveSlots=500, IWaitingQueueService
        /api/queue/{eventId}/join, /status, /leave
        SSE stream — /api/queue/{eventId}/stream
        Reserve öncesi queue kontrolü

### ✅ Faz 5 — Ödeme & Saga (P-15 → P-18)
- P-15: Stripe PaymentIntent — Stripe.net 45.x
        IPaymentService, StripePaymentService
        OutboxMessage entity + migration
        /api/payments/create-intent, /cancel, /booking/{id}
- P-16: Stripe webhook handler — /api/webhooks/stripe
        EventUtility.ConstructEvent imza doğrulama
        payment_intent.succeeded → ConfirmBookingCommand
        payment_intent.payment_failed → ExpireBookingOnPaymentFailureCommand
        booking.confirmed / booking.payment_failed outbox mesajı
- P-17: Outbox Relay Worker (BackgroundService, 5sn)
        IEventPublisher, KafkaEventPublisher
        FIFO, RetryCount<5, IsProcessed flag
        biletix.notifications topic'ine publish
- P-18: BookingSaga — reserve + payment intent + compensation
        ExpireBookingsJob (60sn, Pending+ExpiresAt<now)
        /api/bookings/checkout (tek endpoint saga)
        BookingSagaState enum

### 🔄 Faz 6 — Bildirim & QR (devam ediyor)
- P-19: Notification Service — Kafka consumer + SendGrid email  ← SIRADAKI
- P-20: QR bilet üretimi — JWT imzalı payload, PNG generate
- P-21: Kapı doğrulama endpoint'i — QR scan, JWT verify

### ⏳ Faz 7 — Test & Observability
- P-22: Unit testler — xUnit + FluentAssertions + NSubstitute
- P-23: Integration testler — Testcontainers
- P-24: OpenTelemetry — trace, metric, Prometheus + Grafana
- P-25: Rate limiting + YARP API Gateway

## Önemli Mimari Kararlar
1. ITokenService → Application katmanında (Infrastructure→Application dependency yönü korundu)
2. EventSearchDocument → Application katmanında (aynı sebep)
3. Minimal API + IEndpoint pattern (controller yok)
4. Read → PG Replica, Write → Primary (CQRS ile ayrım)
5. EF local tool: dotnet-ef 8.0.11 (.config/dotnet-tools.json)
6. RuntimeFrameworkVersion: 8.0.26 (preview SDK uyumu)
7. Primary constructor ve [] collection expression kullanılmadı (SDK uyumu)
8. Debezium connector PowerShell ile register edildi (Windows, WSL yok)

## Servis Portları (Docker)
| Servis        | Port  | URL                        |
|---------------|-------|----------------------------|
| API           | 5157  | http://localhost:5157      |
| PostgreSQL    | 5432  | localhost:5432             |
| Redis         | 6379  | localhost:6379             |
| Kafka         | 9092  | localhost:9092             |
| Kafka UI      | 8080  | http://localhost:8080      |
| Elasticsearch | 9200  | http://localhost:9200      |
| Kibana        | 5601  | http://localhost:5601      |
| Debezium      | 8083  | http://localhost:8083      |

## Kafka Topic'leri
| Topic                  | Kullanım                    |
|------------------------|-----------------------------|
| biletix.public.Events  | CDC — PG→ES sync            |
| biletix.notifications  | Outbox → Notification       |
| biletix.outbox         | Diğer eventler              |

## Önemli Notlar
- appsettings.json Stripe key içeriyor — Git'e ATMA
- stripe listen her testte yeniden başlatılmalı (whsec_ değişiyor)
- Debezium connector her docker-compose restart'ta yeniden register edilmeli
- Test verileri her testten sonra temizlendi
- Admin seed uygulama başlangıcında otomatik çalışıyor

## Kalan İş (7 adım)
P-19 → P-20 → P-21 → P-22 → P-23 → P-24 → P-25
