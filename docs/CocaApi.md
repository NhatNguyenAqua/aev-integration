# Tài Liệu Tích Hợp External API - Coca Cola

## Tổng Quan

AEV Integration Services gọi **Coca Cola API** để callback cập nhật trạng thái ticket sau khi GCC hoàn thành xử lý warranty request.  
Đây là chiều **AEV → Coca** (outbound), ngược chiều so với việc Coca gửi request vào AEV.

### Kiến Trúc Tích Hợp

```

    │
    │  (Sau khi GCC cập nhật trạng thái)
    │
    └─► HttpClient (Infrastructure layer)
            │
            ├─ Auth: Basic Authentication
            │   Header: Authorization: Basic Base64(username:password)
            │
            └─ POST {CocaBaseUrl}/api/v1/update-ticket
                Body: { No, finishedTime, firstTimeFix, supplierRemarks }
```

---

## Cấu Hình (appsettings)

```json
{
  "Coca": {
    "BaseUrl": "https://cocacola.micxm.vn",
    "Username": "<coca_api_username>",
    "Password": "<coca_api_password>",
    "CocaUpdateTicketEndpoint": "/api/v1/update-ticket"
  }
}
```

| Môi trường  | BaseUrl                          |
|-------------|----------------------------------|
| UAT/Staging | `https://cocacola.micxm.vn`      |
| Production  | *(xác nhận với Coca Cola team)*  |

---

## Authentication - Basic Auth

Mọi request đến Coca API sử dụng **HTTP Basic Authentication**:

```
Authorization: Basic Base64("<username>:<password>")
```

**Ví dụ:**
```
username: coca_api
password: mypassword
→ Base64("coca_api:mypassword") = "xxxxxxxxxxxxxxxxxx"
→ Authorization: Basic xxxxxxxxxxxxxxxxxx
```

---

## API: Update Ticket

### Endpoint

```
POST {BaseUrl}/api/v1/update-ticket
```

### Request Headers

```
Authorization: Basic <base64_credentials>
Content-Type: application/json
```

### Request Body

```json
{
  "No": 12345,
  "finishedTime": "2024-06-05 14:30:00",
  "firstTimeFix": "Yes",
  "supplierRemarks": "Đã thay thế motor. Máy hoạt động bình thường."
}
```

### Mô Tả Các Trường Request

| Trường            | Kiểu      | Mô tả                                                                         |
|-------------------|-----------|-------------------------------------------------------------------------------|
| `No`              | `integer` | Mã ticket phía Coca Cola. Lấy từ `ExternalReferenceId` (phải parse được sang `int`) |
| `finishedTime`    | `string`  | Thời gian hoàn thành từ GCC Work Order. Nếu null → gửi chuỗi `"null"`        |
| `firstTimeFix`    | `string`  | Sửa thành công lần đầu hay không (từ GCC). Nếu null → gửi chuỗi `"null"`     |
| `supplierRemarks` | `string`  | Ghi chú từ nhà cung cấp / kỹ thuật viên. Nếu null → gửi chuỗi `"null"`      |

> ⚠️ **Lưu ý quan trọng**: Các trường optional khi null **KHÔNG gửi `null` JSON**, mà gửi chuỗi `"null"` (theo yêu cầu của Coca API).

> **`No` là `integer`**: `ExternalReferenceId` phía Coca phải là số nguyên hợp lệ (`int.TryParse`). Nếu không parse được → request sẽ thất bại.

### Response

**Thành công (HTTP 200, code = 200):**
```json
{
  "code": 200,
  "description": "Success",
  "response": {
    "message": "Ticket updated successfully",
    "data": {
      "No": 12345,
      "finishedTime": "2024-06-05 14:30:00",
      "firstTimeFix": "Yes",
      "supplierRemarks": "Đã thay thế motor. Máy hoạt động bình thường."
    }
  }
}
```

**Thất bại:**
```json
{
  "code": 400,
  "description": "Invalid ticket No",
  "response": null
}
```

### Mô Tả Các Trường Response

| Trường                      | Kiểu      | Mô tả                                                  |
|-----------------------------|-----------|--------------------------------------------------------|
| `code`                      | `integer` | HTTP-like status code. `200` = thành công             |
| `description`               | `string`  | Mô tả kết quả                                          |
| `response`                  | `object`  | Dữ liệu trả về (null nếu lỗi)                         |
| `response.message`          | `string`  | Thông báo kết quả chi tiết                             |
| `response.data` | `object` | Echo lại request đã gửi (để xác nhận)         |

---

## Xử Lý Lỗi

| Tình huống                    | Hành vi của AEV                                               |
|-------------------------------|---------------------------------------------------------------|
| Response null                 | Log error + throw `Exception("Failed to update Coca ticket")` |
| `code != 200`                 | Log error + throw `Exception("Failed to update Coca ticket")` |
| HTTP network error            | Exception được propagate lên caller                           |

---

## Tài Liệu Liên Quan

