using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebApiPrototipos.Controllers;

namespace proyectoDivisas.Repositories
{
    public class MonitorService: IHostedService, IDisposable
    {
        private readonly ILogger<MonitorService> _logger;
        private readonly ExternalApiDivisas externalApiDivisas;
        private readonly IAlertaDivisasCollection db;

        //private IAlertaDivisasCollection db = new AlertaDivisaCollection();
        private Timer _timer;

        public MonitorService(ILogger<MonitorService> logger, ExternalApiDivisas externalApiDivisas, IAlertaDivisasCollection db)
        {
            _logger = logger;
            this.externalApiDivisas = externalApiDivisas;
            this.db = db;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Monitor Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Monitor Service is working.");
            var alertas = await db.ReadAllAlertas();
            if (alertas.Count != 0)
            {
                var tasks = alertas.Select(async alerta =>
                {
                    var from = alerta.DivisaBase;
                    var to = alerta.DivisaContraparte;
                    var divisa = await externalApiDivisas.GetExternalData($"/latest?from={from}&to={to}");
                    var exchangeRates = JsonDocument.Parse(divisa);
                    
                    if (exchangeRates.RootElement.TryGetProperty("rates", out JsonElement ratesElement) &&
                        ratesElement.TryGetProperty(to, out JsonElement rateValue))
                    {
                        alerta.ValorActual = (float)rateValue.GetDouble();
                    }
                });

                await Task.WhenAll(tasks);

                var tasksValidaLimites = alertas.Select(async alerta =>
                {
                    var limiteMinimo = alerta.Minimo;
                    var limiteMaximo = alerta.Maximo;
                    var valorActual = alerta.ValorActual;
                    if( limiteMaximo >=  valorActual )
                    {
                        alerta.LimiteMaximoAlcanzado = true;
                        alerta.LimiteMinimoAlcanzado = false;
                        await db.UpdateAlerta(alerta);
                        await NotificationController.SendNotificationAsync(alerta);
                    }
                    else if ( limiteMinimo <= valorActual)
                    {
                        alerta.LimiteMinimoAlcanzado = true;
                        alerta.LimiteMaximoAlcanzado = false;
                        await db.UpdateAlerta(alerta);
                        await NotificationController.SendNotificationAsync(alerta);
                    }
                });

                await Task.WhenAll(tasksValidaLimites);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Monitor Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
