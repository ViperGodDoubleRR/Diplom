<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="modelValue" class="overlay" @click.self="close">
        <div class="modal">

          <!-- HEADER -->
          <div class="header">

            <div>
              <h3>Comments</h3>
              <p>{{ comments.length }} comments</p>
            </div>

            <button class="close-btn" @click="close">
              ✕
            </button>

          </div>

          <!-- LIST -->
          <div class="comments">

            <template v-if="comments.length">

              <div v-for="comment in comments" :key="comment.id" class="comment">
                <img :src="comment.user.avatar" class="avatar" />

                <div class="body">

                  <div class="top">

                    <span class="login">
                      {{ comment.user.login }}
                    </span>

                    <span class="date">
                      {{ formatDate(comment.createdAt) }}
                    </span>

                  </div>

                  <p class="text">
                    {{ comment.text }}
                  </p>

                </div>
              </div>

            </template>

            <div v-else class="empty">
              <div class="empty-icon">
                💬
              </div>

              <h4>No comments yet</h4>

              <p>
                Be the first to start the discussion
              </p>
            </div>

          </div>

          <!-- INPUT -->
          <div class="input-area">

            <input v-model="newComment" placeholder="Write a comment..." @keyup.enter="sendComment" />

            <button @click="sendComment">
              Send
            </button>

          </div>

        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";

const props = defineProps<{
  modelValue: boolean;
  postId: string | null;
}>();

const emit = defineEmits([
  "update:modelValue"
]);

const comments = ref<any[]>([]);
const newComment = ref("");

function close() {
  emit("update:modelValue", false);
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString();
}

async function loadComments() {
  if (!props.postId) return;

  console.log("load comments for", props.postId);


  comments.value = [];
}

async function sendComment() {
  if (!newComment.value.trim()) return;

  comments.value.unshift({
    id: Date.now(),
    text: newComment.value,
    createdAt: new Date(),
    user: {
      login: "You",
      avatar: "https://i.pravatar.cc/100"
    }
  });

  newComment.value = "";
}

watch(
  () => props.modelValue,
  (v) => {
    if (v) {
      loadComments();
    }
  }
);
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;

  z-index: 9999;

  display: flex;
  justify-content: center;
  align-items: center;

  background: rgba(0, 0, 0, .75);
  backdrop-filter: blur(12px);
}

.modal {
  width: 820px;
  max-width: 95vw;

  height: 800px;
  max-height: 92vh;

  display: flex;
  flex-direction: column;

  border-radius: 24px;

  background: #111;

  border: 1px solid rgba(255, 255, 255, .08);

  box-shadow:
    0 30px 80px rgba(0, 0, 0, .6),
    0 0 0 1px rgba(255, 255, 255, .03);

  overflow: hidden;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;

  padding: 24px;

  border-bottom: 1px solid rgba(255, 255, 255, .06);

  background: rgba(255, 255, 255, .02);
}

.header h3 {
  margin: 0;

  font-size: 24px;
  font-weight: 700;

  color: #fff;
}

.header p {
  margin-top: 4px;

  font-size: 13px;
  color: rgba(255, 255, 255, .5);
}

.close-btn {
  width: 42px;
  height: 42px;

  border: none;
  border-radius: 12px;

  background: rgba(255, 255, 255, .05);

  color: white;

  cursor: pointer;

  transition: .2s;
}

.close-btn:hover {
  background: rgba(255, 255, 255, .1);
}

.comments {
  flex: 1;

  overflow-y: auto;

  padding: 24px;

  display: flex;
  flex-direction: column;
  gap: 14px;
}

.comments::-webkit-scrollbar {
  width: 6px;
}

.comments::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, .15);
  border-radius: 20px;
}

.comment {
  display: flex;
  gap: 14px;

  padding: 14px;

  border-radius: 18px;

  background: rgba(255, 255, 255, .03);

  border: 1px solid rgba(255, 255, 255, .05);

  transition: .2s ease;
}

.comment:hover {
  background: rgba(255, 255, 255, .05);

  border-color: rgba(65, 99, 252, .2);

  transform: translateY(-1px);
}

.avatar {
  width: 48px;
  height: 48px;

  flex-shrink: 0;

  border-radius: 50%;

  object-fit: cover;

  border: 2px solid rgba(65, 99, 252, .4);
}

.body {
  flex: 1;
  min-width: 0;
}

.top {
  display: flex;
  align-items: center;
  gap: 10px;

  margin-bottom: 6px;
}

.login {
  font-size: 14px;
  font-weight: 700;

  color: white;
}

.date {
  font-size: 12px;

  color: rgba(255, 255, 255, .45);
}

.text {
  margin: 0;

  font-size: 14px;
  line-height: 1.65;

  color: rgba(255, 255, 255, .92);

  word-break: break-word;
}

.empty {
  flex: 1;

  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;

  text-align: center;
}

.empty-icon {
  font-size: 56px;

  margin-bottom: 12px;
}

.empty h4 {
  margin: 0;

  color: white;

  font-size: 18px;
}

.empty p {
  margin-top: 8px;

  color: rgba(255, 255, 255, .5);
}

.input-area {
  display: flex;
  gap: 12px;

  padding: 18px;

  border-top: 1px solid rgba(255, 255, 255, .06);

  background: rgba(255, 255, 255, .02);
}

.input-area input {
  flex: 1;

  height: 52px;

  border: 1px solid transparent;
  outline: none;

  border-radius: 14px;

  padding: 0 16px;

  background: rgba(255, 255, 255, .05);

  color: white;

  transition: .2s;
}

.input-area input:focus {
  border-color: rgba(65, 99, 252, .5);

  box-shadow:
    0 0 0 3px rgba(65, 99, 252, .12);
}

.input-area button {
  height: 52px;

  padding: 0 22px;

  border: none;
  border-radius: 14px;

  background: #4163FC;

  color: white;
  font-weight: 600;

  cursor: pointer;

  transition: .2s;
}

.input-area button:hover {
  transform: translateY(-1px);

  box-shadow:
    0 10px 30px rgba(65, 99, 252, .35);
}

.fade-enter-active,
.fade-leave-active {
  transition: .25s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
