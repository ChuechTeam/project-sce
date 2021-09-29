<template>
  <div class="home">
    <p>CHUECH!</p>
    <div v-if="userLoading">Loading</div>
    <div v-else-if="currentUser != null">
      <p>Hi, {{ currentUser.profile.name }} !</p>
      <button @click="logout">Log out</button>
    </div>
    <button v-else @click="login">Log in</button>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { userManager } from "@/auth";
import { User } from "oidc-client";

export default defineComponent({
  name: "Home",
  methods: {
    login() {
      userManager.signinRedirect();
    },
    logout() {
      userManager.signoutRedirect();
    }
  },
  data() {
    return {
      userLoading: true,
      currentUser: null as User | null
    };
  },
  async mounted() {
    this.currentUser = await userManager.getUser();
    this.userLoading = false;
  }
});
</script>
vbase-ts
