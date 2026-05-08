**Explicação** === _NOVA FEATURE_ => POSTS FUNCIONAMENTO

*MODELS*
Models/Post.cs
enumerador de TopicoPost {
    opcao1,
    opcao2,
    opcao3,
    opcao4,
    opcao5
};

Nova migration

*CONTROLLERS*
Controllers/PostController.cs
ViewModel para criar/listar
upload de imagem
exibição na home 
relacionamento com Usuario (model)
Cards dos posts no feed

========

*Fluxo:*

        Usuário logado cria post
Escolhe:
        título
        descrição
        tópico
        imagem opcional
Sistema salva:
        usuário
        data
        imagem
        Home mostra feed dos posts
Cada card mostra:
        imagem
        título
        descrição
        tópico
        autor
        data
==========================

