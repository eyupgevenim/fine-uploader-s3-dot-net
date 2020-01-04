using System.Collections.Generic;

namespace WebApp.Services
{
    public interface IPictureService
    {
        /// <summary>
        /// Delete picture
        /// </summary>
        /// <param name="url">picture url</param>
        void DeletePicture(string url);

        /// <summary>
        /// Get all pictures
        /// </summary>
        /// <returns>pictures url list</returns>
        List<string> GetAllPicture();

        /// <summary>
        /// Insert picture
        /// </summary>
        /// <param name="url">picture url</param>
        void InsertPicture(string url);
    }
}
