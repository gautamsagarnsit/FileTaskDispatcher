namespace AutoFileDispatcher.Common
{

    public enum FileEventType
    {
        FileCreated,
        FileDeleted,
        FileChanged,
        FileRenamed
    }

    public enum FileEventAction
    {
        Email,
        FTP,
        Printer
    }
  

}