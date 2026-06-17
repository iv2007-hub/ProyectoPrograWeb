using Google.Cloud.Firestore;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

/// <summary>
/// Servicio para la gestión de solicitudes de donación en DonaCerca.
/// </summary>
public class RequestService
{
    private readonly firebaseservice _firebaseservice;

    public RequestService(firebaseservice firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }

    /// <summary>
    /// Registra una nueva solicitud de donación.
    /// Valida que el receptor no sea el mismo donante del artículo.
    /// </summary>
    public async Task<DonationRequestDto> CrearSolicitudAsync(string postId, string receiverId, string receiverName)
    {
        try
        {
            var colPublicaciones = _firebaseservice.GetCollection("DonationPosts");
            var snapshotPost = await colPublicaciones.Document(postId).GetSnapshotAsync();

            if (!snapshotPost.Exists)
                throw new Exception("No se encontró la publicación solicitada");

            var publicacion = snapshotPost.ConvertTo<donationpost>();

            // Validar que el receptor no sea el mismo donante
            if (publicacion.DonorId == receiverId)
                throw new Exception("No puedes solicitar tu propio artículo");

            if (publicacion.Status != "disponible")
                throw new Exception($"Este artículo no está disponible. Estado: {publicacion.Status}");

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
    /// El donante selecciona al receptor ganador.
    /// 1. La solicitud del receptor elegido cambia a "aceptada"
    /// 2. Las otras solicitudes del mismo artículo cambian a "rechazada"
    /// 3. El artículo cambia a "reservado"
    /// 4. Se guarda el receptor seleccionado en el artículo
    /// </summary>
    public async Task<DonationRequestDto> SeleccionarReceptorAsync(string postId, string requestId, string donorId)
    {
        try
        {
            // Verificar que la publicación existe y pertenece al donante
            var colPublicaciones = _firebaseservice.GetCollection("DonationPosts");
            var snapshotPost = await colPublicaciones.Document(postId).GetSnapshotAsync();

            if (!snapshotPost.Exists)
                throw new Exception("No se encontró la publicación");

            var publicacion = snapshotPost.ConvertTo<donationpost>();

            if (publicacion.DonorId != donorId)
                throw new Exception("No tienes permiso para seleccionar receptor en esta publicación");

            if (publicacion.Status != "disponible")
                throw new Exception("Este artículo ya no está disponible");

            // Obtener la solicitud ganadora
            var colSolicitudes = _firebaseservice.GetCollection("DonationRequests");
            var snapshotSolicitud = await colSolicitudes.Document(requestId).GetSnapshotAsync();

            if (!snapshotSolicitud.Exists)
                throw new Exception("La solicitud no fue encontrada");

            var solicitudGanadora = snapshotSolicitud.ConvertTo<donationrequest>();

            // Aceptar la solicitud ganadora
            await colSolicitudes.Document(requestId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", "aceptada" },
                { "RespondedAt", DateTime.UtcNow }
            });

            // Rechazar todas las otras solicitudes del mismo artículo
            var otrasSolicitudes = await colSolicitudes
                .WhereEqualTo("PostId", postId)
                .WhereEqualTo("Status", "pendiente")
                .GetSnapshotAsync();

            foreach (var doc in otrasSolicitudes.Documents)
            {
                if (doc.Id != requestId)
                {
                    await colSolicitudes.Document(doc.Id).UpdateAsync(new Dictionary<string, object>
                    {
                        { "Status", "rechazada" },
                        { "RespondedAt", DateTime.UtcNow }
                    });
                }
            }

            // Actualizar el artículo a "reservado" y guardar el receptor seleccionado
            await colPublicaciones.Document(postId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", "reservado" },
                { "SelectedReceiverId", solicitudGanadora.ReceiverId },
                { "ReservedAt", DateTime.UtcNow }
            });

            solicitudGanadora.Status = "aceptada";
            return ConvertirADto(solicitudGanadora);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al seleccionar receptor: {ex.Message}");
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
    /// Cambia el estado de una solicitud y rechaza las demás si se acepta.
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
            var respondedAt = DateTime.UtcNow;

            await coleccion.Document(requestId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", nuevoEstado },
                { "RespondedAt", respondedAt }
            });

            // Si se acepta, rechazar automáticamente las otras solicitudes
            if (nuevoEstado == "aceptada")
            {
                var otrasSolicitudes = await coleccion
                    .WhereEqualTo("PostId", solicitud.PostId)
                    .WhereEqualTo("Status", "pendiente")
                    .GetSnapshotAsync();

                foreach (var doc in otrasSolicitudes.Documents)
                {
                    if (doc.Id != requestId)
                    {
                        await coleccion.Document(doc.Id).UpdateAsync(new Dictionary<string, object>
                        {
                            { "Status", "rechazada" },
                            { "RespondedAt", respondedAt }
                        });
                    }
                }
            }

            solicitud.Status = nuevoEstado;
            solicitud.RespondedAt = respondedAt;
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
            RespondedAt = solicitud.RespondedAt
        };
    }
}