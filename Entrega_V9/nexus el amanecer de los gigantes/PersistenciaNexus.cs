using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace nexus_el_amanecer_de_los_gigantes
{
    public sealed class PerfilNexus
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public override string ToString() { return Nombre; }
    }

    public sealed class PartidaNexus
    {
        public int Id { get; set; }
        public int PerfilId { get; set; }
        public string Perfil { get; set; }
        public string Muralla { get; set; }
        public int Salud { get; set; }
        public int Energia { get; set; }
        public int Ataque { get; set; }
        public int Experiencia { get; set; }
        public int Escudos { get; set; }
        public int Materiales { get; set; }
        public int Suministros { get; set; }
        public int Nivel { get; set; }
        public int Eliminaciones { get; set; }
        public int ExperienciaOleada { get; set; }
        public int TiempoOleada { get; set; }
        public int Fila { get; set; }
        public int Columna { get; set; }
        public bool JefeDerrotado { get; set; }
        public bool Terminada { get; set; }
        public string CasillasProcesadas { get; set; }
        public DateTime Actualizada { get; set; }
        public List<string> Inventario { get; set; }
        public string Descripcion
        {
            get { return Muralla + " · Nivel " + Nivel + " · " + Actualizada.ToString("dd/MM/yyyy HH:mm"); }
        }
        public override string ToString() { return Descripcion; }
    }

    public sealed class PersistenciaNexus
    {
        private readonly string cadena;

        public PersistenciaNexus()
        {
            ConnectionStringSettings configuracion = ConfigurationManager.ConnectionStrings["NexusDb"];
            if (configuracion == null || String.IsNullOrWhiteSpace(configuracion.ConnectionString))
                throw new InvalidOperationException("Falta la cadena de conexión NexusDb en App.config.");
            cadena = configuracion.ConnectionString;
        }

        private SqlConnection Abrir()
        {
            SqlConnection conexion = new SqlConnection(cadena);
            conexion.Open();
            return conexion;
        }

        public void VerificarConexion()
        {
            using (SqlConnection conexion = Abrir())
            using (SqlCommand comando = new SqlCommand("SELECT 1 FROM dbo.Perfiles", conexion))
                comando.ExecuteScalar();
        }

        public List<PerfilNexus> ObtenerPerfiles()
        {
            List<PerfilNexus> resultado = new List<PerfilNexus>();
            using (SqlConnection conexion = Abrir())
            using (SqlCommand comando = new SqlCommand("SELECT PerfilId, Nombre FROM dbo.Perfiles ORDER BY Nombre", conexion))
            using (SqlDataReader lector = comando.ExecuteReader())
                while (lector.Read()) resultado.Add(new PerfilNexus { Id = lector.GetInt32(0), Nombre = lector.GetString(1) });
            return resultado;
        }

        public PerfilNexus CrearPerfil(string nombre)
        {
            if (String.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Escribe un nombre para el perfil.");
            nombre = nombre.Trim();
            using (SqlConnection conexion = Abrir())
            using (SqlCommand comando = new SqlCommand("INSERT INTO dbo.Perfiles(Nombre) OUTPUT INSERTED.PerfilId VALUES(@nombre)", conexion))
            {
                comando.Parameters.Add("@nombre", SqlDbType.NVarChar, 60).Value = nombre;
                int id = Convert.ToInt32(comando.ExecuteScalar());
                comando.CommandText = "INSERT INTO dbo.Cadetes(PerfilId, Nombre, Salud, Energia, Ataque, Experiencia, Escudos, Materiales, Suministros) VALUES(@id, @nombre, 100, 100, 20, 0, 0, 0, 0)";
                comando.Parameters.Clear();
                comando.Parameters.Add("@id", SqlDbType.Int).Value = id;
                comando.Parameters.Add("@nombre", SqlDbType.NVarChar, 60).Value = nombre;
                comando.ExecuteNonQuery();
                return new PerfilNexus { Id = id, Nombre = nombre };
            }
        }

        public List<PartidaNexus> ObtenerPartidas(int perfilId, bool incluirTerminadas)
        {
            List<PartidaNexus> partidas = new List<PartidaNexus>();
            const string sql = "SELECT p.PartidaId, p.PerfilId, f.Nombre, p.Muralla, p.Salud, p.Energia, p.Ataque, p.Experiencia, p.Escudos, p.Materiales, p.Suministros, p.Nivel, p.Eliminaciones, p.ExperienciaOleada, p.TiempoOleada, p.Fila, p.Columna, p.JefeDerrotado, p.Terminada, p.CasillasProcesadas, p.FechaActualizacion FROM dbo.Partidas p INNER JOIN dbo.Perfiles f ON f.PerfilId=p.PerfilId WHERE p.PerfilId=@perfil AND (@terminadas=1 OR p.Terminada=0) ORDER BY p.FechaActualizacion DESC";
            using (SqlConnection conexion = Abrir())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@perfil", SqlDbType.Int).Value = perfilId;
                comando.Parameters.Add("@terminadas", SqlDbType.Bit).Value = incluirTerminadas;
                using (SqlDataReader lector = comando.ExecuteReader())
                    while (lector.Read())
                    {
                        partidas.Add(new PartidaNexus {
                            Id=lector.GetInt32(0), PerfilId=lector.GetInt32(1), Perfil=lector.GetString(2), Muralla=lector.GetString(3),
                            Salud=lector.GetInt32(4), Energia=lector.GetInt32(5), Ataque=lector.GetInt32(6), Experiencia=lector.GetInt32(7),
                            Escudos=lector.GetInt32(8), Materiales=lector.GetInt32(9), Suministros=lector.GetInt32(10), Nivel=lector.GetInt32(11),
                            Eliminaciones=lector.GetInt32(12), ExperienciaOleada=lector.GetInt32(13), TiempoOleada=lector.GetInt32(14),
                            Fila=lector.GetInt32(15), Columna=lector.GetInt32(16), JefeDerrotado=lector.GetBoolean(17), Terminada=lector.GetBoolean(18),
                            CasillasProcesadas=lector.GetString(19), Actualizada=lector.GetDateTime(20), Inventario=new List<string>()
                        });
                    }
            }
            foreach (PartidaNexus partida in partidas) partida.Inventario = ObtenerInventario(partida.Id);
            return partidas;
        }

        private List<string> ObtenerInventario(int partidaId)
        {
            List<string> objetos = new List<string>();
            using (SqlConnection conexion = Abrir())
            using (SqlCommand comando = new SqlCommand("SELECT Nombre FROM dbo.Inventario WHERE PartidaId=@id ORDER BY InventarioId", conexion))
            {
                comando.Parameters.Add("@id", SqlDbType.Int).Value = partidaId;
                using (SqlDataReader lector = comando.ExecuteReader()) while (lector.Read()) objetos.Add(lector.GetString(0));
            }
            return objetos;
        }

        public int CrearPartida(int perfilId, Cadete cadete, string muralla)
        {
            using (SqlConnection conexion = Abrir())
            using (SqlCommand comando = new SqlCommand("INSERT INTO dbo.Partidas(PerfilId, Muralla, Salud, Energia, Ataque, Experiencia, Escudos, Materiales, Suministros, Nivel, Eliminaciones, ExperienciaOleada, TiempoOleada, Fila, Columna, JefeDerrotado, Terminada, CasillasProcesadas) OUTPUT INSERTED.PartidaId VALUES(@perfil,@muralla,@salud,@energia,@ataque,@experiencia,@escudos,@materiales,@suministros,1,0,0,0,1,1,0,0,N'')", conexion))
            {
                AgregarEstadisticas(comando, perfilId, cadete, muralla);
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public void Guardar(PartidaNexus p)
        {
            using (SqlConnection conexion = Abrir())
            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                try
                {
                    using (SqlCommand comando = new SqlCommand("UPDATE dbo.Partidas SET Muralla=@muralla, Salud=@salud, Energia=@energia, Ataque=@ataque, Experiencia=@experiencia, Escudos=@escudos, Materiales=@materiales, Suministros=@suministros, Nivel=@nivel, Eliminaciones=@eliminaciones, ExperienciaOleada=@expOleada, TiempoOleada=@tiempo, Fila=@fila, Columna=@columna, JefeDerrotado=@jefe, Terminada=@terminada, CasillasProcesadas=@casillas, FechaActualizacion=GETDATE() WHERE PartidaId=@partida AND PerfilId=@perfil", conexion, transaccion))
                    {
                        comando.Parameters.Add("@muralla", SqlDbType.VarChar, 10).Value=p.Muralla;
                        comando.Parameters.Add("@salud", SqlDbType.Int).Value=p.Salud; comando.Parameters.Add("@energia", SqlDbType.Int).Value=p.Energia;
                        comando.Parameters.Add("@ataque", SqlDbType.Int).Value=p.Ataque; comando.Parameters.Add("@experiencia", SqlDbType.Int).Value=p.Experiencia;
                        comando.Parameters.Add("@escudos", SqlDbType.Int).Value=p.Escudos; comando.Parameters.Add("@materiales", SqlDbType.Int).Value=p.Materiales;
                        comando.Parameters.Add("@suministros", SqlDbType.Int).Value=p.Suministros; comando.Parameters.Add("@nivel", SqlDbType.Int).Value=p.Nivel;
                        comando.Parameters.Add("@eliminaciones", SqlDbType.Int).Value=p.Eliminaciones; comando.Parameters.Add("@expOleada", SqlDbType.Int).Value=p.ExperienciaOleada;
                        comando.Parameters.Add("@tiempo", SqlDbType.Int).Value=p.TiempoOleada; comando.Parameters.Add("@fila", SqlDbType.Int).Value=p.Fila;
                        comando.Parameters.Add("@columna", SqlDbType.Int).Value=p.Columna; comando.Parameters.Add("@jefe", SqlDbType.Bit).Value=p.JefeDerrotado;
                        comando.Parameters.Add("@terminada", SqlDbType.Bit).Value=p.Terminada; comando.Parameters.Add("@casillas", SqlDbType.NVarChar, 500).Value=p.CasillasProcesadas ?? "";
                        comando.Parameters.Add("@partida", SqlDbType.Int).Value=p.Id; comando.Parameters.Add("@perfil", SqlDbType.Int).Value=p.PerfilId;
                        if (comando.ExecuteNonQuery() != 1) throw new InvalidOperationException("La partida no existe o pertenece a otro perfil.");
                    }
                    using (SqlCommand comando = new SqlCommand("UPDATE dbo.Cadetes SET Nombre=@nombre, Salud=@salud, Energia=@energia, Ataque=@ataque, Experiencia=@experiencia, Escudos=@escudos, Materiales=@materiales, Suministros=@suministros, FechaActualizacion=GETDATE() WHERE PerfilId=@perfil; DELETE FROM dbo.Inventario WHERE PartidaId=@partida; INSERT INTO dbo.EventosPartida(PartidaId, Evento) VALUES(@partida, N'Progreso guardado')", conexion, transaccion))
                    {
                        comando.Parameters.Add("@nombre", SqlDbType.NVarChar, 60).Value=p.Perfil;
                        comando.Parameters.Add("@salud", SqlDbType.Int).Value=p.Salud; comando.Parameters.Add("@energia", SqlDbType.Int).Value=p.Energia;
                        comando.Parameters.Add("@ataque", SqlDbType.Int).Value=p.Ataque; comando.Parameters.Add("@experiencia", SqlDbType.Int).Value=p.Experiencia;
                        comando.Parameters.Add("@escudos", SqlDbType.Int).Value=p.Escudos; comando.Parameters.Add("@materiales", SqlDbType.Int).Value=p.Materiales;
                        comando.Parameters.Add("@suministros", SqlDbType.Int).Value=p.Suministros; comando.Parameters.Add("@perfil", SqlDbType.Int).Value=p.PerfilId;
                        comando.Parameters.Add("@partida", SqlDbType.Int).Value=p.Id; comando.ExecuteNonQuery();
                    }
                    foreach (string objeto in p.Inventario ?? new List<string>())
                    {
                        using (SqlCommand comando = new SqlCommand("INSERT INTO dbo.Inventario(PartidaId, Nombre) VALUES(@partida, @nombre)", conexion, transaccion))
                        {
                            comando.Parameters.Add("@partida", SqlDbType.Int).Value=p.Id;
                            comando.Parameters.Add("@nombre", SqlDbType.NVarChar, 120).Value=objeto;
                            comando.ExecuteNonQuery();
                        }
                    }
                    transaccion.Commit();
                }
                catch { transaccion.Rollback(); throw; }
            }
        }

        public void EliminarPartida(int partidaId, int perfilId)
        {
            using (SqlConnection conexion = Abrir())
            using (SqlCommand comando = new SqlCommand("DELETE FROM dbo.Partidas WHERE PartidaId=@partida AND PerfilId=@perfil", conexion))
            {
                comando.Parameters.Add("@partida", SqlDbType.Int).Value=partidaId;
                comando.Parameters.Add("@perfil", SqlDbType.Int).Value=perfilId;
                comando.ExecuteNonQuery();
            }
        }

        private static void AgregarEstadisticas(SqlCommand c, int perfilId, Cadete cadete, string muralla)
        {
            c.Parameters.Add("@perfil", SqlDbType.Int).Value=perfilId;
            c.Parameters.Add("@muralla", SqlDbType.VarChar, 10).Value=muralla;
            c.Parameters.Add("@salud", SqlDbType.Int).Value=cadete.Salud; c.Parameters.Add("@energia", SqlDbType.Int).Value=cadete.Energia;
            c.Parameters.Add("@ataque", SqlDbType.Int).Value=cadete.Ataque; c.Parameters.Add("@experiencia", SqlDbType.Int).Value=cadete.Experiencia;
            c.Parameters.Add("@escudos", SqlDbType.Int).Value=cadete.Escudos; c.Parameters.Add("@materiales", SqlDbType.Int).Value=cadete.Materiales;
            c.Parameters.Add("@suministros", SqlDbType.Int).Value=cadete.Suministros;
        }
    }
}
