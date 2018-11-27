namespace Briver.Logging.Configuration
{
    public class FileLogWriter
    {
        public bool Enabled { get; set; } = true;
        public string FilePrefix { get; set; }
        public string OutputDir { get; set; }
    }
}
