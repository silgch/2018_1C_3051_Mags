using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using TGC.Core.SkeletalAnimation;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using System.Drawing;
using TGC.Core.Example;

namespace TGC.Group.Model
{
   public class Personaje 
    {
        //es para tener una referencia GameModel y poder usar las propiedades que hereda de TGCExample como el MediaDir,Input,ElapsedTime
        private GameModel GModel;
       
        //private TgcMesh personaje;
        private TgcSkeletalMesh personaje;
        //private const float MOVEMENT_SPEED = 200f;
        private float gravedad = -65f;
        float anguloDeRotacion = 0;

        TGCMatrix matrizPosicionamientoPersonaje;
        TGCMatrix matrizRotacionPersonajeY;
        TGCMatrix matrizEscalaPersonaje;
        TGCMatrix matrizDesplazamientoSobreXZ;

        TGCVector3 vistaUp = TGCVector3.Up; //Vector normal, creo que para saltos lo vamos a necesitar
        TGCVector3 orientacion = new TGCVector3(0f, 0f, 1f); //Hacia donde mira el personaje, debe ser un vector normalizado?
        TGCVector3 posicion = new TGCVector3(250, 20, 20); //Posición al iniciar el juego
        TGCVector3 checkpoint; //Ultima posicion para reset
        TGCVector3 vectorColision = TGCVector3.Empty;
        TGCVector3 vectorDesplazamiento = TGCVector3.Empty;

        Boolean saltando;

        public TGCVector3 getPosicion()
        {
            return this.posicion;
        }

        public TGCVector3 getOrientacion()
        {
            return orientacion;
        }

        public TGCVector3 getVectorColision()
        {
            return vectorColision;
        }
        public TGCVector3 getVectorDesplazamiento()
        {
            return vectorDesplazamiento;
        }
        public Boolean estaMuerto()
        {
            return posicion.Y < -200;
        }

        public void Init(GameModel gameModel) {
            GModel = gameModel;

            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    GModel.MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    GModel.MediaDir + "SkeletalAnimations\\Robot\\",
                    new[]
                    {
                        GModel.MediaDir + "SkeletalAnimations\\Robot\\Caminando-TgcSkeletalAnim.xml",
                        GModel.MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml"
                    });
            //Esto hay que desactivarlo
            personaje.AutoTransform = false;

            personaje.Position = new TGCVector3(250, 20, 20);
            matrizPosicionamientoPersonaje = TGCMatrix.Translation(personaje.Position.X, personaje.Position.Y, personaje.Position.Z);
            checkpoint = personaje.Position;

            matrizEscalaPersonaje = TGCMatrix.Scaling(0.25f, 0.25f, 0.25f);
            //La matriz comienza asi porque el personaje comienza dado vuelta
            matrizRotacionPersonajeY = TGCMatrix.RotationY(FastMath.PI);

            saltando = false;

        }
        public void Update() {

            var velocidadCaminar = 300f;
            var velocidadSalto = 100f;
            var velocidadRotacion = 120f;
            vectorDesplazamiento = TGCVector3.Empty;
            vectorColision = TGCVector3.Empty;

            //Calcular proxima posicion de personaje segun Input
            var Input = GModel.Input;
            var moveForward = 0f;
            var moveJump = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;

            var lastPos = personaje.Position;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = velocidadCaminar * GModel.ElapsedTime;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = -velocidadCaminar * GModel.ElapsedTime;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = FastMath.ToRad(rotate * GModel.ElapsedTime);
                matrizRotacionPersonajeY *= TGCMatrix.RotationY(rotAngle);

                GModel.camaraInterna.rotateY(rotAngle);
                anguloDeRotacion += rotAngle;
                //Ajustar la matriz de rotacion segun el angulo de rotacion (sobre el sentido de la orientacion)
                orientacion.X = FastMath.Sin(anguloDeRotacion);
                orientacion.Z = FastMath.Cos(anguloDeRotacion);
            }

            if (moving) {
                //Activar animacion de caminando
                personaje.playAnimation("Caminando", true);
                //Ajustar la matriz de traslacion
                matrizDesplazamientoSobreXZ *= TGCMatrix.Translation(moveForward * orientacion.X, 0, moveForward * orientacion.Z);
                vectorDesplazamiento.X += moveForward * orientacion.X;
                vectorDesplazamiento.Z += moveForward * orientacion.Z;

            } //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            //----------Salto
            if (Input.keyDown(Key.Space) /*&& colliderY != null*/)
            {
                moveJump = velocidadSalto * GModel.ElapsedTime;
                vectorDesplazamiento.Y += moveJump;
            }

            //---------prueba gravedad----------
            vectorDesplazamiento.Y += FastMath.Clamp(gravedad * GModel.ElapsedTime, -10, 10);
            
            //--------Colision con el piso a nivel triangulo?
            TgcBoundingAxisAlignBox colliderPlano = null;
            foreach (var obstaculo in GModel.escenario1.getPiso()/*GModel.escenario1.getPared1()*/)
            {
                if (TgcCollisionUtils.testAABBAABB(personaje.BoundingBox, obstaculo.BoundingBox))
                {

                    colliderPlano = obstaculo.BoundingBox;
                    break;
                }
            }
            if (colliderPlano != null)
            {
                //Si colisiono con el piso no debe seguirse cayendo
                if (vectorDesplazamiento.Y < 0)
                {
                    vectorDesplazamiento.Y = 0;
                }
            }

            //---------Colisiones objetos--------------------------
            var collide = false;
            //TGCBox collider = null;
            TgcBoundingAxisAlignBox collider = null;
            foreach (var obstaculo in GModel.escenario1.getAABBDelEscenario()/*GModel.escenario1.getPared1()*/)
            {
                if (TgcCollisionUtils.testAABBAABB(personaje.BoundingBox, obstaculo))
                {
                    collide = true;
                    collider = obstaculo;
                    break;
                }
            }

            this.posicion = posicion + vectorDesplazamiento;

            personaje.Position = posicion;

            if (collide)
            {
                //Habria que lograr que si colisiona sobre el plano XZ se pueda caminar, como está ahora no puede caminar sobre la tapa de la caja por ejemplo
                var movementRay = lastPos - personaje.Position;
                //Cuando choca con algo que se ponga rojo, nos sirve para probar
                collider.setRenderColor(Color.Red);
                var rs = TGCVector3.Empty;
                if (((personaje.BoundingBox.PMax.X > collider.PMax.X && movementRay.X > 0) ||
                            (personaje.BoundingBox.PMin.X < collider.PMin.X && movementRay.X < 0)) &&
                            ((personaje.BoundingBox.PMax.Z > collider.PMax.Z && movementRay.Z > 0) ||
                            (personaje.BoundingBox.PMin.Z < collider.PMin.Z && movementRay.Z < 0)) &&
                            ((personaje.BoundingBox.PMax.Y > collider.PMax.Y && movementRay.Y > 0) ||
                            (personaje.BoundingBox.PMin.Y < collider.PMin.Y && movementRay.Y < 0)))
                {

                    if (personaje.Position.X > collider.PMin.X && personaje.Position.X < collider.PMax.X)
                    {
                        //El personaje esta contenido en el bounding X

                        rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                    }
                    if (personaje.Position.Z > collider.PMin.Z && personaje.Position.Z < collider.PMax.Z)
                    {
                        //El personaje esta contenido en el bounding Z

                        rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                    }
                    if (personaje.Position.Y > collider.PMin.Y && personaje.Position.Y < collider.PMax.Y)
                    {
                        //El personaje esta contenido en el bounding Y

                        rs = new TGCVector3(movementRay.X, 0, movementRay.Z);
                    }

                }
                else
                {
                    if ((personaje.BoundingBox.PMax.X > collider.PMax.X && movementRay.X > 0) ||
                        (personaje.BoundingBox.PMin.X < collider.PMin.X && movementRay.X < 0))
                    {
                        //Cual seria esta situacion? 

                        rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                    }
                    if ((personaje.BoundingBox.PMax.Z > collider.PMax.Z && movementRay.Z > 0) ||
                        (personaje.BoundingBox.PMin.Z < collider.PMin.Z && movementRay.Z < 0))
                    {

                        rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                    }
                    if ((personaje.BoundingBox.PMax.Y > collider.PMax.Y && movementRay.Y > 0) ||
                        (personaje.BoundingBox.PMin.Y < collider.PMin.Y && movementRay.Y < 0))
                    {

                        rs = new TGCVector3(movementRay.X, 0, movementRay.Z);
                    }
                }
                //El vector rs actua como "freno" al movimiento del personaje
                personaje.Position = lastPos - rs;
                posicion = personaje.Position;
                vectorColision = rs;

            }

            //Una forma de reiniciar, que se active con R o cuando el personaje muere
            if (Input.keyDown(Key.R) || this.estaMuerto())
            {
                posicion = this.checkpoint;
            }

            matrizPosicionamientoPersonaje = TGCMatrix.Translation(posicion);

            GModel.camaraInterna.Target = GModel.tgcPersonaje.getPosicion();

        }
        public void Render() {
            //Render personaje
            var transformacionesDelPersonaje = matrizRotacionPersonajeY * matrizEscalaPersonaje * matrizPosicionamientoPersonaje;
            personaje.Transform = transformacionesDelPersonaje;
            personaje.BoundingBox.transform(transformacionesDelPersonaje);

            personaje.Render();

            personaje.animateAndRender(GModel.ElapsedTime);
        }
        public void Dispose() {
            personaje.Dispose();
        }

        public void DrawBoundingBox()
        {
            personaje.BoundingBox.Render();
        }

    }
    
}
