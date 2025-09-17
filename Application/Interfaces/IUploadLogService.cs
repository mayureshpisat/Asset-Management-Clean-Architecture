using Application.Services;
namespace Application.Interfaces
{

    public interface IUploadLogService
    {
        void UpdateLog(string filename, string importType);
        Dictionary<DateTime, UploadLogEntry> GetUploadLogs();

    }
}
