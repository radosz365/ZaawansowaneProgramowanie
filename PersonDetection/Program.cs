using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PersonDetection.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var serverTask = Task.Run(() =>
{
    Console.WriteLine("Starting ASP.NET MVC server...");
    app.Run();
});

Console.WriteLine("Do you want to run RabbitMQ test? (y/n):");
string input = Console.ReadLine()?.ToLower() ?? "n";

if (input == "y")
{
    RunRabbitMqTest();
}
else
{
    serverTask.Wait();
}

static void RunRabbitMqTest()
{
    Console.WriteLine("Starting RabbitMQ test...");

    const int totalTasks = 1000;
    const int totalConsumers = 10;

    var processedTasks = new ConcurrentBag<string>();

    var producer = new Producer();
    string imageUrl = "https://img.freepik.com/free-photo/lovely-brunette-young-female-with-tanned-skin-toothy-smile-dark-long-hair-dressed-casual-swetaer-happy-spend-free-time-company-best-friend-smile-positively-together-stand-indoor_273609-16189.jpg?t=st=1731971704~exp=1731975304~hmac=468177232cac8d20ef7df51bc142e9aa6bfbbe04c3a88b2d8471f70949e5c934&w=1060";
    for (int i = 0; i < totalTasks; i++)
    {
        producer.EnqueueTask(imageUrl);
    }
    Console.WriteLine($"{totalTasks} tasks enqueued.");

    var tasks = new Task[totalConsumers];
    for (int i = 0; i < totalConsumers; i++)
    {
        int consumerId = i + 1;
        tasks[i] = Task.Run(() =>
        {
            var consumer = new Consumer(processedTasks, totalTasks);
            Console.WriteLine($"Starting Consumer {consumerId}");
            consumer.StartConsumer();
        });
    }

    Task.WhenAll(tasks).Wait();

    Console.WriteLine("All tasks processed. Exiting RabbitMQ test...");
}