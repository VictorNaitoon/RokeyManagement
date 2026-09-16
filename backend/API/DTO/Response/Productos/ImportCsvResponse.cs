namespace API.DTO.Response.Productos
{
    public class ImportError
    {
        public int Row { get; set; }
        public string Column { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ImportCsvResponse
    {
        public int TotalRows { get; set; }
        public int Created { get; set; }
        public int Skipped { get; set; }
        public List<ImportError> Errors { get; set; } = new();
    }
}
