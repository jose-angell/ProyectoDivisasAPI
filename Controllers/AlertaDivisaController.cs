using Microsoft.AspNetCore.Mvc;
using proyectoDivisas.Models;
using proyectoDivisas.Repositories;
using System.Text.RegularExpressions;
using System.Text.Json;
using WebApiPrototipos.Controllers;

namespace proyectoDivisas.Controllers
{
    [Route("api/AlertasDivisas")]
    [ApiController]
    public class AlertaDivisaController : Controller
    {
        //private IAlertaDivisasCollection db = new AlertaDivisaCollection();
        private readonly ExternalApiDivisas externalApiDivisas;
        private readonly IAlertaDivisasCollection db;

        public AlertaDivisaController(ExternalApiDivisas externalApiDivisas, IAlertaDivisasCollection db)
        {
            this.externalApiDivisas = externalApiDivisas;
            this.db = db;
        }

        [HttpGet("ReadAll")]
        public async Task<ActionResult> ReadAllAlertas()
        {
            var alertas = await db.ReadAllAlertas();
            if (alertas.Count == 0)
            {
                return NotFound(new { success = false, message = "No existe alertas guardadas" });
            }
            var tasks = alertas.Select(async alerta =>
            {
                var from = alerta.DivisaBase;
                var to = alerta.DivisaContraparte;
                var divisa = await externalApiDivisas.GetExternalData(from, to);
                
                alerta.ValorActual = divisa[to];
            });

            await Task.WhenAll(tasks);

            return Ok(new { success = true, data = alertas });
        }

        [HttpGet("ReadById/{id}")]
        public async Task<ActionResult> ReadAlertaPorId(string id)
        {

            var esIdValido = Regex.IsMatch(id, @"^[0-9a-fA-F]{24}$");
            if (!esIdValido)
            {
                return NotFound(new { success = false, message = "Id invalido" });
            }

            var alerta = await db.ReadAlertaPorId(id);
            if (alerta == null)
            {
                return NotFound(new { success = false, message = "Alerta no encontrada" });
            }

            var from = alerta.DivisaBase;
            var to = alerta.DivisaContraparte;
            var divisa = await externalApiDivisas.GetExternalData(from, to);
            
            alerta.ValorActual = divisa[to];
            return Ok(new { success = true, data = alerta });
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateAlerta([FromBody] Alerta alerta)
        {
            if (alerta is null)
            {
                return BadRequest();
            }
            alerta.LimiteMinimoAlcanzado = false;
            alerta.LimiteMaximoAlcanzado = false;
            await db.CreateAlerta(alerta);
            return Created("Created", true);
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult> UpdateAlerta([FromBody] Alerta alerta, string id)
        {
            var esIdValido = Regex.IsMatch(id, @"^[0-9a-fA-F]{24}$");
            if (!esIdValido)
            {
                return NotFound(new { success = false, message = "Id invalido" });
            }
            if (alerta is null)
            {
                return BadRequest();
            }
            alerta.Id = id;
            var alertaValidacioon = await db.ReadAlertaPorId(id);
            if (alertaValidacioon == null)
            {
                return NotFound(new { success = false, message = "Alerta no encontrada" });
            }
            await db.UpdateAlerta(alerta);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteAlerta(string id)
        {
            var esIdValido = Regex.IsMatch(id, @"^[0-9a-fA-F]{24}$");
            if (!esIdValido)
            {
                return NotFound(new { success = false, message = "Id invalido" });
            }
            var alertaValidacioon = await db.ReadAlertaPorId(id);
            if (alertaValidacioon == null)
            {
                return NotFound(new { success = false, message = "Alerta no encontrada" });
            }
            await db.DeleteAlerta(id);
            return NoContent();
        }
    }
}
