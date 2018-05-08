using System;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using System.Collections.Generic;
using TGC.Core.Terrain;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        /// 
        
        public Personaje tgcPersonaje=new Personaje();
        public CamaraEnTerceraPersona camaraInterna;
        public Escenario1 escenario1 = new Escenario1();
        public bool boundingBox;
        private TgcSkyBox skyBox;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

                    
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(20000, 5000, 20000);
            //Configurar las texturas para cada una de las 6 caras
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "\\SkyBox\\SkyBox1\\lun4_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "\\SkyBox\\SkyBox1\\lun4_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "\\SkyBox\\SkyBox1\\lun4_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "\\SkyBox\\SkyBox1\\lun4_rt.jpg");

            //Hay veces es necesario invertir las texturas Front y Back si se pasa de un sistema RightHanded a uno LeftHanded
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "\\SkyBox\\SkyBox1\\lun4_ft.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "\\SkyBox\\SkyBox1\\lun4_bk.jpg");
            skyBox.SkyEpsilon = 25f;
            //Inicializa todos los valores para crear el SkyBox
            skyBox.Init();

            tgcPersonaje.Init(this);
            escenario1.Init(this);
            
            camaraInterna = new CamaraEnTerceraPersona(tgcPersonaje.getPosicion(),35,-100);
            Camara = camaraInterna;
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            escenario1.Update();
            
            tgcPersonaje.Update();

            if (Input.keyPressed(Key.F))
            {
                boundingBox = !boundingBox;
            }
            
            PostUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Dibuja un texto por pantalla
            //DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Posicion de la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 20, Color.OrangeRed);
            DrawText.drawText("Posicion del personaje [Actual]: " + TGCVector3.PrintVector3(tgcPersonaje.getPosicion()), 0, 30, Color.OrangeRed);
            DrawText.drawText("Presionar F para ver las boundingBox", 0, 40, Color.OrangeRed);
            DrawText.drawText("Vector direccion personaje" + TGCVector3.PrintVector3(tgcPersonaje.getOrientacion()), 0, 50, Color.OrangeRed);
            DrawText.drawText("Vector direccion colision" + TGCVector3.PrintVector3(tgcPersonaje.getVectorColision()), 0, 60, Color.OrangeRed);

            skyBox.Render();
            tgcPersonaje.Render();
            escenario1.Render();
            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene

            //Mostrar los boundingBox correspondientes, no es algo que el personaje deba poder hacer pero es para ver que 
            if (boundingBox)
            {
                tgcPersonaje.DrawBoundingBox();
                //Cuando tengamos varios escenarios tendríamos que hacer
                //escenarioActual.DrawBoundingBox();
                //Con escenarioActual variable del gameModel
                escenario1.DrawBoundingBox();
            }
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            skyBox.Dispose();
            escenario1.Dispose();
            tgcPersonaje.Dispose();
        }
    }
}