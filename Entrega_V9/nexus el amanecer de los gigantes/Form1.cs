
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace nexus_el_amanecer_de_los_gigantes
{
    public partial class Form1 : Form
    {
        private Panel panelPrincipal;
        private Cadete cadete;
        private readonly PersistenciaNexus persistencia = new PersistenciaNexus();
        private ComboBox comboPerfiles;
        private PerfilNexus perfilActual;
        private int partidaIdActual;
        private int perfilIdPartidaActual;
        private PartidaNexus partidaPendiente;
        private bool avisoPersistenciaMostrado;
        private Image imagenEren;
        private Image imagenTitan;

        // MUSICA DE FONDO
        private bool musicaAbierta = false;

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(
            string comando,
            StringBuilder respuesta,
            int longitud,
            IntPtr callback
        );

        // IMAGENES DEL JUEGO

        // MAPA Y MOVIMIENTO
        private string[,] mapaActual;
        private Button[,] casillasMapa;
        private bool[,] zonasProcesadas;

        private int jugadorFila = 1;
        private int jugadorColumna = 1;
        private string murallaActual;
        private bool partidaTerminada = false;

        // JEFE FINAL
        private bool giganteDerrotado = false;
        private bool combateActivo = false;
        private readonly List<EnemigoMapa> enemigosMapa = new List<EnemigoMapa>();
        private readonly Random azarCombate = new Random();
        private Timer timerSupervivencia;
        private int eliminacionesOleada;
        private int tiempoOleada;
        private int experienciaOleada;
        private int nivelOleada = 1;
        private int dificultadMuralla = 1;
        private int danioDisparo = 20;
        private Panel panelMejoras;
        private Panel barraVida;
        private Panel barraExperiencia;
        private Panel panelJefe;
        private Panel zonaJefe;
        private readonly List<ProyectilJefe> proyectilesJefe = new List<ProyectilJefe>();
        private Panel barraJefe;
        private Label textoEstadoJefe;
        private Label textoAvisoJefe;
        private Panel barraVidaJefe;
        private Timer timerJefe;
        private EnemigoMapa jefeActual;
        private int vidaJefe;
        private int vidaMaximaJefe;
        private int ticksJefe;
        private bool esquivoGolpeJefe;
        private bool destelloJefe;
        private bool turnoEnemigoJefe;
        private int duracionTurnoJefe;
        private int patronJefe;
        private int ataquesJefe;
        private int ticksAtaqueJefe;
        private int invulnerabilidadJefe;
        private bool debilitarJefe;
        private int almaX = 580;
        private int almaY = 320;
        private int velocidadAlma = 18;
        private int cortesRapidos;

        private int RangoAmenaza { get { return Math.Max(0, dificultadMuralla + nivelOleada - 2); } }

        private class ProyectilJefe
        {
            public float X, Y, VX, VY;
            public int Tamano;
        }

        private sealed class DatosBarra
        {
            public int Valor;
            public int Maximo;
            public Color Color;
        }

        // Panels con back-buffer para que las animaciones y las capas HUD
        // se dibujen de una sola vez, sin limpiar la pantalla entre cuadros.
        private sealed class PanelSuave : Panel
        {
            public PanelSuave()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                UpdateStyles();
            }
        }

        // Baldosas dibujadas como terreno del mundo, no como botones de Windows.
        private sealed class BaldosaMapa : Button
        {
            public static Image TexturaPiedra;
            public static Image TexturaTerreno;
            public int Fila;
            public int Columna;

            public BaldosaMapa()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                Margin = Padding.Empty;
                TabStop = false;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                UpdateStyles();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = Math.Max(1, Width), h = Math.Max(1, Height);
                string tipo = Tag as string ?? ".";
                Color baseColor = BackColor;
                using (LinearGradientBrush suelo = new LinearGradientBrush(
                    new Rectangle(0, 0, w, h),
                    Color.FromArgb(Math.Min(255, baseColor.R + 15), Math.Min(255, baseColor.G + 15), Math.Min(255, baseColor.B + 15)),
                    Color.FromArgb(Math.Max(0, baseColor.R - 16), Math.Max(0, baseColor.G - 16), Math.Max(0, baseColor.B - 16)),
                    LinearGradientMode.Vertical))
                    g.FillRectangle(suelo, 0, 0, w, h);

                Random detalle = new Random(Fila * 7919 + Columna * 104729 + 19);
                Image textura = tipo == "#" ? TexturaPiedra : TexturaTerreno;
                if (textura != null && textura.Width > w && textura.Height > h)
                {
                    int sx = (Columna * 137 + Fila * 53 + 29) % (textura.Width - w);
                    int sy = (Fila * 97 + Columna * 71 + 17) % (textura.Height - h);
                    g.DrawImage(textura, new Rectangle(0, 0, w, h), new Rectangle(sx, sy, w, h), GraphicsUnit.Pixel);
                    if (tipo == "#")
                    {
                        using (Brush sombraMuro = new SolidBrush(Color.FromArgb(48, 9, 11, 12))) g.FillRectangle(sombraMuro, 0, h - 4, w, 4);
                        using (Pen bordeLuz = new Pen(Color.FromArgb(72, 236, 216, 182), 1)) g.DrawLine(bordeLuz, 2, 1, w - 3, 1);
                    }
                    else
                    {
                        using (Pen huella = new Pen(Color.FromArgb(48, 225, 211, 171), 1))
                            for (int i = 0; i < 2; i++) { int x = detalle.Next(3, Math.Max(4, w - 3)); int y = detalle.Next(3, Math.Max(4, h - 3)); g.DrawLine(huella, x, y, x + detalle.Next(-5, 6), y + detalle.Next(2, 7)); }
                    }
                }
                if (tipo == "#" && textura == null)
                {
                    // Mamposteria irregular con relieve, juntas y pequeñas grietas.
                    int curso = Math.Max(1, h / 3);
                    for (int filaPiedra = 0; filaPiedra < 3; filaPiedra++)
                    {
                        int y = filaPiedra * curso - 1;
                        int x = (filaPiedra % 2 == 0) ? -w / 5 : -w / 3;
                        while (x < w)
                        {
                            int anchoPiedra = Math.Max(16, detalle.Next(Math.Max(18, w / 4), Math.Max(20, w / 2)));
                            int altoPiedra = Math.Max(8, curso - 2);
                            int margen = detalle.Next(1, 4);
                            Rectangle piedra = new Rectangle(x + margen, y + margen, Math.Max(2, anchoPiedra - margen * 2), Math.Max(2, altoPiedra - margen * 2));
                            int variacion = detalle.Next(-10, 11);
                            Color luzPiedra = Color.FromArgb(Math.Max(0, Math.Min(255, 123 + variacion)), Math.Max(0, Math.Min(255, 121 + variacion)), Math.Max(0, Math.Min(255, 111 + variacion)));
                            Color sombraPiedra = Color.FromArgb(Math.Max(0, Math.Min(255, 52 + variacion)), Math.Max(0, Math.Min(255, 55 + variacion)), Math.Max(0, Math.Min(255, 53 + variacion)));
                            Point[] forma = { new Point(piedra.Left + 2, piedra.Top), new Point(piedra.Right - 3, piedra.Top + 1), new Point(piedra.Right, piedra.Top + piedra.Height / 3), new Point(piedra.Right - 2, piedra.Bottom - 2), new Point(piedra.Left + 3, piedra.Bottom), new Point(piedra.Left, piedra.Top + piedra.Height / 2) };
                            using (LinearGradientBrush relieve = new LinearGradientBrush(piedra, luzPiedra, sombraPiedra, LinearGradientMode.Vertical))
                                g.FillPolygon(relieve, forma);
                            using (Pen bordePiedra = new Pen(Color.FromArgb(100, 24, 25, 23), 1)) g.DrawPolygon(bordePiedra, forma);
                            using (Pen canto = new Pen(Color.FromArgb(90, 218, 206, 175), 1)) g.DrawLine(canto, piedra.Left + 3, piedra.Top + 1, piedra.Right - 5, piedra.Top + 2);
                            if (piedra.Width > 24 && detalle.Next(3) == 0)
                            {
                                using (Pen grieta = new Pen(Color.FromArgb(85, 28, 30, 27), 1))
                                    g.DrawLines(grieta, new Point[] { new Point(piedra.Left + piedra.Width / 2, piedra.Top + 3), new Point(piedra.Left + piedra.Width / 2 - 2, piedra.Top + piedra.Height / 2), new Point(piedra.Left + piedra.Width / 2 + 1, piedra.Bottom - 3) });
                            }
                            x += anchoPiedra;
                        }
                    }
                    using (Brush musgo = new SolidBrush(Color.FromArgb(105, 83, 112, 68)))
                        for (int i = 0; i < 3; i++) g.FillEllipse(musgo, detalle.Next(1, Math.Max(2, w - 5)), detalle.Next(1, Math.Max(2, h - 4)), detalle.Next(2, 6), 2);
                    using (Pen contorno = new Pen(Color.FromArgb(95, 12, 14, 15), 2)) g.DrawLine(contorno, 0, h - 1, w, h - 1);
                }
                else if (tipo != "#" && textura == null)
                {
                    using (Pen huella = new Pen(Color.FromArgb(45, 229, 207, 164), 1))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int x = detalle.Next(3, Math.Max(4, w - 3));
                            int y = detalle.Next(3, Math.Max(4, h - 3));
                            g.DrawLine(huella, x, y, x + detalle.Next(-5, 6), y + detalle.Next(2, 7));
                        }
                    }
                    using (Brush guijarro = new SolidBrush(Color.FromArgb(65, 218, 207, 177)))
                        for (int i = 0; i < 5; i++) g.FillEllipse(guijarro, detalle.Next(2, Math.Max(3, w - 3)), detalle.Next(2, Math.Max(3, h - 3)), 2, 2);
                    if (tipo == "S")
                    {
                        using (Brush brillo = new SolidBrush(Color.FromArgb(32, 105, 183, 129)))
                            g.FillEllipse(brillo, 2, 2, w - 4, h - 4);
                    }
                }

                if (tipo == "#") DibujarCasa(g, w, h, Fila, Columna);

                if (BackgroundImage != null)
                {
                    int margen = Math.Max(2, Math.Min(w, h) / 10);
                    Rectangle destino = new Rectangle(margen, margen, w - margen * 2, h - margen * 2);
                    if (tipo == "G" || Text == "G" || (AccessibleName ?? "").StartsWith("TITAN"))
                        destino = new Rectangle(1, 1, w - 2, h - 2);
                    g.DrawImage(BackgroundImage, destino);
                }

                if (!String.IsNullOrEmpty(Text))
                {
                    using (Font fuente = new Font("Segoe UI Symbol", Math.Max(10, Math.Min(19, h * 0.43f)), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (Brush tinta = new SolidBrush(ForeColor))
                    using (StringFormat formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(Text, fuente, tinta, new RectangleF(0, 0, w, h), formato);
                }

                if (tipo != "#")
                    using (Pen borde = new Pen(Color.FromArgb(38, 20, 17, 14), 1)) g.DrawRectangle(borde, 0, 0, w - 1, h - 1);
                if (Focused)
                {
                    using (Pen foco = new Pen(Color.FromArgb(180, 235, 194, 111), 1))
                        g.DrawRectangle(foco, 2, 2, w - 5, h - 5);
                }
            }

            private static void DibujarCasa(Graphics g, int w, int h, int fila, int columna)
            {
                float sx = w / 84f, sy = h / 56f;
                Func<float, float> X = v => v * sx;
                Func<float, float> Y = v => v * sy;
                int variante = Math.Abs(fila * 7 + columna * 11) % 3;
                Color[] tejados = { Color.FromArgb(111, 52, 43), Color.FromArgb(84, 58, 43), Color.FromArgb(105, 68, 46) };
                Color[] paredes = { Color.FromArgb(163, 143, 108), Color.FromArgb(145, 131, 105), Color.FromArgb(177, 153, 116) };
                using (Brush sombra = new SolidBrush(Color.FromArgb(155, 12, 14, 13))) g.FillEllipse(sombra, X(9), Y(43), X(68), Y(10));

                // Cuerpo de mamposteria y cara lateral en perspectiva.
                PointF[] lateral = { new PointF(X(17), Y(27)), new PointF(X(35), Y(20)), new PointF(X(35), Y(49)), new PointF(X(17), Y(44)) };
                using (Brush lado = new SolidBrush(Color.FromArgb(110, 100, 78))) g.FillPolygon(lado, lateral);
                RectangleF frente = new RectangleF(X(35), Y(24), X(36), Y(25));
                using (LinearGradientBrush muro = new LinearGradientBrush(Rectangle.Round(frente), paredes[variante], Color.FromArgb(84, 75, 60), LinearGradientMode.Vertical)) g.FillRectangle(muro, frente);
                if (TexturaPiedra != null)
                {
                    Rectangle fuente = new Rectangle((columna * 113 + fila * 37) % (TexturaPiedra.Width - 42), (fila * 67 + columna * 31) % (TexturaPiedra.Height - 28), 42, 28);
                    using (Brush velo = new SolidBrush(Color.FromArgb(92, paredes[variante])))
                    {
                        g.DrawImage(TexturaPiedra, frente, fuente, GraphicsUnit.Pixel);
                        g.FillRectangle(velo, frente);
                    }
                }
                using (Pen ciment = new Pen(Color.FromArgb(235, 41, 39, 33), Math.Max(1, X(2)))) g.DrawLine(ciment, X(32), Y(49), X(73), Y(49));
                using (Pen liston = new Pen(Color.FromArgb(235, 54, 39, 29), Math.Max(1, X(2))))
                {
                    g.DrawLine(liston, X(39), Y(27), X(39), Y(47));
                    g.DrawLine(liston, X(67), Y(27), X(67), Y(47));
                    g.DrawLine(liston, X(36), Y(34), X(70), Y(34));
                }

                // Tejado a dos aguas con tejas, cumbrera y chimenea.
                RectangleF chimenea = new RectangleF(X(54), Y(5), X(7), Y(16));
                using (Brush chimeneaSombra = new SolidBrush(Color.FromArgb(75, 36, 34, 31))) g.FillRectangle(chimeneaSombra, chimenea);
                using (Brush humo = new SolidBrush(Color.FromArgb(48, 204, 193, 170)))
                { g.FillEllipse(humo, X(52), Y(1), X(8), Y(7)); g.FillEllipse(humo, X(59), Y(0), X(8), Y(6)); }
                PointF[] techo = { new PointF(X(8), Y(29)), new PointF(X(34), Y(6)), new PointF(X(78), Y(27)), new PointF(X(72), Y(33)), new PointF(X(35), Y(16)), new PointF(X(14), Y(34)) };
                using (LinearGradientBrush teja = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(229, 147, 108, 78), tejados[variante], LinearGradientMode.Vertical)) g.FillPolygon(teja, techo);
                using (Pen filoTejado = new Pen(Color.FromArgb(230, 211, 159, 111), Math.Max(1, X(1.4f)))) g.DrawLine(filoTejado, X(9), Y(29), X(34), Y(7));
                using (Pen juntaTeja = new Pen(Color.FromArgb(135, 45, 31, 27), Math.Max(1, X(1))))
                using (Pen brilloTeja = new Pen(Color.FromArgb(100, 235, 180, 131), Math.Max(1, X(1))))
                {
                    for (int filaTeja = 0; filaTeja < 4; filaTeja++)
                    {
                        float yy = Y(12 + filaTeja * 4.5f);
                        g.DrawLine(juntaTeja, X(34 - filaTeja * 5), yy, X(35), yy + Y(4));
                        g.DrawLine(juntaTeja, X(35), yy + Y(4), X(75 - filaTeja * 2), yy + Y(10));
                        g.DrawLine(brilloTeja, X(17 + filaTeja * 4), yy + Y(1), X(34), yy + Y(4));
                    }
                }
                using (Pen cumbrera = new Pen(Color.FromArgb(240, 62, 43, 34), Math.Max(2, X(3)))) g.DrawLine(cumbrera, X(34), Y(6), X(35), Y(16));

                // Ventanas iluminadas y puerta de madera.
                RectangleF ventana = new RectangleF(X(44), Y(35), X(8), Y(8));
                using (Brush marco = new SolidBrush(Color.FromArgb(36, 30, 25))) g.FillRectangle(marco, ventana);
                using (Brush luz = new SolidBrush(Color.FromArgb(225, 230, 177, 91))) g.FillRectangle(luz, X(45), Y(36), X(6), Y(6));
                using (Pen cruz = new Pen(Color.FromArgb(220, 55, 43, 33), Math.Max(1, X(1)))) { g.DrawLine(cruz, X(48), Y(36), X(48), Y(42)); g.DrawLine(cruz, X(45), Y(39), X(51), Y(39)); }
                using (Brush puerta = new SolidBrush(Color.FromArgb(75, 52, 38))) g.FillRectangle(puerta, X(56), Y(36), X(9), Y(14));
                using (Pen marcoPuerta = new Pen(Color.FromArgb(210, 201, 167, 120), Math.Max(1, X(1)))) g.DrawRectangle(marcoPuerta, X(56), Y(36), X(9), Y(14));
                using (Brush pomo = new SolidBrush(Color.FromArgb(233, 190, 145, 77))) g.FillEllipse(pomo, X(62), Y(43), X(1.8f), Y(1.8f));
                using (Pen contorno = new Pen(Color.FromArgb(220, 28, 27, 25), Math.Max(1, X(1.2f)))) g.DrawPolygon(contorno, techo);
            }
        }

        private class EnemigoMapa
        {
            public int Fila;
            public int Columna;
            public int Vida;
        }

        // INTERFAZ DE EXPLORACION
        private Label textoHUD;
        private Label textoRegistro;
        private Label textoTituloZona;
        private Button botonSuministro;

        private string registroEventos = "";

        // SELECCION DE MURALLAS
        private string murallaSeleccionada = "MARIA";
        private Label descripcionMuralla;
        private Button botonEntrar;
        private Panel tarjetaMaria;
        private Panel tarjetaRose;
        private Panel tarjetaSina;

        // CINEMATICA
        private Timer timerCinematica;
        private Label textoCinematica;
        private Label tituloCinematica;
        private Button botonContinuar;
        private int escenaActual = 0;
        private int caracterActual = 0;

        private string[] titulosEscenas =
        {
            "AÑO 2297: EL MUNDO DEL MAÑANA",
            "EL DESCUBRIMIENTO DEL NUCLEO",
            "EL DIA DE LA RUPTURA",
            "EL ULTIMO REFUGIO",
            "EL DESPERTAR DE NEXUS"
        };

        private string[] textosEscenas =
        {
            "Año 2297. La humanidad habia alcanzado una era de avances extraordinarios. Las ciudades crecieron, la tecnologia transformo nuestras vidas y la humanidad comenzo a buscar una fuente de energia ilimitada. Creiamos que nuestro futuro estaba asegurado...",

            "Un grupo de cientificos descubrio el NUCLEO NEXUS, una fuente de energia desconocida. Su poder parecia infinito. Pero intentaron utilizarlo para abrir una conexion con otra realidad. No comprendian la fuerza que estaban intentando controlar...",

            "El experimento fracaso. El cielo se fragmento y una ruptura dimensional aparecio sobre la Tierra. De ella surgieron criaturas gigantescas. Las ciudades fueron destruidas, las comunicaciones colapsaron y millones de personas desaparecieron. En pocos dias, la civilizacion se derrumbo...",

            "Los supervivientes huyeron y construyeron tres grandes murallas: MARIA, ROSE y SINA. Dentro de ellas nacio una nueva sociedad. Durante años, la humanidad vivio protegida, pero el exterior seguia siendo un territorio desconocido y mortal...",

            "Muchos años despues, el sistema IRIS detecto una señal procedente del antiguo NUCLEO NEXUS. Algo sigue activo al otro lado de la ruptura. Tu eres un nuevo cadete y tendras que explorar las murallas, descubrir la verdad y encontrar una forma de salvar a la humanidad. Tu historia comienza ahora..."
        };

        public Form1()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            BaldosaMapa.TexturaPiedra = CrearTexturaPiedra();
            BaldosaMapa.TexturaTerreno = CrearTexturaTerreno();

            ConfigurarVentana();
            IniciarMusica();
            CargarImagenesPersonajes();
            CargarPortada();
            MostrarMenuPrincipal();

            FormClosing += (s, e) => GuardarProgreso();
            FormClosed += (s, e) => DetenerMusica();
        }

        private Bitmap CrearTexturaPiedra()
        {
            const int ancho = 1024, alto = 768;
            Bitmap mapa = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
            BitmapData datos = mapa.LockBits(new Rectangle(0, 0, ancho, alto), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            byte[] grano = new byte[ancho * alto * 4];
            Random azar = new Random(618203);
            for (int i = 0; i < grano.Length; i += 4)
            {
                int ruido = azar.Next(-17, 18);
                grano[i] = (byte)Math.Max(0, Math.Min(255, 86 + ruido));
                grano[i + 1] = (byte)Math.Max(0, Math.Min(255, 91 + ruido));
                grano[i + 2] = (byte)Math.Max(0, Math.Min(255, 96 + ruido));
                grano[i + 3] = 255;
            }
            Marshal.Copy(grano, 0, datos.Scan0, grano.Length);
            mapa.UnlockBits(datos);

            using (Graphics g = Graphics.FromImage(mapa))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Random r = new Random(44109);
                int curso = 96;
                for (int y = -curso; y < alto + curso; y += curso)
                {
                    int x = (y / curso % 2 == 0) ? -80 : -160;
                    while (x < ancho)
                    {
                        int w = r.Next(110, 225), h = r.Next(69, 88), inset = r.Next(4, 10);
                        Rectangle piedra = new Rectangle(x + inset, y + inset, w - inset * 2, h - inset * 2);
                        int shade = r.Next(-19, 20);
                        Color altoPiedra = Color.FromArgb(255, ClampByte(142 + shade), ClampByte(135 + shade), ClampByte(119 + shade));
                        Color medioPiedra = Color.FromArgb(255, ClampByte(100 + shade), ClampByte(99 + shade), ClampByte(89 + shade));
                        Color basePiedra = Color.FromArgb(255, ClampByte(57 + shade), ClampByte(61 + shade), ClampByte(58 + shade));
                        Point[] forma = {
                            new Point(piedra.Left + r.Next(5, 16), piedra.Top), new Point(piedra.Right - r.Next(6, 16), piedra.Top + r.Next(0, 4)),
                            new Point(piedra.Right, piedra.Top + r.Next(8, 17)), new Point(piedra.Right - r.Next(0, 5), piedra.Bottom - r.Next(8, 15)),
                            new Point(piedra.Right - r.Next(9, 20), piedra.Bottom), new Point(piedra.Left + r.Next(10, 22), piedra.Bottom - r.Next(0, 5)),
                            new Point(piedra.Left, piedra.Bottom - r.Next(9, 18)), new Point(piedra.Left + r.Next(0, 5), piedra.Top + r.Next(10, 22))
                        };
                        using (Pen junta = new Pen(Color.FromArgb(235, 27, 30, 30), 8)) g.DrawPolygon(junta, forma);
                        using (LinearGradientBrush relieve = new LinearGradientBrush(piedra, altoPiedra, basePiedra, LinearGradientMode.Vertical)) g.FillPolygon(relieve, forma);
                        using (Pen filo = new Pen(Color.FromArgb(130, 208, 193, 161), 2)) g.DrawLine(filo, piedra.Left + 12, piedra.Top + 4, piedra.Right - 19, piedra.Top + 6);
                        using (Pen sombra = new Pen(Color.FromArgb(120, 22, 24, 24), 3)) g.DrawLine(sombra, piedra.Left + 8, piedra.Bottom - 4, piedra.Right - 12, piedra.Bottom - 5);
                        using (Pen veta = new Pen(Color.FromArgb(65, 223, 210, 180), 1))
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                int vx = r.Next(piedra.Left + 14, Math.Max(piedra.Left + 15, piedra.Right - 15));
                                int vy = r.Next(piedra.Top + 12, Math.Max(piedra.Top + 13, piedra.Bottom - 12));
                                g.DrawLine(veta, vx, vy, vx + r.Next(4, 21), vy + r.Next(-3, 4));
                            }
                        }
                        if (w > 150)
                        {
                            using (Pen grieta = new Pen(Color.FromArgb(155, 31, 32, 30), 2))
                                g.DrawLines(grieta, new Point[] { new Point(piedra.Left + w / 2, piedra.Top + 18), new Point(piedra.Left + w / 2 - 5, piedra.Top + 30), new Point(piedra.Left + w / 2 + 3, piedra.Top + 38), new Point(piedra.Left + w / 2 - 2, piedra.Top + 49) });
                        }
                        x += w;
                    }
                }

                using (Brush musgo = new SolidBrush(Color.FromArgb(90, 75, 102, 61)))
                using (Brush liquen = new SolidBrush(Color.FromArgb(75, 139, 137, 83)))
                {
                    for (int i = 0; i < 170; i++)
                    {
                        int x = r.Next(0, ancho), y = r.Next(0, alto), w = r.Next(3, 23), h = r.Next(2, 9);
                        g.FillEllipse((i % 3 == 0) ? liquen : musgo, x, y, w, h);
                    }
                }
                Color[] polvo = { Color.FromArgb(55, 229, 216, 190), Color.FromArgb(65, 27, 28, 26), Color.FromArgb(55, 171, 154, 124) };
                using (SolidBrush a = new SolidBrush(polvo[0])) using (SolidBrush b = new SolidBrush(polvo[1])) using (SolidBrush c = new SolidBrush(polvo[2]))
                {
                    Brush[] brochas = { a, b, c };
                    for (int i = 0; i < 9000; i++) g.FillRectangle(brochas[r.Next(3)], r.Next(ancho), r.Next(alto), 1, 1);
                }
            }
            return mapa;
        }

        private Bitmap CrearTexturaTerreno()
        {
            const int ancho = 1024, alto = 512;
            Bitmap mapa = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
            BitmapData datos = mapa.LockBits(new Rectangle(0, 0, ancho, alto), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            byte[] grano = new byte[ancho * alto * 4];
            Random azar = new Random(99621);
            for (int i = 0; i < grano.Length; i += 4)
            {
                int n = azar.Next(-20, 21);
                grano[i] = (byte)Math.Max(0, Math.Min(255, 58 + n));
                grano[i + 1] = (byte)Math.Max(0, Math.Min(255, 72 + n));
                grano[i + 2] = (byte)Math.Max(0, Math.Min(255, 72 + n / 2));
                grano[i + 3] = 255;
            }
            Marshal.Copy(grano, 0, datos.Scan0, grano.Length);
            mapa.UnlockBits(datos);
            using (Graphics g = Graphics.FromImage(mapa))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Random r = new Random(19337);
                using (Pen caminoSombra = new Pen(Color.FromArgb(50, 32, 31, 26), 44))
                using (Pen camino = new Pen(Color.FromArgb(48, 165, 139, 98), 19))
                {
                    Point[] ruta = { new Point(-30, 365), new Point(215, 285), new Point(410, 324), new Point(680, 215), new Point(1040, 245) };
                    g.DrawCurve(caminoSombra, ruta, 0.35f);
                    g.DrawCurve(camino, ruta, 0.35f);
                }
                using (Brush grava = new SolidBrush(Color.FromArgb(92, 199, 178, 135)))
                using (Brush gravaOscura = new SolidBrush(Color.FromArgb(90, 32, 40, 36)))
                using (Pen hierba = new Pen(Color.FromArgb(100, 125, 151, 82), 2))
                {
                    for (int i = 0; i < 2400; i++)
                    {
                        int x = r.Next(ancho), y = r.Next(alto);
                        if (i % 3 == 0) g.FillEllipse(grava, x, y, r.Next(1, 4), r.Next(1, 3));
                        else g.FillEllipse(gravaOscura, x, y, r.Next(1, 3), 2);
                    }
                    for (int i = 0; i < 120; i++)
                    {
                        int x = r.Next(ancho), y = r.Next(alto);
                        g.DrawLine(hierba, x, y, x + r.Next(-6, 7), y - r.Next(3, 10));
                        g.DrawLine(hierba, x + 1, y, x + r.Next(1, 8), y - r.Next(2, 8));
                    }
                }
            }
            return mapa;
        }

        private static int ClampByte(int valor) { return Math.Max(0, Math.Min(255, valor)); }

        // ==========================================
        // MUSICA DE FONDO NEXUS
        // ==========================================

        private void IniciarMusica()
        {
            string ruta = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Musica",
                "musica_nexus.mpeg"
            );

            if (!File.Exists(ruta))
            {
                MessageBox.Show(
                    "No se encontro la musica.\n\n" +
                    "Revisa la carpeta Musica y el archivo musica_nexus.mpeg.",
                    "NEXUS - Musica",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            int resultado = mciSendString(
                "open \"" + ruta + "\" type mpegvideo alias musicaNexus",
                null,
                0,
                IntPtr.Zero
            );

            if (resultado != 0)
            {
                MessageBox.Show(
                    "Windows no pudo abrir el archivo de musica.",
                    "NEXUS - Musica",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            musicaAbierta = true;

            mciSendString(
                "play musicaNexus repeat",
                null,
                0,
                IntPtr.Zero
            );
        }

        private void DetenerMusica()
        {
            if (musicaAbierta)
            {
                mciSendString(
                    "stop musicaNexus",
                    null,
                    0,
                    IntPtr.Zero
                );

                mciSendString(
                    "close musicaNexus",
                    null,
                    0,
                    IntPtr.Zero
                );

                musicaAbierta = false;
            }
        }

        // ==========================================
        // CONFIGURACION GENERAL
        // ==========================================

        private void ConfigurarVentana()
        {
            Text = "NEXUS - El Amanecer de los Gigantes";
            Size = new Size(1280, 720);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;
            ForeColor = Color.White;

            KeyPreview = true;
            KeyDown += Form1_KeyDown;
        }

        private void CargarPortada()
        {
            string ruta = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "portada_nexus.png"
            );

            if (File.Exists(ruta))
            {
                using (Image original = Image.FromFile(ruta))
                {
                    BackgroundImage = new Bitmap(original);
                }

                BackgroundImageLayout = ImageLayout.Stretch;
            }
        }

        private Image CargarImagenPersonaje(string archivo)
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imagenes", archivo);
            if (!File.Exists(ruta)) return null;
            try
            {
                using (Image original = Image.FromFile(ruta))
                {
                    Bitmap salida=new Bitmap(original.Width,original.Height,PixelFormat.Format32bppArgb);
                    using(Graphics g=Graphics.FromImage(salida))g.DrawImageUnscaled(original,0,0);
                    bool[,] visitado=new bool[salida.Width,salida.Height];Queue<Point> cola=new Queue<Point>();
                    for(int x=0;x<salida.Width;x++){cola.Enqueue(new Point(x,0));cola.Enqueue(new Point(x,salida.Height-1));}
                    for(int y=1;y<salida.Height-1;y++){cola.Enqueue(new Point(0,y));cola.Enqueue(new Point(salida.Width-1,y));}
                    Point[] pasos={new Point(1,0),new Point(-1,0),new Point(0,1),new Point(0,-1)};
                    while(cola.Count>0)
                    {
                        Point p=cola.Dequeue();if(p.X<0||p.Y<0||p.X>=salida.Width||p.Y>=salida.Height||visitado[p.X,p.Y])continue;visitado[p.X,p.Y]=true;
                        Color c=salida.GetPixel(p.X,p.Y);if(!EsFondoClaro(c))continue;
                        salida.SetPixel(p.X,p.Y,Color.FromArgb(0,c.R,c.G,c.B));foreach(Point paso in pasos)cola.Enqueue(new Point(p.X+paso.X,p.Y+paso.Y));
                    }
                    return salida;
                }
            }
            catch { return null; }
        }

        private bool EsFondoClaro(Color c)
        {int max=Math.Max(c.R,Math.Max(c.G,c.B)),min=Math.Min(c.R,Math.Min(c.G,c.B));return c.R>220&&c.G>215&&c.B>205&&max-min<40;}

        private void CargarImagenesPersonajes()
        {
            imagenEren = CargarImagenPersonaje("personaje_eren.png");
            imagenTitan = CargarImagenPersonaje("peligro_titan.png");
        }

        private void PrepararPanel()
        {
            if (timerSupervivencia != null)
            {
                timerSupervivencia.Stop();
                timerSupervivencia.Dispose();
                timerSupervivencia = null;
            }
            if (timerJefe != null)
            {
                timerJefe.Stop();
                timerJefe.Dispose();
                timerJefe = null;
            }
            combateActivo = false;
            panelJefe = null;
            zonaJefe = null;
            panelMejoras = null;
            jefeActual = null;
            turnoEnemigoJefe = false;
            proyectilesJefe.Clear();

            if (timerCinematica != null)
            {
                timerCinematica.Stop();
                timerCinematica.Dispose();
                timerCinematica = null;
            }

            Controls.Clear();

            mapaActual = null;
            casillasMapa = null;
            zonasProcesadas = null;

            panelPrincipal = new Panel();
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.BackColor = Color.Transparent;

            Controls.Add(panelPrincipal);
        }

        // ==========================================
        // BOTONES GENERALES
        // ==========================================

        private void CrearBoton(
            string texto, int x, int y, Action accion)
        {
            Button boton = new Button();

            boton.Text = texto;
            boton.Size = new Size(350, 58);
            boton.Location = new Point(x, y);

            boton.BackColor = Color.FromArgb(75, 30, 25);
            boton.ForeColor = Color.White;
            boton.Font = new Font("Georgia", 11, FontStyle.Bold);

            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor =
                Color.FromArgb(190, 140, 95);
            boton.FlatAppearance.BorderSize = 2;

            boton.Cursor = Cursors.Hand;

            boton.MouseEnter += (s, e) =>
            {
                boton.BackColor = Color.FromArgb(130, 55, 35);
            };

            boton.MouseLeave += (s, e) =>
            {
                boton.BackColor = Color.FromArgb(75, 30, 25);
            };

            boton.Click += (s, e) => accion();

            panelPrincipal.Controls.Add(boton);
        }

        private Label CrearEtiqueta(
            string texto, int x, int y, int ancho, int alto,
            int tamaño, Color color, FontStyle estilo)
        {
            Label etiqueta = new Label();

            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.Size = new Size(ancho, alto);
            etiqueta.Font = new Font("Georgia", tamaño, estilo);
            etiqueta.ForeColor = color;
            etiqueta.BackColor = Color.Transparent;

            panelPrincipal.Controls.Add(etiqueta);

            return etiqueta;
        }

        // ==========================================
        // MENU PRINCIPAL
        // ==========================================

        private void MostrarMenuPrincipal()
        {
            GuardarProgreso();
            PrepararPanel();

            CrearEtiqueta(
                "NEXUS", 820, 65, 400, 65,
                42, Color.White, FontStyle.Bold
            );

            CrearEtiqueta(
                "EL AMANECER DE LOS GIGANTES",
                790, 125, 450, 35,
                13, Color.FromArgb(220, 190, 160),
                FontStyle.Bold
            );

            CrearBoton("NUEVA PARTIDA", 830, 195, NuevaPartida);
            CrearBoton("CONTINUAR PARTIDA", 830, 253, ContinuarPartida);
            CrearBoton("ELIMINAR PARTIDA GUARDADA", 830, 311, EliminarPartidaGuardada);
            CrearBoton("MAPAS DE LAS MURALLAS", 830, 369, MostrarMapas);
            CrearBoton("INVENTARIO", 830, 427, MostrarInventario);
            CrearBoton("ESTADO DEL CADETE", 830, 485, MostrarEstadoCadete);
            CrearBoton("SALIR DEL JUEGO", 830, 543, SalirJuego);

            CrearEtiqueta("PERFIL ACTIVO", 830, 608, 130, 25, 9, Color.FromArgb(220, 190, 160), FontStyle.Bold);
            comboPerfiles = new ComboBox { Location=new Point(830,634), Size=new Size(240,30), DropDownStyle=ComboBoxStyle.DropDownList, Font=new Font("Segoe UI",10) };
            comboPerfiles.SelectedIndexChanged += (s,e) => perfilActual = comboPerfiles.SelectedItem as PerfilNexus;
            panelPrincipal.Controls.Add(comboPerfiles);
            Button crearPerfil = new Button { Text="CREAR PERFIL", Location=new Point(1080,633), Size=new Size(170,31), BackColor=Color.FromArgb(75,48,38), ForeColor=Color.White, Font=new Font("Segoe UI",9,FontStyle.Bold), FlatStyle=FlatStyle.Flat };
            crearPerfil.Click += (s,e) => CrearPerfil();
            panelPrincipal.Controls.Add(crearPerfil);
            RecargarPerfiles();

            CrearEtiqueta(
                "LUCHA  •  EXPLORA  •  SOBREVIVE",
                850, 675, 400, 25,
                11, Color.White, FontStyle.Italic
            );
        }

        private void RecargarPerfiles(int seleccionarId = 0)
        {
            if (comboPerfiles == null) return;
            int idPreferido = seleccionarId != 0 ? seleccionarId : (perfilActual == null ? 0 : perfilActual.Id);
            try
            {
                List<PerfilNexus> perfiles = persistencia.ObtenerPerfiles();
                comboPerfiles.Items.Clear();
                foreach (PerfilNexus perfil in perfiles) comboPerfiles.Items.Add(perfil);
                int indice = perfiles.FindIndex(p => p.Id == idPreferido);
                if (indice < 0 && perfiles.Count > 0) indice = 0;
                comboPerfiles.SelectedIndex = indice;
            }
            catch (Exception ex)
            {
                comboPerfiles.Items.Clear();
                perfilActual = null;
                MessageBox.Show("No se pudo conectar con SQL Server. Ejecuta BaseDatos/01_Crear_NexusMemoria.sql y revisa la cadena NexusDb de App.config.\n\nDetalle: " + ex.Message,
                    "Persistencia NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CrearPerfil()
        {
            string nombre = SolicitarTexto("Escribe el nombre del perfil:", "Nuevo perfil", "Jugador");
            if (nombre == null) return;
            try
            {
                PerfilNexus perfil = persistencia.CrearPerfil(nombre);
                perfilActual = perfil;
                RecargarPerfiles(perfil.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo crear el perfil. El nombre debe ser único.\n\nDetalle: " + ex.Message,
                    "Persistencia NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string SolicitarTexto(string mensaje, string titulo, string inicial)
        {
            using (Form dialogo = new Form { Text=titulo, StartPosition=FormStartPosition.CenterParent, FormBorderStyle=FormBorderStyle.FixedDialog,
                MinimizeBox=false, MaximizeBox=false, ShowInTaskbar=false, ClientSize=new Size(390,125) })
            {
                Label etiqueta = new Label { Text=mensaje, Location=new Point(14,12), Size=new Size(360,24) };
                TextBox entrada = new TextBox { Text=inicial, Location=new Point(14,41), Size=new Size(360,25), MaxLength=60 };
                Button aceptar = new Button { Text="Aceptar", DialogResult=DialogResult.OK, Location=new Point(205,82), Size=new Size(80,28) };
                Button cancelar = new Button { Text="Cancelar", DialogResult=DialogResult.Cancel, Location=new Point(294,82), Size=new Size(80,28) };
                dialogo.Controls.AddRange(new Control[] { etiqueta, entrada, aceptar, cancelar });
                dialogo.AcceptButton=aceptar; dialogo.CancelButton=cancelar;
                return dialogo.ShowDialog(this)==DialogResult.OK ? entrada.Text.Trim() : null;
            }
        }

        private PartidaNexus SeleccionarPartida(bool incluirTerminadas)
        {
            if (perfilActual == null)
            {
                MessageBox.Show("Primero selecciona un perfil.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            List<PartidaNexus> partidas;
            try { partidas = persistencia.ObtenerPartidas(perfilActual.Id, incluirTerminadas); }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron consultar las partidas guardadas.\n\nDetalle: " + ex.Message, "Persistencia NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (partidas.Count == 0)
            {
                MessageBox.Show("Este perfil no tiene partidas guardadas para esta acción.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            using (Form dialogo = new Form { Text="Partidas de " + perfilActual.Nombre, StartPosition=FormStartPosition.CenterParent,
                FormBorderStyle=FormBorderStyle.FixedDialog, MinimizeBox=false, MaximizeBox=false, ShowInTaskbar=false, ClientSize=new Size(600,300) })
            using (ListBox lista = new ListBox { Location=new Point(12,12), Size=new Size(576,225), Font=new Font("Segoe UI",10) })
            {
                foreach (PartidaNexus partida in partidas) lista.Items.Add(partida);
                lista.SelectedIndex=0;
                Button aceptar = new Button { Text="Seleccionar", DialogResult=DialogResult.OK, Location=new Point(390,253), Size=new Size(95,30) };
                Button cancelar = new Button { Text="Cancelar", DialogResult=DialogResult.Cancel, Location=new Point(493,253), Size=new Size(95,30) };
                dialogo.Controls.AddRange(new Control[] { lista, aceptar, cancelar });
                dialogo.AcceptButton=aceptar; dialogo.CancelButton=cancelar;
                return dialogo.ShowDialog(this)==DialogResult.OK ? lista.SelectedItem as PartidaNexus : null;
            }
        }

        private void ContinuarPartida()
        {
            PartidaNexus partida = SeleccionarPartida(false);
            if (partida == null) return;
            partidaIdActual = partida.Id;
            perfilIdPartidaActual = partida.PerfilId;
            cadete = new Cadete("Eren Jaeger");
            cadete.RestaurarEstado(partida.Salud, partida.Energia, partida.Ataque, partida.Experiencia,
                partida.Escudos, partida.Materiales, partida.Suministros, partida.Inventario);
            partidaPendiente = partida;
            MostrarMapaMuralla(partida.Muralla);
        }

        private void EliminarPartidaGuardada()
        {
            PartidaNexus partida = SeleccionarPartida(true);
            if (partida == null) return;
            if (MessageBox.Show("¿Eliminar la partida de " + partida.Muralla + "? Esta acción no se puede deshacer.",
                "Eliminar partida", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                persistencia.EliminarPartida(partida.Id, partida.PerfilId);
                if (partida.Id == partidaIdActual) { partidaIdActual=0; perfilIdPartidaActual=0; }
                MessageBox.Show("Partida eliminada.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("No se pudo eliminar la partida.\n\n" + ex.Message, "Persistencia NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private string ObtenerCasillasProcesadas()
        {
            if (zonasProcesadas == null || mapaActual == null) return "";
            List<string> indices = new List<string>();
            int columnas = zonasProcesadas.GetLength(1);
            for (int f=0; f<zonasProcesadas.GetLength(0); f++)
                for (int c=0; c<columnas; c++) if (zonasProcesadas[f,c]) indices.Add((f*columnas+c).ToString());
            return String.Join(",", indices.ToArray());
        }

        private void RestaurarCasillasProcesadas(string texto)
        {
            if (zonasProcesadas == null || String.IsNullOrWhiteSpace(texto)) return;
            int columnas = zonasProcesadas.GetLength(1);
            foreach (string fragmento in texto.Split(','))
            {
                int indice;
                if (!Int32.TryParse(fragmento, out indice)) continue;
                int fila=indice/columnas, columna=indice%columnas;
                if (fila>=0 && fila<zonasProcesadas.GetLength(0)) zonasProcesadas[fila,columna]=true;
            }
        }

        private void GuardarProgreso()
        {
            if (partidaIdActual <= 0 || perfilIdPartidaActual <= 0 || cadete == null || String.IsNullOrEmpty(murallaActual)) return;
            try
            {
                PartidaNexus partida = new PartidaNexus {
                    Id=partidaIdActual, PerfilId=perfilIdPartidaActual, Perfil=cadete.Nombre, Muralla=murallaActual,
                    Salud=cadete.Salud, Energia=cadete.Energia, Ataque=cadete.Ataque, Experiencia=cadete.Experiencia,
                    Escudos=cadete.Escudos, Materiales=cadete.Materiales, Suministros=cadete.Suministros,
                    Nivel=nivelOleada, Eliminaciones=eliminacionesOleada, ExperienciaOleada=experienciaOleada,
                    TiempoOleada=tiempoOleada, Fila=jugadorFila, Columna=jugadorColumna,
                    JefeDerrotado=giganteDerrotado, Terminada=partidaTerminada,
                    CasillasProcesadas=ObtenerCasillasProcesadas(), Inventario=new List<string>(cadete.Inventario)
                };
                persistencia.Guardar(partida);
                avisoPersistenciaMostrado=false;
            }
            catch (Exception ex)
            {
                if (!avisoPersistenciaMostrado)
                {
                    avisoPersistenciaMostrado=true;
                    MessageBox.Show("No se pudo guardar el progreso en SQL Server. La partida seguirá abierta; revisa la conexión y vuelve a guardar.\n\nDetalle: " + ex.Message,
                        "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // ==========================================
        // NUEVA PARTIDA Y CINEMATICA
        // ==========================================

        private void NuevaPartida()
        {
            if (perfilActual == null)
            {
                MessageBox.Show("Selecciona o crea un perfil antes de iniciar una partida.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            GuardarProgreso();
            partidaIdActual = 0;
            perfilIdPartidaActual = 0;
            partidaPendiente = null;
            cadete = new Cadete("Eren Jaeger");

            escenaActual = 0;
            partidaTerminada = false;
            registroEventos = "";

            giganteDerrotado = false;
            combateActivo = false;
            jefeActual = null;
            panelJefe = null;
            zonaJefe = null;
            panelMejoras = null;
            turnoEnemigoJefe = false;
            proyectilesJefe.Clear();
            ataquesJefe = 0;

            MostrarCinematica();
        }

        private void MostrarCinematica()
        {
            PrepararPanel();

            panelPrincipal.BackColor = Color.FromArgb(8, 10, 15);

            CrearEtiqueta(
                "NEXUS ARCHIVES  //  REGISTRO HISTORICO",
                100, 65, 700, 30,
                11, Color.FromArgb(180, 60, 50),
                FontStyle.Bold
            );

            CrearEtiqueta(
                "2297", 850, 100, 300, 90,
                65, Color.FromArgb(65, 65, 75),
                FontStyle.Bold
            );

            Panel linea = new Panel();
            linea.BackColor = Color.DarkRed;
            linea.Location = new Point(100, 145);
            linea.Size = new Size(550, 4);
            panelPrincipal.Controls.Add(linea);

            tituloCinematica = CrearEtiqueta(
                "", 100, 200, 1050, 65,
                25, Color.FromArgb(230, 210, 180),
                FontStyle.Bold
            );

            textoCinematica = new Label();
            textoCinematica.Font = new Font("Georgia", 16);
            textoCinematica.ForeColor = Color.White;
            textoCinematica.BackColor = Color.Transparent;
            textoCinematica.Location = new Point(100, 300);
            textoCinematica.Size = new Size(1050, 190);
            panelPrincipal.Controls.Add(textoCinematica);

            CrearEtiqueta(
                "REGISTRO CLASIFICADO  //  SISTEMA IRIS",
                100, 550, 600, 30,
                10, Color.Gray, FontStyle.Bold
            );

            botonContinuar = new Button();
            botonContinuar.Text = "CONTINUAR";
            botonContinuar.Size = new Size(220, 55);
            botonContinuar.Location = new Point(850, 560);
            botonContinuar.BackColor = Color.FromArgb(100, 25, 25);
            botonContinuar.ForeColor = Color.White;
            botonContinuar.Font = new Font("Arial", 12, FontStyle.Bold);
            botonContinuar.FlatStyle = FlatStyle.Flat;
            botonContinuar.Click += (s, e) => AvanzarCinematica();
            panelPrincipal.Controls.Add(botonContinuar);

            Button omitir = new Button();
            omitir.Text = "OMITIR CINEMATICA";
            omitir.Size = new Size(220, 40);
            omitir.Location = new Point(850, 625);
            omitir.BackColor = Color.FromArgb(35, 35, 40);
            omitir.ForeColor = Color.LightGray;
            omitir.FlatStyle = FlatStyle.Flat;
            omitir.Click += (s, e) => MostrarMapas();
            panelPrincipal.Controls.Add(omitir);

            timerCinematica = new Timer();
            timerCinematica.Interval = 35;
            timerCinematica.Tick += TimerCinematica_Tick;

            CargarEscenaCinematica();
        }

        private void CargarEscenaCinematica()
        {
            timerCinematica.Stop();

            caracterActual = 0;

            tituloCinematica.Text = titulosEscenas[escenaActual];
            textoCinematica.Text = "";

            botonContinuar.Text = "CONTINUAR";

            timerCinematica.Start();
        }

        private void TimerCinematica_Tick(object sender, EventArgs e)
        {
            string texto = textosEscenas[escenaActual];

            if (caracterActual < texto.Length)
            {
                caracterActual++;
                textoCinematica.Text =
                    texto.Substring(0, caracterActual);
            }
            else
            {
                timerCinematica.Stop();
                botonContinuar.Text = "SIGUIENTE ESCENA";
            }
        }

        private void AvanzarCinematica()
        {
            if (timerCinematica.Enabled)
            {
                timerCinematica.Stop();

                textoCinematica.Text = textosEscenas[escenaActual];

                caracterActual = textosEscenas[escenaActual].Length;

                botonContinuar.Text = "SIGUIENTE ESCENA";
                return;
            }

            escenaActual++;

            if (escenaActual >= textosEscenas.Length)
            {
                MostrarMapas();
                return;
            }

            CargarEscenaCinematica();
        }

        // ==========================================
        // SELECCION DE MURALLAS
        // ==========================================

        private void MostrarMapas()
        {
            GuardarProgreso();
            partidaIdActual = 0;
            perfilIdPartidaActual = 0;
            PrepararPanel();

            panelPrincipal.BackColor = Color.FromArgb(17, 15, 14);

            CrearEtiqueta(
                "NEXUS  //  TERRITORIOS HUMANOS",
                70, 20, 700, 25,
                10, Color.FromArgb(190, 150, 110),
                FontStyle.Bold
            );

            CrearEtiqueta(
                "SELECCION DE MURALLAS",
                70, 48, 800, 45,
                27, Color.FromArgb(238, 220, 190),
                FontStyle.Bold
            );

            CrearEtiqueta(
                "ELIGE UN TERRITORIO PARA COMENZAR TU EXPLORACION",
                74, 98, 800, 25,
                10, Color.Silver, FontStyle.Bold
            );

            tarjetaMaria = CrearTarjetaMuralla(
                "MARIA",
                "EL REFUGIO DEL SUR",
                "Territorio de extensos campos, aldeas y antiguas fortalezas. Sus murallas protegen a miles de supervivientes.",
                "mapa_maria.png",
                70
            );

            tarjetaRose = CrearTarjetaMuralla(
                "ROSE",
                "EL TERRITORIO CENTRAL",
                "Una region de grandes asentamientos y caminos antiguos. Fuera de sus puertas acechan criaturas gigantes.",
                "mapa_rose.png",
                455
            );

            tarjetaSina = CrearTarjetaMuralla(
                "SINA",
                "EL ULTIMO BASTION",
                "La zona mas protegida de la humanidad. Sus torres vigilan un territorio lleno de secretos y peligros.",
                "mapa_sina.png",
                840
            );

            descripcionMuralla = new Label();
            descripcionMuralla.Font = new Font("Georgia", 10, FontStyle.Bold);
            descripcionMuralla.ForeColor = Color.FromArgb(235, 215, 185);
            descripcionMuralla.BackColor = Color.Transparent;
            descripcionMuralla.Location = new Point(75, 535);
            descripcionMuralla.Size = new Size(760, 55);
            panelPrincipal.Controls.Add(descripcionMuralla);

            botonEntrar = new Button();
            botonEntrar.Text = "ENTRAR A MURALLA MARIA";
            botonEntrar.Size = new Size(330, 55);
            botonEntrar.Location = new Point(850, 545);
            botonEntrar.BackColor = Color.FromArgb(105, 35, 27);
            botonEntrar.ForeColor = Color.White;
            botonEntrar.Font = new Font("Georgia", 11, FontStyle.Bold);
            botonEntrar.FlatStyle = FlatStyle.Flat;
            botonEntrar.FlatAppearance.BorderColor = Color.FromArgb(205, 165, 115);
            botonEntrar.FlatAppearance.BorderSize = 2;

            botonEntrar.Click += (s, e) =>
            {
                if (cadete == null)
                    cadete = new Cadete("Eren Jaeger");

                MostrarMapaMuralla(murallaSeleccionada);
            };

            panelPrincipal.Controls.Add(botonEntrar);

            Button volver = new Button();
            volver.Text = "VOLVER AL MENU";
            volver.Size = new Size(220, 38);
            volver.Location = new Point(70, 625);
            volver.BackColor = Color.FromArgb(45, 38, 34);
            volver.ForeColor = Color.White;
            volver.FlatStyle = FlatStyle.Flat;
            volver.Click += (s, e) => MostrarMenuPrincipal();
            panelPrincipal.Controls.Add(volver);

            SeleccionarMuralla("MARIA");
        }

        private Panel CrearTarjetaMuralla(
            string nombre,
            string subtitulo,
            string descripcion,
            string archivoImagen,
            int x)
        {
            Panel tarjeta = new Panel();
            tarjeta.Location = new Point(x, 145);
            tarjeta.Size = new Size(350, 370);
            tarjeta.BackColor = Color.FromArgb(38, 32, 28);
            tarjeta.BorderStyle = BorderStyle.FixedSingle;
            tarjeta.Cursor = Cursors.Hand;

            PictureBox imagen = new PictureBox();
            imagen.Location = new Point(8, 8);
            imagen.Size = new Size(332, 205);
            imagen.SizeMode = PictureBoxSizeMode.StretchImage;
            imagen.BackColor = Color.FromArgb(65, 55, 45);

            string ruta = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                archivoImagen
            );

            if (File.Exists(ruta))
            {
                try
                {
                    using (Image original = Image.FromFile(ruta))
                    {
                        imagen.Image = new Bitmap(original);
                    }
                }
                catch
                {
                    imagen.Image = null;
                }
            }

            if (imagen.Image == null)
            {
                Label aviso = new Label();
                aviso.Text = "IMAGEN NO ENCONTRADA\n" + archivoImagen;
                aviso.Font = new Font("Arial", 10, FontStyle.Bold);
                aviso.ForeColor = Color.White;
                aviso.TextAlign = ContentAlignment.MiddleCenter;
                aviso.Dock = DockStyle.Fill;
                imagen.Controls.Add(aviso);
            }

            tarjeta.Controls.Add(imagen);

            Label nombreLabel = new Label();
            nombreLabel.Text = "MURALLA " + nombre;
            nombreLabel.Font = new Font("Georgia", 16, FontStyle.Bold);
            nombreLabel.ForeColor = Color.FromArgb(235, 215, 185);
            nombreLabel.Location = new Point(15, 222);
            nombreLabel.Size = new Size(320, 30);
            tarjeta.Controls.Add(nombreLabel);

            Label subtituloLabel = new Label();
            subtituloLabel.Text = subtitulo;
            subtituloLabel.Font = new Font("Arial", 9, FontStyle.Bold);
            subtituloLabel.ForeColor = Color.FromArgb(190, 145, 105);
            subtituloLabel.Location = new Point(17, 255);
            subtituloLabel.Size = new Size(315, 25);
            tarjeta.Controls.Add(subtituloLabel);

            Label descripcionLabel = new Label();
            descripcionLabel.Text = descripcion;
            descripcionLabel.Font = new Font("Arial", 9);
            descripcionLabel.ForeColor = Color.Gainsboro;
            descripcionLabel.Location = new Point(17, 285);
            descripcionLabel.Size = new Size(315, 65);
            tarjeta.Controls.Add(descripcionLabel);

            tarjeta.Click += (s, e) => SeleccionarMuralla(nombre);
            imagen.Click += (s, e) => SeleccionarMuralla(nombre);
            nombreLabel.Click += (s, e) => SeleccionarMuralla(nombre);
            subtituloLabel.Click += (s, e) => SeleccionarMuralla(nombre);
            descripcionLabel.Click += (s, e) => SeleccionarMuralla(nombre);

            tarjeta.Paint += (s, e) =>
            {
                if (murallaSeleccionada == nombre)
                {
                    using (Pen lapiz = new Pen(Color.Gold, 4))
                    {
                        e.Graphics.DrawRectangle(
                            lapiz, 1, 1,
                            tarjeta.Width - 3,
                            tarjeta.Height - 3
                        );
                    }
                }
            };

            panelPrincipal.Controls.Add(tarjeta);

            return tarjeta;
        }

        private void SeleccionarMuralla(string nombre)
        {
            murallaSeleccionada = nombre;

            tarjetaMaria.Invalidate();
            tarjetaRose.Invalidate();
            tarjetaSina.Invalidate();

            if (nombre == "MARIA")
            {
                descripcionMuralla.Text =
                    "MARIA: Campos verdes, aldeas y antiguas fortalezas.";

                botonEntrar.Text = "ENTRAR A MURALLA MARIA";
            }
            else if (nombre == "ROSE")
            {
                descripcionMuralla.Text =
                    "ROSE: Caminos antiguos y asentamientos humanos.";

                botonEntrar.Text = "ENTRAR A MURALLA ROSE";
            }
            else
            {
                descripcionMuralla.Text =
                    "SINA: El ultimo bastion de la humanidad.";

                botonEntrar.Text = "ENTRAR A MURALLA SINA";
            }
        }

        // ==========================================
        // MAPAS DE EXPLORACION
        // ==========================================

        private void MostrarMapaMuralla(string nombre)
        {
            if (perfilActual == null)
            {
                MessageBox.Show("Selecciona o crea un perfil antes de entrar a la muralla.", "NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MostrarMenuPrincipal();
                return;
            }
            PrepararPanel();

            murallaActual = nombre;
            partidaTerminada = false;

            if (cadete == null) cadete = new Cadete("Eren Jaeger");

            giganteDerrotado = false;
            enemigosMapa.Clear();
            eliminacionesOleada = 0;
            experienciaOleada = 0;
            tiempoOleada = 0;
            nivelOleada = 1;
            dificultadMuralla = nombre == "MARIA" ? 1 : (nombre == "ROSE" ? 2 : 3);
            danioDisparo = cadete.Ataque;

            if (nombre == "MARIA")
            {
                mapaActual = new string[,]
                {
                    {"S","S","S","#","#","#","S","S","S"},
                    {"S",".",".","E",".",".",".",".","S"},
                    {"#","#",".","#",".","R",".","#","#"},
                    {"G",".",".","#",".","#",".",".","E"},
                    {"#",".","R",".",".",".","#",".","#"},
                    {"S",".",".",".","G",".",".",".","S"},
                    {"S","S","S","S","S","S","S","S","S"}
                };
            }
            else if (nombre == "ROSE")
            {
                mapaActual = new string[,]
                {
                    {"S","S","S","S","S","S","S","S","S"},
                    {"S",".",".","R",".",".",".",".","S"},
                    {"#","#",".","#",".","#","#",".","#"},
                    {"E",".",".",".","G",".",".",".","E"},
                    {"#",".","#","#",".","#",".","#","#"},
                    {"S",".",".","R",".",".",".",".","S"},
                    {"S","S","S","S","S","S","S","S","S"}
                };
            }
            else
            {
                mapaActual = new string[,]
                {
                    {"S","S","S","S","S","S","S","S","S"},
                    {"S",".",".",".","R",".",".",".","S"},
                    {"#","#","#",".","#",".","#","#","#"},
                    {"E",".",".",".","G",".",".",".","E"},
                    {"#",".","#",".","#",".","#",".","#"},
                    {"S",".",".",".",".",".","R",".","S"},
                    {"S","S","S","S","S","S","S","S","S"}
                };
            }

            jugadorFila = 1;
            jugadorColumna = 1;

            int filas = mapaActual.GetLength(0);
            int columnas = mapaActual.GetLength(1);

            for (int f = 0; f < filas; f++)
            {
                for (int c = 0; c < columnas; c++)
                {
                    if (mapaActual[f, c] == "G")
                    {
                        AgregarEnemigo(f, c);
                        mapaActual[f, c] = ".";
                    }
                }
            }

            casillasMapa = new Button[filas, columnas];
            zonasProcesadas = new bool[filas, columnas];

            panelPrincipal.BackColor = Color.FromArgb(22, 20, 18);

            Panel encabezado = new Panel();
            encabezado.Location = new Point(0, 0);
            encabezado.Size = new Size(1260, 105);
            encabezado.BackColor = Color.FromArgb(42, 32, 27);
            panelPrincipal.Controls.Add(encabezado);

            Label titulo = new Label();
            titulo.Text = "NEXUS  /  MURALLA " + nombre;
            titulo.Font = new Font("Georgia", 23, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(235, 215, 185);
            titulo.Location = new Point(35, 15);
            titulo.Size = new Size(650, 40);
            encabezado.Controls.Add(titulo);

            textoTituloZona = new Label();
            textoTituloZona.Text = "SUPERVIVENCIA  ·  HORDA DETECTADA";
            textoTituloZona.Font = new Font("Arial", 10, FontStyle.Bold);
            textoTituloZona.ForeColor = Color.FromArgb(195, 155, 110);
            textoTituloZona.Location = new Point(40, 60);
            textoTituloZona.Size = new Size(500, 25);
            encabezado.Controls.Add(textoTituloZona);

            textoHUD = new Label();
            textoHUD.Font = new Font("Arial", 10, FontStyle.Bold);
            textoHUD.ForeColor = Color.White;
            textoHUD.BackColor = Color.FromArgb(55, 43, 35);
            textoHUD.Location = new Point(690, 15);
            textoHUD.Size = new Size(535, 70);
            textoHUD.TextAlign = ContentAlignment.MiddleLeft;
            textoHUD.Padding = new Padding(12);
            encabezado.Controls.Add(textoHUD);

            // PANEL DEL MAPA
            Panel panelMapa = new PanelSuave();
            panelMapa.Location = new Point(35, 125);
            panelMapa.Size = new Size(790, 455);
            panelMapa.BackColor = Color.FromArgb(31, 32, 29);
            panelMapa.BorderStyle = BorderStyle.None;
            panelPrincipal.Controls.Add(panelMapa);

            Label tituloMapa = new Label();
            tituloMapa.Text = "  CARTA DE OPERACIONES  /  " + murallaActual;
            tituloMapa.Font = new Font("Georgia", 12, FontStyle.Bold);
            tituloMapa.ForeColor = Color.FromArgb(236, 207, 160);
            tituloMapa.Location = new Point(15, 7);
            tituloMapa.Size = new Size(600, 27);
            panelMapa.Controls.Add(tituloMapa);

            Label subtituloMapa = new Label { Text = "SECTOR EXTERIOR  ·  IRIS TACTICAL SURVEY  ·  W A S D", Font = new Font("Consolas", 8, FontStyle.Bold), ForeColor = Color.FromArgb(137, 150, 141), Location = new Point(19, 31), Size = new Size(620, 16), BackColor = Color.Transparent };
            panelMapa.Controls.Add(subtituloMapa);

            TableLayoutPanel tabla = new TableLayoutPanel();
            tabla.Location = new Point(18, 52);
            tabla.Size = new Size(750, 380);
            tabla.RowCount = filas;
            tabla.ColumnCount = columnas;
            tabla.BackColor = Color.FromArgb(23, 22, 19);
            tabla.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            for (int c = 0; c < columnas; c++)
            {
                tabla.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 100f / columnas)
                );
            }

            for (int f = 0; f < filas; f++)
            {
                tabla.RowStyles.Add(
                    new RowStyle(SizeType.Percent, 100f / filas)
                );
            }

            for (int f = 0; f < filas; f++)
            {
                for (int c = 0; c < columnas; c++)
                {
                    Button casilla = new BaldosaMapa { Fila = f, Columna = c, Tag = mapaActual[f, c] };

                    casilla.Dock = DockStyle.Fill;
                    casilla.Margin = new Padding(1);

                    casilla.Font = new Font(
                        "Segoe UI Symbol", 17, FontStyle.Bold
                    );

                    casilla.TabStop = false;

                    casillasMapa[f, c] = casilla;
                    tabla.Controls.Add(casilla, c, f);
                }
            }

            panelMapa.Controls.Add(tabla);

            // PANEL LATERAL
            Panel panelLateral = new Panel();
            panelLateral.Location = new Point(845, 125);
            panelLateral.Size = new Size(380, 455);
            panelLateral.BackColor = Color.FromArgb(43, 34, 29);
            panelLateral.BorderStyle = BorderStyle.FixedSingle;
            panelPrincipal.Controls.Add(panelLateral);

            Label tituloEstado = new Label();
            tituloEstado.Text = "ESTADO DEL CADETE";
            tituloEstado.Font = new Font("Georgia", 15, FontStyle.Bold);
            tituloEstado.ForeColor = Color.FromArgb(235, 210, 175);
            tituloEstado.Location = new Point(18, 15);
            tituloEstado.Size = new Size(340, 30);
            panelLateral.Controls.Add(tituloEstado);

            Label separador = new Label();
            separador.BorderStyle = BorderStyle.Fixed3D;
            separador.Location = new Point(18, 50);
            separador.Size = new Size(340, 2);
            panelLateral.Controls.Add(separador);

            barraVida = CrearBarraVisual(22, 60, 330, 18, 100, cadete.Salud, Color.FromArgb(218, 63, 74), panelLateral);
            barraExperiencia = CrearBarraVisual(22, 83, 330, 12, 3, experienciaOleada, Color.FromArgb(92, 177, 238), panelLateral);

            Label leyenda = new Label();
            leyenda.Text =
                "WASD  ·  MOVERSE\n" +
                "AL CHOCAR: COMBATE DE JEFE\n" +
                "ATAQUE AUTOMATICO  ·  ESQUIVA CON WASD\n" +
                "S  DESCANSO     E  SUMINISTROS\n" +
                "R  CAJA: MATERIAL + RACION\n" +
                "BUSCA CAJAS: PAGAN MEJORAS";
            leyenda.Font = new Font("Arial", 10, FontStyle.Bold);
            leyenda.ForeColor = Color.Gainsboro;
            leyenda.Location = new Point(22, 105);
            leyenda.Size = new Size(330, 95);
            panelLateral.Controls.Add(leyenda);

            Label tituloRegistro = new Label();
            tituloRegistro.Text = "REGISTRO DE EXPEDICION";
            tituloRegistro.Font = new Font("Georgia", 11, FontStyle.Bold);
            tituloRegistro.ForeColor = Color.FromArgb(225, 190, 145);
            tituloRegistro.Location = new Point(18, 208);
            tituloRegistro.Size = new Size(340, 25);
            panelLateral.Controls.Add(tituloRegistro);

            textoRegistro = new Label();
            textoRegistro.Font = new Font("Arial", 9);
            textoRegistro.ForeColor = Color.White;
            textoRegistro.BackColor = Color.FromArgb(28, 25, 23);
            textoRegistro.Location = new Point(18, 238);
            textoRegistro.Size = new Size(340, 128);
            textoRegistro.Padding = new Padding(9);
            textoRegistro.Text = "IRIS: Expedicion iniciada.\nExplora el territorio.";
            panelLateral.Controls.Add(textoRegistro);

            botonSuministro = new Button();
            botonSuministro.Text = "USAR SUMINISTRO";
            botonSuministro.Location = new Point(18, 380);
            botonSuministro.Size = new Size(160, 45);
            botonSuministro.BackColor = Color.FromArgb(75, 95, 55);
            botonSuministro.ForeColor = Color.White;
            botonSuministro.Font = new Font("Arial", 9, FontStyle.Bold);
            botonSuministro.FlatStyle = FlatStyle.Flat;
            botonSuministro.Click += (s, e) => UsarSuministro();
            panelLateral.Controls.Add(botonSuministro);

            Button botonInventario = new Button();
            botonInventario.Text = "INVENTARIO";
            botonInventario.Location = new Point(195, 380);
            botonInventario.Size = new Size(160, 45);
            botonInventario.BackColor = Color.FromArgb(75, 55, 40);
            botonInventario.ForeColor = Color.White;
            botonInventario.Font = new Font("Arial", 9, FontStyle.Bold);
            botonInventario.FlatStyle = FlatStyle.Flat;
            botonInventario.Click += (s, e) => MostrarInventario();
            panelLateral.Controls.Add(botonInventario);

            // CONTROLES INFERIORES
            Label controles = new Label();
            controles.Text =
                "W/A/S/D MOVERSE   ·   AL CHOCAR EMPIEZA EL COMBATE DE JEFE";
            controles.Font = new Font("Arial", 9, FontStyle.Bold);
            controles.ForeColor = Color.FromArgb(235, 210, 175);
            controles.Location = new Point(45, 600);
            controles.Size = new Size(780, 30);
            panelPrincipal.Controls.Add(controles);

            Button volver = new Button();
            volver.Text = "VOLVER A LAS MURALLAS";
            volver.Location = new Point(875, 595);
            volver.Size = new Size(330, 45);
            volver.BackColor = Color.FromArgb(75, 30, 25);
            volver.ForeColor = Color.White;
            volver.Font = new Font("Arial", 10, FontStyle.Bold);
            volver.FlatStyle = FlatStyle.Flat;
            volver.Click += (s, e) => MostrarMapas();
            panelPrincipal.Controls.Add(volver);

            if (partidaPendiente != null)
            {
                nivelOleada=Math.Max(1,partidaPendiente.Nivel);
                eliminacionesOleada=partidaPendiente.Eliminaciones;
                experienciaOleada=partidaPendiente.ExperienciaOleada;
                tiempoOleada=partidaPendiente.TiempoOleada;
                giganteDerrotado=partidaPendiente.JefeDerrotado;
                jugadorFila=Math.Max(0,Math.Min(mapaActual.GetLength(0)-1,partidaPendiente.Fila));
                jugadorColumna=Math.Max(0,Math.Min(mapaActual.GetLength(1)-1,partidaPendiente.Columna));
                if (mapaActual[jugadorFila,jugadorColumna]=="#") { jugadorFila=1; jugadorColumna=1; }
                RestaurarCasillasProcesadas(partidaPendiente.CasillasProcesadas);
                if (giganteDerrotado) enemigosMapa.Clear();
                partidaPendiente=null;
            }
            else
            {
                try
                {
                    partidaIdActual=persistencia.CrearPartida(perfilActual.Id,cadete,nombre);
                    perfilIdPartidaActual=perfilActual.Id;
                }
                catch (Exception ex)
                {
                    partidaIdActual=0;
                    perfilIdPartidaActual=0;
                    MessageBox.Show("No se pudo crear el registro de la partida en SQL Server.\n\n" + ex.Message, "Persistencia NEXUS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            AgregarRegistro("IRIS: Conexion establecida.");
            AgregarRegistro("Objetivo: Explorar la muralla " + nombre + ".");
            AgregarRegistro("Sobrevive a la horda. Evita que un Gigante te alcance.");
            AgregarRegistro("Derrota 5 jefes y vuelve a la entrada de la muralla.");

            ActualizarHUD();
            ActualizarMapaVisual();
            if (!giganteDerrotado) IniciarCombate();
            GuardarProgreso();

            ActiveControl = null;
            Focus();
        }

        // ==========================================
        // MOVIMIENTO WASD
        // ==========================================

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (mapaActual == null || casillasMapa == null)
                return;

            if (partidaTerminada)
                return;

            if (panelJefe != null && !panelJefe.IsDisposed && panelJefe.Visible)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) AccionLuchar();
                else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) AccionActuar();
                else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) AccionObjeto();
                else if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4) AccionPerdonar();
                else MoverCadeteEnCombate(e.KeyCode);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            int nuevaFila = jugadorFila;
            int nuevaColumna = jugadorColumna;

            switch (e.KeyCode)
            {
                case Keys.W:
                    nuevaFila--;
                    break;

                case Keys.S:
                    nuevaFila++;
                    break;

                case Keys.A:
                    nuevaColumna--;
                    break;

                case Keys.D:
                    nuevaColumna++;
                    break;

                default:
                    return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (nuevaFila < 0 ||
                nuevaFila >= mapaActual.GetLength(0) ||
                nuevaColumna < 0 ||
                nuevaColumna >= mapaActual.GetLength(1))
                return;

            if (mapaActual[nuevaFila, nuevaColumna] == "#")
            {
                AgregarRegistro("IRIS: Camino bloqueado.");
                return;
            }

            jugadorFila = nuevaFila;
            jugadorColumna = nuevaColumna;

            ProcesarCasilla(jugadorFila, jugadorColumna);

            if (!cadete.Vivo)
            {
                FinalizarDerrota();
                return;
            }

            if (giganteDerrotado &&
                jugadorFila == 1 &&
                jugadorColumna == 1)
            {
                FinalizarVictoria();
                return;
            }

            ActualizarMapaVisual();
            ActualizarHUD();
            GuardarProgreso();
        }

        // ==========================================
        // EFECTOS DE LAS CASILLAS Y COMBATE
        // ==========================================

        private void ProcesarCasilla(int fila, int columna)
        {
            string zona = mapaActual[fila, columna];

            if (zona == "G")
            {
                if (giganteDerrotado)
                {
                    AgregarRegistro("IRIS: El Gigante ya fue derrotado.");
                    zonasProcesadas[fila, columna] = true;
                    return;
                }

                IniciarCombate();
                return;
            }
            if (zonasProcesadas[fila, columna])
            {
                AgregarRegistro("IRIS: Esta zona ya fue explorada.");
                return;
            }

            if (zona == "R")
            {
                cadete.AgregarMateriales(5);
                cadete.AgregarSuministros(1);
                cadete.GanarExperiencia(5);
                cadete.AgregarObjeto("Caja de materiales y raciones");

                AgregarRegistro("CAJA RECUPERADA: +5 materiales y +1 racion.");
                AgregarRegistro("IRIS: Los recursos permiten fabricar mejoras de combate.");
            }
            else if (zona == "E")
            {
                cadete.AgregarSuministros(1);
                cadete.GanarExperiencia(10);
                cadete.AgregarObjeto("Registro de exploracion");

                AgregarRegistro("EXPLORACION: Objeto descubierto.");
                AgregarRegistro("SUMINISTROS: +1 unidad.");
                AgregarRegistro("EXPERIENCIA: +10 puntos.");
            }
            else if (zona == "S")
            {
                cadete.Curar(15);
                cadete.RecuperarEnergia(15);

                AgregarRegistro("ZONA SEGURA: Descanso disponible.");
                AgregarRegistro("SALUD Y ENERGIA: Recuperacion de 15.");
            }
            else
            {
                AgregarRegistro("IRIS: Camino explorado.");
            }

            zonasProcesadas[fila, columna] = true;
        }

        // ==========================================
        // COMBATE TACTICO CONTRA EL GIGANTE
        private void IniciarCombate()
        {
            if (combateActivo || giganteDerrotado) return;
            combateActivo = true;
            timerSupervivencia = new Timer { Interval = 50 };
            timerSupervivencia.Tick += TickSupervivencia;
            timerSupervivencia.Start();
            AgregarRegistro("IRIS: Horda detectada. Evita el contacto o prepara el combate.");
            ActualizarHUD();
        }

        private void TickSupervivencia(object sender, EventArgs e)
        {
            if (!combateActivo || mapaActual == null || panelMejoras != null) return;
            tiempoOleada++;
            int rango=RangoAmenaza;
            int intervaloAparicion=Math.Max(22,45-rango*4);
            int limiteHorda=Math.Min(12,7+rango);
            int intervaloMovimiento=Math.Max(5,10-rango/2);
            if (tiempoOleada % intervaloAparicion == 0 && enemigosMapa.Count < limiteHorda) CrearEnemigoMapa();
            if (tiempoOleada % intervaloMovimiento == 0) MoverHorda();

            EnemigoMapa contacto = EnemigoEnCasilla(jugadorFila, jugadorColumna);
            if (contacto != null)
            {
                AbrirCombateJefe(contacto);
                return;
            }

            if (tiempoOleada % 4 == 0)
            {
                ActualizarMapaVisual();
                ActualizarHUD();
            }
        }

        private void CrearEnemigoMapa()
        {
            for (int intento = 0; intento < 80; intento++)
            {
                int f = azarCombate.Next(mapaActual.GetLength(0));
                int c = azarCombate.Next(mapaActual.GetLength(1));
                if (mapaActual[f,c] == "#" || mapaActual[f,c] == "S" || (f == jugadorFila && c == jugadorColumna) || BuscarEnemigo(f,c) != null) continue;
                if (Math.Abs(f-jugadorFila) + Math.Abs(c-jugadorColumna) < 3) continue;
                AgregarEnemigo(f,c);
                ActualizarMapaVisual();
                return;
            }
        }

        private void AgregarEnemigo(int fila, int columna)
        {
            int rango=RangoAmenaza;
            enemigosMapa.Add(new EnemigoMapa { Fila=fila, Columna=columna, Vida=35+eliminacionesOleada*3+rango*11 });
        }

        private EnemigoMapa BuscarEnemigo(int fila, int columna)
        {
            foreach (EnemigoMapa enemigo in enemigosMapa)
                if (enemigo.Fila == fila && enemigo.Columna == columna) return enemigo;
            return null;
        }

        private EnemigoMapa EnemigoEnCasilla(int fila, int columna)
        {
            foreach (EnemigoMapa enemigo in enemigosMapa)
                if (enemigo.Fila == fila && enemigo.Columna == columna) return enemigo;
            return null;
        }

        private void AbrirCombateJefe(EnemigoMapa enemigo)
        {
            if(panelJefe!=null&&!panelJefe.IsDisposed&&panelJefe.Visible)return;
            timerSupervivencia.Stop();jefeActual=enemigo;vidaMaximaJefe=Math.Max(180+RangoAmenaza*18,enemigo.Vida*(3+Math.Min(2,RangoAmenaza/2)));vidaJefe=vidaMaximaJefe;
            ticksJefe=0;esquivoGolpeJefe=false;debilitarJefe=false;turnoEnemigoJefe=false;ataquesJefe=0;proyectilesJefe.Clear();
            almaX=580;almaY=315;
            AgregarRegistro("CONTACTO: El Gigante te alcanzo. Comienza el duelo.");
            panelJefe=new PanelSuave {Dock=DockStyle.Fill,BackColor=Color.FromArgb(5,6,10)};panelPrincipal.Controls.Add(panelJefe);panelJefe.BringToFront();
            EtiquetaJefe("NEXUS  /  ENCUENTRO",55,20,700,25,10,Color.FromArgb(210,80,75),FontStyle.Bold);
            EtiquetaJefe("GIGANTE DEL NUCLEO",55,45,700,42,24,Color.White,FontStyle.Bold);
            zonaJefe=new PanelSuave {Location=new Point(55,95),Size=new Size(1160,430),BackColor=Color.FromArgb(8,9,14)};
            zonaJefe.Paint+=(s,e)=>DibujarEncuentro(e.Graphics);panelJefe.Controls.Add(zonaJefe);
            EtiquetaJefe("CADETE  ·  "+cadete.Nombre,55,535,300,22,10,Color.White,FontStyle.Bold);
            barraVidaJefe=CrearBarraVisual(55,558,300,20,100,cadete.Salud,Color.FromArgb(220,65,81),panelJefe);
            textoEstadoJefe=EtiquetaJefe("",385,555,360,25,10,Color.Gainsboro,FontStyle.Bold);
            EtiquetaJefe("GIGANTE",800,535,300,22,10,Color.White,FontStyle.Bold);
            barraJefe=CrearBarraVisual(800,558,415,20,vidaMaximaJefe,vidaJefe,Color.FromArgb(246,174,70),panelJefe);
            BotonAccionJefe("1  LUCHAR",55,597,250,()=>AccionLuchar());
            BotonAccionJefe("2  ACTUAR",360,597,250,()=>AccionActuar());
            BotonAccionJefe("3  OBJETO",665,597,250,()=>AccionObjeto());
            BotonAccionJefe("4  PERDONAR",970,597,245,()=>AccionPerdonar());
            textoAvisoJefe=EtiquetaJefe("ELIGE UNA ACCION  ·  DURANTE EL ATAQUE, MUEVE EL CORAZON CON WASD",55,652,1100,25,10,Color.FromArgb(232,210,165),FontStyle.Bold);
            timerJefe=new Timer {Interval=40};timerJefe.Tick+=TickCombateJefe;timerJefe.Start();ActualizarEstadoJefe();ActiveControl=null;Focus();
        }

        private Panel CrearBarraVisual(int x,int y,int ancho,int alto,int maximo,int valor,Color color,Control padre)
        {
            Panel barra=new Panel {Location=new Point(x,y),Size=new Size(ancho,alto),BackColor=Color.FromArgb(23,25,31),Tag=new DatosBarra {Valor=valor,Maximo=Math.Max(1,maximo),Color=color}};
            barra.Paint+=(s,e)=>
            {
                DatosBarra d=(DatosBarra)barra.Tag;int interior=Math.Max(0,barra.Width-4);int relleno=(int)(interior*(Math.Max(0,Math.Min(d.Maximo,d.Valor))/(double)d.Maximo));
                e.Graphics.SmoothingMode=SmoothingMode.None;
                using(Brush fondo=new SolidBrush(Color.FromArgb(17,19,25)))e.Graphics.FillRectangle(fondo,0,0,barra.Width,barra.Height);
                if(relleno>0){Color oscuro=Color.FromArgb(Math.Max(0,d.Color.R-65),Math.Max(0,d.Color.G-65),Math.Max(0,d.Color.B-65));using(LinearGradientBrush gradiente=new LinearGradientBrush(new Rectangle(2,2,Math.Max(1,relleno),Math.Max(1,barra.Height-4)),d.Color,oscuro,LinearGradientMode.Vertical))e.Graphics.FillRectangle(gradiente,2,2,relleno,barra.Height-4);}
                using(Pen borde=new Pen(Color.FromArgb(115,129,145)))e.Graphics.DrawRectangle(borde,0,0,barra.Width-1,barra.Height-1);
            };
            padre.Controls.Add(barra);return barra;
        }

        private void ActualizarBarraVisual(Panel barra,int valor)
        {if(barra==null||barra.IsDisposed)return;DatosBarra d=barra.Tag as DatosBarra;if(d==null)return;d.Valor=Math.Max(0,Math.Min(d.Maximo,valor));barra.Invalidate();}

        private Label EtiquetaJefe(string texto,int x,int y,int w,int h,int tam,Color color,FontStyle estilo)
        {Label l=new Label {Text=texto,Location=new Point(x,y),Size=new Size(w,h),Font=new Font("Consolas",tam,estilo),ForeColor=color,BackColor=Color.Transparent};panelJefe.Controls.Add(l);return l;}

        private void BotonAccionJefe(string texto,int x,int y,int w,Action accion)
        {Button b=new Button {Text=texto,Location=new Point(x,y),Size=new Size(w,45),BackColor=Color.FromArgb(22,23,29),ForeColor=Color.FromArgb(244,202,74),Font=new Font("Consolas",12,FontStyle.Bold),FlatStyle=FlatStyle.Flat};b.FlatAppearance.BorderColor=Color.FromArgb(244,202,74);b.FlatAppearance.BorderSize=2;b.Click+=(s,e)=>accion();panelJefe.Controls.Add(b);}

        private void AccionLuchar(){if(turnoEnemigoJefe)return;int combo=danioDisparo*2;vidaJefe=Math.Max(0,vidaJefe-combo);destelloJefe=true;cortesRapidos=8;ActualizarEstadoJefe();zonaJefe.Invalidate();if(vidaJefe==0){TerminarCombateJefe();return;}textoAvisoJefe.Text="EREN: COMBO RAPIDO x2  ·  "+combo+" DE DAÑO";IniciarTurnoJefe();}
        private void AccionActuar(){if(turnoEnemigoJefe)return;debilitarJefe=true;textoAvisoJefe.Text="OBSERVAS AL GIGANTE: SU PROXIMO ATAQUE SERA MAS LENTO";IniciarTurnoJefe();}
        private void AccionObjeto(){if(turnoEnemigoJefe)return;if(!cadete.UsarSuministro()){textoAvisoJefe.Text="NO QUEDAN SUMINISTROS";return;}ActualizarEstadoJefe();textoAvisoJefe.Text="USASTE UN SUMINISTRO Y RECUPERASTE SALUD";IniciarTurnoJefe();}
        private void AccionPerdonar(){if(turnoEnemigoJefe)return;if(vidaJefe>vidaMaximaJefe/4){textoAvisoJefe.Text="EL GIGANTE TODAVIA QUIERE LUCHAR";IniciarTurnoJefe();return;}TerminarCombateJefe(true);}

        private void IniciarTurnoJefe()
        {turnoEnemigoJefe=true;duracionTurnoJefe=90;ticksAtaqueJefe=0;ataquesJefe++;patronJefe=(ataquesJefe-1)%9;proyectilesJefe.Clear();foreach(Control c in panelJefe.Controls)if(c is Button)c.Enabled=false;string[] avisos={"LLUVIA DE METEOROS · ESQUIVA CON WASD","BARRIDO LATERAL · MUEVE EL CORAZON","PROYECTILES GUIADOS · CAMBIA DE RUMBO","ESPIRAL DEL NUCLEO · NO TE QUEDES QUIETO","ABANICO DE GOLPES · BUSCA LOS HUECOS","ANILLO DEL NUCLEO · ABRETE PASO","CRUZ DE ACERO · CAMBIA DE CARRIL","TRIPLE RASTREO · MUEVETE EN ZIGZAG","OLEADA DE BRECHAS · ENCUENTRA EL HUECO"};textoAvisoJefe.Text=avisos[patronJefe];zonaJefe.Invalidate();}

        private void MoverCadeteEnCombate(Keys tecla)
        {
            if(!turnoEnemigoJefe)return;int dx=0,dy=0;
            switch(tecla){case Keys.W:case Keys.Up:dy=-velocidadAlma;break;case Keys.S:case Keys.Down:dy=velocidadAlma;break;case Keys.A:case Keys.Left:dx=-velocidadAlma;break;case Keys.D:case Keys.Right:dx=velocidadAlma;break;default:return;}
            almaX=Math.Max(326,Math.Min(844,almaX+dx));almaY=Math.Max(248,Math.Min(390,almaY+dy));esquivoGolpeJefe=true;zonaJefe.Invalidate();
        }

        private void TickCombateJefe(object sender,EventArgs e)
        {
            if(panelJefe==null)return;ticksJefe++;
            if(cortesRapidos>0)cortesRapidos--;
            if(turnoEnemigoJefe)
            {
                ticksAtaqueJefe++;
                int[] intervalos={5,6,7,6,24,28,24,13,20};
                if(ticksAtaqueJefe%intervalos[patronJefe]==0)CrearProyectilJefe();
                for(int i=proyectilesJefe.Count-1;i>=0;i--)
                {
                    ProyectilJefe p=proyectilesJefe[i];p.X+=p.VX;p.Y+=p.VY;
                    if(p.X<300||p.X>860||p.Y<230||p.Y>400){proyectilesJefe.RemoveAt(i);continue;}
                    if(invulnerabilidadJefe==0&&Math.Abs(p.X-almaX)<15+p.Tamano&&Math.Abs(p.Y-almaY)<15+p.Tamano)
                    {cadete.RecibirDanio(8+RangoAmenaza*2);invulnerabilidadJefe=12;proyectilesJefe.RemoveAt(i);ActualizarEstadoJefe();if(!cadete.Vivo){FinalizarDerrota();return;}}
                }
                if(invulnerabilidadJefe>0)invulnerabilidadJefe--;
                duracionTurnoJefe--;
                if(duracionTurnoJefe<=0)
                {turnoEnemigoJefe=false;proyectilesJefe.Clear();debilitarJefe=false;foreach(Control c in panelJefe.Controls)if(c is Button)c.Enabled=true;textoAvisoJefe.Text=esquivoGolpeJefe?"ESQUIVASTE EL ATAQUE  ·  TU TURNO":"EL GIGANTE TERMINA SU ATAQUE  ·  ELIGE TU ACCION";}
            }
            if(destelloJefe&&ticksJefe%5==0)destelloJefe=false;
            zonaJefe.Invalidate();
        }

        private void CrearProyectilJefe()
        {
            int primeroNuevo=proyectilesJefe.Count;
            int izquierda=debilitarJefe?330:300,derecha=debilitarJefe?830:860;
            if(patronJefe==0)
                proyectilesJefe.Add(new ProyectilJefe {X=azarCombate.Next(izquierda,derecha),Y=235,VX=0,VY=debilitarJefe?6.2f:8.5f,Tamano=7});
            else if(patronJefe==1)
            {bool desdeIzquierda=(ataquesJefe%2)==1;proyectilesJefe.Add(new ProyectilJefe {X=desdeIzquierda?300:860,Y=azarCombate.Next(250,390),VX=desdeIzquierda?8.6f:-8.6f,VY=0,Tamano=8});}
            else if(patronJefe==2)
            {bool desdeIzquierda=azarCombate.Next(2)==0;proyectilesJefe.Add(new ProyectilJefe {X=desdeIzquierda?300:860,Y=azarCombate.Next(250,390),VX=desdeIzquierda?7.2f:-7.2f,VY=(almaY>320?4.6f:-4.6f),Tamano=7});}
            else if(patronJefe==3)
            {double angulo=(ticksJefe*0.18)+(ataquesJefe*0.7);float velocidad=debilitarJefe?5.8f:7.8f;proyectilesJefe.Add(new ProyectilJefe {X=580,Y=315,VX=(float)Math.Cos(angulo)*velocidad,VY=(float)Math.Sin(angulo)*velocidad,Tamano=7});}
            else if(patronJefe==4)
            {for(int i=-2;i<=2;i++)proyectilesJefe.Add(new ProyectilJefe {X=580+i*15,Y=235,VX=i*1.7f,VY=debilitarJefe?5.8f:7.7f,Tamano=7});}
            else if(patronJefe==5)
            {double giro=ticksAtaqueJefe*.13+ataquesJefe*.6;float velocidad=debilitarJefe?6.2f:8.0f;for(int i=0;i<10;i++){double a=giro+i*Math.PI*2/10;proyectilesJefe.Add(new ProyectilJefe {X=580,Y=315,VX=(float)Math.Cos(a)*velocidad,VY=(float)Math.Sin(a)*velocidad,Tamano=6});}}
            else if(patronJefe==6)
            {for(int i=-2;i<=2;i++){if(i!=0)proyectilesJefe.Add(new ProyectilJefe {X=300,Y=315+i*18,VX=debilitarJefe?8.0f:10.0f,VY=0,Tamano=7});if(i!=0)proyectilesJefe.Add(new ProyectilJefe {X=580+i*18,Y=230,VX=0,VY=debilitarJefe?8.0f:10.0f,Tamano=7});}}
            else if(patronJefe==7)
            {double a=Math.Atan2(almaY-315,almaX-580);float velocidad=debilitarJefe?8.0f:10.0f;for(int i=-1;i<=1;i++){double angulo=a+i*.22;proyectilesJefe.Add(new ProyectilJefe {X=580,Y=315,VX=(float)Math.Cos(angulo)*velocidad,VY=(float)Math.Sin(angulo)*velocidad,Tamano=7});}}
            else
            {int hueco=azarCombate.Next(5);bool desdeIzquierda=(ataquesJefe%2)==1;for(int i=0;i<5;i++)if(i!=hueco)proyectilesJefe.Add(new ProyectilJefe {X=desdeIzquierda?300:860,Y=250+i*34,VX=desdeIzquierda?(debilitarJefe?8.0f:9.5f):(debilitarJefe?-8.0f:-9.5f),VY=0,Tamano=8});}
            float factor=1f+Math.Min(.25f,RangoAmenaza*.05f);
            for(int i=primeroNuevo;i<proyectilesJefe.Count;i++){proyectilesJefe[i].VX*=factor;proyectilesJefe[i].VY*=factor;}
        }

        private void DibujarEncuentro(Graphics g)
        {
            g.SmoothingMode=SmoothingMode.AntiAlias;
            using(LinearGradientBrush fondo=new LinearGradientBrush(new Rectangle(0,0,zonaJefe.Width,zonaJefe.Height),Color.FromArgb(27,30,47),Color.FromArgb(8,8,15),LinearGradientMode.Vertical))g.FillRectangle(fondo,0,0,zonaJefe.Width,zonaJefe.Height);
            using(Pen horizonte=new Pen(Color.FromArgb(75,121,72,77),2))g.DrawLine(horizonte,0,218,zonaJefe.Width,218);
            using(Brush luz=new SolidBrush(Color.FromArgb(25,223,76,57)))g.FillEllipse(luz,435,38,290,180);
            for(int i=0;i<42;i++){int x=(i*137+29)%zonaJefe.Width;int y=(i*71+11)%205;int brillo=90+(int)(Math.Abs(Math.Sin(ticksJefe*.04+i))*120);using(Brush estrella=new SolidBrush(Color.FromArgb(brillo,218,205,191)))g.FillEllipse(estrella,x,y,2,2);}
            int salto=(int)(Math.Sin(ticksJefe/5.0)*5);
            int sacudida=destelloJefe?(ticksJefe%2==0?7:-7):0;
            using(Brush sombra=new SolidBrush(Color.FromArgb(65,0,0,0)))g.FillEllipse(sombra,70,360,160,24);
            if(imagenEren!=null){int erenSalto=(int)(Math.Sin(ticksJefe/7.0)*3);g.DrawImage(imagenEren,new Rectangle(82,175+erenSalto,145,185));}
            using(Font etiqueta=new Font("Consolas",10,FontStyle.Bold))using(Brush color=new SolidBrush(Color.FromArgb(236,173,156)))g.DrawString("EREN JAEGER",etiqueta,color,72,365);
            DibujarGigante(g,580+sacudida,105+salto,destelloJefe);
            using(Font nombre=new Font("Consolas",10,FontStyle.Bold))using(Brush color=new SolidBrush(Color.FromArgb(244,174,80)))g.DrawString("TITAN DE ATAQUE",nombre,color,492,195);
            Rectangle caja=new Rectangle(300,230,560,170);
            using(Brush relleno=new SolidBrush(Color.FromArgb(235,10,11,17)))g.FillRectangle(relleno,caja);
            using(Pen bordeExterno=new Pen(Color.FromArgb(80,185,172,166),8))g.DrawRectangle(bordeExterno,caja);
            using(Pen borde=new Pen(Color.White,4))g.DrawRectangle(borde,caja);
            foreach(ProyectilJefe p in proyectilesJefe)
            {Color tono=patronJefe==0||patronJefe==4?Color.FromArgb(255,198,92):Color.FromArgb(255,91,104);using(Pen aura=new Pen(Color.FromArgb(75,tono),p.Tamano*2+5))g.DrawEllipse(aura,p.X-p.Tamano-2,p.Y-p.Tamano-2,p.Tamano*2+4,p.Tamano*2+4);using(Brush b=new SolidBrush(Color.FromArgb(tono.R,tono.G,tono.B)))g.FillEllipse(b,p.X-p.Tamano,p.Y-p.Tamano,p.Tamano*2,p.Tamano*2);}
            if(cortesRapidos>0){using(Pen corte=new Pen(Color.FromArgb(240,255,74,90),7)){g.DrawLine(corte,440,74,720,135);g.DrawLine(corte,715,65,450,148);}}
            DibujarCorazon(g,almaX,almaY,turnoEnemigoJefe&&invulnerabilidadJefe>0&&(ticksJefe%4<2)?Color.White:Color.FromArgb(242,45,67));
        }

        private void DibujarGigante(Graphics g,int cx,int cy,bool golpeado)
        {
            if(imagenTitan!=null){int ancho=280+(int)(Math.Sin(ticksJefe/6.0)*8);int alto=270+(int)(Math.Cos(ticksJefe/6.0)*7);if(golpeado){using(Brush resplandor=new SolidBrush(Color.FromArgb(55,255,73,79)))g.FillEllipse(resplandor,cx-ancho/2-14,cy-alto/2-8,ancho+28,alto+16);}g.DrawImage(imagenTitan,new Rectangle(cx-ancho/2,cy-alto/2,ancho,alto));return;}
            Color piel=golpeado?Color.White:Color.FromArgb(177,91,77);using(Brush carne=new SolidBrush(piel))using(Brush sombra=new SolidBrush(Color.FromArgb(55,24,29)))using(Brush pelo=new SolidBrush(Color.FromArgb(22,20,25)))using(Brush dientes=new SolidBrush(Color.FromArgb(238,224,199)))using(Brush ojo=new SolidBrush(Color.FromArgb(248,200,92)))
            {
                g.FillEllipse(sombra,cx-82,cy+25,164,126);g.FillEllipse(carne,cx-69,cy+16,138,132);g.FillEllipse(carne,cx-49,cy-50,98,102);
                g.FillPolygon(pelo,new Point[]{new Point(cx-51,cy-12),new Point(cx-63,cy-52),new Point(cx-24,cy-35),new Point(cx,cy-64),new Point(cx+18,cy-36),new Point(cx+60,cy-53),new Point(cx+45,cy-2),new Point(cx+24,cy-17),new Point(cx-32,cy-18)});
                g.FillRectangle(ojo,cx-30,cy-9,18,8);g.FillRectangle(ojo,cx+12,cy-9,18,8);g.FillRectangle(sombra,cx-28,cy-7,7,6);g.FillRectangle(sombra,cx+14,cy-7,7,6);
                g.FillRectangle(sombra,cx-30,cy+20,60,25);g.FillRectangle(dientes,cx-25,cy+21,50,8);g.FillRectangle(dientes,cx-18,cy+34,10,8);g.FillRectangle(dientes,cx+8,cy+34,10,8);
                g.DrawLine(Pens.Black,cx-38,cy+72,cx-5,cy+61);g.DrawLine(Pens.Black,cx+38,cy+72,cx+5,cy+61);
            }
        }

        private void DibujarCorazon(Graphics g,int x,int y,Color color)
        {
            using(Brush b=new SolidBrush(color)){g.FillEllipse(b,x-11,y-10,13,13);g.FillEllipse(b,x-1,y-10,13,13);g.FillPolygon(b,new Point[]{new Point(x-11,y-3),new Point(x+12,y-3),new Point(x,y+13)});}
        }

        private void ActualizarEstadoJefe()
        {
            if(panelJefe==null)return;ActualizarBarraVisual(barraVidaJefe,cadete.Salud);ActualizarBarraVisual(barraJefe,vidaJefe);textoEstadoJefe.Text="HP "+cadete.Salud+"/100  ·  DAÑO "+danioDisparo;
        }

        private void TerminarCombateJefe(bool perdonado=false)
        {
            if(timerJefe!=null){timerJefe.Stop();timerJefe.Dispose();timerJefe=null;}
            enemigosMapa.Remove(jefeActual);jefeActual=null;eliminacionesOleada++;experienciaOleada++;
            cadete.GanarExperiencia(perdonado?10:25);
            AgregarRegistro(perdonado?"El Gigante se retira. El encuentro queda resuelto.":"Jefe derrotado. +25 EXP. El mapa y la horda siguen activos.");
            panelPrincipal.Controls.Remove(panelJefe);panelJefe.Dispose();panelJefe=null;zonaJefe=null;barraJefe=null;barraVidaJefe=null;textoEstadoJefe=null;textoAvisoJefe=null;proyectilesJefe.Clear();
            ActualizarMapaVisual();ActualizarHUD();
            if(eliminacionesOleada>=5){CompletarOleada();return;}
            if(experienciaOleada>=3){MostrarMejorasMapa();return;}
            timerSupervivencia.Start();ActiveControl=null;Focus();
        }
        private void MoverHorda()
        {
            foreach (EnemigoMapa enemigo in new List<EnemigoMapa>(enemigosMapa))
            {
                if (enemigo.Fila == jugadorFila && enemigo.Columna == jugadorColumna) continue;
                Point paso = BuscarPasoEnemigo(enemigo);
                enemigo.Fila = paso.Y;
                enemigo.Columna = paso.X;
            }
        }

        private Point BuscarPasoEnemigo(EnemigoMapa enemigo)
        {
            Point inicio = new Point(enemigo.Columna, enemigo.Fila);
            Point meta = new Point(jugadorColumna, jugadorFila);
            Queue<Point> cola = new Queue<Point>();
            Dictionary<Point, Point> anterior = new Dictionary<Point, Point>();
            cola.Enqueue(inicio);
            anterior[inicio] = inicio;
            Point[] pasos = { new Point(1,0), new Point(-1,0), new Point(0,1), new Point(0,-1) };

            while (cola.Count > 0)
            {
                Point actual = cola.Dequeue();
                if (actual == meta) break;
                foreach (Point paso in pasos)
                {
                    Point siguiente = new Point(actual.X+paso.X, actual.Y+paso.Y);
                    if (siguiente.X<0 || siguiente.Y<0 || siguiente.X>=mapaActual.GetLength(1) || siguiente.Y>=mapaActual.GetLength(0)) continue;
                    if (mapaActual[siguiente.Y,siguiente.X] == "#" || anterior.ContainsKey(siguiente)) continue;
                    EnemigoMapa ocupado = BuscarEnemigo(siguiente.Y,siguiente.X);
                    if (ocupado != null && ocupado != enemigo) continue;
                    anterior[siguiente] = actual;
                    cola.Enqueue(siguiente);
                }
            }
            if (!anterior.ContainsKey(meta)) return inicio;
            Point ruta = meta;
            while (anterior[ruta] != inicio && ruta != inicio) ruta = anterior[ruta];
            return ruta;
        }

        private void MostrarMejorasMapa()
        {
            experienciaOleada = 0;
            nivelOleada++;
            GuardarProgreso();
            timerSupervivencia.Stop();
            panelMejoras = new Panel { Size=new Size(790,230), Location=new Point(200,185), BackColor=Color.FromArgb(29,27,25), BorderStyle=BorderStyle.FixedSingle };
            panelPrincipal.Controls.Add(panelMejoras);
            panelMejoras.BringToFront();
            Label titulo = new Label { Text="NIVEL "+nivelOleada+"  ·  FABRICA UNA MEJORA CON TUS RECURSOS", Location=new Point(15,12), Size=new Size(750,38), Font=new Font("Segoe UI",14,FontStyle.Bold), ForeColor=Color.FromArgb(238,210,165), BackColor=Color.Transparent, TextAlign=ContentAlignment.MiddleCenter };
            panelMejoras.Controls.Add(titulo);
            CrearMejoraMapa("CUCHILLAS REFORZADAS", "+10 daño por combo", 18, 6, 0, () => danioDisparo += 10);
            CrearMejoraMapa("MANIOBRAS AGILES", "+3 velocidad de esquiva", 273, 6, 0, () => velocidadAlma = Math.Min(30,velocidadAlma+3));
            CrearMejoraMapa("BOTIQUIN", "consume 1 racion · +35 salud", 528, 0, 1, () => { cadete.UsarSuministro(); cadete.Curar(15); });
            Button omitir = new Button { Text="CONTINUAR SIN FABRICAR", Location=new Point(260,194), Size=new Size(270,27), BackColor=Color.FromArgb(42,45,48), ForeColor=Color.FromArgb(220,218,204), Font=new Font("Segoe UI",8,FontStyle.Bold), FlatStyle=FlatStyle.Flat };
            omitir.Click += (s,e) => ReanudarTrasMejora(null);
            panelMejoras.Controls.Add(omitir);
        }

        private void CrearMejoraMapa(string nombre, string detalle, int x, int costeMateriales, int costeSuministros, Action mejora)
        {
            bool disponible=cadete.Materiales>=costeMateriales&&cadete.Suministros>=costeSuministros;
            string coste=costeMateriales>0?"\r\n\r\nCOSTE  "+costeMateriales+" MATERIALES":"\r\n\r\nCOSTE  "+costeSuministros+" RACION";
            Button boton = new Button { Text=nombre+"\r\n"+detalle+coste, Location=new Point(x,58), Size=new Size(244,128), BackColor=disponible?Color.FromArgb(77,43,34):Color.FromArgb(47,47,46), ForeColor=disponible?Color.White:Color.FromArgb(145,145,140), Font=new Font("Segoe UI",9,FontStyle.Bold), FlatStyle=FlatStyle.Flat, Enabled=disponible };
            boton.FlatAppearance.BorderColor=disponible?Color.FromArgb(188,141,91):Color.FromArgb(76,76,72);
            boton.Click += (s,e) =>
            {
                if(costeMateriales>0&&!cadete.GastarMateriales(costeMateriales))return;
                if(costeSuministros>0&&!cadete.UsarSuministro())return;
                mejora();
                ReanudarTrasMejora(null);
            };
            panelMejoras.Controls.Add(boton);
        }

        private void ReanudarTrasMejora(Action mejora)
        {
            if(mejora!=null)mejora();
            if(panelMejoras!=null){panelPrincipal.Controls.Remove(panelMejoras);panelMejoras.Dispose();panelMejoras=null;}
            timerSupervivencia.Start();
            ActiveControl=null;Focus();ActualizarHUD();GuardarProgreso();
        }

        private void CompletarOleada()
        {
            giganteDerrotado = true;
            combateActivo = false;
            if(timerSupervivencia!=null)timerSupervivencia.Stop();
            enemigosMapa.Clear();
            cadete.GanarExperiencia(100);
            cadete.AgregarObjeto("Trofeo del Gigante");
            AgregarRegistro("MISION COMPLETADA: 5 jefes derrotados. Salida desbloqueada.");
            ActualizarMapaVisual();
            ActualizarHUD();
            GuardarProgreso();
            if(jugadorFila==1&&jugadorColumna==1)FinalizarVictoria();
        }

        // ACTUALIZAR MAPA VISUAL CON IMAGENES
        // ==========================================

        private void ActualizarMapaVisual()
        {
            if (mapaActual == null || casillasMapa == null)
                return;

            for (int fila = 0; fila < mapaActual.GetLength(0); fila++)
            {
                for (int columna = 0; columna < mapaActual.GetLength(1); columna++)
                {
                    Button casilla = casillasMapa[fila, columna];
                    string simbolo = mapaActual[fila, columna];

                    casilla.Text = "";
                    casilla.Tag = simbolo;
                    casilla.BackgroundImage = null;
                    casilla.BackgroundImageLayout = ImageLayout.Zoom;
                    casilla.ForeColor = Color.White;

                    if (fila == jugadorFila && columna == jugadorColumna)
                    {
                        casilla.Text = "";
                        casilla.BackgroundImage = imagenEren;
                        casilla.BackgroundImageLayout = ImageLayout.Zoom;
                        casilla.BackColor = Color.FromArgb(230, 175, 55);
                    }
                    else if (BuscarEnemigo(fila, columna) != null)
                    {
                        EnemigoMapa enemigo = BuscarEnemigo(fila, columna);
                        casilla.Text = "";
                        casilla.BackgroundImage = imagenTitan;
                        casilla.BackgroundImageLayout = ImageLayout.Zoom;
                        casilla.BackColor = enemigo.Vida < 50 ? Color.FromArgb(190, 93, 48) : Color.FromArgb(100, 36, 32);
                        casilla.Tag = "G";
                    }
                    else if (simbolo == "G" && !giganteDerrotado)
                    {
                        casilla.BackgroundImage = imagenTitan;
                        casilla.BackgroundImageLayout = ImageLayout.Zoom;
                        casilla.BackColor = Color.DarkRed;
                    }
                    else if (simbolo == "G" && giganteDerrotado)
                    {
                        casilla.Text = "✓";
                        casilla.BackColor = Color.DarkGreen;
                    }
                    else if (zonasProcesadas[fila, columna])
                    {
                        casilla.Text = "·";
                        casilla.BackColor = Color.FromArgb(67, 72, 61);
                        casilla.ForeColor = Color.FromArgb(208, 218, 179);
                    }
                    else if (simbolo == "R")
                    {
                        casilla.Text = "✦";
                        casilla.BackColor = Color.FromArgb(112, 83, 42);
                        casilla.ForeColor = Color.FromArgb(255, 223, 143);
                    }
                    else if (simbolo == "#")
                    {
                        casilla.Text = "";
                        casilla.BackColor = Color.FromArgb(73, 74, 68);
                    }
                    else if (simbolo == "S")
                    {
                        casilla.Text = "";
                        casilla.BackColor = Color.FromArgb(51, 68, 54);
                    }
                    else if (simbolo == "E")
                    {
                        casilla.Text = "⌂";
                        casilla.BackColor = Color.FromArgb(49, 81, 103);
                        casilla.ForeColor = Color.FromArgb(185, 226, 237);
                    }
                    else
                    {
                        casilla.Text = "";
                        casilla.BackColor = Color.FromArgb(76, 75, 60);
                    }
                    casilla.Invalidate();
                }
            }
        }

        // ==========================================
        // HUD Y REGISTRO DE ACONTECIMIENTOS
        // ==========================================

        private void ActualizarHUD()
        {
            if (cadete == null || textoHUD == null)
                return;

            string estadoJefe = giganteDerrotado
                ? "SALIDA ABIERTA"
                : eliminacionesOleada + "/5 JEFES";

            textoHUD.Text =
                "SALUD: " + cadete.Salud + "/100" +
                "       ENERGIA: " + cadete.Energia + "/100\n" +
                "ESCUDOS: " + cadete.Escudos +
                "       MATERIALES: " + cadete.Materiales +
                "       EXP: " + cadete.Experiencia +
                "       HORDA: " + estadoJefe + "\n" +
                "NIVEL " + nivelOleada + "   ·   AMENAZA " + (RangoAmenaza + 1) +
                "   ·   RACIONES " + cadete.Suministros;
            ActualizarBarraVisual(barraVida,cadete.Salud);
            ActualizarBarraVisual(barraExperiencia,experienciaOleada);
        }

        private void AgregarRegistro(string mensaje)
        {
            registroEventos += "• " + mensaje + "\n";

            if (registroEventos.Length > 450)
            {
                registroEventos = registroEventos.Substring(
                    registroEventos.Length - 450
                );
            }

            if (textoRegistro != null)
                textoRegistro.Text = registroEventos;
        }

        // ==========================================
        // USAR SUMINISTROS
        // ==========================================

        private void UsarSuministro()
        {
            if (cadete == null || !cadete.Vivo || partidaTerminada)
                return;

            if (cadete.UsarSuministro())
            {
                AgregarRegistro("SUMINISTRO UTILIZADO.");
                AgregarRegistro("Salud y energia recuperadas.");
            }
            else
            {
                AgregarRegistro("No tienes suministros disponibles.");
            }

            ActualizarHUD();
            GuardarProgreso();
        }

        // ==========================================
        // INVENTARIO
        // ==========================================

        private void MostrarInventario()
        {
            if (cadete == null)
            {
                MessageBox.Show(
                    "Primero debes iniciar una nueva partida.",
                    "NEXUS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            PrepararPanel();

            panelPrincipal.BackColor = Color.FromArgb(25, 22, 19);

            CrearEtiqueta(
                "NEXUS  /  EQUIPAMIENTO",
                80, 45, 700, 30,
                11, Color.FromArgb(190, 150, 110),
                FontStyle.Bold
            );

            CrearEtiqueta(
                "INVENTARIO DEL CADETE",
                80, 85, 850, 55,
                27, Color.FromArgb(235, 215, 185),
                FontStyle.Bold
            );

            Panel panelInventario = new Panel();
            panelInventario.Location = new Point(80, 165);
            panelInventario.Size = new Size(1080, 380);
            panelInventario.BackColor = Color.FromArgb(48, 40, 33);
            panelInventario.BorderStyle = BorderStyle.FixedSingle;
            panelPrincipal.Controls.Add(panelInventario);

            Label contenido = new Label();
            contenido.Text = cadete.ObtenerInventario();
            contenido.Font = new Font("Consolas", 15, FontStyle.Bold);
            contenido.ForeColor = Color.White;
            contenido.Location = new Point(30, 25);
            contenido.Size = new Size(1000, 330);
            panelInventario.Controls.Add(contenido);

            Button volver = new Button();
            volver.Text = "VOLVER A LAS MURALLAS";
            volver.Location = new Point(80, 590);
            volver.Size = new Size(300, 50);
            volver.BackColor = Color.FromArgb(85, 35, 28);
            volver.ForeColor = Color.White;
            volver.Font = new Font("Arial", 11, FontStyle.Bold);
            volver.FlatStyle = FlatStyle.Flat;
            volver.Click += (s, e) => MostrarMapas();
            panelPrincipal.Controls.Add(volver);
        }

        // ==========================================
        // ESTADO DEL CADETE
        // ==========================================

        private void MostrarEstadoCadete()
        {
            if (cadete == null)
            {
                cadete = new Cadete("Eren Jaeger");
            }

            PrepararPanel();
            panelPrincipal.BackColor = Color.FromArgb(10, 15, 22);
            CrearEtiqueta("CUERPO DE EXPLORACION  /  ARCHIVO DE SOLDADO",65,42,790,25,10,Color.FromArgb(210,93,80),FontStyle.Bold);
            CrearEtiqueta("EREN JAEGER",65,77,790,60,32,Color.FromArgb(239,226,202),FontStyle.Bold);
            CrearEtiqueta("PERFIL DE COMBATE  ·  REGISTRO NEXUS",68,138,650,28,11,Color.Silver,FontStyle.Regular);

            if(imagenEren!=null){PictureBox retrato=new PictureBox {Image=imagenEren,SizeMode=PictureBoxSizeMode.Zoom,Location=new Point(1110,58),Size=new Size(145,175),BackColor=Color.Transparent};panelPrincipal.Controls.Add(retrato);}
            CrearTarjetaEstado(65,"SALUD",cadete.Salud+" / 100","RESISTENCIA",Color.FromArgb(218,63,74));
            CrearTarjetaEstado(330,"ENERGIA",cadete.Energia+" / 100","MOVILIDAD",Color.FromArgb(63,174,208));
            CrearTarjetaEstado(595,"ATAQUE",cadete.Ataque.ToString(),"POTENCIA",Color.FromArgb(241,174,70));
            CrearTarjetaEstado(860,"EXPERIENCIA",cadete.Experiencia.ToString(),"PROGRESO",Color.FromArgb(139,114,225));

            Panel equipo=new Panel {Location=new Point(65,345),Size=new Size(1005,195),BackColor=Color.FromArgb(20,25,33),BorderStyle=BorderStyle.FixedSingle};panelPrincipal.Controls.Add(equipo);
            Label tituloEquipo=new Label {Text="EQUIPO Y SUMINISTROS",Location=new Point(24,18),Size=new Size(700,30),Font=new Font("Segoe UI",13,FontStyle.Bold),ForeColor=Color.FromArgb(230,208,172),BackColor=Color.Transparent};equipo.Controls.Add(tituloEquipo);
            Label recursos=new Label {Text="ESCUDOS     "+cadete.Escudos+"\r\nMATERIALES  "+cadete.Materiales+"\r\nSUMINISTROS "+cadete.Suministros+"\r\nOBJETOS     "+cadete.CantidadObjetos(),Location=new Point(28,65),Size=new Size(430,105),Font=new Font("Consolas",12,FontStyle.Bold),ForeColor=Color.Gainsboro,BackColor=Color.Transparent};equipo.Controls.Add(recursos);
            Label arma=new Label {Text="EQUIPO ACTIVO\r\nDispositivo de maniobras\r\nCombo rapido x2\r\n\r\nESTADO  "+(cadete.Vivo?"EN SERVICIO":"FUERA DE COMBATE"),Location=new Point(525,65),Size=new Size(440,110),Font=new Font("Segoe UI",11),ForeColor=Color.Gainsboro,BackColor=Color.Transparent};equipo.Controls.Add(arma);
            Button volver=new Button {Text="VOLVER",Location=new Point(65,580),Size=new Size(230,54),BackColor=Color.FromArgb(39,47,58),ForeColor=Color.White,Font=new Font("Segoe UI",11,FontStyle.Bold),FlatStyle=FlatStyle.Flat};volver.FlatAppearance.BorderColor=Color.FromArgb(191,151,96);volver.Click+=(s,e)=>MostrarMenuPrincipal();panelPrincipal.Controls.Add(volver);
            CrearEtiqueta("E  ·  EREN JAEGER  /  UNIDAD DE RECONOCIMIENTO",340,594,730,28,10,Color.FromArgb(166,173,181),FontStyle.Bold);
            return;
        }

        private void CrearTarjetaEstado(int x,string nombre,string valor,string detalle,Color acento)
        {
            Panel tarjeta=new Panel {Location=new Point(x,215),Size=new Size(240,105),BackColor=Color.FromArgb(22,27,35),BorderStyle=BorderStyle.FixedSingle};panelPrincipal.Controls.Add(tarjeta);
            Panel franja=new Panel {Location=Point.Empty,Size=new Size(5,103),BackColor=acento};tarjeta.Controls.Add(franja);
            Label encabezado=new Label {Text=nombre,Location=new Point(20,12),Size=new Size(205,22),Font=new Font("Segoe UI",9,FontStyle.Bold),ForeColor=Color.FromArgb(170,178,187),BackColor=Color.Transparent};tarjeta.Controls.Add(encabezado);
            Label numero=new Label {Text=valor,Location=new Point(20,34),Size=new Size(205,40),Font=new Font("Segoe UI",22,FontStyle.Bold),ForeColor=Color.White,BackColor=Color.Transparent};tarjeta.Controls.Add(numero);
            Label pie=new Label {Text=detalle,Location=new Point(20,76),Size=new Size(205,18),Font=new Font("Segoe UI",8,FontStyle.Bold),ForeColor=acento,BackColor=Color.Transparent};tarjeta.Controls.Add(pie);
        }

        // ==========================================
        // FINAL DEL JUEGO: VICTORIA
        // ==========================================

        private void MostrarPantallaFinal(bool victoria)
        {
            partidaTerminada=true;combateActivo=false;
            GuardarProgreso();
            if(timerSupervivencia!=null){timerSupervivencia.Stop();timerSupervivencia.Dispose();timerSupervivencia=null;}
            if(timerJefe!=null){timerJefe.Stop();timerJefe.Dispose();timerJefe=null;}
            panelJefe=null;zonaJefe=null;panelMejoras=null;jefeActual=null;turnoEnemigoJefe=false;proyectilesJefe.Clear();
            Controls.Clear();mapaActual=null;casillasMapa=null;zonasProcesadas=null;
            panelPrincipal=new Panel { Dock=DockStyle.Fill,BackColor=Color.FromArgb(12,14,18) };Controls.Add(panelPrincipal);
            Label marca=new Label { Text="NEXUS  /  EXPEDICION "+murallaActual,Location=new Point(70,55),Size=new Size(900,30),Font=new Font("Segoe UI",11,FontStyle.Bold),ForeColor=Color.FromArgb(195,150,110),BackColor=Color.Transparent };panelPrincipal.Controls.Add(marca);
            Label titulo=new Label { Text=victoria?"OLEADA SUPERADA":"EXPEDICION FALLIDA",Location=new Point(70,150),Size=new Size(1050,75),Font=new Font("Segoe UI",34,FontStyle.Bold),ForeColor=victoria?Color.FromArgb(230,205,150):Color.FromArgb(210,95,80),BackColor=Color.Transparent };panelPrincipal.Controls.Add(titulo);
            Label detalle=new Label { Text=victoria?"Has sobrevivido a la horda. La ruta de regreso a la muralla esta despejada.":"Los Gigantes alcanzaron al cadete. Reorganiza tu equipo y vuelve a intentarlo.",Location=new Point(75,245),Size=new Size(1000,80),Font=new Font("Segoe UI",15),ForeColor=Color.Gainsboro,BackColor=Color.Transparent };panelPrincipal.Controls.Add(detalle);
            Label datos=new Label { Text="ELIMINACIONES  "+eliminacionesOleada+"     ·     EXPERIENCIA  "+(cadete==null?0:cadete.Experiencia),Location=new Point(75,355),Size=new Size(900,40),Font=new Font("Segoe UI",13,FontStyle.Bold),ForeColor=Color.FromArgb(225,190,145),BackColor=Color.Transparent };panelPrincipal.Controls.Add(datos);
            Button reintentar=new Button { Text="NUEVA EXPEDICION",Location=new Point(75,455),Size=new Size(280,58),BackColor=Color.FromArgb(100,43,34),ForeColor=Color.White,Font=new Font("Segoe UI",12,FontStyle.Bold),FlatStyle=FlatStyle.Flat };reintentar.Click+=(s,e)=>NuevaPartida();panelPrincipal.Controls.Add(reintentar);
            Button menu=new Button { Text="MENU PRINCIPAL",Location=new Point(375,455),Size=new Size(260,58),BackColor=Color.FromArgb(50,48,45),ForeColor=Color.White,Font=new Font("Segoe UI",12,FontStyle.Bold),FlatStyle=FlatStyle.Flat };menu.Click+=(s,e)=>MostrarMenuPrincipal();panelPrincipal.Controls.Add(menu);
        }

        private void FinalizarVictoria(){MostrarPantallaFinal(true);}

        private void FinalizarDerrota(){MostrarPantallaFinal(false);}
        private void SalirJuego()
        {
            DialogResult respuesta = MessageBox.Show(
                "¿Deseas abandonar NEXUS?",
                "Salir del juego",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Yes)
            {
                GuardarProgreso();
                Application.Exit();
            }
        }
    }
}

