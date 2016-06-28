﻿using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Web.Http;
using WebAPI.DataContracts;

namespace WebAPI.Controllers {

    [RoutePrefix(@"api/Calculator")]
    public class CalculatorController : ApiController {

        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private QueueingBasicConsumer consumer;


        [HttpPost]
        [Route(@"Calculate")]
        public IHttpActionResult Calculate(CalculateRequest request) {
            //Definindo response
            CalculateResponse response = null;

            //Serializando o request
            var jsonCalculateRequest = JsonConvert.SerializeObject(request);
            var guidGeneratedInRequest = Guid.NewGuid().ToString();

            //Criando conexão, canal do rabbit
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            //declarando a fila de callback do rabbit, usando como nome, o guid gerado no request
            channel.QueueDeclare(guidGeneratedInRequest, false, false, true, arguments:null);

            //Criando um Header
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();

            //preenchendo o header
            props.ReplyTo = guidGeneratedInRequest;
            props.CorrelationId = corrId;

            //transformando o json do request em bytes
            var bytesCalculateRequest = Encoding.UTF8.GetBytes(jsonCalculateRequest);

            //PUBLICANDO NA FILA, passando os props para poder acontecer o reply
            channel.BasicPublish(exchange: "", //enviando para o exchange default
                                 routingKey: "rpc_queue",
                                 basicProperties: props,
                                 body: bytesCalculateRequest);

            //CRIANDO CONSUMER na fila de callback
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queue: guidGeneratedInRequest,
                                 noAck: true,
                                 consumer: consumer);

            try {

                while (true) {
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    //verifica se a chave do response, é igual a chave passada no request
                    if (ea.BasicProperties.CorrelationId == corrId) {
                        //return Encoding.UTF8.GetString(ea.Body);

                        var responseFromRPC = Encoding.UTF8.GetString(ea.Body);

                        response = JsonConvert.DeserializeObject<CalculateResponse>(responseFromRPC);

                        if (response != null) {
                            return Ok(response);
                        }
                        return BadRequest();
                    }
                }

                #region Consumindo com Evento
                //var consumerTag = string.Empty;

                //var consumer = new EventingBasicConsumer(channel);

                //consumer.Received += (model, ea) => {
                //    var body = ea.Body;

                //    //OnMessageReceived?.Invoke(body, body.Length, ea.BasicProperties);
                //    //todo:Eduardo Souza testando ack(noAck: de true para false)
                //    channel.BasicAck(ea.DeliveryTag, false);
                //    if (ea.BasicProperties.CorrelationId == corrId) {
                //        //        //return Encoding.UTF8.GetString(ea.Body);
                //        //    }

                //        var responseFromRPC = Encoding.UTF8.GetString(ea.Body);

                //        response = JsonConvert.DeserializeObject<CalculateResponse>(responseFromRPC);

                //    }
                //};

                //consumerTag = channel.BasicConsume(queue: QueueName, noAck: false, consumer: consumer);
                //manualResetEvent.WaitOne();
                #endregion


            } catch (Exception ex) {
                //Channel.BasicCancel(consumerTag);
                return BadRequest(ex.Message);
            }

        }
    }
}