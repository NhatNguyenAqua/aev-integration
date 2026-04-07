# Tài Liệu Tích Hợp External API - GCC (Haier)

## Tổng Quan

**GCC** là hệ thống quản lý warranty/service của **Haier**. AEV Integration Services sử dụng GCC API để:
- Tạo Case bảo hành mới cho khách hàng
- Tra cứu thông tin Case
- Truy vấn trạng thái Work Order
- Đẩy thông tin vận chuyển (Transportation Info)

### Kiến Trúc Tích Hợp

```
AEV System
    │
    ├─► HttpClient (Infrastructure layer)
    │       │
    │       ├─ Token Cache (Thread-safe, per ClientType)
    │       │   ├─ spare part token──► Azure AD Tenant A(Waybill)
    │       │   └─ call center token ──► Azure AD Tenant B(servicequest)
    │       │
    │       └─ HTTP Calls (Bearer token)
    │           ├─ POST /Client/api/case/createforvn(call center token)
    │           ├─ GET  /Client/api/case/getcaseinfobycasenumber(call center token)
    │           ├─ POST /Client/api/workerorder/statusquery(call center token)
    │           └─ POST /Client/api/Best/TransportationInfo(spare part token)
    │
    └─► 
```

---

## Cấu Hình (appsettings)

```json
Get token (nhờ bạn tự thiết kế)
{
  "ClientId": "<azure_client_id_transportation>",
  "ClientSecret": "<azure_client_secret_transportation>",
  "Scope": "api://clientapi-gccprod/.default",
  "GrantType": "client_credentials",
  "TokenEndpoint": "https://login.microsoftonline.com/<tenant_id_a>/oauth2/v2.0/token"
}
```

| Môi trường  | BaseUrl                              |
|-------------|--------------------------------------|
| UAT         | `https://gcc-uat-outer.haier.net`    |
| Production  | `https://gcc-prd-outer.haier.net`    |

---

## Authentication - OAuth2 Client Credentials

GCC sử dụng **Azure AD / Microsoft Identity Platform** với flow `client_credentials`.  
AEV duy trì **2 token độc lập** theo `ClientType`:

| ClientType       | Dùng cho                              |
|------------------|---------------------------------------|
| `call center token`| Create Case, Get Case, Work Order     |
| `spare part token` | Transportation Info                   |

### Lấy Token

**Endpoint:** `POST {TokenEndpoint}` (Microsoft OAuth2 endpoint)

**Request (form-urlencoded):**
```
client_id=<client_id>
client_secret=<client_secret>
scope=<scope>
grant_type=client_credentials
```

**Response:**
```json
{
  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6...",
  "expires_in": 3599,
  "token_type": "Bearer"
}
```

### Token Caching

- Token được cache in-memory theo `ClientType`, tự động refresh khi hết hạn
- Buffer an toàn: **60 giây** trước khi hết hạn sẽ tự refresh
- Thread-safe với **double-check locking** + `SemaphoreSlim`
- Tất cả GCC API calls dùng header: `Authorization: Bearer <access_token>`

---

## API 1: Tạo GCC Case

### Endpoint

```
POST {BaseUrl}/Client/api/case/createforvn
GET  {BaseUrl}/Client/api/case/getcaseinfobycasenumber
POST {BaseUrl}/Client/api/workerorder/statusquery
POST {BaseUrl}/Client/api/Best/TransportationInfo
```

**ClientType:** `WarrantyRequest`

### Request Headers

```
Authorization: Bearer <access_token>
Content-Type: application/json
```

### Request Body

```json
{
  "SourceChannel": 35,(DMX:35, WARRANTY:21, ZALO:22, COCA:25, ZALOMINIAPP:42)
  "ChannelId": "{RequestNumber}",
  "ServiceType": "Customer Service",
  "SerialNumber": "SN20240601001",
  "ProductCategory": "Washing Machine",
  "ProductModel": "HW80-BP14979",
  "PurchaseDate": "2024-01-15",
  "DealerName": "Coke Store HCM",
  "DeliveryDate": "2024-01-20",
  "PreferredVisitDate": "2024-06-05T09:00:00",
  "PreferredVisitTime": "09:00-17:00",
  "IssueDescription": "Máy không hoạt động, không lên nguồn",
  "FirstName": "Văn A",
  "LastName": "Nguyễn",
  "MobilePhone": "0901234567",
  "OtherPhone": "0907654321",
  "Email": "customer@example.com",
  "LocationPinCode": null,-> Address
  "LocationStateCode": "HCM",
  "LocationStateName": "TP. Hồ Chí Minh",
  "LocationCityName": "Quận 1",
  "LocationCityCode": "Q1",
  "LocationLocalityCode": "BN",
  "LocationLocalityName": "Bến Nghé",
  "DetailedAddress": "123 Lê Lợi, P. Bến Nghé, Q.1, TP.HCM",
  "Remark": "Khách phản ánh máy đột ngột tắt",
  "CountryCode": "VN",
  "ACOutdoorSN": null,
  "Jobcardid": "{ExternalReferenceId}"
}
```

### Mô Tả Các Trường Request

| Trường                | Kiểu       | Bắt buộc | Mô tả                                                                   |
|-----------------------|------------|----------|-------------------------------------------------------------------------|
| `SourceChannel`       | `integer`  | ✅ Có    | Mã kênh nguồn (vd: `35` cho Coca Cola). Phải khác `0`                  |
| `ChannelId`           | `string`   | ✅ Có    | ID định danh case phía AEV (thường là `RequestNumber`)                    |
| `ServiceType`         | `string`   | ✅ Có    | Loại dịch vụ (vd: `"Customer Service"`)                                 |
| `SerialNumber`        | `string`   | Không    | Serial number thiết bị                                                  |
| `ProductCategory`     | `string`   | ✅ Có    | Danh mục sản phẩm                                                       |
| `ProductModel`        | `string`   | Không    | Model sản phẩm                                                          |
| `PurchaseDate`        | `string`   | ✅ Có    | Ngày mua, **format: `yyyy-MM-dd`**                                      |
| `DealerName`          | `string`   | Không    | Tên đại lý / cửa hàng                                                   |
| `DeliveryDate`        | `string`   | Không    | Ngày giao hàng                                                          |
| `PreferredVisitDate`  | `datetime` | ✅ Có    | Ngày hẹn kỹ thuật viên, **ISO 8601**                                    |
| `PreferredVisitTime`  | `string`   | ✅ Có    | Khung giờ hẹn (vd: `"09:00-17:00"`)                                    |
| `IssueDescription`    | `string`   | ✅ Có    | Mô tả vấn đề / lỗi thiết bị                                             |
| `FirstName`           | `string`   | ✅ Có    | Tên khách hàng                                                          |
| `LastName`            | `string`   | ✅ Có    | Họ khách hàng                                                           |
| `MobilePhone`         | `string`   | ✅ Có    | Số điện thoại di động                                                   |
| `OtherPhone`          | `string`   | Không    | Số điện thoại phụ                                                       |
| `Email`               | `string`   | Không    | Email khách hàng                                                        |
| `LocationPinCode`     | `string`   | ✅ Có    | Mã bưu điện                                                             |
| `LocationStateCode`   | `string`   | ✅ Có    | Mã tỉnh/thành phố                                                       |
| `LocationStateName`   | `string`   | ✅ Có    | Tên tỉnh/thành phố                                                      |
| `LocationCityName`    | `string`   | ✅ Có    | Tên quận/huyện                                                          |
| `LocationCityCode`    | `string`   | ✅ Có    | Mã quận/huyện                                                           |
| `LocationLocalityCode`| `string`   | ✅ Có    | Mã phường/xã                                                            |
| `LocationLocalityName`| `string`   | ✅ Có    | Tên phường/xã                                                           |
| `DetailedAddress`     | `string`   | ✅ Có    | Địa chỉ chi tiết đầy đủ                                                 |
| `Remark`              | `string`   | Không    | Ghi chú thêm                                                            |
| `CountryCode`         | `string`   | ✅ Có    | Mã quốc gia (vd: `"VN"`)                                               |
| `ACOutdoorSN`         | `string`   | Không    | Serial number dàn nóng điều hòa (nếu có)                               |
| `Jobcardid`           | `string`   | ✅ Có    | Job Card ID (thường trùng với `ExternalReferenceId`)              |

### Response

**Thành công:**
```json
{
  "Result": true,
  "Description": "Success",
  "Data": {
    "ChannelId": "AEV-2024-001",
    "CaseNumber": "CAS-20240601-001"
  }
}
```

**Thất bại:**
```json
{
  "Result": false,
  "Description": "Duplicate ChannelId",
  "Data": null
}
```

### Mô Tả Các Trường Response

| Trường            | Kiểu      | Mô tả                                           |
|-------------------|-----------|-------------------------------------------------|
| `Result`          | `boolean` | `true` nếu tạo case thành công                 |
| `Description`     | `string`  | Thông báo kết quả từ GCC                        |
| `Data.ChannelId`  | `string`  | Channel ID đã gửi                               |
| `Data.CaseNumber` | `string`  | **GCC Case Number** — lưu lại để dùng sau       |

> ⚠️ Nếu `Result = false`, AEV sẽ throw exception với message từ `Description`.

---

## API 2: Lấy Thông Tin Case

### Endpoint

```
GET {BaseUrl}/Client/api/case/getcaseinfobycasenumber?caseNumber={caseNumber}
```

**ClientType:** `WarrantyRequest`

### Request Headers

```
Authorization: Bearer <access_token>
```

### Request Parameters

| Parameter    | Kiểu     | Bắt buộc | Mô tả              |
|--------------|----------|----------|--------------------|
| `caseNumber` | `string` | ✅ Có    | GCC Case Number    |

### Response

```json
{
  "Data": {
    "CaseStatusCode": 2,
    "CaseStatus": "InProgress",
    "WorkOrderNumber": "WO-20240601-001",
    "CustomerDescription": "Khách phản ánh máy đột ngột tắt",
    "JobCardId": "AEV-2024-001",
    "FinishTime": null
  }
}
```

### Mô Tả Các Trường Response

| Trường                | Kiểu       | Mô tả                                                  |
|-----------------------|------------|--------------------------------------------------------|
| `CaseStatusCode`      | `integer`  | Mã trạng thái case                                     |
| `CaseStatus`          | `string`   | Tên trạng thái case                                    |
| `WorkOrderNumber`     | `string`   | Mã Work Order (nếu đã được assign kỹ thuật viên)       |
| `CustomerDescription` | `string`   | Mô tả vấn đề từ khách hàng                             |
| `JobCardId`           | `string`   | Job Card ID tương ứng                                  |
| `FinishTime`          | `datetime` | Thời gian hoàn thành (null nếu chưa xong)              |

> **Lưu ý:** Nếu `WorkOrderNumber` không null → gọi tiếp **API Work Order Status** để lấy trạng thái chi tiết hơn.

---

## API 3: Truy Vấn Trạng Thái Work Order

### Endpoint

```
POST {BaseUrl}/Client/api/workerorder/statusquery
```

**ClientType:** `WarrantyRequest`

### Request Headers

```
Authorization: Bearer <access_token>
Content-Type: application/json
```

### Request Body

```json
{
  "WorkOrderNumber": "WO-20240601-001",
  "ApplyId": "AEV-2024-001",
  "PhoneNumber": "0901234567"
}
```

### Mô Tả Các Trường Request

| Trường            | Kiểu     | Bắt buộc | Mô tả                     |
|-------------------|----------|----------|---------------------------|
| `WorkOrderNumber` | `string` | ✅ Có    | Mã Work Order từ GCC      |
| `ApplyId`         | `string` | ✅ Có    | Job Card ID / Channel ID  |
| `PhoneNumber`     | `string` | ✅ Có    | Số điện thoại khách hàng  |

### Response

```json
{
  "Data": {
    "WorkOrderNumber": "WO-20240601-001",
    "Status": 4,
    "SubStatus": 1,
    "SubStatusComment": 0,
    "ChangeTime": 1717200000,
    "StatusDescription": "Completed",
    "Remark": "",
    "CustomerName": "Nguyễn Văn A",
    "IssueDescrtption": "Máy không hoạt động",
    "ModelType": "HW80-BP14979",
    "SN": "SN20240601001",
    "Address": "123 Lê Lợi, Q.1, TP.HCM",
    "Brand": "Haier",
    "ServiceClass": "In-Warranty",
    "ServiceType": "Customer Service",
    "ProductWarranty": "Yes",
    "RMAStatus": "",
    "PhoneNumbe": "0901234567",
    "ProductGroup": "Washing Machine",
    "PurchaseDate": "01/15/2024 00:00:00",
    "TechnicianPhoneNumber": "0987654321",
    "TechnicianName": "Kỹ Thuật Viên A",
    "ChannelId": "AEV-2024-001",
    "AppointmentTime": "06/05/2024 09:00:00",
    "SubStatusCommentText": "",
    "FinishTime": "06/05/2024 14:30:00",
    "FirstFixTime": "",
    "DefectDescription": "Lỗi motor",
    "ServiceDescription": "Thay thế motor mới",
    "IsCustomerDealerMishandling": false,
    "DefectPart": "Motor"
  }
}
```

### Mô Tả Các Trường Response

| Trường                         | Kiểu       | Mô tả                                                                     |
|--------------------------------|------------|---------------------------------------------------------------------------|
| `WorkOrderNumber`              | `string`   | Mã Work Order                                                             |
| `Status`                       | `integer`  | Mã trạng thái chính                                                       |
| `SubStatus`                    | `integer`  | Mã trạng thái phụ                                                         |
| `ChangeTime`                   | `integer`  | Unix timestamp (seconds) thời điểm thay đổi cuối                         |
| `StatusDescription`            | `string`   | Mô tả trạng thái                                                          |
| `IssueDescrtption`             | `string`   | ⚠️ **Typo của GCC API** (thiếu chữ `i`) — Mô tả vấn đề                   |
| `PhoneNumbe`                   | `string`   | ⚠️ **Typo của GCC API** (thiếu chữ `r`) — Số điện thoại khách hàng       |
| `AppointmentTime`              | `datetime` | Thời gian hẹn, **format GCC: `MM/dd/yyyy HH:mm:ss`**                     |
| `FinishTime`                   | `datetime?`| Thời gian hoàn thành (nullable), **format GCC: `MM/dd/yyyy HH:mm:ss`**   |
| `PurchaseDate`                 | `string`   | Ngày mua, **format GCC: `MM/dd/yyyy HH:mm:ss`** — giữ nguyên dạng string |
| `TechnicianName`               | `string`   | Tên kỹ thuật viên được assign                                             |
| `TechnicianPhoneNumber`        | `string`   | Số điện thoại kỹ thuật viên                                               |
| `DefectDescription`            | `string`   | Mô tả lỗi được xác định                                                   |
| `ServiceDescription`           | `string`   | Mô tả công việc sửa chữa đã thực hiện                                    |
| `IsCustomerDealerMishandling`  | `boolean`  | Khách hàng / đại lý sử dụng sai hay không                                |

> ⚠️ **Lưu ý quan trọng**: GCC API có 2 typo trong field name:  
> - `IssueDescrtption` (thiếu `i`) — JSON property mapping: `[JsonPropertyName("IssueDescrtption")]`  
> - `PhoneNumbe` (thiếu `r`) — JSON property mapping: `[JsonPropertyName("PhoneNumbe")]`

---

## API 4: Tạo Transportation Info

### Endpoint

```
POST {BaseUrl}/Client/api/Best/TransportationInfo
```

**ClientType:** `Transportation`

### Request Headers

```
Authorization: Bearer <access_token>
Content-Type: application/json
```

### Request Body

```json
[
  {
    "OrderNumber": "PL-001",
    "Plant": "ORD-20240601-001",
    "WaybillNo": "BEST-WB-001",
    "TransporterName": "800Best",
    "LogisticsStatus": "Delivered",
    "LogisticsReceiverName": "Nguyễn Văn A",
    "LogisticsDeliveryTime": "2024-06-01 08:00:00",
    "LogisticsArriveTime": "2024-06-01 14:00:00",
    "TransportationType": "Standard",
    "DeliveredTimeFor1st": "2024-06-01 14:00:00",
    "ItemList": [
      {
        "Qty": "2",
        "PartCode": "PART-001"
      }
    ]
  }
]
```

### Mô Tả Các Trường Request

| Trường                  | Kiểu     | Mô tả                                                        |
|-------------------------|----------|--------------------------------------------------------------|
| `OrderNumber`           | `string` | Plant Code                                                   |
| `Plant`                 | `string` | Shipping Order Number / Waybill Number                       |
| `WaybillNo`             | `string` | Waybill Number / Mã vận đơn                                  |
| `TransporterName`       | `string` | Tên đơn vị vận chuyển                                        |
| `LogisticsStatus`       | `string` | Trạng thái logistics                                         |
| `LogisticsReceiverName` | `string` | Tên người nhận                                               |
| `LogisticsDeliveryTime` | `string` | Thời gian xuất kho, **format: `yyyy-MM-dd HH:mm:ss`**        |
| `LogisticsArriveTime`   | `string` | Thời gian giao đến, **format: `yyyy-MM-dd HH:mm:ss`**        |
| `TransportationType`    | `string` | Phương thức vận chuyển                                       |
| `DeliveredTimeFor1st`   | `string` | Thời gian giao lần đầu, **format: `yyyy-MM-dd HH:mm:ss`**   |
| `ItemList`              | `array`  | Danh sách vật tư                                             |
| `ItemList[].Qty`        | `string` | Số lượng                                                     |
| `ItemList[].PartCode`   | `string` | Mã vật tư                                                    |

### Response

```json
{
  "Result": true,
  "Status": 200,
  "Description": "Success"
}
```

| Trường        | Kiểu      | Mô tả                           |
|---------------|-----------|---------------------------------|
| `Result`      | `boolean` | `true` nếu thành công          |
| `Status`      | `integer` | HTTP-like status code           |
| `Description` | `string`  | Thông báo kết quả               |

---

## Xử Lý Lỗi

---

## Tài Liệu Liên Quan
