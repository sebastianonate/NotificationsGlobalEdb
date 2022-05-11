using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure.BlobStorage;
using System;
using System.IO;

namespace Notifications.Infrastructure.Logs
{
    public class LogService<T>: IDisposable where T : class 
    {
        //.
        private string folderPath = string.Empty;
        private readonly IHostingEnvironment _env;
        private readonly BlobStorageService _blobStorageService;
        public LogService(IHostingEnvironment env, BlobStorageService blobStorageService)
        {
            _env = env;
            _blobStorageService = blobStorageService;
        }
        public void  LogWrite(string level, string evento, string logMessage)
        {
            DateTime utcTime = DateTime.UtcNow;
            TimeZoneInfo myZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTime currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, myZone);

            var modulo = typeof(T).FullName.ToString();
            string date = currentDateTime.ToString("dd/MM/yyyy").Replace('/', '-');
            folderPath = _env.ContentRootPath + Path.DirectorySeparatorChar + "Logs";
            using (var stream = File.Open(folderPath + Path.DirectorySeparatorChar + $"LOG_{date}.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                
                string fecha = currentDateTime.ToString();
                string dataasstring = $"{fecha}, [{level}] {modulo}, {evento}: {logMessage} \n";
                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                stream.Write(info, 0, info.Length);
                stream.Close();
            // await _blobStorageService.SaveHostedFile($"LOG_{date}.txt", folderPath);
            }
        }

        public void DeleteFileLogs(int numberDelete)
        {
            var folderPath = _env.ContentRootPath + Path.DirectorySeparatorChar + "Logs";

            string[] files = Directory.GetFiles(folderPath, "*.txt");
            int i = 1;
            foreach (var fi in files)
            {
                // Remove path from the file name.
                string fName = fi.Substring(folderPath.Length + 1);   
                if (i > numberDelete)
                {
                    try
                    {
                        // Check if file exists with its full path 
                        if (File.Exists(Path.Combine(folderPath, fName)))
                        {
                            Console.WriteLine(fi);
                            // If file found, delete it    
                            File.Delete(Path.Combine(folderPath, fName));
                            Console.WriteLine("File deleted.");
                        } else
                        {
                            Console.WriteLine("Not exists file path");
                        }
                    }
                    catch (DirectoryNotFoundException dirNotFound)
                    {
                        Console.WriteLine(dirNotFound.Message);
                    }
                }
                i++;
            }
        }

        public void  LogInformation(string evento, string logMessage) {
             LogWrite("INFO", evento, logMessage);
        }

        public void LogInformation(string evento, string logMessage, Guid traceId)
        {
            LogWrite("INFO", evento, $"{traceId} - {logMessage}");
        }

        public void LogError(string evento, string logMessage, Guid traceId)
        {

            LogWrite("ERROR", evento, $"{traceId} - {logMessage}");
        }

        public void  LogError(string evento, string logMessage) { 
        
             LogWrite("ERROR", evento, logMessage);
        }
        public void LogWarning(string evento, string logMessage)
        {
             LogWrite("ADVERTENCIA", evento, logMessage);
        }


        public void Dispose()
        {
        }
    }
}
