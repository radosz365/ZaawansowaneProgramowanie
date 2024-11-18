using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

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

    private int ExecutePythonScript(string scriptPath, string imagePath)
    {
        try
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"\"{scriptPath}\" \"{imagePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    process.WaitForExit();
                    return int.Parse(result.Trim());
                }
            }
        }
        catch (Exception ex)
        {
            return -1;
        }
    }
}
