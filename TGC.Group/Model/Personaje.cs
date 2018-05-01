﻿using System;
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


        TGCVector3 vistaUp = TGCVector3.Up; //Vector normal, creo que para saltos lo vamos a necesitar
        TGCVector3 orientacion = new TGCVector3(1f, 0f, 0f); //Hacia donde mira el personaje, debe ser un vector normalizado?
        TGCVector3 posicion = new TGCVector3(250, 5, 0); //Posición al iniciar el juego
        TGCVector3 checkpoint; //Ultima posicion para reset

        Boolean saltando;

        public TGCVector3 getPosicion()
        {
            return this.posicion;
        }

        public void Init(GameModel gameModel) {
            GModel = gameModel;

            /*var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(GModel.MediaDir + "ModelosTgc\\Robot\\Robot-TgcScene.xml");
            personaje = scene.Meshes[0];
            */
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
            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);

            //Esto hay que desactivarlo
            personaje.AutoTransform = true;

            personaje.Position = new TGCVector3(250, 5, 0);
            checkpoint = personaje.Position;

            personaje.Scale = new TGCVector3(0.25f, 0.25f, 0.25f);
            //Rotar porque empieza dado vuelta
            personaje.RotateY(FastMath.PI);

            saltando = false;

        }
        public void Update() {

            var velocidadCaminar = 300f;
            var velocidadRotacion = 120f;


            //Calcular proxima posicion de personaje segun Input
            var Input = GModel.Input;
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
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
                personaje.RotateY(rotAngle);
                GModel.camaraInterna.rotateY(rotAngle);
            }

            if (moving) {
                //Activar animacion de caminando
                personaje.playAnimation("Caminando", true);

            } //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            if (Input.keyPressed(Key.Space))
            {
                saltando = true;
            }

            if (saltando)
            {
                //No hay animacion para saltar, no se cual conviene usar
                //personaje.playAnimation("Parado", true);
            }

            var lastPos = personaje.Position;

            //---------prueba gravedad----------
            personaje.Position += new TGCVector3(0, FastMath.Clamp(gravedad * GModel.ElapsedTime, -10, 10), 0);
            //--------Colicion con el piso
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
                personaje.Position = lastPos;
            }

            //----------Salto
            if (Input.keyDown(Key.Space) /*&& colliderY != null*/)
            {
                personaje.Position += new TGCVector3(0, 1, 0);
            }
            
            personaje.MoveOrientedY(moveForward * GModel.ElapsedTime);
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

            if (collide)
            {
                var movementRay = lastPos - personaje.Position;
                //collider.BoundingBox.setRenderColor(Color.Red);
                var rs = TGCVector3.Empty;
                if (((personaje.BoundingBox.PMax.X > collider.PMax.X && movementRay.X > 0) ||
                            (personaje.BoundingBox.PMin.X < collider.PMin.X && movementRay.X < 0)) &&
                            ((personaje.BoundingBox.PMax.Z > collider.PMax.Z && movementRay.Z > 0) ||
                            (personaje.BoundingBox.PMin.Z < collider.PMin.Z && movementRay.Z < 0))                            )
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
                    
                }
                else
                {
                    if ((personaje.BoundingBox.PMax.X > collider.PMax.X && movementRay.X > 0) ||
                        (personaje.BoundingBox.PMin.X < collider.PMin.X && movementRay.X < 0))
                    {

                        rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                    }
                    if ((personaje.BoundingBox.PMax.Z > collider.PMax.Z && movementRay.Z > 0) ||
                        (personaje.BoundingBox.PMin.Z < collider.PMin.Z && movementRay.Z < 0))
                    {

                        rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                    }
                }
                personaje.Position = lastPos - rs;
            }


            //--------------------------------------------
             GModel.camaraInterna.Target = GModel.tgcPersonaje.getPosicion();

            //Una forma de reiniciar, que se active con R o cuando el personaje muere
            if (Input.keyDown(Key.R) /* || Personaje.estaMuerto() */)
            {
                lastPos = this.checkpoint;
                personaje.Position = lastPos;
            }

            this.posicion = lastPos;
            

            ////movimiento sin rotacoin
            //var input = GModel.Input;
            //var movement = TGCVector3.Empty;
            ////Movernos de izquierda a derecha, sobre el eje X.
            //if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            //{
            //    movement.X = 1;
            //}
            //else if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            //{
            //    movement.X = -1;
            //    //  var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
            //    // personaje.RotateY(rotAngle);
            //    //var rotAngle = FastMath.ToRad(10*ElapsedTime);
            //    //personaje.RotateY(rotAngle);
            //}

            ////Movernos adelante y atras, sobre el eje Z.
            //if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            //{
            //    movement.Z = -1;
            //}
            //else if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            //{
            //    movement.Z = 1;
            //}

            ////Guardar posicion original antes de cambiarla
            //var originalPos = personaje.Position;

            ////Multiplicar movimiento por velocidad y elapsedTime
            //movement *= MOVEMENT_SPEED * GModel.ElapsedTime;
            //personaje.Move(movement);

        }
        public void Render() {
            personaje.Render();
            //Render personaje
            personaje.animateAndRender(GModel.ElapsedTime);
        }
        public void Dispose() {
            personaje.Dispose();
        }
        
    }
}
