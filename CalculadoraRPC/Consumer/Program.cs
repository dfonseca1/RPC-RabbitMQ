using Consumer.DataContracts;
using Consumer.Processor;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Consumer {

    internal class Program {

        private static void Main(string[] args) {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            //criando a conexao
            using (var connection = factory.CreateConnection())
            //criando o canal e declarando a fila
            using (var channel = connection.CreateModel()) {
                channel.QueueDeclare(queue: "rpc_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                //o segundo parametro usado no BaicQos, serve para criarmos quantos server process quisermos.
                channel.BasicQos(0, 1, false);
                //criando um basic consumer para acessar a fila
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(queue: "rpc_queue",
                                     noAck: false,
                                     consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");

                //usado para ficar pegando os requests e criando os responses
                while (true) {
                    CalculateResponse response = null;

                    //pegando o request
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                    //pegando as informações do request
                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();

                    replyProps.CorrelationId = props.CorrelationId;

                    try {
                        var message = Encoding.UTF8.GetString(body);

                        var calculateRequest = JsonConvert.DeserializeObject<CalculateRequest>(message);

                        Console.WriteLine("--------------------Processing a new request!--------------------");
                        Console.WriteLine();
                        Console.WriteLine("Fila:{0}", ea.BasicProperties.ReplyTo);
                        Console.WriteLine();
                        Console.WriteLine(" [Request] - ValueX:{0}, ValueY:{1}, Operation:{2}", calculateRequest.ValueX, calculateRequest.ValueY, calculateRequest.Operation.ToString());

                        //preenchendo o response
                        response = CalculateProcessor.Calculate(calculateRequest);

                        Console.WriteLine(" [Response] - Result:{0}", response.Result);

                    } catch (Exception e) {
                        Console.WriteLine(" [Exception] " + e.Message);
                        response = new CalculateResponse() { Result = 0 };
                    } finally {
                        //Convertendo o response
                        var responseJson = JsonConvert.SerializeObject(response);

                        //publicando o response na fila de resposta para o cliente
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                        channel.BasicPublish(exchange: "",
                                             routingKey: props.ReplyTo,
                                             basicProperties: replyProps,
                                             body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                         multiple: false);
                    }
                }
            }
        }
    }
}