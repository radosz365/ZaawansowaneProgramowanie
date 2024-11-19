using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace PersonDetection.RabbitMQ;

public class Producer
{
    private readonly string _queueName = "image_tasks";

    public void EnqueueTask(string imageUrl)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: _queueName,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var taskId = Guid.NewGuid().ToString();
            var taskMessage = new TaskMessage
            {
                TaskId = taskId,
                ImageUrl = imageUrl
            };

            var messageBody = JsonSerializer.Serialize(taskMessage);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                    routingKey: _queueName,
                                    basicProperties: properties,
                                    body: body);

            Console.WriteLine($"Task {taskId} enqueued with URL: {imageUrl}");
        }
    }

    private class TaskMessage
    {
        public string TaskId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
