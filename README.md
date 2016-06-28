# Calculadora RPC-RabbitMQ


Exemplo de calculadora de operações básicas como: 
- Adição, subtração, multiplicação e divisão, usando webapi e rabbitmq com padrão rpc.

### Chamando a API
Use o [postman](https://www.getpostman.com/) ou outro client para enviar a chamada para a API pela URL:
```
http://localhost:[porta]/Calculator/Calculate
```

Passando os valores:
```
{
  "ValueX": 6,
  "ValueY": 2,
  "Operation": Sum
}
```

Onde..
- ValueX e ValueY são os valores de entrada para o cálculo
- Operation são operações: Sum, Sub, Div, Mult


### Resultado esperado
```
{
  "Result": 3,
}
```


### Como funciona?

O client chama a API passando o request (ValueX, ValueY e Operation),
A API envia para a fila rpc_queue criada pelo Consumer application, 
A API cria uma fila com o nome do guid gerado no controller, que vai ser utilizada para receber a resposta do Consumer application,
O Consumer application fica esperando chegar a resposta na fila rpc_queue,
O Consumer application recebe o request na fila rpc_queue e enviada para o processador fazer o cálculo e retorna para a fila especificada pela API (no campo replyTo), 
A API que já estava aguardando alguma mensagem nessa fila, recebe a mensagem e responde para o client que a chamou.

Great Success!
