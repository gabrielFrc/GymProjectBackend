// sobre tabelas e consultas, tomar cuidado pra não fazer uma varredura completa
porque algumas consultas podem não utilizar indices e acabar realizando uma varredura completa,
onde pode comprometer a eficiencia e desempenho. Tente usar ferramentas que apontam se uma consulta
está realizando uma varredura completa

// Por que falo sobre isso? porque quando faz uma pesquisa de academias mais proximas nesse software
antigamente ele faria uma verificação de 3 em 3 dados da tabela de academias, agora ele pega todos os
dados pra poder filtrar de forma que sempre vai retornar a mais proxima de todas que estão dentro da tabela