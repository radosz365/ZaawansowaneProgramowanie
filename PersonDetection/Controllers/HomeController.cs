using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PersonDetection.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetPersonCount()
    {
        string pythonScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "PythonScripts", "person_counter.py");
        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "ti1p3.jpg");

        int personCount = ExecutePythonScript(pythonScriptPath, imagePath);

        return Json(new { count = personCount });
    }

    [HttpGet]
    public IActionResult GetPersonCountFromUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return Json(new { count = -1, error = "Invalid URL" });
        }

        string pythonScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "PythonScripts", "person_counter.py");
        string tempImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "temp_image.jpg");

        try
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(imageUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var imageData = response.Content.ReadAsByteArrayAsync().Result;
                    System.IO.File.WriteAllBytes(tempImagePath, imageData);
                }
                else
                {
                    return Json(new { count = -1, error = "Failed to download image" });
                }
            }

            int personCount = ExecutePythonScript(pythonScriptPath, tempImagePath);

            System.IO.File.Delete(tempImagePath);

            return Json(new { count = personCount });
        }
        catch (Exception ex)
        {
            return Json(new { count = -1, error = ex.Message });
        }
    }


    private int ExecutePythonScript(string scriptPath, string imagePath)
    {
        try
        {
            var start = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"\"{scriptPath}\" \"{imagePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(start);
            if (process == null)
                throw new InvalidOperationException("Failed to start Python process.");

            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            process.WaitForExit();

            if (!int.TryParse(result.Trim(), out int count))
                throw new FormatException("Python script did not return a valid integer.");

            return count;
        }
        catch
        {
            return -1;
        }
    }
}
