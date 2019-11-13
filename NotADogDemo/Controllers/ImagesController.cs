using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using NotADog.Services;
using System.Collections.Generic;
using System.IO;
using NotADog.Models;
using Microsoft.Extensions.Options;

namespace NotADog.Controllers
{
    public class ImagesController : Controller
    {
        private readonly IBlobStorageManager _blobStorageManager;
        private readonly AzureStorageConfig storageConfig = null;

        public ImagesController(IBlobStorageManager blobStorageManager, IOptions<AzureStorageConfig> config)
        {
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
            storageConfig = config.Value;
        }

        public IActionResult Index()
        {
            var files = _blobStorageManager.GetFiles("images").Select(item => item.Uri).ToList();
            
            if(files.Count == 0){
                _blobStorageManager.UploadFile("https://github.com/alicejgibbons/cancwic-2019/raw/master/medias/bianca-tennis.png", "images");
                _blobStorageManager.UploadFile("https://github.com/alicejgibbons/cancwic-2019/raw/master/medias/raptors-basketball.png", "images");
                files = _blobStorageManager.GetFiles("images").Select(item => item.Uri).ToList();
            }

            // Logic here for displaying tags

            var tag_files = _blobStorageManager.GetFiles("tags").ToList();
            IList<string> tag_file_list = new List<string>();
            System.Diagnostics.Debug.WriteLine($"CREATING TAG LISTS");

            foreach (CloudBlob b in tag_files)
            {
                string text;
                using (var memoryStream = new MemoryStream())
                {
                    b.DownloadToStream(memoryStream);
                    var length = memoryStream.Length;
                    text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                System.Diagnostics.Debug.WriteLine($"TEXT IS: {text}");
                tag_file_list.Add(text);
            }

            System.Diagnostics.Debug.WriteLine($"Image List creation completed, list contents: {files.Count}");
            System.Diagnostics.Debug.WriteLine($"List creation completed, list size: {tag_file_list.Count}");

            for (int i=0; i < tag_file_list.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine($"Item URI {files[i]} tags: {tag_file_list[i]}");
            }

            if (tag_files.Count == 0)
            {
                _blobStorageManager.UploadFile("https://github.com/alicejgibbons/cancwic-2019/raw/master/medias/bianca-tennis", "tags");
                _blobStorageManager.UploadFile("https://github.com/alicejgibbons/cancwic-2019/raw/master/medias/raptors-basketball", "tags");
                tag_files = _blobStorageManager.GetFiles("tags").ToList();
            }

            ViewBag.Tag_files = tag_file_list;
            ViewBag.Files = files;
            return View();
        }
    }
}