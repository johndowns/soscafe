export const environment = {
  production: true,
  appBaseUrl: 'http://localhost:4200/',
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
      storeAuthStateInCookie: true, // set to true for IE 11
    },
    consentScopes: [
      'openid',
      'profile',
      'https://soscafe.onmicrosoft.com/vendorfunctionsapis/user_impersonation',
    ],
  }
};
