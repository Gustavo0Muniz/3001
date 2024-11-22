<!DOCTYPE html>
<html lang="en">
<head>
    <link rel="stylesheet" href="personalizando.css">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
    <link rel="stylesheet" href="style.css">
</head>
<body>
<style>
         body{
            background-image: url(plano_de_Fundo.jpg);
            background-size: 100%;
            background-repeat: no-repeat;
            background-attachment: fixed;
        }
        nav{
        backdrop-filter: blur(20px)  ;
        background-color: black;
    }
    nav a{
 
 color: rgb(255, 255, 255);
}
nav a:hover{
background-color: #5d5858;
}
@import url('https://fonts.googleapis.com/css2?family=Host+Grotesk:ital,wght@0,300..800;1,300..800&display=swap');


</style>

    
    
    <nav>
    <ul class="sidebar">
            <li onclick=hideSideBar()><a href="#"><svg xmlns="http://www.w3.org/2000/svg" height="26px" viewBox="0 -960 960 960" width="26px" fill="#e8eaed"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg></a></li>
            <li><a href="personagens.php">Personagens</a></li>
            <li><a href="botao.php">Baixe Aqui</a></li>
            <li><a href="index.php">Sobre Nós</a></li>
            
        </ul>
    <ul>
        <li ><img style="width: 10%;" src="logo3001Canva.png" alt=""></li>
        <li class="hideOnMobile"><a href="personagens.php">Personagens</a></li>
        <li class="hideOnMobile"><a href="botao.php">Baixe Aqui</a></li>
        <li class="hideOnMobile"><a href="index.php">Sobre Nós</a></li>
        
        <li class="menu-button" onclick=showSidebar()><a href="#"><svg xmlns="http://www.w3.org/2000/svg" height="26px" viewBox="0 -960 960 960" width="26px" fill="#e8eaed"><path d="M120-240v-80h720v80H120Zm0-200v-80h720v80H120Zm0-200v-80h720v80H120Z"/></svg></a></li>
    </ul>
</nav>
<div class="centralizar">
    
    <h1 style="color: white;
  font-family: Host Grotesk, sans-serif;">3001: O Recomeço.</h1>
    <button style="color: black;
  font-family: Host Grotesk, sans-serif;">Baixar</button>
    
</div>
<script>
        function showSidebar(){
            const sidebar = document.querySelector('.sidebar')
            sidebar.style.display = 'flex'
        }
        function hideSideBar(){
            const sidebar = document.querySelector('.sidebar')
            sidebar.style.display = 'none'
        }
    </script>
</body>
</html>