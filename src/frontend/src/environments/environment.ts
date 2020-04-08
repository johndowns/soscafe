// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  appBaseUrl: 'http://localhost:4200/',
  apiBaseUrl: 'https://vendorapi.soscafe.nz/',
  msal: {
    auth: {
      clientId: '1cc0426e-f8d7-4ddb-94b5-18185c09a6bd',
      authority: 'https://soscafe.b2clogin.com/tfp/soscafe.onmicrosoft.com/b2c_1_signupsignin',
      validateAuthority: false,
      navigateToLoginRequestUrl: true,
    },

    consentScopes: [
      'openid',
      'profile',
      'https://soscafe.onmicrosoft.com/vendorfunctionsapis/user_impersonation',
    ],
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
