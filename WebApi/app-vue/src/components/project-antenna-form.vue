<template>
    <div>
        <dx-data-grid
            :data-source="dataSource"
            :show-borders="true"
            :remote-operations="false"
            :column-auto-width="true"
            key-expr="id"
            @row-updating="onRowUpdating"
            @editor-preparing="editorPreparing"
        >
          <dx-editing
              :allow-updating="true"
              :allow-adding="true"
              :allow-deleting="true"
              :texts="{confirmDeleteMessage: 'Вы уверены, что хотите удалить эту запись?'}"
              mode="popup"
          >
              <dx-popup :show-title="true" title="Добавление антенны" col-count="2" width="50%"></dx-popup>
          </dx-editing>
            <dx-toolbar>
                <dx-item name="addRowButton" show-text="always" location="after" :options="addButton">
                </dx-item>
            </dx-toolbar>
          <dx-column
              data-field="antennaId"
              caption="Антенна"
              data-type="string"
              alignment="left"
              :allow-updating="false"
              :width="200">
            <dx-required-rule message="Вы не выбрали антенну"></dx-required-rule>
            <dx-lookup
                :data-source="antennas"
                value-expr="id"
                display-expr="model"
            />
          </dx-column>
          <dx-column 
              data-field="azimuth" 
              data-type="number" 
              caption="Азимут"
              :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
              alignment="left">
            <dx-label :visible="false"/>
              <dx-required-rule message="Вы не запонили азимут"></dx-required-rule>
          </dx-column>
          <dx-column 
              data-field="heightFromGroundLevel" 
              data-type="number" 
              caption="Высота подвеса антенны от уровня земли в метрах"
              :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
              alignment="left">
            <dx-required-rule message="Вы не запонили высоту подвеса антенны от уровня земли"></dx-required-rule>
          </dx-column>
          <dx-column 
              data-field="heightFromRoofLevel" 
              data-type="number" 
              caption="Высота подвеса антенны от уровня крыши в метрах"
              :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
              alignment="left">
            <dx-required-rule message="Вы не запонили высоту подвеса антенны от уровня крыши"></dx-required-rule>
          </dx-column>
          <dx-column 
              data-field="latitude" 
              data-type="number" 
              caption="Широта"
              :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
              alignment="left">
            <dx-required-rule message="Вы не запонили широту установки антенны"></dx-required-rule>
          </dx-column>
          <dx-column 
              data-field="longitude" 
              data-type="number" 
              caption="Долгота"
              :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
              alignment="left">
            <dx-required-rule message="Вы не запонили долготу установки антенны"></dx-required-rule>
          </dx-column>
          <dx-column 
              data-field="tilt" 
              data-type="number" 
              caption="Тильт"
              :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
              alignment="left">
            <dx-required-rule message="Вы не запонили тильт антенны"></dx-required-rule>
          </dx-column>
          <dx-column
                  data-field="rtoRadiationMode"
                  data-type="string"
                  caption="Режим работы РТО на излучение"
                  :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
                  alignment="left">
              <dx-form-item
                editor-type="dxSelectBox"
                :editor-options="{
                  placeholder: 'Выберите или режим работы РТО на излучение', 
                  items: rtoRadiationMode, 
                  acceptCustomValue: true,
                  searchEnabled: true,
                  stylingMode: 'filled', 
                  label: 'Режим работы РТО на излучение', 
                  labelMode: 'floating'}"
              />
              <dx-required-rule message="Вы не запонили режим работы РТО на излучение"></dx-required-rule>
          </dx-column>
          <dx-column 
              data-field="projectId" 
              data-type="string" 
              :visible="false">
            <dx-form-item
                :editor-options="{
                disabled: true}"
                editor-type="dxTextArea"
                :visible="false"
                :data="projectId"
            />
          </dx-column>
          <DxMasterDetail
              :enabled="true"
              template="master-detail"
          />
          <template #master-detail="{ data }">
            <antenna-translator-form :master-detail-data="data"/>
          </template>
          <dx-paging :page-size="5"/>
          <dx-pager :show-page-size-selector="true" :allowed-page-sizes="[8, 12, 20]"/>
          <dx-sorting mode="multiple"/>
        </dx-data-grid>
    </div>
</template>
<script setup>
import {
  DxLabel
} from 'devextreme-vue/form';
import {onMounted, ref} from "vue";
import antennaService from "@/api/antennaService";
import projectAntennaService from "@/api/projectAntennaService";
import {useRoute} from "vue-router";
import {
    DxDataGrid,
    DxColumn,
    DxFormItem,
    DxPaging,
    DxEditing,
    DxPager,
    DxToolbar,
    DxItem,
    DxSorting,
    DxLookup, DxMasterDetail, DxPopup
} from 'devextreme-vue/data-grid';
import 'devextreme-vue/text-area';
import {DxRequiredRule} from "devextreme-vue/validator";
import CustomStore from "devextreme/data/custom_store";
import notify from "devextreme/ui/notify";
import AntennaTranslatorForm from "@/components/antenna-translator-form.vue";

const route = useRoute();
let projectId = route.params.id;
let dataSource = ref(null);
const antennas = ref([]);
const addButton = {
    text: "Добавить антенну",
    icon: 'add',
    type: 'success',
    stylingMode:"contained"
}
let rtoRadiationMode = ref([
    'постоянный',
    'повторно-кратковременный',
    'импульсный']);
const store = new CustomStore({
  key: "id",
  async load(loadOptions) {
    const response = await projectAntennaService.getProjectAntennaeForGrid(loadOptions, projectId);
    return response;
  },
  async insert(values) {
    values.projectId = projectId;
    const baseResponse = await projectAntennaService.createProjectAntenna(values)
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
    const baseResponse = await projectAntennaService.deleteProjectAntenna(id);
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
    const baseResponse = await projectAntennaService.updateProjectAntenna(options.newData);
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

function editorPreparing(e) {
    if (e.dataField === 'antennaId' && e.parentType === 'dataRow' && e.row.isNewRow
     !== true) {
        e.editorOptions.readOnly = true;
}}

onMounted(async () => {
  dataSource.value = store;

  const antennaResponse = await antennaService.getAntennae();
  antennas.value = antennaResponse.data.result;
})

</script>
<style scoped>
</style>
