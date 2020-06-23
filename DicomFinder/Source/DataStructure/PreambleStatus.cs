namespace DicomFinder
{
    public enum PreambleStatus
    {
        None,
        Ok,
        MismatchPreamble128,
        WrongPreambleDicm
    }
}