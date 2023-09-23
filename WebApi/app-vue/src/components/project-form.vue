<template>
  <div class="project-form">
  <dx-form
      id="project-form"
      ref="formRef"
      label-location="top"
      :form-data="dataSource"
      :read-only="isFormDisabled"
      :show-colon-after-label="true"
      :show-validation-summary="true"
  >
    <dx-item
        data-field='projectNumber'
        editor-type='dxTextBox'
        :editor-options="{ 
                      stylingMode: 'filled',
                      labelMode: 'floating',
                      label: 'Номер проекта (сайт)' }"
    >
      <dx-required-rule message="Укажите Sitename(номер) проекта"></dx-required-rule>
      <dx-label
          :visible="false"
      />
    </dx-item>
    <dx-item
        data-field='contrAgentId'
        editor-type="dxSelectBox"
        :editor-options="{ 
                        placeholder: 'Выберите контрагента', 
                        items: contrAgents, 
                        displayExpr: 'companyName', 
                        valueExpr: 'id',
                        labelMode: 'floating',
                        label: 'Контрагент'}"
    >
      <dx-required-rule message="Вы не выбрали контрагента"></dx-required-rule>
      <dx-label :visible="false"/>
    </dx-item>
    <dx-item
        data-field='townName'
        editor-type="dxSelectBox"
        :editor-options="{ 
                        placeholder: 'Выберите город', 
                        items: towns, 
                        displayExpr: 'townName', 
                        valueExpr: 'townName',
                        labelMode: 'floating',
                        label: 'Город'}"
    >
      <dx-required-rule message="Вы не выбрали город установки"></dx-required-rule>
      <dx-label
          :visible="false"
      />
    </dx-item>
    <dx-item
        data-field='address'
        editor-type='dxTextBox'
        :editor-options="{ 
                      stylingMode: 'filled', 
                      label: 'Район, улица, дом', 
                      labelMode: 'floating' }"
    >
      <dx-label
          :visible="false"
      />
    </dx-item>
    <dx-item
            data-field='purposeRto'
            editor-type='dxTextBox'
            :editor-options="{ 
                    stylingMode: 'filled', 
                    label: 'Назначение РТО', 
                    labelMode: 'floating' }"
    >
        <dx-label
                :visible="false"
        />
    </dx-item>
    <dx-item
            data-field='placeOfInstall'
            editor-type='dxSelectBox'
            :editor-options="{ 
              placeholder: 'Выберите или запишите место дислокации', 
              items: placeOfInstall, 
              labelMode: 'floating',
              acceptCustomValue: true,
              searchEnabled: true,
              label: 'Место дислокации РТО (РЭС)'}"
    >
        <dx-label
                :visible="false"
        />
    </dx-item>
    <dx-group-item
      caption="Заполнить если передающие антенны установлены на здании"
      :col-count="3">
        <dx-item
                data-field='purposeBuild'
                editor-type='dxSelectBox'
                :editor-options="{ 
              placeholder: 'Выберите или запишите назначение здания', 
              items: purposeBuild,
              acceptCustomValue: true,
              searchEnabled: true,
              stylingMode: 'filled', 
              label: 'Назначение здания, где размещается РТО (РЭС)', 
              labelMode: 'floating' }"
        >
            <dx-label
                    :visible="false"
            />
        </dx-item>
        <dx-item
                data-field='typeORoof'
                editor-type='dxTextBox'
                :editor-options="{ 
            stylingMode: 'filled', 
            label: 'Тип крыши здания (указать ровный или с уклоном /уклон в градусах/, материал покрытия кровли)', 
            labelMode: 'floating' }"
        >
            <dx-label
                    :visible="false"
            />
        </dx-item>
        <dx-item
                data-field='typeOfTopCover'
                editor-type='dxSelectBox'
                :editor-options="{
              placeholder: 'Выберите или запишите тип верхнего перекрытия', 
              items: typeOfTopCover, 
              acceptCustomValue: true,
              searchEnabled: true,
              stylingMode: 'filled', 
              label: 'Тип верхнего перекрытия', 
              labelMode: 'floating' }"
        >
            <dx-label
                    :visible="false"
            />
        </dx-item>
    </dx-group-item>
    <dx-item
            data-field='maxHeightAdjoinBuild'
            editor-type='dxNumberBox'
            :editor-options="{ 
                stylingMode: 'filled', 
                label: 'Максимальная высота прилегающей застройки в метрах', 
                labelMode: 'floating' }"
    >
        <dx-label
                :visible="false"
        />
    </dx-item>
    <dx-item
            data-field='placeOfCommunicationCloset'
            editor-type='dxTextBox'
            :editor-options="{ 
        stylingMode: 'filled', 
        label: 'Телекоммуникационные шкафы (стойки) с сетевым оборудованием планируется разместить в закрывающимся на замок контейнере:', 
        labelMode: 'floating' }"
    >
        <dx-label
                :visible="false"
        />
    </dx-item>
    <dx-item
      data-field='hasTechnicalLevel'
      editor-type='dxCheckBox'
      :disabled="!isFormDisabled"
      :editor-options="{
        text:'Да'}"
    >
      <dx-label
        :visible="true"
        text="Наличие в здании, где размещается антенна РЭС технического этажа"
      />
    </dx-item>
    <dx-item
      data-field='hasOtherRto'
      editor-type='dxCheckBox'
      :disabled="!isFormDisabled"
      :editor-options="{ 
        text:'Да'}"
    >
      <dx-label
        :visible="true"
        text="Наличие других передающих средств на крыше здания или радиомачте"
      />
    </dx-item>
    <dx-button-item>
      <dx-button-options
          width="100%"
          type="success"
          styling-mode="outlined"
          :template="mode === 'create' ? 'Создать и продолжить' : 'Сохранить изменения'"
          :on-click="onClickSaveChanges"
          :visible="!isFormDisabled"
          :use-submit-behavior="true"
      >
      </dx-button-options>
    </dx-button-item>
    <dx-button-item>
      <dx-button-options
          width="100%"
          type="default"
          styling-mode="outlined"
          template="Редактировать"
          :on-click="onClickEditProject"
          :visible="isFormDisabled"
          :use-submit-behavior="true"
      >
      </dx-button-options>
    </dx-button-item>
  </dx-form>
  </div>
</template>
<script setup>

import {
  DxButtonItem,
  DxButtonOptions,
  DxForm,
  DxItem,
  DxLabel, DxGroupItem
} from "devextreme-vue/form";
import {
  DxRequiredRule
} from 'devextreme-vue/validator';
import {onBeforeMount, reactive, ref, defineProps} from "vue";
import contrAgentService from "@/api/contrAgentService";
import townService from "@/api/townService";
import projectService from "@/api/projectService";
import {useRoute, useRouter} from "vue-router";
import notify from "devextreme/ui/notify";

const props = defineProps({
    onSaveProject: Function,
})
const router = useRouter();
const route = useRoute();
let dataSource = reactive({});
let isFormDisabled = ref(true);
let oid = route.params.id;
const mode = ref(route.params.mode);
const formRef = ref(null);
const contrAgents = ref([]);
const towns = ref([]);
let placeOfInstall = ref([
    'На трубостойке на мачте', 
    'На трубостойке на вышке',
    'На трубостойке на АМС',
    'На трубостойке на башне',
    'На трубостойке на крыше']);
let typeOfTopCover = ref([
    'Железобетонное',
    'Металлическое ',
    'Деревянное ']);
let purposeBuild = ref([
    'Административное ',
    'Жилое ']);

onBeforeMount(async () => {
  const response = await contrAgentService.getContrAgents();
  contrAgents.value = response.data.result;

  const townResponse = await townService.getTowns();
  towns.value = townResponse.data.result;

  if (mode.value === "read") {
    const response = await projectService.getProject(oid);
    Object.assign(dataSource, response.data.result);
  } else {
    isFormDisabled.value = false;
  }
})
function onClickEditProject() {
  isFormDisabled.value = false;
}
async function onClickSaveChanges() {
  try {
    const formInstance = formRef.value.instance;
    const isFormValid = await formInstance.validate();
    if (isFormValid.isValid === false) {
      notify({
        message: 'Данные не корректны',
        position: {
          my: 'center top',
          at: 'center top',
        },
      }, 'warning', 1000);
    }
    else {
      if (mode.value === "read") {
        const responseUpdate = await projectService.updateProject(dataSource);
        if (responseUpdate.data.success) {
          notify({
            message: 'Проект успешно отредактирован',
            position: {
              my: 'center top',
              at: 'center top',
            },
          }, 'success', 1000);
        } else {
          notify(responseUpdate.data.messages, 'error', 2000);
        }
        isFormDisabled.value = true;
      } else {
        const response = await projectService.createProject(dataSource);
        if (response.data.success) {
          notify({
            message: 'Проект успешно создан',
            position: {
              my: 'center top',
              at: 'center top',
            },
          }, 'success', 1000);
            await router.push({name: 'projectDetail', params: {mode: "read", id: response.data.result}});
            props.onSaveProject()
        } else {
          notify({
            message: response.data.messages,
            position: {
              my: 'center top',
              at: 'center top'}
          }, 'error', 2000);
        }
      }
    }

  } catch (error) {
    console.error("Ошибка при сохранении изменений:", error);
    notify({
      message: "Ошибка сервера при сохранении изменений:",
      position: {
        my: 'center top',
        at: 'center top'}
    }, 'error', 2000);
  }
}
</script>

<style scoped>
.project-form {
    margin: 50px 50px auto;
}
</style>