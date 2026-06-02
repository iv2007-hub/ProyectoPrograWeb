using Google.Cloud.Firestore;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

/// <summary>
/// servicio para la gestion de solicitudes de donacion en DonaCerca.
/// permite a los receptores solicitar articulos y a los donantes
/// administrar las solicitudes recibidas.
/// </summary>
public class RequestService
{
    private readonly firebaseservice _firebaseservice;

    public RequestService(firebaseservice firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }

    /// <summary>
    /// registra una nueva solicitud de donacion por parte de un receptor.
    /// </summary>
    public async Task<DonationRequestDto> CrearSolicitudAsync(string postId, string receiverId, string receiverName)
    {
        try
        {
            var colPublicaciones = _firebaseservice.GetCollection("DonationPosts");
            var snapshotPost = await colPublicaciones.Document(postId).GetSnapshotAsync();

            if (!snapshotPost.Exists)
                throw new Exception("No se encontro la publicacion solicitada");

            var publicacion = snapshotPost.ConvertTo<donationpost>();

            if (publicacion.Status != "Disponible")
                throw new Exception($"Este articulo no esta disponible. Estado: {publicacion.Status}");

            var colSolicitudes = _firebaseservice.GetCollection("DonationRequests");
            var solicitudesExistentes = await colSolicitudes
                .WhereEqualTo("PostId", postId)
                .WhereEqualTo("ReceiverId", receiverId)
                .WhereIn("Status", new[] { "pendiente", "aceptada" })
                .GetSnapshotAsync();

            if (solicitudesExistentes.Count > 0)
                throw new Exception("Ya enviaste una solicitud para este artículo.");

            var nuevaSolicitud = new donationrequest
            {
                Id = Guid.NewGuid().ToString(),
                PostId = postId,
                ReceiverId = receiverId,
                ReceiverName = receiverName,
                Status = "pendiente",
                RequestTimestamp = DateTime.UtcNow
            };

            await colSolicitudes.Document(nuevaSolicitud.Id).SetAsync(new Dictionary<string, object>
            {
                { "Id", nuevaSolicitud.Id },
                { "PostId", nuevaSolicitud.PostId },
                { "ReceiverId", nuevaSolicitud.ReceiverId },
                { "ReceiverName", nuevaSolicitud.ReceiverName },
                { "Status", nuevaSolicitud.Status },
                { "RequestTimestamp", nuevaSolicitud.RequestTimestamp }
            });

            return ConvertirADto(nuevaSolicitud);
        }
        catch (Exception ex)
        {
            throw new Exception($"No se pudo crear la solicitud: {ex.Message}");
        }
    }

    /// <summary>
    /// Devuelve todas las solicitudes de una publicación.
    /// </summary>
    public async Task<List<DonationRequestDto>> ObtenerSolicitudesPorPostAsync(string postId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var resultado = await coleccion
                .WhereEqualTo("PostId", postId)
                .GetSnapshotAsync();

            var lista = new List<DonationRequestDto>();
            foreach (var doc in resultado.Documents)
                lista.Add(ConvertirADto(doc.ConvertTo<donationrequest>()));

            return lista;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener solicitudes: {ex.Message}");
        }
    }

    /// <summary>
    /// Devuelve todas las solicitudes de un receptor.
    /// </summary>
    public async Task<List<DonationRequestDto>> ObtenerSolicitudesPorReceptorAsync(string receiverId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var resultado = await coleccion
                .WhereEqualTo("ReceiverId", receiverId)
                .GetSnapshotAsync();

            var lista = new List<DonationRequestDto>();
            foreach (var doc in resultado.Documents)
                lista.Add(ConvertirADto(doc.ConvertTo<donationrequest>()));

            return lista;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener solicitudes del receptor: {ex.Message}");
        }
    }

    /// <summary>
    /// Busca una solicitud por su ID.
    /// </summary>
    public async Task<DonationRequestDto> ObtenerSolicitudPorIdAsync(string requestId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var snapshot = await coleccion.Document(requestId).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("La solicitud no fue encontrada");

            return ConvertirADto(snapshot.ConvertTo<donationrequest>());
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al buscar la solicitud: {ex.Message}");
        }
    }

    /// <summary>
    /// Cambia el estado de una solicitud.
    /// </summary>
    public async Task<DonationRequestDto> ActualizarEstadoSolicitudAsync(string requestId, string nuevoEstado)
    {
        try
        {
            var estadosPermitidos = new[] { "aceptada", "rechazada", "cancelada" };
            if (!estadosPermitidos.Contains(nuevoEstado))
                throw new Exception($"Estado no válido. Permitidos: {string.Join(", ", estadosPermitidos)}");

            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var snapshot = await coleccion.Document(requestId).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("La solicitud no fue encontrada");

            var solicitud = snapshot.ConvertTo<donationrequest>();

            await coleccion.Document(requestId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", nuevoEstado },
                { "RespondedAt", DateTime.UtcNow }
            });

            solicitud.Status = nuevoEstado;
            return ConvertirADto(solicitud);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al actualizar el estado: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancela una solicitud pendiente.
    /// </summary>
    public async Task<bool> CancelarSolicitudAsync(string requestId, string receiverId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var snapshot = await coleccion.Document(requestId).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("La solicitud no fue encontrada");

            var solicitud = snapshot.ConvertTo<donationrequest>();

            if (solicitud.ReceiverId != receiverId)
                throw new Exception("No tienes permiso para cancelar esta solicitud");

            if (solicitud.Status != "pendiente")
                throw new Exception("Solo puedes cancelar solicitudes pendientes");

            await coleccion.Document(requestId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", "cancelada" },
                { "RespondedAt", DateTime.UtcNow }
            });

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al cancelar la solicitud: {ex.Message}");
        }
    }

    private DonationRequestDto ConvertirADto(donationrequest solicitud)
    {
        return new DonationRequestDto
        {
            Id = solicitud.Id,
            PostId = solicitud.PostId,
            ReceiverId = solicitud.ReceiverId,
            ReceiverName = solicitud.ReceiverName,
            Status = solicitud.Status,
            RequestTimestamp = solicitud.RequestTimestamp,
            RespondedAt = null
        };
    }
}