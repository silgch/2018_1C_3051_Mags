using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Camara;
using TGC.Core.Textures;
using TGC.Examples.Camara;



namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer m�s ejemplos chicos, en el caso de copiar para que se
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
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        //Caja que se muestra en el ejemplo.
        private TGCBox Box { get; set; }

        //Mesh del Personaje.
        private TgcMesh MeshPersonaje { get; set; }

        // Declaro el Mesh de la scena
        private TgcMesh MeshScena { get; set; }


        //scena del juego
        private TgcScene Scene { get; set; }


        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }


        // el target seria mi personaje para que lo siga la camara
        

        //Constantes para velocidades de movimiento
        private const float ROTATION_SPEED = 3f;
        private const float MOVEMENT_SPEED = 10f;

        private TgcThirdPersonCamera camaraInterna;


        

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, estructuras de optimizaci�n, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Textura de la carperta Media. Game.Default es un archivo de configuracion (Game.settings) util para poner cosas.
            //Pueden abrir el Game.settings que se ubica dentro de nuestro proyecto para configurar.
            var pathTexturaCaja = MediaDir + Game.Default.TexturaCaja;

            //Cargamos una textura, tener en cuenta que cargar una textura significa crear una copia en memoria.
            //Es importante cargar texturas en Init, si se hace en el render loop podemos tener grandes problemas si instanciamos muchas.
            var texture = TgcTexture.createTexture(pathTexturaCaja);

            //Creamos una caja 3D ubicada de dimensiones (5, 10, 5) y la textura como color.
            var size = new TGCVector3(5, 10, 5);
            //Construimos una caja seg�n los par�metros, por defecto la misma se crea con centro en el origen y se recomienda as� para facilitar las transformaciones.
            Box = TGCBox.fromSize(size, texture);
            //Posici�n donde quiero que este la caja, es com�n que se utilicen estructuras internas para las transformaciones.
            //Entonces actualizamos la posici�n l�gica, luego podemos utilizar esto en render para posicionar donde corresponda con transformaciones.
            Box.Position = new TGCVector3(-25, 0, 0);

            //Cargo a nuestro personaje  que esta mirando hacia atras
            MeshPersonaje = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Robot-TgcScene.xml").Meshes[0];


            //Quiero rotar al robot mirando hacia afuera de la pantalla
            MeshPersonaje.RotateY(FastMath.PI - (System.Convert.ToSingle(0.20)));

            //Si quiero que mira para adelante
            // MeshPersonaje.RotateY(FastMath.PI_HALF);

            //Si quiero que mira para atras
            //MeshPersonaje.RotateY((FastMath.QUARTER_PI) * 6);



            //Creo un loader para cargar la scena
            var loader = new TgcSceneLoader();
            //Cargo la scena del juego
            Scene =
               loader.loadSceneFromFile(MediaDir +
                                        "Iglesia-TgcScene.xml");
            MeshScena = Scene.Meshes[0];
            


            //Defino una escala en el modelo logico del mesh que es muy grande.
            MeshScena.Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
            MeshScena.Position = new TGCVector3(0f, 0f, 0f);
            MeshScena.AutoTransform = true;

            MeshPersonaje.Scale = new TGCVector3(0.20f, 0.20f, 0.20f);
            //defino la posicion del jugador para que este posicionado en medio de la escena
            MeshPersonaje.Position = new TGCVector3(0.0f, 50.0f, 50.0f);

        

            //Suelen utilizarse objetos que manejan el comportamiento de la camara.
            //Lo que en realidad necesitamos gr�ficamente es una matriz de View.
            //El framework maneja una c�mara est�tica, pero debe ser inicializada.
            //Posici�n de la camara.
           // var cameraPosition = new TGCVector3(0, 120, 100);
            //Quiero que la camara mire hacia el origen (0,0,0).
            //lo que va a hacer es alejarse cada vez mas del origen
            //el lookAt es  la direcci�n hacia donde apunta la c�mara en un momento determinado,
            // partiendo desde la posici�n especificada del personaje
            //var lookAt = TGCVector3.Empty;
        
            //Configuro donde esta la posicion de la camara y hacia donde mira.
            camaraInterna = new TgcThirdPersonCamera(MeshPersonaje.Position, 70, 65);
            Camara = camaraInterna;

            //Internamente el framework construye la matriz de view con estos dos vectores.
            //Luego en nuestro juego tendremos que crear una c�mara que cambie la matriz de view con variables como movimientos o animaciones de escenas.
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la l�gica de computo del modelo, as� como tambi�n verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();
            // defino variables de ingreso de datos
            var input = Input;
            var movement = TGCVector3.Empty;

            //Guardar posicion original antes de cambiarla
            var originalPos = MeshPersonaje.Position;



            //Movernos adelante y atras, sobre el eje Z.
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                movement.Z = 1;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                movement.Z = -1;
            }

           
            //Movernos de izquierda a derecha, sobre el eje X.
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                movement.X = 1;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                movement.X = -1;
            }

            //Movernos adelante y atras, sobre el eje Z.
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                movement.Z = -1;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                movement.Z = 1;
            }

            //Multiplicar movimiento por velocidad y elapsedTime
            movement *= MOVEMENT_SPEED * ElapsedTime;
            MeshPersonaje.Move(movement);



            //Capturar Input Mouse
            if (Input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Como ejemplo podemos hacer un movimiento simple de la c�mara.
                //En este caso le sumamos un valor en Y
                Camara.SetCamera(Camara.Position + new TGCVector3(0, 10f, 0), Camara.LookAt);
                //Ver ejemplos de c�mara para otras operaciones posibles.

                //Si superamos cierto Y volvemos a la posici�n original.
                if (Camara.Position.Y > 300f)
                {
                    Camara.SetCamera(new TGCVector3(Camara.Position.X, 0f, Camara.Position.Z), Camara.LookAt);
                }
            }
            camaraInterna.Target = MeshPersonaje.Position;

            PostUpdate();
         

        
        }
       
        
        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqu� todo el c�digo referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones seg�n nuestra conveniencia.
            PreRender();

            //Dibuja un texto por pantalla
            DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con clic izquierdo subimos la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);

            //Siempre antes de renderizar el modelo necesitamos actualizar la matriz de transformacion.
            //Debemos recordar el orden en cual debemos multiplicar las matrices, en caso de tener modelos jer�rquicos, tenemos control total.
            Box.Transform = TGCMatrix.Scaling(Box.Scale) * TGCMatrix.RotationYawPitchRoll(Box.Rotation.Y, Box.Rotation.X, Box.Rotation.Z) * TGCMatrix.Translation(Box.Position);
            //A modo ejemplo realizamos toda las multiplicaciones, pero aqu� solo nos hacia falta la traslaci�n.
            //Finalmente invocamos al render de la caja
            Box.Render();
            Scene.RenderAll();

            //Cuando tenemos modelos mesh podemos utilizar un m�todo que hace la matriz de transformaci�n est�ndar.
            //Es �til cuando tenemos transformaciones simples, pero OJO cuando tenemos transformaciones jer�rquicas o complicadas.
            MeshPersonaje.UpdateMeshTransform();
            //Render del mesh
            MeshPersonaje.Render();
            MeshScena.Render();

            //Render de BoundingBox, muy �til para debug de colisiones.
            if (BoundingBox)
            {
                Box.BoundingBox.Render();
                MeshPersonaje.BoundingBox.Render();
            }

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecuci�n del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gr�ficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            //Dispose de la caja.
            Box.Dispose();
            //Dispose del mesh.
            MeshPersonaje.Dispose();
            MeshScena.Dispose();
            Scene.DisposeAll();
        }
    }
}