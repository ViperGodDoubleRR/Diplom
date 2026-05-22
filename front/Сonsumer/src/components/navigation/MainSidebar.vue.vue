<template>
  <aside class="sidebar">
    <div>
      <h1 class="logo">GoatBridge</h1>

      <nav class="menu">
        <button class="menu-btn active">
          Лента
        </button>

        <button class="menu-btn">
          Профиль
        </button>

        <button class="menu-btn">
          Сообщения
        </button>

        <button class="menu-btn">
          Настройки
        </button>
      </nav>
    </div>

    <div class="user-box">
      <img src="https://i.pravatar.cc/150?img=12" class="avatar" />

      <div>
        <h3>Arseny</h3>
        <p>@goat_artist</p>
      </div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 290px;
  min-height: 100vh;

  padding: 30px 20px;
  box-sizing: border-box;

  display: flex;
  flex-direction: column;
  justify-content: space-between;

  background: rgba(255, 255, 255, 0.03);
  border-right: 1px solid rgba(255, 255, 255, 0.06);

  backdrop-filter: blur(16px);

  color: white;
}

/* LOGO */
.logo {
  font-size: 34px;
  margin-bottom: 45px;

  color: white;
  letter-spacing: 1px;
}

/* MENU */
.menu {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.menu-btn {
  width: 100%;
  height: 58px;

  border: none;
  border-radius: 18px;

  font-size: 18px;

  cursor: pointer;

  color: rgba(255, 255, 255, 0.7);
  background: rgba(255, 255, 255, 0.03);

  transition: 0.2s ease;

  text-align: left;
  padding-left: 16px;
}

.menu-btn:hover {
  background: rgba(65, 99, 252, 0.12);
  transform: translateX(4px);
  color: white;
}

/* ACTIVE */
.active {
  background: rgba(65, 99, 252, 0.18);
  border: 1px solid rgba(65, 99, 252, 0.35);
  color: white;
  box-shadow: 0 0 18px rgba(65, 99, 252, 0.25);
}

/* USER */
.user-box {
  display: flex;
  align-items: center;
  gap: 12px;

  padding: 14px;
  border-radius: 18px;

  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.avatar {
  width: 52px;
  height: 52px;

  border-radius: 50%;
  object-fit: cover;

  border: 2px solid rgba(65, 99, 252, 0.4);
}

h3 {
  margin: 0;
  font-size: 15px;
}

p {
  margin: 0;
  font-size: 12px;
  opacity: 0.6;
}
</style>
