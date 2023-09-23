<template>
  <dx-data-grid
      :data-source="dataSource"
      :show-borders="true"
      :remote-operations="false"
      :column-auto-width="true"
      key-expr="id"
      @row-updating="onRowUpdating"
  >
    <dx-editing
        :allow-updating="true"
        :allow-adding="true"
        :allow-deleting="true"
        :texts="{confirmDeleteMessage: 'Вы уверены, что хотите удалить эту запись?'}"
        mode="popup"
    >
      <dx-popup :show-title="true" title="Добавление передатчика" col-count="2" width="50%" height="50%"></dx-popup>
    </dx-editing>
      <dx-toolbar>
          <dx-item name="addRowButton" show-text="always" location="before" widget="dxButton" :options="addButton">
          </dx-item>
      </dx-toolbar>
    <dx-column
        data-field="translatorSpecsId"
        caption="Частота"
        data-type="string"
        alignment="left">
      <dx-required-rule message="Вы не выбрали частоту"></dx-required-rule>
      <dx-lookup
          :data-source="translators"
          value-expr="id"
          display-expr="frequency"
      />
    </dx-column>
    <dx-column
        data-field="power"
        data-type="number"
        caption="Мощность"
        :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
        alignment="left">
      <dx-label :visible="false"/>
      <dx-required-rule message="Вы не заполнили мощность"></dx-required-rule>
    </dx-column>
    <dx-column
        data-field="transmitLossFactor"
        data-type="number"
        caption="КФ потери сигнала"
        :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
        alignment="left">
      <dx-label :visible="false"/>
      <dx-required-rule message="Вы не заполнили коэффициент потери сигнала"></dx-required-rule>
    </dx-column>
    <dx-column
        data-field="translatorTypeId"
        caption="Тип передатчика"
        data-type="string"
        alignment="left">
      <dx-lookup
          :data-source="translatorTypes"
          value-expr="id"
          display-expr="type"
      />
    </dx-column>
    <dx-column
        data-field="gain"
        data-type="number"
        caption="КФ усиления"
        :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
        alignment="left">
      <dx-required-rule message="Вы не запонели коэффициент усиления сигнала"></dx-required-rule>
    </dx-column>
    <dx-column
            data-field="tilt"
            data-type="number"
            caption="Тильт"
            :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
            alignment="left">
        <dx-required-rule message="Вы не запонили угол наклона передатчика"></dx-required-rule>
    </dx-column>
    <dx-column
        data-field="projectAntennaId"
        data-type="string"
        :visible="false">
      <dx-form-item
          :editor-options="{
              disabled: true}"
          editor-type="dxTextArea"
          :visible="false"
          :data="projectAntennaId"
      />
    </dx-column>
    <dx-paging :page-size="5"/>
    <dx-pager :show-page-size-selector="true" :allowed-page-sizes="[8, 12, 20]"/>
    <dx-sorting mode="multiple"/>
  </dx-data-grid>
</template>
<script setup>
import {
    DxLabel
} from 'devextreme-vue/form';

import {onMounted, ref, defineProps} from "vue";
import {
    DxColumn,
    DxDataGrid,
    DxEditing,
    DxFormItem, 
    DxItem, 
    DxSorting,
    DxLookup,
    DxPager,
    DxPaging, DxToolbar, DxPopup
} from "devextreme-vue/data-grid";
import {DxRequiredRule} from "devextreme-vue/validator";

import translatorTypeService from "@/api/transaltorTypeService";
import notify from "devextreme/ui/notify";
import antennaTranslatorService from "@/api/antennaTranslatorService";
import CustomStore from "devextreme/data/custom_store";
import translatorSpecsService from "@/api/translatorSpecsService";

const props = defineProps({
  masterDetailData: {
    type: Object,
    default: () => ({}),
  }})
const antennaId = ref();
const projectAntennaId = ref();
const translatorTypes = ref();
let dataSource = ref(null);
const translators = ref([]);
const addButton = {
    text: "Добавить передатчики",
    icon: 'login',
    type: 'success',
    stylingMode:"contained"
}

const store = new CustomStore({
  key: "id",
  async load(loadOptions) {
    const response = await antennaTranslatorService.getAntennaTranslatorForGrid(loadOptions, projectAntennaId.value);
    return response;
  },
  async insert(values) {
    values.projectAntennaId = projectAntennaId.value;
    const baseResponse = await antennaTranslatorService.createAntennaTranslator(values)
    await dataSource.value.load();
    if (baseResponse.data.success) {
      notify({
        message: 'Данные сохранены',
        position: {
          my: 'center top',
          at: 'center top',
        },
      }, 'success', 1000);
    } else {
      notify(baseResponse.data.messages, 'error', 2000);
    }
    return {data: baseResponse};
  },
  async remove(id) {
    const baseResponse = await antennaTranslatorService.deleteAntennaTranslator(id);
    if (baseResponse.data.success) {
      notify({
        message: 'Антенна удалена',
        position: {
          my: 'center top',
          at: 'center top',
        },
      }, 'success', 1000);
    } else {
      notify(baseResponse.data.messages, 'error', 2000);
    }
    return {data: baseResponse};
  },
  async update(id, values) {
    console.log(id + values)
  }
});
async function onRowUpdating(options) {
  options.newData = Object.assign(options.oldData, options.newData);
  const baseResponse = await antennaTranslatorService.updateAntennaTranslator(options.newData);
  await dataSource.value.load();
  if (baseResponse.data.success) {
    notify({
      message: 'Данные сохранены',
      position: {
        my: 'center top',
        at: 'center top',
      },
    }, 'success', 1000);
  } else {
    notify(baseResponse.data.messages, 'error', 2000);
  }
  return {data: baseResponse};
}

onMounted(async () => {
  dataSource.value = store;
  console.log(props.masterDetailData)
  antennaId.value = props.masterDetailData.row.data.antennaId;
  projectAntennaId.value = props.masterDetailData.key;
  const response = await translatorSpecsService.getAllByAntennaId(antennaId.value);
  const translatorTypesResponse = await translatorTypeService.getTranslatorTypes();
  translatorTypes.value = translatorTypesResponse.data.result;
  translators.value = response.data.result;
})

</script>
<style scoped>
</style>
