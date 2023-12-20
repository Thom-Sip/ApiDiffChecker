namespace RefactorHelper.Models
{
    public class RefactorHelperState
    {
        public string SwaggerJson { get; set; } = string.Empty;

        public int CurrentRequest { get; set; }

        public List<RequestWrapper> Data { get; set; } = [];

        public RequestWrapper GetCurrentRequest() => Data[CurrentRequest];
    }
}
