# Tài Liệu Tích Hợp External API - DMX (Thế Giới Di Động)

## Tổng Quan

**DMX** là hệ thống ERP của **Thế Giới Di Động (MWG)**. AEV Integration Services gọi DMX API để đẩy cập nhật trạng thái warranty về hệ thống của Thế Giới Di Động sau khi GCC hoàn thành xử lý.

### Kiến Trúc Tích Hợp

```
AEV System
    │
    └─► HttpClient (Infrastructure layer)
            │
            ├─ Step 1: TokenIssue (Zuul)
            │   POST /api/Authentication/TokenIssue
            │   Header: ReverseHost
            │   Body: { ClientID, UserName, PasswordDoubleMd5 }
            │
            ├─ Token Cache (5 phút, thread-safe SemaphoreSlim)
            │
            ├─ Step 2: Build AuthenData
            │   authStr = Base64(tokenString) + "|" + "yyyyMMdd.HHmm"
            │   AuthenData = RSA_Encrypt(authStr, publicKey)
            │
            └─ Step 3: Update Warranty Status (Zuul)
                POST /api/TanTam/UpdateWarrantyReqStatus
                Headers: RequestID, ClientID, ReverseHost, AuthenData
```

---

## Cấu Hình (appsettings)

```json

```

> ⚠️ `Password` lưu dưới dạng **plain text** trong config. Khi gọi API sẽ được hash **MD5 hai lần** (double MD5).

| Môi trường  | BaseUrl                       |
|-------------|-----------------------------------|
| UAT/Beta    | `https://betazuulv2.tgdd.vn`     |
| Production  | `https://zuulv2.tgdd.vn`         |

---

## Authentication - 2 Bước

### Bước 1: Lấy Token (TokenIssue)

**Endpoint:**
```
POST {BaseUrl}/api/Authentication/TokenIssue
```

**Request Headers:**
```
Content-Type: application/json
ReverseHost: <ReverseHost>
```

**Request Body:**
```json
{
  "ClientID": "3cb946ee-6b01-4994-8a79-0997237dde60",
  "UserName": "aqua2023",
  "PasswordDoubleMd5": "e10adc3949ba59abbe56e057f20f883e"
}
```

> **PasswordDoubleMd5** = `MD5(MD5(plain_password))` — hash hex lowercase 2 lần

| Trường            | Mô tả                                                            |
|-------------------|------------------------------------------------------------------|
| `ClientID`        | Client ID cấp bởi DMX                                           |
| `UserName`        | Tên tài khoản DMX                                               |
| `PasswordDoubleMd5`| Password sau khi hash MD5 hai lần (hex lowercase)               |

**Response thành công:**
```json
{
  "IsError": false,
  "StatusID": 1,
  "Message": "Success",
  "MessageDetail": null,
  "TokenString": "eyJVc2VyTmFtZSI6ImFxdWEyMDIzIiwiUGFydG5l..."
}
```

**Response lỗi:**
```json
{
  "IsError": true,
  "StatusID": 2,
  "Message": "Authentication failed",
  "MessageDetail": "Invalid username or password"
}
```

| Trường          | Kiểu      | Mô tả                                        |
|-----------------|-----------|----------------------------------------------|
| `IsError`       | `boolean` | `true` nếu có lỗi                           |
| `StatusID`      | `integer` | Mã status (1 = success)                      |
| `Message`       | `string`  | Thông báo kết quả                            |
| `MessageDetail` | `string`  | Chi tiết lỗi (nếu có)                        |
| `TokenString`   | `string`  | **Token dùng để build `AuthenData`**          |

### Bước 2: Build AuthenData (RSA Encryption)

Mỗi request đến Zuul API cần header `AuthenData` được tạo theo quy trình:

```
1. tokenBase64  = Base64Encode(UTF8(tokenString))
2. timestamp    = DateTime.Now.ToString("yyyyMMdd.HHmm")   ← Local time
3. authStr      = tokenBase64 + "|" + timestamp
4. AuthenData   = RSA_Encrypt(authStr, rsaPublicKey, keySize=512)
```

> ⚠️ DMX yêu cầu **local time** cho timestamp (không phải UTC).

**Request Headers cho mọi Zuul API call:**
```
Content-Type: application/json
RequestID: <new_guid>
ClientID: <ClientID>
ReverseHost: <ReverseHost>
AuthenData: <rsa_encrypted_auth_data>
```

### Token Caching

- Token được cache in-memory **5 phút** (theo `TokenValidityMinutes`)
- Thread-safe với `SemaphoreSlim` + double-check locking
- **Auto-retry**: Nếu server trả `StatusID = 13` (token expired) → clear cache → gọi lại TokenIssue → retry request

---

## API: Update Warranty Status

### Endpoint

```
POST {BaseUrl}/api/TanTam/UpdateWarrantyReqStatus
```

### Request Headers

```
Content-Type: application/json
RequestID: <new_guid>
ClientID: <ClientID>
ReverseHost: <ReverseHost>
AuthenData: <rsa_encrypted_auth_data>
```

### Request Body

```json
{
  "RetailWarrantyRequestID": "TGDD-WR-2024-001",
  "Content": "Hoàn thành sửa chữa. Thay thế motor.",
  "WarrantyHomeStatusID": "4",
  "OutRepairCost": 0,
  "WarrantArrivalDate": "20240601080000",
  "AppointmentDate": "20240605090000",
  "FeedBackID": "12",
  "SolutionID": "31",
  "JobIDOfPartner": "AEV-2024-001",
  "SymptomID": ""
}
```

### Mô Tả Các Trường Request

| Trường                   | Kiểu      | Mô tả                                                                     |
|--------------------------|-----------|---------------------------------------------------------------------------|
| `RetailWarrantyRequestID`| `string`  | ID yêu cầu bảo hành phía DMX/TGDD (thường là `ExternalReferenceId`)       |
| `Content`                | `string`  | Nội dung cập nhật / mô tả công việc đã thực hiện                          |
| `WarrantyHomeStatusID`   | `string`  | Mã trạng thái bảo hành (xem bảng bên dưới)                               |
| `OutRepairCost`          | `integer` | Chi phí sửa chữa ngoài bảo hành (0 = không có)                           |
| `WarrantArrivalDate`     | `string`  | Ngày kỹ thuật viên đến, **format: `yyyyMMddHHmmss`**                      |
| `AppointmentDate`        | `string`  | Ngày hẹn, **format: `yyyyMMddHHmmss`**. Nếu trong quá khứ → dùng +8h UTC|
| `FeedBackID`             | `string`  | Mã phản hồi                                                               |
| `SolutionID`             | `string`  | Mã giải pháp xử lý                                                        |
| `JobIDOfPartner`         | `string`  | Job ID phía AEV (thường là `RequestNumber`)                                 |
| `SymptomID`              | `string`  | Mã triệu chứng lỗi                                                        |

> **Lưu ý về datetime**: Tất cả datetime fields dùng **format `yyyyMMddHHmmss`** (không có dấu phân cách).  
> Nếu `AppointmentDate` trong quá khứ, hệ thống tự động dùng `DateTime.UtcNow + 8 giờ`.

### Response

**Thành công:**
```json
{
  "IsError": false,
  "StatusID": 1,
  "Message": "Update successful",
  "MessageDetail": null,
  "MessageDetailSVC": null,
  "ResultObject": null,
  "ResponseID": "RSP-20240601-001"
}
```

**Token hết hạn (sẽ auto-retry):**
```json
{
  "IsError": true,
  "StatusID": 13,
  "Message": "Token expired",
  "MessageDetail": "Please re-authenticate",
  "MessageDetailSVC": null,
  "ResultObject": null,
  "ResponseID": null
}
```

**Lỗi business:**
```json
{
  "IsError": true,
  "StatusID": 4,
  "Message": "RetailWarrantyRequestID not found",
  "MessageDetail": "TGDD-WR-2024-001 does not exist",
  "MessageDetailSVC": null,
  "ResultObject": null,
  "ResponseID": null
}
```

### Mô Tả Các Trường Response

| Trường             | Kiểu      | Mô tả                                                                     |
|--------------------|-----------|---------------------------------------------------------------------------|
| `IsError`          | `boolean` | `false` = thành công, `true` = có lỗi                                   |
| `StatusID`         | `integer` | Mã kết quả. `13` = token hết hạn (auto-retry)                            |
| `Message`          | `string`  | Thông báo ngắn gọn                                                        |
| `MessageDetail`    | `string`  | Chi tiết lỗi (nếu `IsError = true`)                                      |
| `MessageDetailSVC` | `string`  | Chi tiết lỗi từ service layer (nếu có)                                   |
| `ResultObject`     | `object`  | Dữ liệu trả về (thường null)                                             |
| `ResponseID`       | `string`  | ID giao dịch phía DMX (dùng để tra cứu log)                              |

---

## Xử Lý Lỗi & Retry

| Tình huống                    | Hành vi của AEV                                                  |
|-------------------------------|------------------------------------------------------------------|
| `StatusID = 13`               | Clear token cache → gọi lại TokenIssue → retry UpdateStatus     |
| `IsError = true` (other)      | Log warning, trả về response (không throw exception)             |
| TokenIssue `IsError = true`   | Throw `InvalidOperationException` với message từ DMX             |
| TokenIssue trả về null        | Throw `InvalidOperationException("DMX TokenIssue API returned null response")` |
| `TokenString` rỗng            | Throw `InvalidOperationException("DMX TokenIssue returned empty tokenString")` |
| HTTP 401                      | Throw `UnauthorizedAccessException`                              |
| Null response                 | Throw `InvalidOperationException` với `RetailWarrantyRequestID`  |

---

## Luồng Hoàn Chỉnh

```
AEV UpdateWarrantyStatus
    │
    ├─► Kiểm tra token cache còn hạn?
    │       ├─ Còn hạn ──────────────────────────────────────────────────────┐
    │       └─ Hết hạn / chưa có                                             │
    │               │                                                        │
    │               ▼                                                        │
    │         SemaphoreSlim.WaitAsync()                                      │
    │               │                                                        │
    │               ├─ Double-check cache                                    │
    │               └─ Gọi POST /api/Authentication/TokenIssue               │
    │                       │                                                │
    │                       ▼                                                │
    │                 Cache tokenString (5 phút)                             │
    │                                                                        │
    ▼                                                                        │
Build AuthenData:                                                            │
    Base64(tokenString) + "|" + "yyyyMMdd.HHmm" ──► RSA Encrypt ◄───────────┘
    │
    ▼
POST /api/TanTam/UpdateWarrantyReqStatus
    │
    ├─ StatusID = 13 ──► Clear cache ──► Retry (1 lần)
    └─ Thành công ──► Return response
```

---