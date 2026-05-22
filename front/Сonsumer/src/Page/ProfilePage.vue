<template>
  <div class="profile-page">

    <div class="bg"></div>


    <section class="profile-card">

      <div class="avatar-block">
        <img class="avatar" src="https://i.pravatar.cc/150?img=12" />
        <div class="online"></div>
      </div>

      <div class="info-block">

        <div class="name-row">
          <div>
            <h1 class="name">Arseny Dev</h1>
            <p class="tag">@arseny</p>
          </div>
        </div>

        <p class="bio">
          Building next-gen social network with clean architecture and scalable UI.
        </p>

        <div class="stats">
          <div class="stat">
            <span class="num">128</span>
            <span class="label">Friends</span>
          </div>

          <div class="stat">
            <span class="num">54</span>
            <span class="label">Posts</span>
          </div>

          <div class="stat">
            <span class="num">320</span>
            <span class="label">Likes</span>
          </div>
        </div>

        <div class="actions">
          <button class="edit-btn">
            Edit Profile
          </button>
        </div>

      </div>
    </section>

    <div class="tabs">
      <button class="tab active">Posts</button>
      <button class="tab">Liked</button>
      <button class="tab">Saved</button>
    </div>

    <section class="grid">

      <div v-for="post in posts" :key="post.id" class="post" @click="openPost(post.id)">
        <div class="post-media"></div>

        <div class="post-info">
          <div class="post-title">Post #{{ post.id }}</div>
          <div class="post-meta">@arseny</div>
        </div>
      </div>

    </section>

  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();

const posts = ref([
  { id: 1 },
  { id: 2 },
  { id: 3 },
  { id: 4 },
  { id: 5 },
  { id: 6 },
]);

const openPost = (id: number) => {
  router.push(`/post/${id}`);
};
</script>

<style scoped>
/* PAGE */
.profile-page {
  min-height: 100vh;
  background: #0F0F0F;
  padding: 40px;
  color: white;
  position: relative;
  overflow-x: hidden;
}

/* BACKGROUND GLOW */
.bg {
  position: absolute;
  top: -200px;
  left: 50%;
  transform: translateX(-50%);
  width: 700px;
  height: 700px;
  background: radial-gradient(circle, rgba(65, 99, 252, 0.18), transparent 60%);
  filter: blur(80px);
}

/* PROFILE CARD */
.profile-card {
  position: relative;
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: 25px;

  padding: 25px;
  border-radius: 22px;

  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  backdrop-filter: blur(12px);

  max-width: 900px;
}

/* AVATAR */
.avatar-block {
  position: relative;
  width: 120px;
  height: 120px;
}

.avatar {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid rgba(65, 99, 252, 0.6);
}

.online {
  position: absolute;
  bottom: 8px;
  right: 8px;
  width: 12px;
  height: 12px;
  background: #3CFF78;
  border-radius: 50%;
  border: 2px solid #0F0F0F;
}

/* INFO */
.info-block {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

/* NAME */
.name {
  font-size: 26px;
  margin: 0;
}

.tag {
  opacity: 0.6;
  font-size: 14px;
}

/* BIO */
.bio {
  font-size: 14px;
  opacity: 0.8;
  max-width: 500px;
}

/* STATS */
.stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;

  margin-top: 10px;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;

  padding: 10px 0;
}

.num {
  font-size: 18px;
  font-weight: 600;
  line-height: 1;
}

.label {
  font-size: 12px;
  opacity: 0.5;
  letter-spacing: 0.3px;
}

/* ACTIONS */
.actions {
  margin-top: 10px;
}

.edit-btn {
  background: #4163FC;
  border: none;
  padding: 10px 14px;
  border-radius: 12px;
  color: white;
  cursor: pointer;
}

/* TABS */
.tabs {
  margin-top: 20px;
  display: flex;
  gap: 15px;
}

.tab {
  background: transparent;
  border: none;
  color: white;
  opacity: 0.5;
  cursor: pointer;
  padding: 6px 10px;
}

.tab.active {
  opacity: 1;
  border-bottom: 2px solid #4163FC;
}

/* POSTS GRID */
.grid {
  margin-top: 25px;
  max-width: 900px;

  column-count: 3;
  column-gap: 15px;
}

.post {
  break-inside: avoid;
  margin-bottom: 15px;

  border-radius: 15px;
  overflow: hidden;

  background: rgba(255, 255, 255, 0.03);
  cursor: pointer;

  transition: 0.2s;
}

.post:hover {
  transform: scale(1.03);
}

.post-media {
  height: 170px;
  background: linear-gradient(135deg, #4163FC, #A944BD);
}

.post-info {
  padding: 10px;
}

.post-title {
  font-size: 14px;
}

.post-meta {
  font-size: 12px;
  opacity: 0.5;
}
</style>
