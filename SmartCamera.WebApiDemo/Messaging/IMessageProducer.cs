using System.Threading;
using System.Threading.Tasks;

namespace SmartCamera.WebApiDemo.Messaging
{
    /// <summary>
    /// Defines the contract for a message producer.
    /// </summary>
    public interface IMessageProducer
    {
        /// <summary>
        /// Publishes a message asynchronously.
        /// </summary>
        /// <param name="exchange">The target exchange.</param>
        /// <param name="routingKey">The routing key for the message.</param>
        /// <param name="message">The message object to be serialized and sent.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PublishAsync(string exchange, string routingKey, object message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the producer is connected and ready to publish messages.
        /// </summary>
        bool IsConnected { get; }
    }
}
