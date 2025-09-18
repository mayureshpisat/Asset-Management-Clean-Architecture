using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IQueueService
    {
        void Enque(int assetId);

        bool TryDequeue(out int assetId);

    }
}
