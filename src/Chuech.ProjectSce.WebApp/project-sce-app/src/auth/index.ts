import {
  InMemoryWebStorage,
  UserManager,
  WebStorageStateStore
} from "oidc-client";

// Oidc is dumb
/* eslint-disable @typescript-eslint/camelcase */

interface AuthInfo {
  authUrl: string;
}

export let userManager = new UserManager({});

export async function initialiseUserManager() {
  const response = await fetch("/auth-info");
  const authInfo = (await response.json()) as AuthInfo;

  userManager = new UserManager({
    authority: authInfo.authUrl,
    client_id: "spa",
    redirect_uri: `${location.origin}/authcallback`,
    response_type: "code",
    scope: "openid profile core.full core_identity",
    post_logout_redirect_uri: `${location.origin}/`,
    automaticSilentRenew: true,
    silent_redirect_uri: `${location.origin}/silentrenew`,
    userStore: new WebStorageStateStore({ store: new InMemoryWebStorage() })
  });
}
