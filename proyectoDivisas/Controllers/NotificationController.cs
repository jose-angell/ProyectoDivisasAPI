using Microsoft.AspNetCore.Mvc;
using proyectoDivisas.Models;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace WebApiPrototipos.Controllers
{
    [ApiController]
    [Route("Alerta/notificacion")]
    public class NotificationController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, StreamWriter> _clients = new ConcurrentDictionary<string, StreamWriter>();

        [HttpGet("subscribe")]
        public async Task SubScribe(CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            var cliendId = Guid.NewGuid().ToString();
            var cliendStream = new StreamWriter(Response.Body, Encoding.UTF8);
            _clients.TryAdd(cliendId, cliendStream);
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                _clients.TryRemove(cliendId, out _);
            }
            finally
            {
                if (_clients.TryRemove(cliendId, out var stream))
                {
                    await stream.DisposeAsync();
                }
            }
        }
        public static async Task SendNotificationAsync(Alerta message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            foreach (var client in _clients.Values)
            {
                try
                {
                    await client.WriteLineAsync($"data: {jsonMessage} \n");
                    await client.FlushAsync();
                }
                catch (Exception) { }
            }
        }
    }
}
