<template>
  <div class="user-panel">
    <div class="user-info">
      <div class="image-container">
        <div class="user-image" />
      </div>
      <div class="user-name">{{email}}</div>
    </div>

    <dx-context-menu
      v-if="menuMode === 'context'"
      target=".user-button"
      :items="menuItems"
      :width="210"
      show-event="dxclick"
      css-class="user-menu"
    >
      <dx-position my="top center" at="bottom center" />
    </dx-context-menu>

    <dx-list
      v-if="menuMode === 'list'"
      class="dx-toolbar-menu-action"
      :items="menuItems"
    />
  </div>
</template>

<script>
import DxContextMenu, { DxPosition } from "devextreme-vue/context-menu";
import DxList from "devextreme-vue/list";

export default {
  props: {
    menuMode: String,
    menuItems: Array,
    email: String
  },
  components: {
    DxContextMenu,
    DxPosition,
    DxList
  }
};
</script>

<style lang="scss">
@import "../themes/generated/variables.base.scss";

.user-info {
  display: flex;
  align-items: center;

  .dx-toolbar-menu-section & {
    padding: 10px 6px;
    border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  }

  .image-container {
    overflow: hidden;
    border-radius: 50%;
    height: 30px;
    width: 30px;
    margin: 0 4px;
    border: 1px solid rgba(0, 0, 0, 0.1);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.15);

    .user-image {
      width: 100%;
      height: 100%;
      background: url("data:image/jpeg;base64,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")
        no-repeat #fff;
      background-size: cover;
    }
  }

  .user-name {
    font-size: 14px;
    color: $base-text-color;
    margin: 0 9px;
  }
}

.user-panel {
  .dx-list-item .dx-icon {
    vertical-align: middle;
    color: $base-text-color;
    margin-right: 16px;
  }
  .dx-rtl .dx-list-item .dx-icon {
    margin-right: 0;
    margin-left: 16px;
  }
}

.dx-context-menu.user-menu.dx-menu-base {
  &.dx-rtl {
    .dx-submenu .dx-menu-items-container .dx-icon {
      margin-left: 16px;
    }
  }
  .dx-submenu .dx-menu-items-container .dx-icon {
    margin-right: 16px;
  }
  .dx-menu-item .dx-menu-item-content {
    padding: 3px 15px 4px;
  }
}

.dx-theme-generic .user-menu .dx-menu-item-content .dx-menu-item-text {
  padding-left: 4px;
  padding-right: 4px;
}
</style>
