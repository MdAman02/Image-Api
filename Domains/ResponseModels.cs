namespace ImageApi.ResponseModels
{
    public class DownloadImageResponseModel {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IDictionary<string, string> UrlAndNames { get; set; }
    }
}
