// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  appBaseUrl: 'http://localhost:4200/',
  apiBaseUrl: 'https://soscafevendor-test.azurewebsites.net/',
  msal: {
    tenant: 'soscafetest', 
    policy: 'b2c_1_signupsignin',
    auth: {
      clientId: 'f6577358-0ee8-454b-9678-7d02519ede64',
      authority: 'https://soscafetest.b2clogin.com/tfp/soscafetest.onmicrosoft.com/b2c_1_signupsignin',
      validateAuthority: false,
      navigateToLoginRequestUrl: true,
    },
    consentScopes: [
      'openid',
      'profile',
      'offline_access',
      'https://soscafetest.onmicrosoft.com/vendorfunctionsapistest/user_impersonation'
    ],
    appScope: 'https://soscafetest.onmicrosoft.com/vendorfunctionsapistest/user_impersonation'
  }
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
import 'zone.js/dist/zone-error';  // Included with Angular CLI.
