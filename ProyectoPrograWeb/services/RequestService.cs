using Google.Cloud.Firestore;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class RequestService
{
    private readonly FirebaseService _firebaseservice;
    private readonly DeliveryService _deliveryService;
    private readonly NotificationService _notificationService;

    public RequestService(FirebaseService firebaseservice, DeliveryService deliveryService, NotificationService notificationService)
    {
        _firebaseservice = firebaseservice;
        _deliveryService = deliveryService;
        _notificationService = notificationService;
    }

    public async Task<DonationRequestDTo> CrearSolicitudAsync(string postId, string receiverId, string receiverName)
    {
        try
        {
            var colPublicaciones = _firebaseservice.GetCollection("DonationPosts");
            var snapshotPost = await colPublicaciones.Document(postId).GetSnapshotAsync();

            if (!snapshotPost.Exists)
                throw new Exception("No se encontro la publicacion solicitada");

            var publicacion = snapshotPost.ConvertTo<DonationPost>();

            if (publicacion.DonorId == receiverId)
                throw new Exception("No puedes solicitar tu propio articulo");

            if (publicacion.Status.ToLower() != "disponible")
                throw new Exception($"Este articulo no esta disponible. Estado: {publicacion.Status}");

            var colSolicitudes = _firebaseservice.GetCollection("DonationRequests");
            var solicitudesExistentes = await colSolicitudes
                .WhereEqualTo("PostId", postId)
                .WhereEqualTo("ReceiverId", receiverId)
                .WhereIn("Status", new[] { "pendiente", "aceptada" })
                .GetSnapshotAsync();

            if (solicitudesExistentes.Count > 0)
                throw new Exception("Ya enviaste una solicitud para este articulo.");

            var nuevaSolicitud = new DonationRequest
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

    public async Task<DonationRequestDTo> SeleccionarReceptorAsync(string postId, string requestId, string donorId)
    {
        try
        {
            var colPublicaciones = _firebaseservice.GetCollection("DonationPosts");
            var snapshotPost = await colPublicaciones.Document(postId).GetSnapshotAsync();

            if (!snapshotPost.Exists)
                throw new Exception("No se encontro la publicacion");

            var publicacion = snapshotPost.ConvertTo<DonationPost>();

            if (publicacion.DonorId != donorId)
                throw new Exception("No tienes permiso para seleccionar receptor en esta publicacion");

            if (publicacion.Status.ToLower() != "disponible")
                throw new Exception("Este articulo ya no esta disponible");

            var colSolicitudes = _firebaseservice.GetCollection("DonationRequests");
            var snapshotSolicitud = await colSolicitudes.Document(requestId).GetSnapshotAsync();

            if (!snapshotSolicitud.Exists)
                throw new Exception("La solicitud no fue encontrada");

            var solicitudGanadora = snapshotSolicitud.ConvertTo<DonationRequest>();

            await colSolicitudes.Document(requestId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", "aceptada" },
                { "RespondedAt", DateTime.UtcNow }
            });

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

            await colPublicaciones.Document(postId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", "reservado" },
                { "SelectedReceiverId", solicitudGanadora.ReceiverId },
                { "ReservedAt", DateTime.UtcNow }
            });
            
            
            //crear notificacion para el receptor seleccionado
            await _notificationService.CreateNotification(
                solicitudGanadora.ReceiverId,
                $"Has sido seleccionado para recibir el articulo: {publicacion.ItemName}",
                "seleccionado",
                postId
            );

            solicitudGanadora.Status = "aceptada";
            return ConvertirADto(solicitudGanadora);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al seleccionar receptor: {ex.Message}");
        }
    }

    public async Task<List<DonationRequestDTo>> ObtenerSolicitudesPorPostAsync(string postId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var resultado = await coleccion
                .WhereEqualTo("PostId", postId)
                .GetSnapshotAsync();

            var lista = new List<DonationRequestDTo>();
            foreach (var doc in resultado.Documents)
                lista.Add(ConvertirADto(doc.ConvertTo<DonationRequest>()));

            return lista;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener solicitudes: {ex.Message}");
        }
    }

    public async Task<List<DonationRequestDTo>> ObtenerSolicitudesPorReceptorAsync(string receiverId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var resultado = await coleccion
                .WhereEqualTo("ReceiverId", receiverId)
                .GetSnapshotAsync();

            var lista = new List<DonationRequestDTo>();
            foreach (var doc in resultado.Documents)
                lista.Add(ConvertirADto(doc.ConvertTo<DonationRequest>()));

            return lista;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener solicitudes del receptor: {ex.Message}");
        }
    }

    public async Task<DonationRequestDTo> ObtenerSolicitudPorIdAsync(string requestId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var snapshot = await coleccion.Document(requestId).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("La solicitud no fue encontrada");

            return ConvertirADto(snapshot.ConvertTo<DonationRequest>());
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al buscar la solicitud: {ex.Message}");
        }
    }

    public async Task<DonationRequestDTo> ActualizarEstadoSolicitudAsync(string requestId, string nuevoEstado)
    {
        try
        {
            var estadosPermitidos = new[] { "aceptada", "rechazada", "cancelada" };
            if (!estadosPermitidos.Contains(nuevoEstado))
                throw new Exception($"Estado no valido. Permitidos: {string.Join(", ", estadosPermitidos)}");

            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var snapshot = await coleccion.Document(requestId).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("La solicitud no fue encontrada");

            var solicitud = snapshot.ConvertTo<DonationRequest>();
            var respondedAt = DateTime.UtcNow;

            await coleccion.Document(requestId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", nuevoEstado },
                { "RespondedAt", respondedAt }
            });

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

    public async Task<bool> CancelarSolicitudAsync(string requestId, string receiverId)
    {
        try
        {
            var coleccion = _firebaseservice.GetCollection("DonationRequests");
            var snapshot = await coleccion.Document(requestId).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("La solicitud no fue encontrada");

            var solicitud = snapshot.ConvertTo<DonationRequest>();

            if (solicitud.ReceiverId != receiverId)
                throw new Exception("No tienes permiso para cancelar esta solicitud");

            if (solicitud.Status.ToLower() != "pendiente")
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

    public async Task<bool> ConfirmarRecepcionAsync(string requestId, string receiverId)
    {
        try
        {
            var colSolicitudes = _firebaseservice.GetCollection("DonationRequests");
            var snapshot = await colSolicitudes.Document(requestId).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new Exception("La solicitud no fue encontrada");

            var solicitud = snapshot.ConvertTo<DonationRequest>();

            if (solicitud.ReceiverId != receiverId)
                throw new Exception("No tienes permiso para confirmar esta solicitud");

            if (solicitud.Status.ToLower() != "aceptada")
                throw new Exception("Solo puedes confirmar recepcion de solicitudes aceptadas");

            var colPublicaciones = _firebaseservice.GetCollection("DonationPosts");
            var snapshotPost = await colPublicaciones.Document(solicitud.PostId).GetSnapshotAsync();

            if (!snapshotPost.Exists)
                throw new Exception("No se encontro la publicacion");

            var publicacion = snapshotPost.ConvertTo<DonationPost>();

            if (publicacion.Status.ToLower() != "entregado")
                throw new Exception("El donante aun no ha confirmado la entrega del articulo");

            // Actualizar solicitud a recibida
            await colSolicitudes.Document(requestId).UpdateAsync(new Dictionary<string, object>
            {
                { "Status", "recibida" },
                { "RespondedAt", DateTime.UtcNow }
            });

            // Crear DeliveryRecord automaticamente
            var existing = await _deliveryService.GetByPostId(solicitud.PostId);
            if (existing == null)
            {
                var dto = new DeliveryDTo
                {
                    DeliveryDate = DateTime.UtcNow,
                    DeliveryLocation = publicacion.Zone,
                    ConfirmedByDonor = true,
                    ConfirmedByReceiver = true
                };
                await _deliveryService.Create(dto, publicacion);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al confirmar recepcion: {ex.Message}");
        }
    }

    private DonationRequestDTo ConvertirADto(DonationRequest solicitud)
    {
        return new DonationRequestDTo
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