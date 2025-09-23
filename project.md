# 🛟 Safe Harbour – Project Documentation

> **Version:** 1.0
> **Platform:** C# .NET 8.0
> **Author:** Adekunle Adeniyi
> **Purpose:** A two-sided service marketplace platform connecting clients with service workers, with scheduling, payments, and escrow protection.

---

## 🧭 Overview

**Safe Harbour** is a secure marketplace platform that connects **Clients** with skilled **Service Workers** (e.g., care workers, snow plowers, cooks, cleaners). The system supports job postings, direct hiring, payment via Stripe (including escrow-like fund holding), service scheduling, and an admin dashboard for system monitoring.

---

## 👥 User Roles

| Role               | Description                                                         |
| ------------------ | ------------------------------------------------------------------- |
| **Client**         | Posts jobs, hires workers, pays for services, manages engagements   |
| **Service Worker** | Browses/apply for jobs, manages service schedules, receives payouts |
| **Admin**          | Views system metrics, manages disputes, oversees compliance         |

---

## 🔄 Core User Flows

### 🔹 Client

* Create and manage job postings
* Accept job applications
* Hire workers directly
* View and manage services by status (pending, accepted, etc.)
* Attach payment methods (Stripe)
* Pay for services (escrow model)

### 🔹 Service Worker

* Browse/filter jobs (by service, location)
* Apply for jobs
* View job history
* Manage availability via service schedule
* Receive payouts via Stripe Connect

### 🔹 Super Admin

* Monitor all jobs, users, payments
* Access analytics & metrics
* Manage disputes and user reports

---

## 💳 Wallet & Payments (Stripe)

### Model: Escrow-Like Protection

* **Client pays** at acceptance or hire time
* **Funds are held** (via Stripe PaymentIntent - manual capture)
* **Funds are released** to Service Worker upon job completion
* **Stripe Connect Express** used for Service Worker payouts
* **Refunds** or **disputes** managed by Admin

---

## 🧱 Project Structure
> Follow **Clean Architecture** & **DDD**: Data → Application → Infrastructure → API.

---

## 🧾 Key Domain Entities

* **User** (Client or Service Worker)
* **Service** (e.g., Care, Cook, etc.)
* **Job** (Posted by Client)
* **Application** (Worker → Job)
* **Schedule** (Worker availability)
* **Payment** (Job payment, escrowed)
* **Transfer** (Worker payout via Stripe)
* **Review** (Rating & feedback)
* **Dispute** (Admin oversight)

---

## 📡 API Endpoints (High-Level)

### ✅ Client

* `POST /jobs` – Create Job
* `GET /jobs?status=` – View Posted Jobs
* `POST /jobs/{jobId}/applications/{applicationId}/accept`
* `POST /hires` – Direct Hire
* `POST /wallet/cards` – Add Card
* `POST /jobs/{jobId}/complete` – Complete Job

### ✅ Service Worker

* `GET /jobs` – Filterable Job List
* `POST /jobs/{jobId}/applications`
* `GET /jobs/history`
* `POST /schedules`
* `GET/PUT/DELETE /schedules/{id}`

### ✅ Admin

* `GET /metrics`
* `GET /disputes`
* `POST /disputes/{id}/resolve`
* `GET /users?role=`

---

## 🔁 Payment Workflow

1. Client accepts application or hires worker → Stripe PaymentIntent created
2. Funds are **authorized** (held) or **captured immediately** based on policy
3. Upon service completion:

   * **Manual capture** → `payment_intent.capture()`
   * **Payout** → Stripe Connect Transfer to Service Worker
4. Dispute or cancellation → refunds managed

---

## 📊 Admin Metrics

* **Marketplace Metrics**: Total GMV, Jobs filled, Acceptance Rate
* **Operations**: Cancellation Rate, Disputes, Completion Time
* **Finance**: Authorized vs Captured payments, Stripe Fees, Refunds
* **Users**: Active Workers, Repeat Clients

---

## 🛠️ Developer Setup

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Clone the repo and restore NuGet packages
3. Set up environment variables:

   * `Stripe__ApiKey`
   * `Stripe__WebhookSecret`
   * `ConnectionStrings__Default`
4. Apply EF Core migrations:

   ```bash
   dotnet ef database update --project SafeHabour.API
   ```
5. Run with:

   ```bash
   dotnet run --project SafeHabour.API
   ```

---

## 🧪 Testing

* Unit Tests in `/tests/UnitTests`
* Integration Tests in `/tests/IntegrationTests`
* Stripe webhook test support available using `stripe listen` CLI

---

## 🔐 Security & Compliance

* Role-based policies via `[Authorize(Policy = "AdminOnly")]`
* Stripe PCI-compliant vaulting (no card info stored)
* Idempotency keys for payments
* Audit trail for state transitions and fund movements

---

## 💡 Ideas for Future Sprints

* Push notifications (e.g., Twilio or Firebase)
* ServiceWorker background check/KYC
* Ratings & reputation algorithm
* Mobile app integration
* Auto-payout schedule (daily/weekly via Stripe)