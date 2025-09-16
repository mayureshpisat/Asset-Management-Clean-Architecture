using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISignalsService
    {
        Task<IEnumerable<Signal>> GetSignals(int assetId);

        Task<Signal> GetSpecificSignal(int assetId, int signalId);

        Task AddSignal(int assetId, GlobalSignalDTO signal);

        Task UpdateSignal(int assetId, int signalId , GlobalSignalDTO signal);

        Task DeleteSignal(int signalId, int assetId);
    }
}
