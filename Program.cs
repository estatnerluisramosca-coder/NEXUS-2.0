
using System;
using System.Collections.Generic;

// ============================================================
// NEXUS v1.0 - ECOS DEL VACIO
// Juego de exploracion, decisiones y supervivencia
// ============================================================

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "NEXUS v1.0 - Ecos del Vacio";
        JuegoNexus juego = new JuegoNexus();
        juego.Iniciar();
    }
}

// ======================= PERSONAJES ==========================

abstract class Personaje
{
    public string Nombre { get; protected set; }
    public int Salud { get; protected set; }
    public int Ataque { get; protected set; }

    public bool Vivo
    {
        get { return Salud > 0; }
    }

    protected Personaje(string nombre, int salud, int ataque)
    {
        Nombre = nombre;
        Salud = salud;
        Ataque = ataque;
    }

    public virtual void RecibirDanio(int cantidad)
    {
        if (cantidad < 0)
            cantidad = 0;

        Salud = Math.Max(0, Salud - cantidad);
    }
}

class Cadete : Personaje
{
    public int Energia { get; private set; }
    public int Experiencia { get; private set; }
    public int Escudos { get; private set; }

    public Cadete(string nombre) : base(nombre, 100, 20)
    {
        Energia = 70;
        Experiencia = 0;
        Escudos = 0;
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

    public override void RecibirDanio(int cantidad)
    {
        if (Escudos > 0 && cantidad > 0)
        {
            cantidad = (cantidad + 1) / 2;
            Escudos--;
            Console.WriteLine("El escudo reduce el impacto.");
        }

        base.RecibirDanio(cantidad);
    }

    public void MostrarEstado()
    {
        Console.WriteLine("\n--- ESTADO DEL CADETE ---");
        Console.WriteLine("Nombre: " + Nombre);
        Console.WriteLine("Salud: " + Salud + "/100");
        Console.WriteLine("Energia: " + Energia + "/100");
        Console.WriteLine("Ataque: " + Ataque);
        Console.WriteLine("Escudos: " + Escudos);
        Console.WriteLine("Experiencia: " + Experiencia + " XP");
    }
}

// ======================== ENTIDADES ==========================

abstract class Entidad : Personaje
{
    public string Descripcion { get; protected set; }

    protected Entidad(
        string nombre,
        int salud,
        int ataque,
        string descripcion)
        : base(nombre, salud, ataque)
    {
        Descripcion = descripcion;
    }

    public abstract void Presentarse();

    public abstract void Atacar(Cadete cadete);
}

class Robot : Entidad
{
    public Robot()
        : base(
            "VIGIA-07",
            40,
            12,
            "Robot de seguridad de una civilizacion perdida.")
    {
    }

    public override void Presentarse()
    {
        Console.WriteLine("\nROBOT DETECTADO: " + Nombre);
        Console.WriteLine(Descripcion);
        Console.WriteLine("Sistema defensivo activado.");
    }

    public override void Atacar(Cadete cadete)
    {
        Console.WriteLine("El robot dispara un pulso defensivo.");
        cadete.RecibirDanio(Ataque);
    }
}

class Enemigo : Entidad
{
    public Enemigo()
        : base(
            "ACECHADOR DEL VACIO",
            50,
            16,
            "Entidad hostil transformada por la anomalia.")
    {
    }

    public override void Presentarse()
    {
        Console.WriteLine("\nENTIDAD HOSTIL: " + Nombre);
        Console.WriteLine(Descripcion);
        Console.WriteLine("La entidad se prepara para atacar.");
    }

    public override void Atacar(Cadete cadete)
    {
        Console.WriteLine("El acechador se lanza contra el cadete.");
        cadete.RecibirDanio(Ataque);
    }
}

// ========================= INVENTARIO ========================

class Objeto
{
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }

    public Objeto(string nombre, string descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }
}

class Inventario
{
    private List<Objeto> objetos = new List<Objeto>();

    public int Cantidad
    {
        get { return objetos.Count; }
    }

    public void Agregar(Objeto objeto)
    {
        objetos.Add(objeto);
        Console.WriteLine("Objeto obtenido: " + objeto.Nombre);
    }

    public Objeto Obtener(int indice)
    {
        if (indice < 0 || indice >= objetos.Count)
            return null;

        return objetos[indice];
    }

    public bool Contiene(string nombre)
    {
        foreach (Objeto objeto in objetos)
        {
            if (objeto.Nombre.Equals(
                nombre,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool Eliminar(Objeto objeto)
    {
        if (objeto == null)
            return false;

        if (objeto.Nombre == "Nucleo energetico")
        {
            Console.WriteLine(
                "No puedes eliminar un objeto obligatorio de la mision.");
            return false;
        }

        return objetos.Remove(objeto);
    }

    public bool Consumir(Objeto objeto)
    {
        return objetos.Remove(objeto);
    }

    public void Mostrar()
    {
        Console.WriteLine("\n--- INVENTARIO ---");

        if (objetos.Count == 0)
        {
            Console.WriteLine("No tienes objetos.");
            return;
        }

        for (int i = 0; i < objetos.Count; i++)
        {
            Console.WriteLine(
                (i + 1) + ". " + objetos[i].Nombre
                + " - " + objetos[i].Descripcion);
        }
    }
}

// =========================== ZONAS ===========================

class Zona
{
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }

    public Zona(string nombre, string descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }
}

// =========================== MISION ==========================

class Mision
{
    public bool AnomaliaInvestigada { get; private set; }
    public bool NucleoEncontrado { get; private set; }
    public bool EncuentroSobrevivido { get; private set; }
    public bool Completada { get; private set; }

    public void RegistrarAnomalia()
    {
        AnomaliaInvestigada = true;
    }

    public void RegistrarNucleo()
    {
        NucleoEncontrado = true;
    }

    public void RegistrarEncuentro()
    {
        EncuentroSobrevivido = true;
    }

    public bool PuedeCompletar()
    {
        return AnomaliaInvestigada
            && NucleoEncontrado
            && EncuentroSobrevivido;
    }

    public bool Completar(Cadete cadete)
    {
        if (!PuedeCompletar())
        {
            Console.WriteLine("Todavia faltan objetivos.");
            MostrarObjetivos();
            return false;
        }

        if (Completada)
            return true;

        Completada = true;
        cadete.GanarExperiencia(100);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n================================");
        Console.WriteLine("       MISION COMPLETADA");
        Console.WriteLine("================================");
        Console.WriteLine("+100 XP");
        Console.WriteLine("+1 modulo desconocido");
        Console.WriteLine("Has regresado con vida.");
        Console.ResetColor();

        return true;
    }

    public void MostrarObjetivos()
    {
        Console.WriteLine("\n--- MISION: LA ANOMALIA PERDIDA ---");

        Console.WriteLine(
            "[" + (AnomaliaInvestigada ? "X" : " ") +
            "] Investigar la anomalia");

        Console.WriteLine(
            "[" + (NucleoEncontrado ? "X" : " ") +
            "] Encontrar el nucleo energetico");

        Console.WriteLine(
            "[" + (EncuentroSobrevivido ? "X" : " ") +
            "] Sobrevivir a un encuentro");

        Console.WriteLine(
            "[" + (Completada ? "X" : " ") +
            "] Regresar al punto de extraccion");
    }
}

// ========================= JUEGO =============================

class JuegoNexus
{
    private Random azar = new Random();
    private Cadete cadete;
    private Inventario inventario = new Inventario();
    private Mision mision = new Mision();
    private List<Zona> zonas = new List<Zona>();

    private int zonaActual = 0;
    private int inestabilidad = 0;
    private bool jugando = true;
    private bool recompensaEntregada = false;

    public JuegoNexus()
    {
        CrearZonas();
    }

    private void CrearZonas()
    {
        zonas.Add(new Zona(
            "Punto de Extraccion",
            "Una plataforma protegida permite regresar a casa."));

        zonas.Add(new Zona(
            "Bosque de Cristal",
            "Cristales luminosos reaccionan a tu presencia."));

        zonas.Add(new Zona(
            "Laboratorio Abandonado",
            "Antiguos investigadores estudiaban realidades paralelas."));

        zonas.Add(new Zona(
            "Grieta de Resonancia",
            "La realidad se encuentra fragmentada por una anomalia."));

        zonas.Add(new Zona(
            "Nucleo de la Anomalia",
            "Una fuente energetica desconocida permanece en el centro."));
    }

    public void Iniciar()
    {
        MostrarIntroduccion();

        Console.Write("\nIdentificate, cadete: ");
        string nombre = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nombre))
            nombre = "Explorer";

        cadete = new Cadete(nombre.Trim());

        Console.WriteLine(
            "\nBienvenido, " + cadete.Nombre + ". Mision iniciada.");

        while (jugando && cadete.Vivo && !mision.Completada)
        {
            MostrarEstadoGeneral();
            MostrarMenu();

            string opcion = Console.ReadLine() ?? "";

            switch (opcion)
            {
                case "1":
                    Explorar();
                    break;

                case "2":
                    Desplazarse();
                    break;

                case "3":
                    MenuInventario();
                    break;

                case "4":
                    cadete.MostrarEstado();
                    break;

                case "5":
                    mision.MostrarObjetivos();
                    break;

                case "6":
                    AnalizarZona();
                    break;

                case "0":
                    jugando = false;
                    Console.WriteLine("Expedicion interrumpida.");
                    break;

                default:
                    Console.WriteLine(
                        "Opcion invalida. Intenta nuevamente.");
                    break;
            }
        }

        MostrarFinal();
    }

    private void MostrarIntroduccion()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("======================================");
        Console.WriteLine("       NEXUS v1.0: ECOS DEL VACIO");
        Console.WriteLine("                 ANO 2297");
        Console.WriteLine("======================================");
        Console.ResetColor();

        Console.WriteLine(
            "Una anomalia ha abierto una puerta a otra realidad.");

        Console.WriteLine(
            "Tu objetivo es investigar la grieta, recuperar el nucleo");
        Console.WriteLine(
            "energetico y regresar al punto de extraccion con vida.");
    }

    private void MostrarEstadoGeneral()
    {
        Console.WriteLine("\n--------------------------------------");
        Console.WriteLine("Cadete: " + cadete.Nombre);
        Console.WriteLine("Zona: " + zonas[zonaActual].Nombre);
        Console.WriteLine("Salud: " + cadete.Salud
            + " | Energia: " + cadete.Energia);
        Console.WriteLine("Resonancia: " + inestabilidad + "/100");
        Console.WriteLine("--------------------------------------");
    }

    private void MostrarMenu()
    {
        Console.WriteLine("\n1. Explorar");
        Console.WriteLine("2. Desplazarse");
        Console.WriteLine("3. Administrar inventario");
        Console.WriteLine("4. Estado del cadete");
        Console.WriteLine("5. Objetivos de la mision");
        Console.WriteLine("6. Analizar zona");
        Console.WriteLine("0. Abandonar expedicion");
        Console.Write("Seleccion: ");
    }

    // ----------------------- EXPLORACION ---------------------

    private void Explorar()
    {
        if (cadete.Energia < 10)
        {
            Console.WriteLine(
                "No tienes energia suficiente para explorar.");
            return;
        }

        cadete.GastarEnergia(10);

        Console.WriteLine(
            "\nEscaneando: " + zonas[zonaActual].Nombre);

        if (zonaActual == 3 && !mision.AnomaliaInvestigada)
        {
            mision.RegistrarAnomalia();
            inestabilidad = Math.Min(100, inestabilidad + 20);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n*** ANOMALIA DETECTADA ***");
            Console.WriteLine(
                "La grieta ha sido localizada y registrada.");
            Console.ResetColor();
            return;
        }

        if (zonaActual == 4)
        {
            if (!mision.AnomaliaInvestigada)
            {
                Console.WriteLine(
                    "Primero debes investigar la grieta de resonancia.");
                return;
            }

            if (!mision.NucleoEncontrado)
            {
                mision.RegistrarNucleo();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n*** NUCLEO ENERGETICO RECUPERADO ***");
                Console.ResetColor();

                inventario.Agregar(new Objeto(
                    "Nucleo energetico",
                    "Artefacto esencial para la mision."));
                inestabilidad = Math.Min(100, inestabilidad + 15);
                return;
            }
        }

        int evento = azar.Next(100);

        if (inestabilidad >= 70 && evento < 45)
        {
            EventoSistemaDanado();
        }
        else if (evento < 25)
        {
            EncontrarObjeto();
        }
        else if (evento < 50)
        {
            EncontrarEntidad();
        }
        else if (evento < 65)
        {
            EventoPerdidaEnergia();
        }
        else if (evento < 80)
        {
            EventoSistemaDanado();
        }
        else if (evento < 95)
        {
            EncontrarInformacion();
        }
        else
        {
            Console.WriteLine(
                "No se encontraron novedades en esta zona.");
        }

        inestabilidad = Math.Min(100, inestabilidad + 5);
    }

    private void EncontrarObjeto()
    {
        string[] nombres = {
            "Celula de energia",
            "Escudo",
            "Modulo de ataque",
            "Analizador"
        };

        string[] descripciones = {
            "Recupera 25 puntos de energia.",
            "Reduce el siguiente impacto recibido.",
            "Aumenta el ataque en 10 puntos.",
            "Permite analizar entidades."
        };

        int i = azar.Next(nombres.Length);

        Console.WriteLine("\n*** OBJETO ENCONTRADO ***");
        inventario.Agregar(
            new Objeto(nombres[i], descripciones[i]));
    }

    private void EncontrarInformacion()
    {
        Console.WriteLine("\n*** DATOS RECUPERADOS ***");
        Console.WriteLine(
            "La dimension parece reaccionar a tus decisiones.");
        cadete.GanarExperiencia(10);
        Console.WriteLine("+10 XP");
    }

    private void EventoPerdidaEnergia()
    {
        int perdida = azar.Next(5, 16);
        cadete.GastarEnergia(perdida);

        Console.WriteLine("\n*** PERDIDA DE ENERGIA ***");
        Console.WriteLine("Energia perdida: " + perdida);
    }

    private void EventoSistemaDanado()
    {
        int dano = azar.Next(5, 16);
        cadete.RecibirDanio(dano);

        Console.WriteLine("\n*** SISTEMA DANADO ***");
        Console.WriteLine("Dano recibido: " + dano);
        inestabilidad = Math.Min(100, inestabilidad + 10);
    }

    // ----------------------- MOVIMIENTO ----------------------

    private void Desplazarse()
    {
        Console.WriteLine("\nZonas disponibles:");

        for (int i = 0; i < zonas.Count; i++)
        {
            Console.WriteLine((i + 1) + ". " + zonas[i].Nombre);
        }

        Console.Write("Numero de destino: ");
        string entrada = Console.ReadLine() ?? "";
        int destino;

        if (!int.TryParse(entrada, out destino))
        {
            Console.WriteLine("Debes introducir un numero valido.");
            return;
        }

        destino--;

        if (destino < 0 || destino >= zonas.Count)
        {
            Console.WriteLine("Destino fuera del mapa.");
            return;
        }

        if (destino == zonaActual)
        {
            Console.WriteLine("Ya te encuentras en esa zona.");
            return;
        }

        if (Math.Abs(destino - zonaActual) > 1)
        {
            Console.WriteLine(
                "Solo puedes desplazarte a una zona adyacente.");
            return;
        }

        if (destino == 4 && !mision.AnomaliaInvestigada)
        {
            Console.WriteLine(
                "La grieta bloquea el acceso al nucleo.");
            return;
        }

        zonaActual = destino;

        Console.WriteLine("\nHas llegado a: " + zonas[zonaActual].Nombre);
        Console.WriteLine(zonas[zonaActual].Descripcion);

        if (zonaActual == 0 && mision.NucleoEncontrado)
        {
            if (mision.Completar(cadete))
            {
                if (!recompensaEntregada)
                {
                    inventario.Agregar(new Objeto(
                        "Modulo desconocido",
                        "Recompensa especial de la expedicion."));
                    recompensaEntregada = true;
                }
            }
        }
    }

    // ------------------------- COMBATE -----------------------

    private void EncontrarEntidad()
    {
        Entidad entidad;

        if (azar.Next(100) < 45)
            entidad = new Robot();
        else
            entidad = new Enemigo();

        entidad.Presentarse();

        while (entidad.Vivo && cadete.Vivo)
        {
            Console.WriteLine("\n1. Atacar");
            Console.WriteLine("2. Escapar");
            Console.WriteLine("3. Analizar entidad");
            Console.WriteLine("4. Utilizar objeto");
            Console.Write("Accion: ");

            string opcion = Console.ReadLine() ?? "";

            if (opcion == "1")
            {
                if (cadete.Energia < 5)
                {
                    Console.WriteLine(
                        "Energia insuficiente para atacar.");
                    continue;
                }

                cadete.GastarEnergia(5);
                entidad.RecibirDanio(cadete.Ataque);

                Console.WriteLine(
                    "Dano causado: " + cadete.Ataque);

                if (!entidad.Vivo)
                {
                    Console.WriteLine(
                        entidad.Nombre + " ha sido neutralizado.");
                    cadete.GanarExperiencia(20);
                    Console.WriteLine("+20 XP");
                    break;
                }

                entidad.Atacar(cadete);
            }
            else if (opcion == "2")
            {
                Console.WriteLine("Has escapado del encuentro.");
                cadete.GastarEnergia(5);
                break;
            }
            else if (opcion == "3")
            {
                if (!inventario.Contiene("Analizador"))
                {
                    Console.WriteLine(
                        "No tienes un analizador.");
                }
                else
                {
                    Console.WriteLine("\n--- ANALISIS ---");
                    Console.WriteLine("Entidad: " + entidad.Nombre);
                    Console.WriteLine("Salud: " + entidad.Salud);
                    Console.WriteLine("Ataque: " + entidad.Ataque);
                    Console.WriteLine(entidad.Descripcion);
                }
            }
            else if (opcion == "4")
            {
                UsarObjeto();
            }
            else
            {
                Console.WriteLine("Accion invalida.");
            }
        }

        if (cadete.Vivo)
        {
            mision.RegistrarEncuentro();
            Console.WriteLine(
                "Encuentro sobrevivido. Objetivo registrado.");
        }
    }

    // ------------------------ INVENTARIO ---------------------

    private void MenuInventario()
    {
        bool volver = false;

        while (!volver && cadete.Vivo)
        {
            inventario.Mostrar();

            Console.WriteLine("\n1. Utilizar objeto");
            Console.WriteLine("2. Eliminar objeto");
            Console.WriteLine("0. Volver");
            Console.Write("Seleccion: ");

            string opcion = Console.ReadLine() ?? "";

            switch (opcion)
            {
                case "1":
                    UsarObjeto();
                    break;

                case "2":
                    EliminarObjeto();
                    break;

                case "0":
                    volver = true;
                    break;

                default:
                    Console.WriteLine("Opcion invalida.");
                    break;
            }
        }
    }

    private void UsarObjeto()
    {
        inventario.Mostrar();

        if (inventario.Cantidad == 0)
            return;

        Console.Write("Numero del objeto: ");
        string entrada = Console.ReadLine() ?? "";
        int numero;

        if (!int.TryParse(entrada, out numero))
        {
            Console.WriteLine("Numero invalido.");
            return;
        }

        Objeto objeto = inventario.Obtener(numero - 1);

        if (objeto == null)
        {
            Console.WriteLine("Ese objeto no existe.");
            return;
        }

        if (objeto.Nombre == "Nucleo energetico" ||
            objeto.Nombre == "Modulo desconocido")
        {
            Console.WriteLine(
                "Este objeto no se puede consumir.");
            return;
        }

        switch (objeto.Nombre)
        {
            case "Celula de energia":
                if (cadete.Energia >= 100)
                {
                    Console.WriteLine("Energia al maximo.");
                    return;
                }

                int antes = cadete.Energia;
                cadete.RecuperarEnergia(25);
                inventario.Consumir(objeto);

                Console.WriteLine(
                    "Energia: " + antes + " -> " + cadete.Energia);
                break;

            case "Escudo":
                cadete.AgregarEscudo();
                inventario.Consumir(objeto);
                Console.WriteLine("Escudo activado.");
                break;

            case "Modulo de ataque":
                cadete.MejorarAtaque(10);
                inventario.Consumir(objeto);
                Console.WriteLine(
                    "Ataque aumentado a " + cadete.Ataque);
                break;

            case "Analizador":
                Console.WriteLine(
                    "Conserva el analizador y usalo durante un encuentro.");
                break;

            default:
                Console.WriteLine("Objeto desconocido.");
                break;
        }
    }

    private void EliminarObjeto()
    {
        inventario.Mostrar();

        if (inventario.Cantidad == 0)
            return;

        Console.Write("Numero del objeto: ");
        string entrada = Console.ReadLine() ?? "";
        int numero;

        if (!int.TryParse(entrada, out numero))
        {
            Console.WriteLine("Numero invalido.");
            return;
        }

        Objeto objeto = inventario.Obtener(numero - 1);

        if (objeto == null)
        {
            Console.WriteLine("Ese objeto no existe.");
            return;
        }

        if (inventario.Eliminar(objeto))
            Console.WriteLine("Objeto eliminado.");
    }

    // ---------------------- ANALISIS DE ZONA -----------------

    private void AnalizarZona()
    {
        Console.WriteLine("\n--- ANALISIS DIMENSIONAL ---");
        Console.WriteLine("Zona: " + zonas[zonaActual].Nombre);
        Console.WriteLine(zonas[zonaActual].Descripcion);
        Console.WriteLine(
            "Inestabilidad: " + inestabilidad + "/100");

        if (zonaActual == 3 && !mision.AnomaliaInvestigada)
        {
            Console.WriteLine(
                "Se requiere investigar la anomalia.");
        }
        else if (zonaActual == 4 && !mision.NucleoEncontrado)
        {
            Console.WriteLine(
                "Se detecta un nucleo energetico.");
        }
        else
        {
            Console.WriteLine(
                "No hay objetivos nuevos detectados.");
        }
    }

    // --------------------------- FINAL -----------------------

    private void MostrarFinal()
    {
        Console.WriteLine("\n======================================");

        if (mision.Completada)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("MISION COMPLETADA");
        }
        else if (!cadete.Vivo)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("MISION FALLIDA: CADETE SIN VIDA");
        }
        else
        {
            Console.WriteLine("EXPEDICION INTERRUMPIDA");
        }

        Console.ResetColor();

        Console.WriteLine(
            "Experiencia final: " + cadete.Experiencia + " XP");

        Console.WriteLine("NEXUS v1.0 - Fin de la expedicion.");
    }
}
    
