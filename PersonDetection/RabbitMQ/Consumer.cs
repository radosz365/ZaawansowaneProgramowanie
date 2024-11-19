using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonDetection.RabbitMQ;

public class Consumer
{
    private readonly ConcurrentBag<string> _processedTasks;
    private readonly int _totalTasks;
    private readonly HttpClient _httpClient;

    public Consumer(ConcurrentBag<string> processedTasks, int totalTasks)
    {
        _processedTasks = processedTasks;
        _totalTasks = totalTasks;
        _httpClient = new HttpClient();
    }

    public void StartConsumer()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "image_tasks",
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<TaskMessage>(body);
                string taskId = message?.TaskId ?? Guid.NewGuid().ToString();
                string imageUrl = message?.ImageUrl ?? string.Empty;

                Console.WriteLine($"Processing Task: {taskId}");

                try
                {
                    string endpointUrl = $"http://localhost:5260/Home/GetPersonCountFromUrl?imageUrl={Uri.EscapeDataString(imageUrl)}";
                    Console.WriteLine($"Calling endpoint: {endpointUrl}");

                    var response = await _httpClient.GetAsync(endpointUrl);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseContent);
                    var personCount = document.RootElement.GetProperty("count").GetInt32();

                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "SavedImages");
                    Directory.CreateDirectory(folderPath);

                    string fileExtension = Path.GetExtension(new Uri(imageUrl).AbsolutePath) ?? ".jpg";
                    string fileName = $"{taskId}-{personCount}{fileExtension}";
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var imageResponse = await _httpClient.GetAsync(imageUrl))
                    {
                        imageResponse.EnsureSuccessStatusCode();
                        var imageData = await imageResponse.Content.ReadAsByteArrayAsync();
                        await File.WriteAllBytesAsync(filePath, imageData);
                    }

                    Console.WriteLine($"Task {taskId} completed. File saved: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing task {taskId}: {ex.Message}");
                }
                finally
                {
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine($"Task {taskId} acknowledged.");
                }
            };

            channel.BasicConsume(queue: "image_tasks",
                                    autoAck: false,
                                    consumer: consumer);

            Console.WriteLine("Waiting for messages. Press [enter] to exit.");
            Console.ReadLine();
        }
    }

    private class TaskMessage
    {
        public string TaskId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}