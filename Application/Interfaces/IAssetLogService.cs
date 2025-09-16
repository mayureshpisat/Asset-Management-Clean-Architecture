using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAssetLogService
    {

        Task Log(string action, string? asset = null, string? signal = null);
    }
}
