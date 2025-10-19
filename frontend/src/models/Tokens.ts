interface JwtPayload {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string | string[];
  nbf: number;
  exp: number;
  iss: string;
  aud: string;
}

interface RefreshAccessTokenResponse {
  accessToken: string;
}

export type { JwtPayload, RefreshAccessTokenResponse };
