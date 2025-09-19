using Application.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class QueueService : IQueueService
    {
        public ConcurrentQueue<int> queue = new ConcurrentQueue<int>();


        public void Enque(int assetId)
        {
            Console.WriteLine($"{assetId} FROM QUEUE SERVICE");
            queue.Enqueue(assetId);
        }

        public bool TryDequeue(out int assetId)
        {
            return queue.TryDequeue(out assetId);
            
        }


        
    }
}
