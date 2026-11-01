# Payment Methods Integration Guide

This document explains how to use the payment methods functionality integrated into the POS system, including both Cash and Bakong KH QR payments.

## Overview

The POS system now supports multiple payment methods:
- **Cash**: Traditional cash payment
- **Bakong KH QR**: Digital payment via Bakong app QR code using the following configuration:
  - Merchant ID: `norm_soklim@bkrt`
  - Merchant Name: `NORM SOKLIM` (formatted as provided)
  - Merchant City: `PHNOMPENH` (formatted as required by EMV standard)
  - Currency: KHR (code 116)

## API Endpoints

### 1. Create Sale with Payment Method

**Endpoint:** `POST /api/sales`

**Authorization:** Required (CanSell policy)

**Request Body:**
```json
{
  "customerId": "customer-guid",
  "discount": 0.00,
  "tax": 0.00,
  "paymentMethod": "Cash",  // or "Bakong KH QR"
  "notes": "Sale notes",
  "saleItems": [
    {
      "productId": "product-guid",
      "quantity": 1,
      "unitPrice": 100.00
    }
  ]
}
```

**Response:**
```json
{
  "id": "sale-guid",
  "customerId": "customer-guid",
  "customerName": "Customer Name",
  "totalAmount": 100.00,
  "discount": 0.00,
  "tax": 0.00,
  "finalAmount": 100.00,
  "paymentMethod": "Cash",
  "status": "Completed",
  "saleDate": "2023-01-01T00:00:00Z",
  "notes": "Sale notes",
  "saleItems": [
    {
      "id": "sale-item-guid",
      "productId": "product-guid",
      "productName": "Product Name",
      "quantity": 1,
      "unitPrice": 100.00,
      "totalPrice": 100.00
    }
  ]
}
```

### 2. Generate Bakong Payment QR Code

**Endpoint:** `POST /api/bakong-payment/generate`

**Authorization:** Required

**Request Body:**
```json
{
  "merchantId": "norm_soklim@bkrt",
  "merchantName": "NORM SOKLIM",
  "amount": 100.00,
  "currency": "KHR",
  "description": "Payment for order #123",
  "referenceId": "ref123"
}
```

**Response:**
```json
{
  "success": true,
  "qrCodeText": "EMV QR code string",
  "qrCodeImage": "base64 encoded image",
  "message": "QR code generated successfully"
}
```

### 3. Get QR Code Image

**Endpoint:** `GET /api/bakong-payment/qr-code?data={qrCodeData}`

**Authorization:** Not required

### 4. Process Bakong Payment for Existing Sale

**Endpoint:** `POST /api/sales/{saleId}/process-bakong-payment`

**Authorization:** Required (CanSell policy)

**Request Body (optional):**
```json
{
  "description": "Payment for sale #123"
}
```

**Response:**
```json
{
  "success": true,
  "qrCodeText": "EMV QR code string",
  "qrCodeImage": "base64 encoded image",
  "message": "QR code generated successfully"
}
```

## Usage Examples

### Example 1: Create Sale with Cash Payment

```bash
curl -X POST "http://localhost:5055/api/sales" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "65e5041e-dc75-45b9-94b1-ee00b0e9b16d",
    "discount": 0.00,
    "tax": 0.00,
    "paymentMethod": "Cash",
    "notes": "Cash payment for goods",
    "saleItems": [
      {
        "productId": "86da8d34-0fc4-47bf-b740-10830c8de945",
        "quantity": 2,
        "unitPrice": 50.00
      }
    ]
  }'
```

### Example 2: Create Sale with Bakong Payment Method

```bash
curl -X POST "http://localhost:5055/api/sales" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "65e5041e-dc75-45b9-94b1-ee00b0e9b16d",
    "discount": 0.00,
    "tax": 0.00,
    "paymentMethod": "Bakong KH QR",
    "notes": "Bakong payment for goods",
    "saleItems": [
      {
        "productId": "86da8d34-0fc4-47bf-b740-10830c8de945",
        "quantity": 1,
        "unitPrice": 100.00
      }
    ]
  }'
```

### Example 3: Process Bakong Payment for Existing Sale

```bash
curl -X POST "http://localhost:5055/api/sales/12345678-1234-1234-1234-123456789012/process-bakong-payment" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Bakong payment for completed sale"
  }'
```

## Integration with Sales

The system allows you to specify the payment method when creating sales, or generate Bakong payment QR codes for existing sales. This is useful when a customer wants to pay for a completed sale using Bakong.

When processing a Bakong payment for an existing sale, the system automatically updates the sale's payment method to "Bakong KH QR".

## EMV QR Code Format

The generated QR codes follow the EMV QR Code specification for payment systems, ensuring compatibility with Bakong and other payment apps.

## Error Handling

- If the payment generation fails, the API returns a 400 Bad Request with an error message
- If the sale ID doesn't exist when processing a payment, the API returns a 404 Not Found
- Proper validation is performed on all input parameters
- Payment method must be either "Cash" or "Bakong KH QR"

## Security

- All payment generation endpoints require authentication
- The merchant ID and name are validated and secured
- Transaction references help track payments
- Payment methods are stored securely in the database

## Configuration

Before implementing the payment methods, you need to configure the following in your `appsettings.json` file:

```json
{
  "Bakong": {
    "MerchantId": "norm_soklim@bkrt",
    "MerchantName": "NORM SOKLIM",
    "MerchantCity": "PHNOMPENH",
    "Currency": "116",
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjp7ImlkIjoiOTAzNDQ5OWFkNWE1NDU2NSJ9LCJpYXQiOjE3NjcyODM0MDAsImV4cCI6MTc3NTA1OTQwMH0.Q6W8-QaqdjJUFd1vSqiOCgmWr3gFlzmvxXfduCp88oE"
  }
}
```

## Bakong API Configuration

The system is configured with the following Bakong API credentials:

- BAKONG_TOKEN: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjp7ImlkIjoiOTAzNDQ5OWFkNWE1NDU2NSJ9LCJpYXQiOjE3NjcyODM0MDAsImV4cCI6MTc3NTA1OTQwMH0.Q6W8-QaqdjJUFd1vSqiOCgmWr3gFlzmvxXfduCp88oE`
- BAKONG_ACCOUNT_ID: `norm_soklim@bkrt`
- BAKONG_MERCHANT_NAME: `NORM SOKLIM`
- BAKONG_MERCHANT_CITY: `Phnom Penh`
- BAKONG_CURRENCY: `116` (KHR)

## CORS Configuration

If you're using the test UI from a different origin (e.g., file:// or different port), you'll need to configure CORS in your `Program.cs`:

```csharp
// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500", "file://") // Add your frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Use CORS middleware (place before MapControllers)
app.UseCors("AllowSpecificOrigin");
```

For development purposes, you can allow all origins:

```csharp
// Add CORS services for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Use CORS middleware (place before MapControllers)
app.UseCors("AllowAll");
```

## Testing with UI

A test UI is available at `demos/bakong-payment-test.html` that allows you to:
- Create sales with Bakong payment method
- Generate Bakong payment QR codes
- Process Bakong payments for existing sales
- View QR code images

To use the test UI:
1. Ensure your API server is running
2. Configure CORS as described above
3. Open `demos/bakong-payment-test.html` in a web browser
4. Enter your API URL and authorization token
5. Test the various payment functions