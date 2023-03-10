using System;
using System.IO;
using System.Net;
using ImageApi.RequestModels;
using ImageApi.ResponseModels;

namespace ImageApi.Services
{
    public interface IImageService {
        public Task<DownloadImageResponseModel> DownloadAndStoreImage (ImageDownloadRequestModel request);
        public string GetImageByName (string imageName);
    }

    public class ImageService : IImageService {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _webHost;
        private Dictionary<string, string> scopedResult = new Dictionary<string, string>();
        private List<Task> awaitingTasks = new List<Task>();

        public ImageService(IHttpClientFactory httpClientFactory, IWebHostEnvironment webHost) {
            this._httpClientFactory = httpClientFactory;
            this._webHost = webHost;
        }

        private async Task copyToLocalStorage (HttpResponseMessage response, string path) {
            using (var fileStream = new FileStream(path, FileMode.Create)) {
                HttpContent content = response.Content;
                await content.CopyToAsync(fileStream);
                fileStream.Close();
            }
        }

        private List<List<string>> groupUrls (List<string> urls, int groupCount) {
            List<List<string>> groupedUrls = new List<List<string>>();
            groupedUrls.Add(new List<string>());
            int t = 0;
            for (int i=0; i<urls.Count; i++) {
                if (i % groupCount == 0) {
                    groupedUrls.Add(new List<string>());
                }
                List<string> currGroup = groupedUrls.Last();
                currGroup.Add(urls[i]);
            }
            return groupedUrls;
        }

        private async Task<List<Tuple<string, HttpResponseMessage>>> processUrlGroups (List<string> urls, HttpClient httpClient)
        {
            List<Tuple<string, HttpResponseMessage>> res = new List<Tuple<string, HttpResponseMessage>>();

            var tasks = urls.ConvertAll(async url => {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage resp = await httpClient.SendAsync(httpRequestMessage);
                res.Add(new Tuple<string, HttpResponseMessage>(url, resp));
                return;
            });
            Task.WaitAll(tasks.ToArray());
            // Run further processing on different thread
            Task.Run(() => {
                var task = this.saveDownloadedImages(res);
                this.awaitingTasks.Add(task);
                return;
            });
            return res;
        }

        private async Task saveDownloadedImages (List<Tuple<string, HttpResponseMessage>> downloadedImages)
        {
            var tasks = downloadedImages.ConvertAll(async item => {
                var guid = Guid.NewGuid();
                string guidString = guid.ToString();
                await this.copyToLocalStorage(item.Item2, this._webHost.ContentRootPath + $"/images/image-{guidString}.jpg");
                scopedResult.Add(item.Item1, $"image-{guidString}.jpg");
            });
            Task.WaitAll(tasks.ToArray());
            return;
        }

        public async Task<DownloadImageResponseModel> DownloadAndStoreImage (ImageDownloadRequestModel request)
        {
            var httpClient = this._httpClientFactory.CreateClient();
            HashSet<string> urlSet = new HashSet<string>(request.ImageUrls);
            List<string> imageUrls = urlSet.ToList();
            var groupedUrls = groupUrls(imageUrls, request.MaxDownloadAtOnce);

            for (int i=0; i<groupedUrls.Count; i++) {
                await this.processUrlGroups(groupedUrls[i], httpClient);
            }

            // Await for pending tasks involving local storage write
            Task.WaitAll(this.awaitingTasks.ToArray());
            
            return new DownloadImageResponseModel(){
                Success =  true,
                Message = "Successfully Downloaded Image",
                UrlAndNames = this.scopedResult,
            };
        }

        public string GetImageByName (string imageName)
        {
            string result = "";
            using(FileStream stream = File.OpenRead(this._webHost.ContentRootPath + $"/images/{imageName}")) {
                MemoryStream mem = new MemoryStream();
                stream.CopyTo(mem);
                byte[] buffer = mem.ToArray();
                result += Convert.ToBase64String(buffer);
            }
            return result;
        }
    }
}