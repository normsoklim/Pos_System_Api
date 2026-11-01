# Bakong Payment Integration Guide

This document explains how to use the Bakong payment functionality integrated into the POS system.

## Overview

The POS system now supports Bakong QR code payments using the merchant ID `norm_soklim@bkrt` and merchant name `NORMSOKLIM`.

## API Endpoints

### 1. Generate Bakong Payment QR Code

**Endpoint:** `POST /api/bakongpayment/generate`

**Authorization:** Required

**Request Body:**
```json
{
  "merchantId": "norm_soklim@bkrt",
  "merchantName": "NORMSOKLIM",
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
  "message": "Bakong payment QR code generated successfully",
  "qrCodeData": "base64_encoded_image_data",
  "qrCodeText": "EMV_QR_code_text",
  "transactionId": "guid"
}
```

### 2. Get QR Code Image

**Endpoint:** `GET /api/bakongpayment/qr-code`

**Authorization:** Not required

**Query Parameters:**
- `merchantId` (optional): Defaults to "norm_soklim@bkrt"
- `merchantName` (optional): Defaults to "NORMSOKLIM"
- `amount` (optional): Amount to be paid
- `currency` (optional): Defaults to "KHR"
- `description` (optional): Payment description
- `referenceId` (optional): Transaction reference ID

### 3. Process Bakong Payment for Existing Sale

**Endpoint:** `POST /api/sales/{saleId}/process-bakong-payment`

**Authorization:** Required

**Request Body:**
```json
{
  "description": "Payment for sale #123"
}
```

## Usage Examples

### Example 1: Generate QR Code for Specific Amount

```bash
curl -X POST "https://your-api-url/api/bakongpayment/generate" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "merchantId": "norm_soklim@bkrt",
    "merchantName": "NORMSOKLIM",
    "amount": 50000.00,
    "currency": "KHR",
    "description": "Payment for order #456",
    "referenceId": "ref456"
  }'
```

### Example 2: Process Bakong Payment for Existing Sale

```bash
curl -X POST "https://your-api-url/api/sales/12345678-1234-1234-1234-123456789012/process-bakong-payment" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '"Payment for completed sale"'
```

## Integration with Sales

The system allows you to generate Bakong payment QR codes for existing sales. This is useful when a customer wants to pay for a completed sale using Bakong.

## EMV QR Code Format

The generated QR codes follow the EMV QR Code specification for payment systems, ensuring compatibility with Bakong and other payment apps.

## Error Handling

- If the payment generation fails, the API returns a 400 Bad Request with an error message
- If the sale ID doesn't exist when processing a payment, the API returns a 404 Not Found
- Proper validation is performed on all input parameters

## Security

- All payment generation endpoints require authentication
- The merchant ID and name are validated and secured
- Transaction references help track payments