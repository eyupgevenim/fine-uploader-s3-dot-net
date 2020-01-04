using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebApp.Services
{
    public class PictureService : IPictureService
    {
        /// <summary>
        /// pictures url file
        /// </summary>
        private const string Path = "pictures_url_data.txt";
        /// <summary>
        /// temp pictures url file
        /// </summary>
        private const string TempPath = "temp_pictures_url_data.txt";
        /// <summary>
        /// lock thread safe file read write 
        /// </summary>
        private readonly object _lock = new object();

        private readonly IAwsS3Service awsService;
        public PictureService(IAwsS3Service awsService)
        {
            this.awsService = awsService;
        }

        /// <summary>
        /// Delete picture
        /// </summary>
        /// <param name="url">picture url</param>
        public void DeletePicture(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            lock (_lock)
            {
                using (StreamReader sr = File.OpenText(Path))
                using (StreamWriter sw = new StreamWriter(TempPath, false))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line != url)
                            sw.WriteLine(line);
                    }
                }

                File.Delete(Path);
                File.Move(TempPath, Path);
            }

            awsService.DeleteObjectAsync(url).Wait();
        }

        /// <summary>
        /// Get all pictures
        /// </summary>
        /// <returns>pictures url list</returns>
        public List<string> GetAllPicture()
        {
            lock (_lock)
            {
                var lines = File.ReadAllLines(Path);
                return lines.ToList(); 
            }
        }

        /// <summary>
        /// Insert picture
        /// </summary>
        /// <param name="url">picture url</param>
        public void InsertPicture(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            lock (_lock)
            {
                using (StreamWriter sw = File.AppendText(Path))
                    sw.WriteLine(url); 
            }
        }
    }
}
