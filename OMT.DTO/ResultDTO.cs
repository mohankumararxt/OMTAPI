namespace OMT.DTO
{
    public class ResultDTO
    {
        public bool IsSuccess { get; set; }
        public dynamic Data { get; set; }
        public string Message { get; set; }
        public string StatusCode { get; set; }

    }
}
