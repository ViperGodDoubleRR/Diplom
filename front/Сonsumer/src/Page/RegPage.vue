<template>
  <div class="content">
    <figure>
      <img src="@/assets/image/logo.png" alt="Арт соц сети" class="logo" />
      <figcaption>Достойный арт Социальной сети "GoatBridge"</figcaption>
    </figure>

    <div class="form">
      <h1>Registration</h1>

      <div v-if="step === 1" class="step-container">
        <h2>Email</h2>
        <input v-model.trim="email" placeholder="arsenyads" class="main-input" />

        <h2>Verification Code</h2>
        <div class="input-wrapper">
          <input v-model.trim="code" placeholder="H89AKSDS" class="verification-input" />
          <button @click="sendEmail" class="request-btn">request</button>
        </div>
      </div>

      <div v-else class="step-container">
        <h2>Login</h2>
        <input v-model="login" type="text" placeholder="Your_Nickname" class="main-input" />

        <h2>Password</h2>
        <div class="input-wrapper">
          <input v-model="password" :type="showPassword ? 'text' : 'password'" placeholder="********"
            class="verification-input" />
          <button class="eye-btn" @click="showPassword = !showPassword">
            <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
          </button>
        </div>
      </div>

      <div class="buttonmenu">
        <button class="action-btn cancel" @click="step = 1">
          {{ step === 1 ? 'Cancel' : 'Back' }}
        </button>

        <button class="action-btn next" @click="NextButton">
          {{ step === 1 ? 'Next' : 'Finish' }} </button>
      </div>

      <h4>Have account?</h4>
      <h3>Click Authentification</h3>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { RegService } from '@/service/regService';
import { useRouter } from 'vue-router';

const router = useRouter();
const regService = new RegService();
const step = ref(1);
const showPassword = ref(false);
const email = ref('')
const code = ref('');
const login = ref('');
const password = ref('');
const sendEmail = async () => {
  const response = await regService.sendEmail(email.value);
  console.log(response);
  if (response.success) {
    alert("Код успешно отправле на :" + response.data)
  }
  else {
    alert(response.error?.code);
    alert(response.error?.message);
  }
}

const NextButton = async () => {
  if (step.value == 1) {
    const response = await stepOne();
    console.log(response);
    if (response?.success) {
      step.value++;
      alert(response.data);
    }
    else {
      alert(response?.error?.code);
      alert(response?.error?.message);
    }
  }
  else if (step.value == 2) {
    const response = await stepTwo();
    if (response?.success) {
      alert(response.data)
      router.push('/auth');
    }
    else {
      alert(response?.error?.code)
      alert(response?.error?.message);
    }
  }
}
const stepOne = async () => {
  try {
    return await regService.checkCode(email.value, code.value);
  }
  catch (e) {
    console.log(e);
  }
}
const stepTwo = async () => {
  try {
    return await regService.registerUser(email.value, login.value, password.value);
  }
  catch (e) {
    console.log(e);
  }
}
</script>

<style scoped>
.step-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 15px;
  width: 100%;
}


.eye-btn {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  width: 60px;
  height: 45px;
  background-color: white;
  border-radius: 30px;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.2);
}

.eye-icon {
  width: 35px;
  height: auto;
}


.content {
  display: flex;
  width: 100%;
  height: 100vh;
  align-items: center;
  justify-content: center;
  background-color: #0F0F0F;
}

.logo {
  width: 720px;
  height: auto;
  max-height: 900px;
  margin-right: 100px;
}

figcaption {
  color: #F8F9FA;
  font-family: 'Ouroboros', sans-serif;
  text-align: center;
  margin-top: 10px;
  opacity: 0.7;
}

.form {
  width: 704px;
  min-height: 780px;
  padding: 40px;
  box-sizing: border-box;
  background: linear-gradient(to bottom right,
      rgba(169, 68, 189, 0.83) 8%,
      rgba(65, 99, 252, 0.61) 100%);
  box-shadow:
    6px 6px 4px 6px rgba(65, 99, 252, 0.59),
    inset 6px 6px 4px 6px rgba(169, 68, 189, 0.59);
  border-radius: 40px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  gap: 15px;
}

h1,
h2,
h3,
h4,
button,
input {
  font-family: 'Ouroboros', sans-serif;
  color: #F8F9FA;
  margin: 0;
}

h1 {
  font-size: 64px;
  margin-bottom: 10px;
}

h2 {
  font-size: 36px;
  align-self: center;
}

h3 {
  font-size: 32px;
  opacity: 0.85;
  cursor: pointer;
}

h4 {
  font-size: 24px;
  opacity: 0.75;
  margin-top: 10px;
}

input {
  width: 589px;
  height: 67px;
  border-radius: 50px;
  border: none;
  font-size: 28px;
  padding: 0 30px;
  box-sizing: border-box;
  outline: none;
  background: linear-gradient(to right, rgba(108, 40, 197, 0.9), rgba(52, 19, 95, 0.9));
}

.input-wrapper {
  position: relative;
  width: 589px;
  height: 67px;
}

.verification-input {
  width: 100%;
  height: 100%;
  padding-right: 180px;
}

.request-btn {
  position: absolute;
  right: 0;
  top: 0;
  width: 170px;
  height: 100%;
  background-color: #4163FC;
  border-radius: 50px;
  border: none;
  font-size: 24px;
  opacity: 0.85;
  cursor: pointer;
}

.buttonmenu {
  display: flex;
  gap: 30px;
  margin-top: 30px;
}

.action-btn {
  width: 252px;
  height: 82px;
  border-radius: 50px;
  border: none;
  font-size: 32px;
  cursor: pointer;
}

.cancel {
  background-color: rgba(255, 255, 255, 0.2);
  opacity: 0.6;
}

.next {
  background-color: #4163FC;
  opacity: 0.8;
}
</style>
