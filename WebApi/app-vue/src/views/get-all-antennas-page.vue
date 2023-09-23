<template>
  <div class="row justify-content-center mt-5">
    <div class="col-11 ">
    <dx-data-grid
        :data-source="dataSource"
        :show-borders="true"
        :remote-operations="false"
        :columnAutoWidth="true"
        :allowColumnResizing="true"
        key-expr="id"
        @row-updating="onRowUpdating"
        @editor-preparing="editorPreparing"
    >
      <dx-search-panel
          :visible="true"
          placeholder="Поиск"
          width= 250
      />
      <dx-editing
          :allow-updating="true"
          :allow-adding="true"
          :allow-deleting="true"
          :texts="{confirmDeleteMessage: 'Вы уверены, что хотите удалить эту запись?'}"
          mode="popup"
      >
          <dx-popup :show-title="true" title="Добавление передатчика" col-count="auto" width="50%" height="50%"></dx-popup>
      </dx-editing>
      <dx-toolbar>
        <dx-item name="addRowButton" show-text="always" location="after" widget="dxButton" :options="addButton">
        </dx-item>
        <dx-item name="searchPanel">
        </dx-item>
      </dx-toolbar>
      <dx-column
          data-field="model"
          caption="Модель антенны"
          data-type="string"
          alignment="left"
          :editor-options="{stylingMode: 'filled', labelMode: 'floating'}">
        <dx-label :visible="false"/>
        <dx-required-rule message="Модель антенны должна быть заполнена"></dx-required-rule>
      </dx-column>
      <dx-column
          data-field="verticalSizeDiameter"
          data-type="number"
          caption="Вертикальный размер(диаметр антенны)"
          :editor-options="{stylingMode: 'filled', labelMode: 'floating'}"
          alignment="left">
        <dx-label :visible="false"/>
        <dx-required-rule message="Вертикальный размер(диаметр антенны) должнен быть заполнен"></dx-required-rule>
      </dx-column>
      <dx-master-detail
          :enabled="true"
          template="master-detail"
      />
      <template #master-detail="{ data }">
        <translator-spec-form :master-detail-data="data"/>
      </template>
      <dx-paging :page-size="5"/>
      <dx-pager :show-page-size-selector="true" :allowed-page-sizes="[8, 12, 20]"/>
      <dx-sorting mode="multiple"/>
      <dx-header-filter :visible="true"/>
    </dx-data-grid>
    </div>
  </div>
</template>

<script setup>

import {
  DxLabel
} from 'devextreme-vue/form';
import {onMounted, ref} from "vue";
import antennaService from "@/api/antennaService";
import {
    DxDataGrid,
    DxColumn,
    DxPaging,
    DxEditing,
    DxPager,
    DxToolbar,
    DxItem,
    DxSorting,
    DxMasterDetail, DxSearchPanel, DxHeaderFilter, DxPopup
} from 'devextreme-vue/data-grid';
import 'devextreme-vue/text-area';
import {DxRequiredRule} from "devextreme-vue/validator";
import CustomStore from "devextreme/data/custom_store";
import notify from "devextreme/ui/notify";
import TranslatorSpecForm from "@/components/translator-spec-form.vue";

let dataSource = ref(null);
const antennas = ref([]);
const addButton = {
  text: "Добавить антенну",
  icon: 'add',
  type: 'success',
  stylingMode:"contained"
}


const store = new CustomStore({
  key: "id",
  async load(loadOptions) {
    const response = await antennaService.getAntennaeForGrid(loadOptions);
    return response;
  },
  async insert(values) {
    const baseResponse = await antennaService.createAntenna(values);
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
    const baseResponse = await antennaService.deleteAntenna(id);
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
    console.log(id + values);
  }
});

async function onRowUpdating(options) {
  options.newData = Object.assign(options.oldData, options.newData);
  const baseResponse = await antennaService.updateAntenna(options.newData);
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
  console.log(e);
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
#form h2 {
  margin-left: 40px;
  font-weight: normal;
  font-size: 35px;
}
</style>
