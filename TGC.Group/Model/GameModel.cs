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
using TGC.Core.SkeletalAnimation;
using System;
using Microsoft.DirectX.Direct3D;




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
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        //Mesh del Personaje.
        private TgcSkeletalMesh MeshPersonaje { get; set; }

        // Declaro el Mesh de la scena
        private TgcMesh MeshScena { get; set; }

        //scena del juego
        private TgcScene Scene { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        //Constantes para velocidades de movimiento
        private const float ROTATION_SPEED = 3f;
        private const float MOVEMENT_SPEED = 8f;

        public Personaje Tgcito;

        public TGCVector3 AjusteDeCamara { get; private set; }
        private TgcCamera CamaraPersonaje { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>

        public class Personaje
        {
            TgcSkeletalMesh mesh;//Acá debe cargarse el mesh del personaje (Tgcito por el momento)
            TGCVector3 vistaUp = TGCVector3.Up;
            TGCVector3 orientacion = new TGCVector3(1f, 0f, 0f); //Hacia donde mira el personaje, debe ser un vector normalizado?
            TGCVector3 posicion; //Posición al iniciar el juego
          

            public Personaje(TgcSkeletalMesh mesh)
            {
                this.mesh = mesh;
                posicion = mesh.Position;
            }

            public TGCVector3 GetPosicion()
            {
                return posicion;
            }

            public TGCVector3 GetOrientacion()
            {
                return orientacion;
            }

            internal void Mover(TGCVector3 movement)
            {
                //Muevo en cada coordenada
                this.mesh.Move(movement);
                //Actualizo la nueva posición de Tgcito
                this.posicion = mesh.Position;
            }

        }


        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Cargo a nuestro personaje  que esta mirando hacia atras
            var skeletalLoader = new TgcSkeletalLoader();
            MeshPersonaje = skeletalLoader.loadMeshAndAnimationsFromFile(
                MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                MediaDir + "SkeletalAnimations\\Robot\\",
                new[]
                {
                    MediaDir + "SkeletalAnimations\\Robot\\Caminando-TgcSkeletalAnim.xml",
                    MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml"
                });

            // Configurar animacion inicial
            MeshPersonaje.playAnimation("Parado", true);

            //Escalarlo porque es muy grande
            MeshPersonaje.Position = new TGCVector3(100, 0, 100);
            MeshPersonaje.Scale = new TGCVector3(0.75f, 0.75f, 0.75f);

            //Rotarlo 180° porque esta mirando para el otro lado
            MeshPersonaje.RotateY(Geometry.DegreeToRadian(180f));
            
            //Creo un loader para cargar la scena
            var loader = new TgcSceneLoader();

            //Cargo la escena del juego
            Scene =
               loader.loadSceneFromFile(MediaDir +
                                        "Iglesia\\Iglesia-TgcScene.xml");
            MeshScena = Scene.Meshes[0];
            
            //Defino una escala en el modelo logico del mesh que es muy grande.
            MeshScena.Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
            MeshScena.Position = new TGCVector3(0f, 0f, 0f);
            MeshScena.AutoTransform = true;

            MeshPersonaje.Scale = new TGCVector3(0.20f, 0.20f, 0.20f);
            //defino la posición del jugador para que este posicionado en medio de la escena
            MeshPersonaje.Position = new TGCVector3(0.0f, 50.0f, 50.0f);

            //Instancio el personaje, estructura para facilitar el manejo del personaje

            Tgcito = new Personaje(MeshPersonaje);

            //Configuro donde esta la posicion de la camara y hacia donde mira.
            AjusteDeCamara = new TGCVector3(0f, 50f, -40f); // Se posiciona sobre el personaje y un poco hacia atrás

            CamaraPersonaje = new TgcCamera();
            CamaraPersonaje.SetCamera(Tgcito.GetPosicion() + AjusteDeCamara, Tgcito.GetPosicion());

            //Seteo la cámara del juego con la que creé para mi personaje
            Camara = CamaraPersonaje;
            //Internamente el framework construye la matriz de view con estos dos vectores.
            //Luego en nuestro juego tendremos que crear una cámara que cambie la matriz de view con variables como movimientos o animaciones de escenas.
        }


        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            // Defino variables de ingreso de datos
            var input = Input;

            // El movimiento comienza como nulo
            var movement = TGCVector3.Empty;

            // Tengo la posición actual de la cámara
            var posicionDeCamara = Camara.Position;

            // Variables para el movimiento y rotación
            var velocidadCaminar = 100f;
            //var velocidadRotacion = 100f; // No lo necesitamos por el momento

            // Tengo la posicion original de Tgcito 
            var originalPos = Tgcito.GetPosicion();

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            var moveSide = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;

            //Movernos hacia adelante

            if (Input.keyDown(Key.UpArrow) || Input.keyDown(Key.W))
            {
               moveForward = velocidadCaminar;
                moving = true;
            }
            else if (Input.keyDown(Key.DownArrow) || Input.keyDown(Key.S))
            {
                moveForward = -velocidadCaminar;
                moving = true;
                rotate = FastMath.PI;
                rotating = true;
            }

            if (Input.keyDown(Key.RightArrow) || Input.keyDown(Key.D))
            {
                moveSide = velocidadCaminar;
                moving = true;
                rotate = FastMath.PI_HALF;
                rotating = true;
            }

            else if (Input.keyDown(Key.LeftArrow) || Input.keyDown(Key.A))
            {
                moveSide = -velocidadCaminar;
                moving = true;
                rotate = FastMath.PI_HALF * 3;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
             //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
             // var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime); //Empecemos por rotaciones simples
                // MeshPersonaje.RotateY(rotate); //El problema es que rota todo el tiempo 
             }
           
            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                MeshPersonaje.playAnimation("Caminando", true);

                //Calculo el desplazamiento total como un vector de 3 componentes
                //Es el total de movimiento en cada uno de los ejes
                var movimientoTotal = new TGCVector3(moveSide * ElapsedTime, 0, moveForward * ElapsedTime);

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                Tgcito.Mover(movimientoTotal);   
                
                //Vuelvo a setear la cámara con los valores actualizados
                Camara.SetCamera(Camara.Position + movimientoTotal, Tgcito.GetPosicion());
               
            }
       
            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                MeshPersonaje.playAnimation("Parado", true);
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

            //Siempre antes de renderizar el modelo necesitamos actualizar la matriz de transformacion.
            //Debemos recordar el orden en cual debemos multiplicar las matrices, en caso de tener modelos jerárquicos, tenemos control total.
            //A modo ejemplo realizamos toda las multiplicaciones, pero aquí solo nos hacia falta la traslación.
            //Finalmente invocamos al render de la caja
            Scene.RenderAll();

            //Cuando tenemos modelos mesh podemos utilizar un método que hace la matriz de transformación estándar.
            //Es útil cuando tenemos transformaciones simples, pero OJO cuando tenemos transformaciones jerárquicas o complicadas.
            MeshPersonaje.UpdateMeshTransform();
            //Render del mesh
            MeshPersonaje.Render();
            MeshScena.Render();

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                //MeshPersonaje.BoundingBox.Render(); //Creo que nos va a ser útil
            }

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            //Dispose del mesh.
            MeshPersonaje.Dispose();
            Scene.DisposeAll();
            //No se si falta algo más
        }
    }
}
