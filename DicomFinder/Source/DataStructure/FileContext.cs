namespace DicomFinder
{
    public class FileContext
    {
        public string Path { get; }
        public string Value { get; }
        public PreambleStatus PreambleStatus { get; }

        public FileContext(string path, string value, PreambleStatus preambleStatus)
        {
            Path = path;
            Value = value;
            PreambleStatus = preambleStatus;
        }
    }
}
