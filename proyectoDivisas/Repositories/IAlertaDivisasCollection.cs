using proyectoDivisas.Models;

namespace proyectoDivisas.Repositories
{
    public interface IAlertaDivisasCollection
    {
        Task CreateAlerta(Alerta alerta);
        Task<List<Alerta>> ReadAllAlertas();
        Task<Alerta> ReadAlertaPorId(string id);
        Task UpdateAlerta(Alerta alerta);
        Task DeleteAlerta(string id);
    }
}
