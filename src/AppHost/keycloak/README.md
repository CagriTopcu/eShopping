# Keycloak Realm Configuration

## eshopping-realm.json

This file contains **development-only** seed data for the Keycloak realm.

### Security Notice

- All passwords in this file are **placeholder values for local development only**.
- These credentials are NOT used in staging or production environments.
- Production Keycloak is provisioned via Vault-backed secret injection and admin API.
- Never commit real credentials to this file.

### Realm Users (Dev Only)

| User       | Purpose               |
|------------|-----------------------|
| admin      | Admin panel testing   |
| testuser   | Customer flow testing |

All dev passwords are set to simple values for convenience. Production users are managed
through Keycloak admin console with Vault-sourced initial admin credentials.
