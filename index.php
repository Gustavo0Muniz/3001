<!DOCTYPE html>
<html lang="pt-br">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
    <link rel="stylesheet" href="style.css">
</head>
<body>
    <style>
        main{
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
    </style> 
    <main>
    <nav>
    <ul class="sidebar">
            <li onclick=hideSideBar()><a href="#"><svg xmlns="http://www.w3.org/2000/svg" height="26px" viewBox="0 -960 960 960" width="26px" fill="#e8eaed"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg></a></li>
            <li><a href="personagens.php">Personagens</a></li>
            <li><a href="botao.php">Baixe Aqui</a></li>
            <li><a href="index.php">Sobre Nós</a></li>
            
        </ul>
       
        <!--separação header acima ser versão sidebar-->
        <ul   >
            <li ><img style="width: 10%;" src="logo3001Canva.png" alt=""></li>
            <li class="hideOnMobile"><a href="personagens.php">Personagens</a></li>
            <li class="hideOnMobile"><a href="botao.php">Baixe Aqui</a></li>
            <li class="hideOnMobile"><a href="index.php">Sobre Nós</a></li>
            
            <li class="menu-button" onclick=showSidebar()><a href="#"><svg xmlns="http://www.w3.org/2000/svg" height="26px" viewBox="0 -960 960 960" width="26px" fill="#e8eaed"><path d="M120-240v-80h720v80H120Zm0-200v-80h720v80H120Zm0-200v-80h720v80H120Z"/></svg></a></li>
        </ul>
    </nav>
    </main>  <?php
    include_once "footer.php"
    ?>
  
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