using Google.Cloud.Firestore;

namespace ProyectoPrograWeb.services;

public class Firebaseservice
{
    //este archivo sirve de fuente de comunicacion entre la app y firebase
    //todas las operaciones pasan por aqui si o si
    private readonly FirestoreDb _firestoreDb;

    public Firebaseservice()
    {
        //le vamos a decir a firebase donde esta el archivo con las creedenciales
        //usamos las rutas del folder para encontrar
        
        var credentialPath =Path.Combine(AppContext.BaseDirectory, "Config", "firebase-credentials.json");
        
        //una variable para que podamos utilizar sdk de google
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
        
        //ahora agregamos el id del proyecto para acceso a la fb
        _firestoreDb = FirestoreDb.Create("chechoproyecto-848");
    }

    public CollectionReference GetCollection (string collectionName)
    {
        return _firestoreDb.Collection(collectionName);
    }
}