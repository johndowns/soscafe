# SOS Cafe vendor portal

## Front-end

The front-end is an Angular.js app, hosted in an Azure Storage blob container as a static website, fronted by an Azure Functions proxy to allow for Angular routing, and served through Azure CDN.

The production front-end is accessible at https://vendors.soscafe.nz.

## Back-end

The back-end is an Azure Functions app, and consists of API endpoints as well as background worker functions.

The back-end API is accessible at https://vendorapi.soscafe.nz.

## Identity

Identities are maintained in Azure AD B2C. Only standard (built-in) policies are used currently.

## Hosting

| Resource Type | Description | Production Resource Name | Test Resource Name |
|-|-|-|-|
| CDN profile | | `SosCafe` | N/A |
| CDN endpoint | | `soscafevendor` | N/A |
| Azure AD B2C tenant | Vendor identity management | `soscafe.onmicrosoft.com` | `soscafetest.onmicrosoft.com` |
| Functions app | Proxy for front-end | `soscafevendoruiproxy` | `soscafevendoruiproxytest` |
| App service plan | Proxy for front-end (consumption tier) | `ASP-SosCafe-a270` | `ASP-SosCafeTest-b4e5` |
| Functions app | Back-end | `soscafevendor` | `soscafevendor-test` |
| App service plan | Back-end (consumption tier) | `ASP-SosCafe-9649` | `ASP-SosCafeTest-b701` |
| Storage account | Primary | `soscafe` | `soscafetest` |
| Storage account | Front-end | `soscafevendorui` | `soscafevendoruitest` |
| Storage account | Functions apps | `soscafevendorfn` | `soscafevendortestfn` |
| Application Insights | Telemetry for all components | `soscafevendor` | `soscafevendor-test` |
