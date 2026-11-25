
using System;
using System.Collections.Generic;

namespace nexus_el_amanecer_de_los_gigantes
{
    // ================= PERSONAJE =================

    public class CadeteNexus
    {
        public string Nombre { get; set; }
        public int Salud { get; private set; }
        public int Energia { get; private set; }
        public int Ataque { get; private set; }
        public int Experiencia { get; private set; }
        public int Escudos { get; private set; }

        public bool Vivo
        {
            get { return Salud > 0; }
        }

        public CadeteNexus(string nombre)
        {
            Nombre = nombre;
            Salud = 100;
            Energia = 100;
            Ataque = 20;
            Experiencia = 0;
            Escudos = 0;
        }

        public void RecibirDanio(int cantidad)
        {
            if (Escudos > 0 && cantidad > 0)
            {
                cantidad = (cantidad + 1) / 2;
                Escudos--;
            }

            Salud = Math.Max(0, Salud - cantidad);
        }

        public void GastarEnergia(int cantidad)
        {
            Energia = Math.Max(0, Energia - cantidad);
        }

        public void RecuperarEnergia(int cantidad)
        {
            Energia = Math.Min(100, Energia + cantidad);
        }

        public void Curar(int cantidad)
        {
            Salud = Math.Min(100, Salud + cantidad);
        }

        public void MejorarAtaque(int cantidad)
        {
            Ataque += cantidad;
        }

        public void GanarExperiencia(int cantidad)
        {
            Experiencia += cantidad;
        }

        public void AgregarEscudo()
        {
            Escudos++;
        }
    }

    // ================= INVENTARIO =================

    public class ObjetoNexus
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public ObjetoNexus(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;
        }
    }

    public class InventarioNexus
    {
        public List<ObjetoNexus> Objetos { get; private set; }

        public InventarioNexus()
        {
            Objetos = new List<ObjetoNexus>();
        }

        public void Agregar(ObjetoNexus objeto)
        {
            Objetos.Add(objeto);
        }

        public bool Eliminar(ObjetoNexus objeto)
        {
            return Objetos.Remove(objeto);
        }

        public bool Contiene(string nombre)
        {
            foreach (ObjetoNexus objeto in Objetos)
            {
                if (objeto.Nombre == nombre)
                    return true;
            }

            return false;
        }
    }

    // ================= ENEMIGOS =================

    public class CriaturaNexus
    {
        public string Nombre { get; set; }
        public int Salud { get; private set; }
        public int Ataque { get; set; }
        public string Descripcion { get; set; }

        public bool Vivo
        {
            get { return Salud > 0; }
        }

        public CriaturaNexus(
            string nombre, int salud, int ataque, string descripcion)
        {
            Nombre = nombre;
            Salud = salud;
            Ataque = ataque;
            Descripcion = descripcion;
        }

        public void RecibirDanio(int cantidad)
        {
            Salud = Math.Max(0, Salud - cantidad);
        }
    }

    // ================= MISIONES =================

    public class MisionNexus
    {
        public string Nombre { get; set; }
        public bool ExploracionCompletada { get; set; }
        public bool CombateCompletado { get; set; }
        public bool RecursoEncontrado { get; set; }
        public bool Completada { get; private set; }

        public MisionNexus()
        {
            Nombre = "El territorio olvidado";
        }

        public bool VerificarObjetivos()
        {
            return ExploracionCompletada &&
                   CombateCompletado &&
                   RecursoEncontrado;
        }

        public bool Completar()
        {
            if (!VerificarObjetivos())
                return false;

            Completada = true;
            return true;
        }
    }

    // ================= MAPAS =================

    public class MapaNexus
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }

        public int[,] Territorio { get; private set; }

        public MapaNexus(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;

            // 0 = camino
            // 1 = bosque
            // 2 = peligro
            // 3 = puesto de suministros
            // 4 = base militar

            Territorio = new int[,]
            {
                { 4, 0, 1, 0, 3 },
                { 0, 1, 2, 1, 0 },
                { 0, 0, 0, 2, 0 },
                { 1, 2, 0, 1, 0 },
                { 3, 0, 1, 0, 4 }
            };
        }
    }
}
