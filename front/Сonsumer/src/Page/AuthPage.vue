<template>
  <div class="content">
    <h1>SIGN DOWN</h1>
    <h2>Entrace your account</h2>

    <section>
      <p class='text'>Email</p>
      <div class="input-wrapper">
        <input type="text" placeholder="zabiyaka">
        <select>
          <option>@gmail.com</option>
          <option>@yahoo.com</option>
          <option>@mail.ru</option>
        </select>
      </div>
    </section>

    <section>
      <p class='text'>Password</p>
      <div class="input-wrapper">
        <input type="password" placeholder="123456">
        <button class="password">asd</button>
      </div>
    </section>

    <section>
      <p class="text">Verification code</p>
      <div class="input-wrapper">
        <input type="number" placeholder="123456">
        <button class="request">Request Code</button>
      </div>
    </section>


    <button class="go">Continue</button>
    <p class='text'>Already have an account</p>
    <p class='link'>Log In</p>
    <p class='text'>Did you forget your password?</p>
    <p class='link'>Reset password</p>
  </div>
</template>
<script lang="ts">

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


h1 {
  color: #FFFFFF;
  font-size: 50px;
}

h2 {
  color: #E0E0E0;
}

section {
  width: 364px;
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
  flex:1;
  padding-left: 10px;
}

select {
  border: none;
  border-radius: 0px 20px 20px 0px;
  font-size: 15px;
  flex:0.7;
  min-width: 0;
  width: 100%;
}
.request{
  opacity: 20%;
  border: none;
  flex:1.5 ;
  border-radius: 0px 20px 20px 0px;
}
.password{
  opacity: 20%;
  border: none;
 border-radius: 0px 20px 20px 0px;

  flex:0.3;
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
}

.link {
  color: #B0E0FF;
  margin: 0px;
}
</style>
