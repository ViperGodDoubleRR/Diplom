<template>
  <div class="content">
    <h1> SIGN UP</h1>
    <h2>Create your account</h2>
    <div :hidden="isStep">
      <img :src="photo">
      <section>
        <p class='text'>Email</p>
        <div class="input-wrapper">
          <input type="text" placeholder="zabiyaka" v-model.trim="emailfirst">
          <select v-model.trim="emailtwo">
            <option>@gmail.com</option>
            <option>@yahoo.com</option>
            <option>@mail.ru</option>
          </select>
        </div>
      </section>
      <section>
        <p class="text">Verification code</p>
        <div class="input-wrapper">
          <input type="text" placeholder="asd312sajk13" v-model.trim="code">
          <button class="request" @click="sendEmail">Request Code</button>
        </div>
      </section>
    </div>

    <div :hidden="!isStep">
      <section>
        <p class="text">Login</p>
        <div class="input-wrapper">
          <input type="text" placeholder="Arseniy" v-model="login">
          <button class="request">Request Code</button>
        </div>
      </section>
      <section>
        <p class='text'>Password</p>
        <div class="input-wrapper">
          <input type="password" placeholder="****" v-model="passwordfirst">
          <button class="password">asd</button>
        </div>
      </section>
      <section>
        <p class='text'>Repeat Password</p>
        <div class="input-wrapper">
          <input type="password" placeholder="***" v-model="passwordtwo">
          <button class="password">asd</button>
        </div>
      </section>

    </div>

    <div>
      <button class="back">Cancel</button>
      <button class="go" @click="nextStep">Continue</button>
    </div>
  </div>
</template>


<script setup lang="ts">
import { ref } from 'vue'
import { RegService } from '@/service/regService';
const service = new RegService();
const photo = ref('/Photo.png');
const isStep = ref(false);

const emailfirst = ref('');
const emailtwo = ref('');
const code = ref('');
const login = ref('');
const passwordfirst = ref('');
const passwordtwo = ref('');


const sendEmail = async () => {
  const email = emailfirst.value + emailtwo.value;
  const response = await service.sendEmail(email);
  alert(response);
}
const checkCode = async () => {
  const email = emailfirst.value + emailtwo.value;
  const response = await service.checkCode(email, code.value);
  return response
}
const nextStep = async () => {
  if (!isStep.value) {
    const response = await checkCode()
    isStep.value = response;
    alert(response);
  }
  else {
    const email = emailfirst.value + emailtwo.value;
    if (login.value.length > 3 && passwordfirst.value == passwordtwo.value) {
      service.registerUser(email, login.value, passwordfirst.value);
    }
  }
}
</script>

<style scoped>
.content {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  text-align: center;
  border-radius: 20px;
  backdrop-filter: blur(20px);
  background: linear-gradient(135deg, #6C00FF, #00C2FF, #FF00C7);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
  padding: 50px;
  font-family: Georgia, 'Times New Roman', Times, serif;
}

img {
  width: 120px;
  height: 120px;
  margin: 0px;
  background: transparent;
}

h1 {
  color: #FFFFFF;
  font-size: 50px;
  margin: 0px;
}

h2 {
  color: #E0E0E0;
}

section {
  width: 364px;
  margin: 0px;
}

.text {
  color: #D0BFFF;
  font-size: 30px;
  margin-bottom: 5px;
  margin-left: 10px;
}

.input-wrapper {
  background-color: white;
  border-radius: 20px 20px 20px 20px;
  display: flex;
  opacity: 70%;
}

input {
  border-radius: 20px 0px 0px 20px;
  font-size: 20px;
  height: 40px;
  border: none;
  opacity: 50%;
  background-color: transparent;
  color: #7a0eed;
  font-weight: bold;
  flex: 1;
  padding-left: 10px;
}

select {
  border: none;
  border-radius: 0px 20px 20px 0px;
  font-size: 15px;
  flex: 0.7;
  min-width: 0;
  width: 100%;
}

.request {
  opacity: 0.8;
  border: none;
  flex: 1.5;
  border-radius: 0px 20px 20px 0px;
}

.request:hover {
  opacity: 1;
}

.password {
  opacity: 20%;
  border: none;
  border-radius: 0px 20px 20px 0px;

  flex: 0.3;
}

.go {
  background: linear-gradient(90deg, #00F5FF, #00C2FF);
  color: #FFFFFF;
  box-shadow: 0 0 12px rgba(0, 245, 255, 0.4);
  font-size: 40px;
  margin-top: 20px;
  border: none;
  border-radius: 20px;
  box-shadow: 0 0 1 0.3;
  margin-left: 25px;

}

.back {
  background: linear-gradient(90deg, #b02ff0, #00C2FF);
  color: #FFFFFF;
  box-shadow: 0 0 12px rgba(0, 245, 255, 0.4);
  font-size: 40px;
  margin-top: 20px;
  border: none;
  border-radius: 20px;
  box-shadow: 0 0 1 0.3;
  margin-right: 25px;
}
</style>
