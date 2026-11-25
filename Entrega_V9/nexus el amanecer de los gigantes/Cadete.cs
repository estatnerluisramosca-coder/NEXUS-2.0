
using System;
using System.Collections.Generic;

namespace nexus_el_amanecer_de_los_gigantes
{
    public class Cadete
    {
        // IDENTIDAD
        public string Nombre { get; set; }

        // ESTADISTICAS
        public int Salud { get; private set; }
        public int Energia { get; private set; }
        public int Ataque { get; private set; }
        public int Experiencia { get; private set; }
        public int Escudos { get; private set; }

        // RECURSOS
        public int Materiales { get; private set; }
        public int Suministros { get; private set; }

        // INVENTARIO
        public List<string> Inventario { get; private set; }

        public bool Vivo
        {
            get { return Salud > 0; }
        }

        // CONSTRUCTOR
        public Cadete(string nombre)
        {
            Nombre = nombre;

            Salud = 100;
            Energia = 100;
            Ataque = 20;
            Experiencia = 0;
            Escudos = 0;

            Materiales = 0;
            Suministros = 0;

            Inventario = new List<string>();
        }

        public void RestaurarEstado(int salud, int energia, int ataque, int experiencia, int escudos,
            int materiales, int suministros, IEnumerable<string> objetos)
        {
            Salud = Math.Max(0, Math.Min(100, salud));
            Energia = Math.Max(0, Math.Min(100, energia));
            Ataque = Math.Max(1, ataque);
            Experiencia = Math.Max(0, experiencia);
            Escudos = Math.Max(0, escudos);
            Materiales = Math.Max(0, materiales);
            Suministros = Math.Max(0, suministros);
            Inventario.Clear();
            if (objetos != null) Inventario.AddRange(objetos);
        }

        // RECIBIR DAÑO
        public void RecibirDanio(int cantidad)
        {
            if (cantidad <= 0 || !Vivo)
                return;

            if (Escudos > 0)
            {
                cantidad = cantidad / 2;
                Escudos--;
            }

            Salud -= cantidad;

            if (Salud < 0)
                Salud = 0;
        }

        // RECUPERAR SALUD
        public void Curar(int cantidad)
        {
            if (cantidad > 0 && Vivo)
            {
                Salud += cantidad;

                if (Salud > 100)
                    Salud = 100;
            }
        }

        // GASTAR ENERGIA
        public bool GastarEnergia(int cantidad)
        {
            if (cantidad <= 0 || !Vivo)
                return false;

            if (Energia >= cantidad)
            {
                Energia -= cantidad;
                return true;
            }

            return false;
        }

        // RECUPERAR ENERGIA
        public void RecuperarEnergia(int cantidad)
        {
            if (cantidad > 0 && Vivo)
            {
                Energia += cantidad;

                if (Energia > 100)
                    Energia = 100;
            }
        }

        // MEJORAR ATAQUE
        public void MejorarAtaque(int cantidad)
        {
            if (cantidad > 0)
                Ataque += cantidad;
        }

        // EXPERIENCIA
        public void GanarExperiencia(int cantidad)
        {
            if (cantidad > 0)
                Experiencia += cantidad;
        }

        // AGREGAR ESCUDO
        public void AgregarEscudo()
        {
            Escudos++;
        }

        // AGREGAR MATERIALES
        public void AgregarMateriales(int cantidad)
        {
            if (cantidad > 0)
                Materiales += cantidad;
        }

        // UTILIZAR MATERIALES
        public bool GastarMateriales(int cantidad)
        {
            if (cantidad <= 0 || Materiales < cantidad)
                return false;

            Materiales -= cantidad;
            return true;
        }

        // AGREGAR SUMINISTROS
        public void AgregarSuministros(int cantidad)
        {
            if (cantidad > 0)
                Suministros += cantidad;
        }

        // UTILIZAR SUMINISTROS
        public bool UsarSuministro()
        {
            if (Suministros <= 0 || !Vivo)
                return false;

            Suministros--;
            Curar(20);
            RecuperarEnergia(15);

            return true;
        }

        // AGREGAR OBJETO AL INVENTARIO
        public void AgregarObjeto(string objeto)
        {
            if (!string.IsNullOrWhiteSpace(objeto))
                Inventario.Add(objeto);
        }

        // CONTAR OBJETOS
        public int CantidadObjetos()
        {
            return Inventario.Count;
        }

        // MOSTRAR INVENTARIO
        public string ObtenerInventario()
        {
            string resultado = "INVENTARIO DEL CADETE\n\n";

            resultado += "Materiales: " + Materiales + "\n";
            resultado += "Suministros: " + Suministros + "\n";
            resultado += "Escudos: " + Escudos + "\n\n";

            resultado += "OBJETOS ENCONTRADOS:\n";

            if (Inventario.Count == 0)
            {
                resultado += "No tienes objetos todavía.\n";
            }
            else
            {
                for (int i = 0; i < Inventario.Count; i++)
                {
                    resultado += (i + 1) + ". " + Inventario[i] + "\n";
                }
            }

            return resultado;
        }

        // ESTADO DEL CADETE
        public string ObtenerEstado()
        {
            return
                "CADETE: " + Nombre + "\n\n" +
                "❤️ Salud: " + Salud + "/100\n" +
                "⚡ Energía: " + Energia + "/100\n" +
                "⚔️ Ataque: " + Ataque + "\n" +
                "⭐ Experiencia: " + Experiencia + "\n" +
                "🛡️ Escudos: " + Escudos + "\n" +
                "🪵 Materiales: " + Materiales + "\n" +
                "🎒 Suministros: " + Suministros + "\n" +
                "📦 Objetos: " + Inventario.Count + "\n\n" +
                "Estado: " + (Vivo ? "CON VIDA" : "CAÍDO");
        }
    }
}
