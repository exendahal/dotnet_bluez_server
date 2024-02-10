namespace bletest;

public class ResponseModel
{
        public string ResponseMessage { get; set; }
        public int ResponseCode { get; set; }
        public int ChunkSize { get; set; }
        public int CurrentChunk { get; set; }
        public string FileName  { get; set; }
        public int FileLength  { get; set; }
        public string LocationName { get; set; }

}
