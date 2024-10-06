using Microsoft.AspNetCore.Mvc;
using proyectoDivisas.Models;
using proyectoDivisas.Repositories;
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
        private readonly ExternalApiDivisas externalApiDivisas;
        private readonly IAlertaDivisasCollection db;

        public NotificationController(ExternalApiDivisas externalApiDivisas, IAlertaDivisasCollection db)
        {
            this.externalApiDivisas = externalApiDivisas;
            this.db = db;
        }

        [HttpGet("subscribe")]
        public async Task SubScribe(CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
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

        [HttpGet("CheckNotification")]
        public async Task<ActionResult> CheckNotification()
        {
            var alertas = await db.ReadAllAlertas();
            if (alertas.Count != 0)
            {
                var tasks = alertas.Select(async alerta =>
                {
                    var from = alerta.DivisaBase;
                    var to = alerta.DivisaContraparte;
                    var divisa = await externalApiDivisas.GetExternalData(from, to);
                    
                    alerta.ValorActual = divisa[to];
                });

                await Task.WhenAll(tasks);

                var tasksValidaLimites = alertas.Select(async alerta =>
                {
                    var limiteMinimo = alerta.Minimo;
                    var limiteMaximo = alerta.Maximo;
                    var valorActual = alerta.ValorActual;
                    if(limiteMaximo != 0 && valorActual != 0)
                    {
                        if (valorActual >= limiteMaximo)
                        {
                            alerta.LimiteMaximoAlcanzado = true;
                            alerta.LimiteMinimoAlcanzado = false;
                            await db.UpdateAlerta(alerta);
                            await SendNotificationAsync(alerta);
                        }
                    }
                    if(limiteMinimo != 0 && valorActual != 0)
                    {
                        if (valorActual <= limiteMinimo)
                        {
                            alerta.LimiteMinimoAlcanzado = true;
                            alerta.LimiteMaximoAlcanzado = false;
                            await db.UpdateAlerta(alerta);
                            await SendNotificationAsync(alerta);
                        }
                    }
                    
                });

                await Task.WhenAll(tasksValidaLimites);
            }
            return Ok(new { succes = true, message = "Alertas revisadas" });
        }

        [HttpGet("ManualMinimoNotification")]
        public async Task<ActionResult> ManualMinimoNotification()
        {
            var alerta = new Alerta();
            alerta.Id = "66d3c3e9024c7eb9e2c8d212";
            alerta.DivisaBase = "USD";
            alerta.DivisaContraparte = "MNX";
            alerta.Minimo = 19.745f;
            alerta.Maximo = 22.825f;
            alerta.ValorActual = 18.923f;
            alerta.LimiteMinimoAlcanzado = true;
            alerta.LimiteMaximoAlcanzado = false;

            await SendNotificationAsync(alerta);
            return Ok(new { succes = true, message = "Alertas notificada" });
        }


        [HttpGet("ManualMaximoNotification")]
        public async Task<ActionResult> ManualMaximoNotification()
        {
            var alerta = new Alerta();
            alerta.Id = "66d3c3e9024c7eb9e2c8d212";
            alerta.DivisaBase = "USD";
            alerta.DivisaContraparte = "MNX";
            alerta.Minimo = 19.745f;
            alerta.Maximo = 22.825f;
            alerta.ValorActual = 23.923f;
            alerta.LimiteMinimoAlcanzado = false;
            alerta.LimiteMaximoAlcanzado = true;

            await SendNotificationAsync(alerta);
            return Ok(new { succes = true, message = "Alertas notificada" });
        }
    }
}
