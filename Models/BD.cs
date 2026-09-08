using Dapper;
using Microsoft.Data.SqlClient;

namespace TP6_Gorojod_Schwartz_Waserman;

public class BD {
    private string connectionString = "Server=localhost;Database=BaseSala;Trusted_Connection=True;TrustServerCertificate=True;";

    public int crearPartida(Partida partida)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = @"INSERT INTO Partida (FechaInicio, HoraInicio, IdSala, EstadoActual, NombreJugador)
                                          VALUES (@fechaInicio, @horaInicio, @IdSala, @estadoActual, @nombreJugador);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
            return connection.ExecuteScalar<int>(query, partida);
        }
    }
    
    public bool TieneAcceso(int? partidaId, int IdSala)
    {
        if (partidaId == null)
            return false;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT COUNT(*) FROM Partida WHERE Id = @partidaId AND IdSala = @pIdSala";
            int count = connection.ExecuteScalar<int>(query, new { partidaId, pIdSala = IdSala });
            return count > 0;
        }
    }

    public void pasarSala(int partidaId, int nuevaSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "UPDATE Partida SET IdSala = @nuevaSala WHERE Id = @partidaId";
            connection.Execute(query, new { partidaId, nuevaSala });
        }
    }

    public Partida ObtenerPartidaPorNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return null;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT TOP 1 * FROM Partida WHERE NombreJugador = @nombre ORDER BY Id DESC";
            return connection.QueryFirstOrDefault<Partida>(query, new { nombre });
        }
    }
}