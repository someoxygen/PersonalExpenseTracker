// Memory-only token. A reload requires login; no token is persisted in web storage.
let token: string | null = null;
let unauthorized: () => void = () => {};
export const session = {
  getToken: () => token,
  setToken: (value: string | null) => {
    token = value;
  },
  onUnauthorized: (handler: () => void) => {
    unauthorized = handler;
  },
  expire: () => unauthorized(),
};
