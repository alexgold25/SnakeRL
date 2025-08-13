using System.Threading.Tasks;
using Grpc.Net.Client;

namespace SnakeRL.Grpc
{
    public class SnakeEnvClient
    {
        private readonly Snake.SnakeClient _client;

        public SnakeEnvClient(string address)
        {
            var channel = GrpcChannel.ForAddress(address);
            _client = new Snake.SnakeClient(channel);
        }

        public Task<ResetResponse> ResetAsync()
        {
            return _client.ResetAsync(new ResetRequest()).ResponseAsync;
        }

        public Task<StepResponse> StepAsync(Action action)
        {
            var request = new StepRequest { Action = action };
            return _client.StepAsync(request).ResponseAsync;
        }
    }
}
