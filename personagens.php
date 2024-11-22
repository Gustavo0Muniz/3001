<!DOCTYPE html>
<html lang="pt-br">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>3001: O recomeço</title>
    <link rel="stylesheet" href="cards.css">
    <link rel="stylesheet" href="style.css">
</head>

<body>
<style>
    
        nav{
        backdrop-filter: blur(60px)  ;
        background-color: black;
    }
    nav a{
 
 color: rgb(255, 255, 255);
}
nav a:hover{
background-color: #5d5858;
}
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
    <h1>Conheça os personagens do jogo:</h1>
    <div class="container">
        <div class="card__container">
            <article class="card__article">
                <img src="https://www.opovo.com.br/_midias/jpg/2024/04/09/750x500/1_neymar_santos_jogo_palmeiras_final_paulistao_2024__2_-26355148.jpg" alt="" class="card__img">
                <div class="card__data">
                    <span class="card__description">O Hacker irrastreável ou quase.. um dos melhores hackers do mundo responsável por descobrir a verdadeira face da (nome da empresa)</span>
                    <h2 class="card__title">Henry</h2>
                    <a href="#" class="card__button">Saiba mais</a>
                </div>
            </article>
            
            <article class="card__article">
                <img src="https://www.opovo.com.br/_midias/jpg/2024/04/09/750x500/1_neymar_santos_jogo_palmeiras_final_paulistao_2024__2_-26355148.jpg" alt="" class="card__img">
                <div class="card__data">
                    <span class="card__description">Um dom divino? Benções? O mais forte com puro esforço este é Akales, um coração gigante assim como seus músculos</span>
                    <h2 class="card__title">Akales</h2>
                    <a href="#" class="card__button">Saiba mais</a>
                </div>
            </article>
            
            <article class="card__article">
                <img src="https://www.opovo.com.br/_midias/jpg/2024/04/09/750x500/1_neymar_santos_jogo_palmeiras_final_paulistao_2024__2_-26355148.jpg" alt="" class="card__img">
                <div class="card__data">
                    <span class="card__description">O necromante de máquinas, mas máquinas antigas? Sem um braço e amargurado, este é o Akun</span>
                    <h2 class="card__title">Akun</h2>
                    <a href="#" class="card__button">Saiba mais</a>
                </div>
            </article>

            <article class="card__article">
                <img src="https://www.opovo.com.br/_midias/jpg/2024/04/09/750x500/1_neymar_santos_jogo_palmeiras_final_paulistao_2024__2_-26355148.jpg" alt="" class="card__img">
                <div class="card__data">
                    <span class="card__description">Adrian é um cientista calmo com personalidade tranquila, mas prefere sempre ver as coisas de muitos ângulos antes de tomar uma decisão.</span>
                    <h2 class="card__title">Adrian</h2>
                    <a href="#" class="card__button">Saiba mais</a>
                </div>
            </article>

            <article class="card__article">
                <img src="https://www.opovo.com.br/_midias/jpg/2024/04/09/750x500/1_neymar_santos_jogo_palmeiras_final_paulistao_2024__2_-26355148.jpg" alt="" class="card__img">
                <div class="card__data">
                    <span class="card__description">Soldado da nova geração tecnológica e armas de ponta, entretanto com um passado obscuro.</span>
                    <h2 class="card__title">Draco</h2>
                    <a href="#" class="card__button">Saiba mais</a>
                </div>
            </article>

            <article class="card__article">
                <img src="https://www.opovo.com.br/_midias/jpg/2024/04/09/750x500/1_neymar_santos_jogo_palmeiras_final_paulistao_2024__2_-26355148.jpg" alt="" class="card__img">
                <div class="card__data">
                    <span class="card__description">Um robô ou humano, um verdadeiro cyborg. Velocidade, Dano, Som: você escolhe o que preferir.</span>
                    <h2 class="card__title">Joel</h2>
                    <a href="#" class="card__button">Saiba mais</a>
                </div>
            </article>
        </div>
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
