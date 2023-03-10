namespace ImageApi.RequestModels
{
    public class ImageDownloadRequestModel {
        public IEnumerable<string> ImageUrls { get; set; }
        public int MaxDownloadAtOnce { get; set; }
    }
}