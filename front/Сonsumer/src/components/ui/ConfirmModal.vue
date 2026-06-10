<template>
  <Teleport to="body">
    <div v-if="modelValue" class="overlay" @click.self="cancel">
      <div class="modal">
        <h3>{{ title }}</h3>
        <p>{{ message }}</p>
        <div class="actions">
          <button class="btn ghost" type="button" @click="cancel">{{ cancelLabel }}</button>
          <button class="btn danger" type="button" :disabled="loading" @click="confirm">
            {{ loading ? "..." : confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    modelValue: boolean;
    title: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    loading?: boolean;
  }>(),
  {
    confirmLabel: "Подтвердить",
    cancelLabel: "Отмена",
    loading: false,
  }
);

const emit = defineEmits<{
  (e: "update:modelValue", value: boolean): void;
  (e: "confirm"): void;
}>();

function cancel() {
  emit("update:modelValue", false);
}

function confirm() {
  emit("confirm");
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.72);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10060;
  padding: 16px;
}

.modal {
  width: min(420px, 100%);
  background: #151515;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 20px;
  color: #fff;
}

.modal h3 {
  margin: 0 0 10px;
}

.modal p {
  margin: 0 0 18px;
  color: #b8bfd4;
  line-height: 1.5;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn {
  border: none;
  border-radius: 10px;
  padding: 10px 14px;
  cursor: pointer;
  font-weight: 600;
}

.btn.ghost {
  background: rgba(255, 255, 255, 0.08);
  color: #fff;
}

.btn.danger {
  background: rgba(255, 77, 77, 0.2);
  color: #ff7b7b;
  border: 1px solid rgba(255, 77, 77, 0.35);
}
</style>
