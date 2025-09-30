
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Application.Interfaces
{
    public interface IQueueService : IDisposable
    {
        void Enqueue(int assetId);
        IModel GetChannel(); // Add this to expose the channel



    }
}
