# ServiceWorker Services and Languages - JSON Structure

This document describes the new JSON structure for Services and Languages in the SafeHabour ServiceWorker system.

## Overview

The `Services` and `Languages` fields are now stored as JSON-serialized lists of objects instead of comma-separated strings. This provides much more flexibility and structure for the data.

## Services Structure

Each service is represented by a `ServiceItem` object with the following properties:

```json
{
  "name": "House Cleaning",
  "description": "Professional residential cleaning services including deep cleaning, regular maintenance, and move-in/move-out cleaning",
  "category": "Cleaning",
  "isActive": true
}
```

### ServiceItem Properties:
- **name** (required, max 100 chars): The service name
- **description** (optional, max 500 chars): Detailed description of the service
- **category** (required, max 50 chars): Service category (e.g., "Cleaning", "Maintenance", "Tutoring")
- **isActive** (boolean): Whether the service is currently being offered

### Example Services Array:
```json
[
  {
    "name": "House Cleaning",
    "description": "Professional residential cleaning services",
    "category": "Cleaning",
    "isActive": true
  },
  {
    "name": "Window Cleaning",
    "description": "Interior and exterior window cleaning",
    "category": "Cleaning",
    "isActive": true
  },
  {
    "name": "Garden Maintenance",
    "description": "Lawn mowing, trimming, and general garden upkeep",
    "category": "Gardening",
    "isActive": false
  }
]
```

## Languages Structure

Each language is represented by a `LanguageItem` object with the following properties:

```json
{
  "name": "English",
  "code": "en",
  "proficiencyLevel": "Native",
  "isNative": true
}
```

### LanguageItem Properties:
- **name** (required, max 50 chars): The language name
- **code** (required, max 20 chars): ISO language code (e.g., "en", "fr", "es", "de")
- **proficiencyLevel** (required, max 20 chars): Proficiency level ("Native", "Fluent", "Intermediate", "Basic")
- **isNative** (boolean): Whether this is a native language

### Example Languages Array:
```json
[
  {
    "name": "English",
    "code": "en",
    "proficiencyLevel": "Native",
    "isNative": true
  },
  {
    "name": "French",
    "code": "fr",
    "proficiencyLevel": "Fluent",
    "isNative": false
  },
  {
    "name": "Spanish",
    "code": "es",
    "proficiencyLevel": "Intermediate",
    "isNative": false
  }
]
```

## API Request Examples

### Create ServiceWorker User Request:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "phoneNumber": "+1234567890",
  "bio": "Experienced cleaning professional with 5+ years",
  "address": "123 Main Street",
  "city": "Toronto",
  "postalCode": "M5V 1A1",
  "country": "Canada",
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "hourlyRate": 25.50,
  "latitude": 43.651070,
  "longitude": -79.347015,
  "services": [
    {
      "name": "House Cleaning",
      "description": "Professional residential cleaning",
      "category": "Cleaning",
      "isActive": true
    },
    {
      "name": "Office Cleaning",
      "description": "Commercial office cleaning services",
      "category": "Cleaning",
      "isActive": true
    }
  ],
  "languages": [
    {
      "name": "English",
      "code": "en",
      "proficiencyLevel": "Native",
      "isNative": true
    },
    {
      "name": "French",
      "code": "fr",
      "proficiencyLevel": "Fluent",
      "isNative": false
    }
  ]
}
```

### Update ServiceWorker Request:
```json
{
  "userId": "12345678-1234-1234-1234-123456789012",
  "hourlyRate": 30.00,
  "services": [
    {
      "name": "Premium House Cleaning",
      "description": "Premium residential cleaning with eco-friendly products",
      "category": "Cleaning",
      "isActive": true
    }
  ],
  "languages": [
    {
      "name": "English",
      "code": "en",
      "proficiencyLevel": "Native",
      "isNative": true
    },
    {
      "name": "Italian",
      "code": "it",
      "proficiencyLevel": "Basic",
      "isNative": false
    }
  ]
}
```

## Database Storage

In the database, these JSON objects are stored as strings in the following columns:
- `ServicesJson` - Contains the serialized Services array
- `LanguagesJson` - Contains the serialized Languages array

The entity automatically handles serialization/deserialization when accessing the `Services` and `Languages` properties.

## Validation Rules

### Services:
- Must have at least one service
- Each service name is required and must be unique within the array
- Service category is required
- Description is optional but limited to 500 characters

### Languages:
- Must have at least one language
- Each language name and code is required
- Language codes should be unique within the array
- Proficiency level must be one of: "Native", "Fluent", "Intermediate", "Basic"

### Hourly Rate:
- Must be a decimal value
- Range: 0.01 to 10,000.00
- Supports up to 2 decimal places

## Migration Notes

If you have existing data with comma-separated strings, you'll need to migrate them to the new JSON structure. Here's a sample migration approach:

1. **Old format**: `"House Cleaning,Window Cleaning,Garden Work"`
2. **New format**: 
```json
[
  {"name": "House Cleaning", "category": "Cleaning", "isActive": true},
  {"name": "Window Cleaning", "category": "Cleaning", "isActive": true},
  {"name": "Garden Work", "category": "Gardening", "isActive": true}
]
```

This new structure provides much more flexibility for filtering, searching, and managing services and languages in the SafeHabour platform.
