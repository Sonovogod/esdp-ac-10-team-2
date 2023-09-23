<template>
  <form class="login-form" @submit.prevent="onSubmit">
    <dx-form :form-data="formData" :disabled="loading">
      <dx-item
          data-field="email"
          editor-type="dxTextBox"
          :editor-options="{ stylingMode: 'filled', placeholder: 'Email', mode: 'email' }"
      >
        <dx-required-rule message="Поле Email обязательно" />
        <dx-email-rule message="Неверный формат Email" />
        <dx-label :visible="false" />
      </dx-item>
      <dx-item
          data-field='password'
          editor-type='dxTextBox'
          :editor-options="{ stylingMode: 'filled', placeholder: 'Password', mode: 'password' }"
      >
        <dx-required-rule message="Введите пароль" />
        <dx-label :visible="false" />
      </dx-item>
      <dx-button-item>
        <dx-button-options
            width="100%"
            type="default"
            template="signInTemplate"
            :use-submit-behavior="true"
        >
        </dx-button-options>
      </dx-button-item>
      <template #signInTemplate>
        <div>
          <span class="dx-button-text">
            <dx-load-indicator v-if="loading" width="24px" height="24px" :visible="true" />
            <span v-if="!loading">Войти</span>
          </span>
        </div>
      </template>
    </dx-form>
  </form>
</template>

<script>
import DxLoadIndicator from "devextreme-vue/load-indicator";
import DxForm, {
  DxItem,
  DxEmailRule,
  DxRequiredRule,
  DxLabel,
  DxButtonItem,
  DxButtonOptions
} from "devextreme-vue/form";
import notify from 'devextreme/ui/notify';

import { reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import authorizationService from "@/api/AuthorizationService";

export default {
  setup() {
    const route = useRoute();
    const router = useRouter();

    const formData = reactive({
      email:"",
      password:""
    });
    const loading = ref(false);


    async function onSubmit() {
      loading.value = true;
      const loginModel = {login: formData.email, password: formData.password};
      try {
        await authorizationService.login(loginModel);
        await router.push(route.query.redirect || '/home');
      } catch (error){
        loading.value = false;
        notify("Неправильный логин или пароль", 'error', 2000);
      }
    }

    return {
      formData,
      loading,
      onSubmit
    };
  },
  components: {
    DxLoadIndicator,
    DxForm,
    DxEmailRule,
    DxRequiredRule,
    DxItem,
    DxLabel,
    DxButtonItem,
    DxButtonOptions
  }
};
</script>

<style lang="scss">
@import "../themes/generated/variables.base.scss";

.login-form {
  .link {
    text-align: center;
    font-size: 16px;
    font-style: normal;

    a {
      text-decoration: none;
    }
  }

  .form-text {
    margin: 10px 0;
    color: rgba($base-text-color, alpha($base-text-color) * 0.7);
  }
}
</style>
