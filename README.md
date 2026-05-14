# 🎫 Biletix — TicketMaster Clone

> Sistem tasarımından production-grade implementasyona: Biletix/TicketMaster'ın tam backend implementasyonu.

Bu proje, bir sistem tasarımı eğitimindeki Biletix konseptinin **gerçek koda dönüştürülmüş halidir**. Eğitimde yalnızca mimari tasarım yapıldı — bu repoda o tasarım uçtan uca implement edildi.

---

## 📐 Mimari

<img width="656" height="655" alt="image" src="https://github.com/user-attachments/assets/cc9732c5-3718-468e-94ff-017c4a8dee20" />

Sistem **Clean Architecture** prensiplerine göre katmanlı olarak tasarlandı. Her katmanın tek bir sorumluluğu var ve bağımlılıklar her zaman içe doğru akar:

```
API Gateway (.NET 8 Minimal API)
        ↓
Application Layer (CQRS + MediatR)
        ↓
Infrastructure Layer (EF Core, Redis, Kafka, ES, Stripe)
        ↓
PostgreSQL · Redis · Elasticsearch · Kafka
```

---

## 🏗️ Mimari Kararlar ve Nedenleri

### 1. Clean Architecture + CQRS
Domain hiçbir şeye bağımlı değil. Application sadece Domain'i biliyor. Bu sayede her katman bağımsız test edilebiliyor. CQRS ile okuma (Query) ve yazma (Command) işlemleri tamamen ayrıldı — read trafiği replica'ya, write trafiği primary'ye gidiyor.

### 2. Domain Driven Design
İş kuralları entity'lerin içinde yaşıyor. `Booking.Confirm()`, `Event.Publish()`, `TicketType.Reserve()` gibi metodlar hem iş kuralını uygulayıp hem de domain event fırlatıyor. Dışarıdan `booking.Status = Confirmed` yazmak mümkün değil — private setter bunu engeller.

### 3. CDC Pipeline (Change Data Capture)
PostgreSQL'deki her değişiklik **Debezium** tarafından WAL'dan okunuyor, **Kafka**'ya yazılıyor, oradan **Elasticsearch**'e senkronize ediliyor. Uygulama kodu bu süreci hiç bilmiyor. Yarın yeni bir consumer eklesen mevcut koda dokunmana gerek yok.

```
PostgreSQL WAL → Debezium → Kafka → ES Sync Consumer → Elasticsearch
```

### 4. Redis Distributed Lock
Aynı koltuğa aynı anda iki kişi basamaz. Redis `SET NX` (only if Not eXists) komutu ile atomik kilit alınıyor. Lua script ile "kilidi sadece sahibi açabilir" garantisi veriliyor.

```
SET ticket_lock:{ticketTypeId} {userId} NX EX 600
```

### 5. Outbox Pattern
Booking onaylandığında hem DB'ye yazılıyor hem de bildirim gönderilmeli. Bu iki işlemi atomik yapmak için **Outbox** pattern: her ikisi aynı DB transaction'ı içinde yapılıyor. Ayrı bir worker Outbox tablosunu okuyup Kafka'ya gönderiyor. Hiçbir bildirim kaybolmuyor.

```
BEGIN TRANSACTION
  UPDATE booking SET status = 'Confirmed'
  INSERT INTO outbox (event_type, payload) VALUES ('booking.confirmed', ...)
COMMIT
-- Outbox worker okur → Kafka'ya yayınlar → Email gönderilir
```

### 6. Saga Pattern
Ödeme akışı birden fazla servisi kapsıyor: rezervasyon → payment intent → Stripe onayı. Herhangi bir adım başarısız olursa **compensating transaction** devreye giriyor: lock serbest bırakılıyor, rezervasyon iptal ediliyor, kullanıcıya bildirim gönderiliyor.

### 7. Virtual Waiting Queue
Popüler konser bileti açıldığında anlık yüz binlerce istek gelir. **Redis SortedSet** ile kullanıcılara sıra numarası veriliyor. **SSE (Server-Sent Events)** ile sıra pozisyonu gerçek zamanlı olarak güncelleniyor. Booking Service'e sadece belirlenen slot kadar kullanıcı alınıyor.

```
ZADD queue:{eventId} {timestamp} {userId}   ← kuyruğa ekle
ZRANK queue:{eventId} {userId}              ← sıra pozisyonu
```

### 8. Idempotency
"Satın Al" butonuna iki kez basılırsa veya network retry olursa aynı sipariş iki kez oluşmamalı. Client her istek için UUID üretiyor, backend bu key'i Redis'te kontrol ediyor. Aynı key gelirse kayıtlı cevabı dönüyor, yeni işlem yapmıyor.

---

## 🛠️ Teknoloji Stack

| Teknoloji | Kullanım |
|-----------|---------|
| .NET 8 Minimal API | Web API, endpoint routing |
| EF Core 8 + PostgreSQL 16 | ORM, migrations, audit, soft delete |
| MediatR 12 | CQRS, pipeline behaviors |
| FluentValidation | Request validation |
| Redis 7 (StackExchange) | Distributed lock, cache, idempotency, queue |
| Apache Kafka + Debezium | CDC pipeline, event streaming |
| Elasticsearch 8 | Fuzzy search, filtering, sorting |
| Stripe.net | Payment intents, webhook handler |
| MailKit | Gmail SMTP, HTML email templates |
| QRCoder | JWT-signed QR ticket generation |
| BCrypt.Net | Password hashing (workFactor: 12) |
| Serilog | Structured logging |
| OpenTelemetry | Distributed tracing, metrics |
| Prometheus + Grafana | Metrics dashboard |
| Docker Compose | Local infrastructure (10 servis) |
| xUnit + FluentAssertions | Unit ve integration testler |
| Testcontainers | Gerçek DB ile integration test |

---

## ✅ Uygulanan Pattern'lar

| Pattern | Açıklama |
|---------|---------|
| Clean Architecture | Domain → Application → Infrastructure → API |
| CQRS | Command (write) ve Query (read) tamamen ayrı |
| Domain Driven Design | AggregateRoot, Domain Events, Value Objects |
| CDC (Change Data Capture) | DB değişikliklerini otomatik ES'e senkronize et |
| Distributed Lock | Redis SET NX ile race condition koruması |
| Outbox Pattern | DB + event publish atomik garantisi |
| Saga Pattern | Dağıtık işlemlerde compensating transactions |
| Idempotency | Çift tıklama / network retry koruması |
| Virtual Queue | Yüksek talep anında Redis SortedSet bekleme sırası |
| Soft Delete | IsDeleted global filter ile veri koruma |
| Refresh Token Rotation | Her refresh'te yeni token, eski iptal |

---

## 🔐 Güvenlik

- JWT access token (15 dk) + refresh token rotation (7 gün)
- BCrypt ile password hashing (workFactor: 12)
- Role-based authorization: Admin, Organizer, Customer
- Resource-based authorization: Organizer sadece kendi etkinliğini yönetir
- Rate limiting: endpoint bazlı (auth: 5/dk, booking: 10/dk, search: 30/dk)
- Security headers: X-Frame-Options, X-Content-Type-Options, Referrer-Policy
- Stripe webhook imza doğrulama (EventUtility.ConstructEvent)
- JWT imzalı QR bilet — sahte bilet üretilemiyor

---

## 📊 Test

```
Domain Tests:       27 ✓   (entity logic, state machine kuralları)
Application Tests:   9 ✓   (handler, validation behaviour)
Integration Tests:  20 ✓   (Testcontainers — gerçek PG + Redis)
─────────────────────────────────────────────────────────────────
Toplam:            56 ✓   — 0 Failed, 0 Skipped
```

---

## 🚀 Kurulum

### Gereksinimler
- .NET 9 SDK
- Docker Desktop

### Altyapıyı başlat

```bash
git clone https://github.com/[kullaniciadiniz]/biletix.git
cd biletix

docker-compose up -d
```

### Debezium connector kaydet (ilk kurulumda bir kez)

**Windows (PowerShell):**
```powershell
$body = Get-Content scripts/debezium-connector.json -Raw
Invoke-RestMethod -Method POST `
  -Uri http://localhost:8083/connectors `
  -ContentType "application/json" `
  -Body $body
```

**Linux/Mac:**
```bash
./scripts/register-debezium-connector.sh
```

### API'yi başlat

```bash
dotnet run --project src/Biletix.API
```

### Environment Variables

`src/Biletix.API/appsettings.json` dosyasını düzenle:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "Email": {
    "Username": "your@gmail.com",
    "Password": "gmail-app-password"
  }
}
```

### Varsayılan Admin Hesabı
Uygulama başladığında otomatik oluşturulur:
```
Email:    admin@biletix.com
Password: Admin123!
```

---

## 🐳 Docker Servisleri

| Servis | Port | URL |
|--------|------|-----|
| API | 5157 | http://localhost:5157 |
| PostgreSQL | 5432 | localhost:5432 |
| Redis | 6379 | localhost:6379 |
| Kafka | 9092 | localhost:9092 |
| Kafka UI | 8080 | http://localhost:8080 |
| Elasticsearch | 9200 | http://localhost:9200 |
| Kibana | 5601 | http://localhost:5601 |
| Debezium | 8083 | http://localhost:8083 |
| Prometheus | 9090 | http://localhost:9090 |
| Grafana | 3000 | http://localhost:3000 |

---

## 📁 Proje Yapısı

```
Biletix/
├── src/
│   ├── Biletix.Domain/
│   │   ├── Entities/        # User, Event, Venue, Booking, Ticket...
│   │   ├── Events/          # Domain events
│   │   └── Exceptions/      # DomainException, NotFoundException
│   │
│   ├── Biletix.Application/
│   │   ├── Common/          # Behaviours, Interfaces, Models
│   │   └── Features/        # Auth, Events, Bookings, Payments...
│   │
│   ├── Biletix.Infrastructure/
│   │   ├── Persistence/     # EF Core, Migrations, Configurations
│   │   ├── Redis/           # Lock, Cache, Queue servisleri
│   │   ├── Messaging/       # Kafka consumers, producers, workers
│   │   ├── Payment/         # Stripe entegrasyonu
│   │   └── Notifications/   # Email, QR ticket
│   │
│   └── Biletix.API/
│       ├── Features/        # Minimal API endpoints
│       └── Middleware/      # GlobalException, CorrelationId, Security
│
├── tests/
│   ├── Biletix.Domain.Tests/
│   ├── Biletix.Application.Tests/
│   └── Biletix.Integration.Tests/
│
├── monitoring/              # Prometheus + Grafana config
├── scripts/                 # Debezium connector setup
└── docker-compose.yml
```

---

## 🎯 Senaryo: Tarkan İstanbul Konseri

Sistemin nasıl çalıştığını uçtan uca görelim:

1. **Organizatör** konseri oluşturur → `status: Draft`
2. **Organizatör** yayınlar → `status: Published`, CDC tetiklenir, Elasticsearch'e index düşer
3. **Kullanıcı** "tarkan" arar → Elasticsearch fuzzy search, <500ms sonuç
4. **200.000 kullanıcı** aynı anda sisteme girer → Virtual Queue devreye girer
5. **Ahmet** sırasına gelir, 2 Standart bilet seçer
6. **Redis lock** alınır: `SET ticket_lock:standart:tarkan {ahmetId} NX EX 600`
7. **Mehmet** aynı bileti almaya çalışır → `422 "Başka biri rezerve ediyor"`
8. **Ahmet** ödeme yapar → Stripe PaymentIntent oluşturulur
9. **Stripe** ödemeyi onaylar → webhook → Booking `Confirmed`
10. **Outbox worker** → Kafka → email + JWT imzalı QR kod gönderilir
11. **Kapıda** QR okutulur → JWT verify → giriş kaydı oluşturulur

---

## 📈 Observability

| Araç | URL | Açıklama |
|------|-----|---------|
| Swagger UI | http://localhost:5157/swagger | API dokümantasyonu |
| Prometheus Metrics | http://localhost:5157/metrics | Raw metrikler |
| Grafana Dashboard | http://localhost:3000 | admin/biletix123 |
| Kibana | http://localhost:5601 | ES monitoring |
| Kafka UI | http://localhost:8080 | Topic monitoring |

---

## 📝 Lisans

MIT

---

*Bu proje bir sistem tasarımı eğitiminin implementasyonudur.*  
*Eğitim: [Sistem Tasarımı Masterclass][(link)](https://mvpakademi.com/egitimler)*
