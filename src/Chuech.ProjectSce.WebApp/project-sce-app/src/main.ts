import { createApp } from "vue";
import App from "@/App.vue";
import { initialiseUserManager, userManager } from "@/auth";
import router from "@/router";
import {User} from "oidc-client";

async function run() {
  try {
    await initialiseUserManager();
    await userManager.signinSilent(); // Temporary solution
    const userProfile = await userManager.getUser() as User;
    console.log(
      "User profile: ",
      userProfile.profile
    );
    if (process.env.NODE_ENV === "development") {
      console.log("Access token: " + userProfile.access_token);
    }
  } catch (e) {
    console.log(e);
    // Usually, the exception is "require login", so we ignore it.
  }

  createApp(App)
    .use(router)
    .mount("#app");
}

run();
