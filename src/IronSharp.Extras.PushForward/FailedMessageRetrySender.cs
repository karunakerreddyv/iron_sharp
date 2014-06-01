﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IronSharp.IronMQ;

namespace IronSharp.Extras.PushForward
{
    public class FailedMessageRetrySender : IFailedMessageRetrySender
    {
        private readonly PushForwardClient _client;
        private readonly QueueClient _queueClient;

        public FailedMessageRetrySender(PushForwardClient client, QueueClient queueClient)
        {
            _client = client;
            _queueClient = queueClient;
        }

        public async Task<MessageIdCollection> ResendFailedMessages(CancellationToken cancellationToken, int? limit = null)
        {
            QueueInfo queueInfo = await _queueClient.Info();

            string errorQueueName = queueInfo.ErrorQueue;

            var result = new MessageIdCollection();

            if (string.IsNullOrEmpty(errorQueueName))
            {
                return result;
            }

            QueueClient errorQ = _client.IronMqClient.Queue(errorQueueName);

            QueueMessage next;

            int count = 0;

            while (errorQ.Read(out next))
            {
                if (cancellationToken.IsCancellationRequested || (limit.HasValue && count >= limit.Value))
                {
                    break;
                }

                string messageId = await _queueClient.Post(next);

                result.Ids.Add(messageId);

                if (result.Success)
                {
                    await next.Delete();
                }

                count++;
            }

            result.Message = "Messages put on queue.";

            return result;
        }
    }
}