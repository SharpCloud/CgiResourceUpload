using System;
using System.Threading.Tasks;

namespace CgiResourceUpload.Interfaces
{
    public interface ILogger
    {
        Task Log(string message);
        Task LogError(string message);
        Task LogWarning(string message);
        Task LogError(Exception e);
        string GetLogFilePath();
    }
}
