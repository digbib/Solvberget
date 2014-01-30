﻿using System.IO;
using System.Web;

namespace Solvberget.Domain.Utils
{
    public class StorageHelper
    {

        private readonly string _pathToImageCache = null;

        public StorageHelper(string pathToImageCache)
        {
            _pathToImageCache = pathToImageCache;
        }


        public string GetLocalImageFileCacheUrl(string id, bool isThumb)
        {
            if (!Directory.Exists(_pathToImageCache))
                return string.Empty;

            var fileName = isThumb ? Path.Combine(_pathToImageCache, "thumb" + id + ".jpg") : Path.Combine(_pathToImageCache, id + ".jpg");

            if (File.Exists(fileName))
            {
                var imageName = isThumb ? "thumb" + id + ".jpg" : id + ".jpg";
                return imageName;
            }
            return string.Empty;
        }
    }
}
