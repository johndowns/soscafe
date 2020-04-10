import { isIE } from '../app/config';

export const environment = {
  production: true,
  appBaseUrl: 'https://vendors.soscafe.nz/',
  apiBaseUrl: 'https://vendorapi.soscafe.nz/',
  msal: {
    auth: {
      clientId: '1cc0426e-f8d7-4ddb-94b5-18185c09a6bd',
      authority: 'https://soscafe.b2clogin.com/tfp/soscafe.onmicrosoft.com/b2c_1_signupsignin',
      validateAuthority: false,
      navigateToLoginRequestUrl: true,
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: isIE
    },
    consentScopes: [
      'openid',
      'profile',
      'https://soscafe.onmicrosoft.com/vendorfunctionsapis/user_impersonation'
    ],
    appScope: 'https://soscafe.onmicrosoft.com/vendorfunctionsapis/user_impersonation'
  }
};
