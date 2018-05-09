using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.Geometry;
using TGC.Core.Direct3D;
using Microsoft.DirectX.DirectInput;
using TGC.Core.SceneLoader;
using TGC.Core.BoundingVolumes;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    public class Escenario1
    {
        private GameModel GModel;
        private List<TGCBox> pared1;
        private List<TGCBox> pared2;
        private List<TgcPlane> piso;
        private TGCBox caja1, caja2, caja3, caja4, caja5;
        private TgcMesh barril1, calabera, esqueleto, pilar, mueble1, pilarCaido1, pilarCaido2;
        TGCMatrix escalaBaseParaCajas;
        TGCMatrix posicionamientoCaja3, posicionamientoCaja4, pivoteCaja5, radioCaja5;
        TGCMatrix movimientoTraslacionY, movimientoTraslacionX, movimientoRotacionY;
        float traslacionCaja3 = 0f;
        float traslacionCaja4 = 0f;
        float velocidadTraslacionCaja = 10f;
        float velocidadRotacionCaja = 1f;
        float tiempoDeSubida, tiempoDeMovLateral, rotacionTotal;
        bool subir, movLateral;

        TGCVector3 desplazamientoCaja3;
        TGCVector3 desplazamientoCaja4;
        TGCVector3 desplazamientoCaja5;

        private List<TgcMesh> pilares;
        private List<TgcMesh> muebles;
        private TGCBox plaFija1, plaFija2, plaFija3, plaFija4, plaFija5;
        private TGCBox plaMovil1;

        private List<TgcBoundingAxisAlignBox> aabbDelEscenario;
        private List<TgcBoundingAxisAlignBox> plataformasDelEscenario;
        private List<TGCVector3> desplazamientosDePlataformasDelEscenario;
        TGCMatrix posicionamientoPla1, posicionamientoPla2, posicionamientoPla3, posicionamientoPla4, posicionamientoPla5, posPlaMovil1;

        private TgcMp3Player musicaFondo;

        public void Init(GameModel gameModel)
        {
            GModel = gameModel;
            var pisoTextura = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "Texturas\\largerblock3b3dim.jpg");
            var planeSize = new TGCVector3(500, 0, 500);
            var pisoOrientacion = TgcPlane.Orientations.XZplane;

            var texturaTipo1 = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "\\Texturas\\blocks9.jpg");


            var boxSize = new TGCVector3(20, 150, 500);
            var boxSize2 = new TGCVector3(500, 150, 20);

            pared1 = new List<TGCBox>();
            pared2 = new List<TGCBox>();
            piso = new List<TgcPlane>();

            aabbDelEscenario = new List<TgcBoundingAxisAlignBox>();
            plataformasDelEscenario = new List<TgcBoundingAxisAlignBox>();
            desplazamientosDePlataformasDelEscenario = new List<TGCVector3>();

            pilares = new List<TgcMesh>();
            muebles = new List<TgcMesh>();

            //--------------piso
            for (var i = 0; i < 8; i++)
            {
                var plane = new TgcPlane(new TGCVector3(0, 0, planeSize.Z * i), planeSize, pisoOrientacion, pisoTextura);
                piso.Add(plane);

            }
            for (var i = 9; i < 12; i++)
            {
                var plane = new TgcPlane(new TGCVector3(0, 0, planeSize.Z * i), planeSize, pisoOrientacion, pisoTextura);
                piso.Add(plane);

            }
            for (var i = 13; i < 16; i++)
            {
                var plane = new TgcPlane(new TGCVector3(0, 0, planeSize.Z * i), planeSize, pisoOrientacion, pisoTextura);
                piso.Add(plane);

            }
            for (var i = 2; i < 4; i++)
            {
                var plane = new TgcPlane(new TGCVector3(planeSize.X * i, 0, /*planeSize.Z * i*/8000), planeSize, pisoOrientacion, pisoTextura);
                piso.Add(plane);

            }

            //---------pared1
            for (var i = 0; i < 8; i++)
            {
                var center = new TGCVector3(0, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);
                box.AutoTransform = true;
                pared1.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }

            for (var i = 9; i < 12; i++)
            {
                var center = new TGCVector3(0, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);
                box.AutoTransform = true;
                pared1.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }
            for (var i = 12; i < 17; i++)
            {
                var center = new TGCVector3(0, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);
                box.AutoTransform = true;
                pared1.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }

            for (var i = 0; i < 4; i++)
            {
                var center = new TGCVector3((planeSize.X / 2) + boxSize2.X * i, boxSize2.Y / 2, 8500/*(boxSize.Z / 2) + boxSize.Z * i*/);
                var box = TGCBox.fromSize(center, boxSize2, texturaTipo1);
                box.AutoTransform = true;
                pared1.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }

            //------pared2
            for (var i = 0; i < 8; i++)
            {
                var center = new TGCVector3(planeSize.X, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);

                box.AutoTransform = true;
                pared2.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }

            for (var i = 8; i < 12; i++)
            {
                var center = new TGCVector3(planeSize.X, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);

                box.AutoTransform = true;
                pared2.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }
            for (var i = 12; i < 16; i++)
            {
                var center = new TGCVector3(planeSize.X, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);

                box.AutoTransform = true;
                pared2.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }

            for (var i = 1; i < 4; i++)
            {
                var center = new TGCVector3((planeSize.X / 2) + boxSize2.X * i, boxSize2.Y / 2, 8000/*(boxSize.Z / 2) + boxSize.Z * i*/);
                var box = TGCBox.fromSize(center, boxSize2, texturaTipo1);
                box.AutoTransform = true;
                pared1.Add(box);

                aabbDelEscenario.Add(box.BoundingBox);
            }

            //------------objetos estaticos----------------------------
            //----------
            var texturaCaja = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "\\Texturas\\cajaMadera4.jpg");
            var sizeCaja = new TGCVector3(50, 50, 50);

            caja1 = TGCBox.fromSize(new TGCVector3(35, sizeCaja.Y / 2, 150), sizeCaja, texturaCaja);
            caja1.AutoTransform = true;
            aabbDelEscenario.Add(caja1.BoundingBox);

            caja2 = TGCBox.fromSize(new TGCVector3(465, sizeCaja.Y / 2, 2300), sizeCaja, texturaCaja);
            caja2.AutoTransform = true;
            aabbDelEscenario.Add(caja2.BoundingBox);


            //------------objetos con matrices de movimiento----------------------------
            var centro = new TGCVector3(0f, 0f, 0f);
            var tamanio = new TGCVector3(5f, 5f, 5f);
            //Caja con movimiento en Y
            caja3 = TGCBox.fromSize(centro, tamanio, texturaCaja);
            caja3.AutoTransform = false;
            plataformasDelEscenario.Add(caja3.BoundingBox);

            //Caja con movimiento en X
            caja4 = TGCBox.fromSize(centro, tamanio, texturaCaja);
            caja4.AutoTransform = false;
            plataformasDelEscenario.Add(caja4.BoundingBox);

            //Caja con movimiento circular sobre el eje Y
            caja5 = TGCBox.fromSize(centro, tamanio, texturaCaja);
            caja5.AutoTransform = false;
            plataformasDelEscenario.Add(caja5.BoundingBox);

            //---------------------Otros objetos
            var loader = new TgcSceneLoader();
            var sceneBarril = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Objetos\\BarrilPolvora\\BarrilPolvora-TgcScene.xml");
            barril1 = sceneBarril.Meshes[0];
            barril1.AutoTransform = true;
            barril1.Position = new TGCVector3(100, 0, 300);
            aabbDelEscenario.Add(barril1.BoundingBox);

            var sceneCalabera = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\Calabera\\Calabera-TgcScene.xml");
            var sceneEsqueleto = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\EsqueletoHumano\\Esqueleto-TgcScene.xml");
            calabera = sceneCalabera.Meshes[0];
            calabera.AutoTransform = true;
            calabera.Position = new TGCVector3(400, 0, 1000);
            aabbDelEscenario.Add(calabera.BoundingBox);

            esqueleto = sceneEsqueleto.Meshes[0];
            esqueleto.AutoTransform = true;
            esqueleto.Position = new TGCVector3(25, 0, 1600);
            aabbDelEscenario.Add(esqueleto.BoundingBox);


            for (var i = 1; i < 8; i++)
            {
                var scenePilar = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Cimientos\\PilarEgipcio\\PilarEgipcio-TgcScene.xml");

                pilar = scenePilar.Meshes[0];
                pilar.Position = new TGCVector3(35, 0, planeSize.Z * i);
                pilar.Scale = new TGCVector3(2f, 2f, 2f);
                pilar.AutoTransform = true;
                pilares.Add(pilar);
                aabbDelEscenario.Add(pilar.BoundingBox);
            }
            for (var i = 1; i < 8; i++)
            {
                var scenePilar = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Cimientos\\PilarEgipcio\\PilarEgipcio-TgcScene.xml");

                pilar = scenePilar.Meshes[0];
                pilar.Position = new TGCVector3(465, 0, planeSize.Z * i);
                pilar.Scale = new TGCVector3(2f, 2f, 2f);
                pilar.AutoTransform = true;
                pilares.Add(pilar);
                aabbDelEscenario.Add(pilar.BoundingBox);
            }
            for (var i = 1; i < 7; i++)
            {
                var sceneMesaLuz = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Muebles\\MesaDeLuz\\MesaDeLuz-TgcScene.xml");
                mueble1 = sceneMesaLuz.Meshes[0];
                if ((i % 2) == 0)
                {
                    mueble1.Position = new TGCVector3(465, 0, 575 * i);
                    mueble1.RotateY(FastMath.PI_HALF);
                }
                else
                {
                    mueble1.Position = new TGCVector3(35, 0, 575 * i);
                    mueble1.RotateY(-FastMath.PI_HALF);
                }

                mueble1.AutoTransform = true;
                muebles.Add(mueble1);
                aabbDelEscenario.Add(mueble1.BoundingBox);
            }


            var scenePilar2 = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Cimientos\\PilarEgipcio\\PilarEgipcio-TgcScene.xml");
            pilarCaido1 = scenePilar2.Meshes[0];
            pilarCaido1.AutoTransform = false;
            aabbDelEscenario.Add(pilarCaido1.BoundingBox);
            pilarCaido1.Transform = TGCMatrix.RotationYawPitchRoll(0f, 0f, 1.5f) * TGCMatrix.Translation(120f, 10f, 1330f) * TGCMatrix.Scaling(3f, 1.2f, 1.2f);
            pilarCaido1.BoundingBox.transform(TGCMatrix.RotationYawPitchRoll(0f, 0f, 1.5f) * TGCMatrix.Translation(120f, 10f, 1330f) * TGCMatrix.Scaling(3f, 1.2f, 1.2f));//TGCMatrix.Scaling(6f, 0.5f, 1f)); //* TGCMatrix.RotationYawPitchRoll(0f, 0f, 1f));

            var scenePilar3 = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Cimientos\\PilarEgipcio\\PilarEgipcio-TgcScene.xml");

            pilarCaido2 = scenePilar3.Meshes[0];
            pilarCaido2.AutoTransform = false;
            aabbDelEscenario.Add(pilarCaido2.BoundingBox);
            pilarCaido2.Transform = TGCMatrix.RotationYawPitchRoll(0f, 0f, 1.5f) * TGCMatrix.Translation(120f, 10f, 2330f) * TGCMatrix.Scaling(3f, 1.2f, 1.2f);
            pilarCaido2.BoundingBox.transform(TGCMatrix.RotationYawPitchRoll(0f, 0f, 1.5f) * TGCMatrix.Translation(120f, 10f, 2330f) * TGCMatrix.Scaling(3f, 1.2f, 1.2f));//TGCMatrix.Scaling(6f, 0.5f, 1f)); //* TGCMatrix.RotationYawPitchRoll(0f, 0f, 1f));

            //--------------------------------------------------
            //---------plataformas
            var texturaPlataforma = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "Texturas\\BM_DiffuseMap_pared.jpg");
            var plataformaSize = new TGCVector3(100f, 20f, 100f);
            plaFija1 = TGCBox.fromSize(new TGCVector3(0, 0, 0), plataformaSize, texturaPlataforma);
            aabbDelEscenario.Add(plaFija1.BoundingBox);

            plaFija2 = TGCBox.fromSize(new TGCVector3(0, 0, 0), plataformaSize, texturaPlataforma);
            aabbDelEscenario.Add(plaFija2.BoundingBox);

            plaFija3 = TGCBox.fromSize(new TGCVector3(0, 0, 0), plataformaSize, texturaPlataforma);
            aabbDelEscenario.Add(plaFija3.BoundingBox);

            plaFija4 = TGCBox.fromSize(new TGCVector3(0, 0, 0), plataformaSize, texturaPlataforma);
            aabbDelEscenario.Add(plaFija4.BoundingBox);

            plaFija5 = TGCBox.fromSize(new TGCVector3(0, 0, 0), plataformaSize, texturaPlataforma);
            aabbDelEscenario.Add(plaFija5.BoundingBox);

            plaMovil1 = TGCBox.fromSize(new TGCVector3(0f, 0f, 0f), plataformaSize, texturaPlataforma);
            plaMovil1.AutoTransform = false;
            aabbDelEscenario.Add(plaMovil1.BoundingBox);

            //Matrices para el manejo de cajas
            escalaBaseParaCajas = TGCMatrix.Scaling(10f, 10f, 10f);
            var ajusteAlturaDelCentro = caja3.Size.Y / 2;
            posicionamientoCaja3 = TGCMatrix.Translation(20, ajusteAlturaDelCentro, 20);
            posicionamientoCaja4 = TGCMatrix.Translation(30, ajusteAlturaDelCentro, 30);
            pivoteCaja5 = TGCMatrix.Translation(30, ajusteAlturaDelCentro, 60);
            radioCaja5 = TGCMatrix.Translation(0, 0, 5);

            //Matrices para el manejo de plataformas
            posicionamientoPla1 = TGCMatrix.Translation(250, -10, 500 * 8 + 500 / 2);

            posicionamientoPla2 = TGCMatrix.Translation(150, -10, 500 * 16 + 500 / 2);

            posicionamientoPla3 = TGCMatrix.Translation(400, -10, 500 * 16 + 300);

            posicionamientoPla4 = TGCMatrix.Translation(650, -10, 500 * 16 + 500 / 2);

            posicionamientoPla5 = TGCMatrix.Translation(900, -10, 500 * 16 + 500 / 2);

            posPlaMovil1 = TGCMatrix.Translation(250, -10, 500 * 12 + plaMovil1.Size.Z / 2);

            //Asumo el orden en el que fueron agregadas
            desplazamientoCaja3 = new TGCVector3(0, 1, 0);
            desplazamientoCaja4 = new TGCVector3(1, 0, 0);
            desplazamientoCaja5 = new TGCVector3(0, 1, 0);

            //En principio pensamos en tener una lista de matricesDePlataformasDelEscenario pero
            //Al pasar las matrices no logramos lo esperado
            desplazamientosDePlataformasDelEscenario.Add(desplazamientoCaja3); //caja3
            desplazamientosDePlataformasDelEscenario.Add(desplazamientoCaja4); //caja4
            desplazamientosDePlataformasDelEscenario.Add(desplazamientoCaja5); //caja5

            tiempoDeSubida = 5f; //Equivalente a 5 segundos?
            tiempoDeMovLateral = 10f;
            subir = true; //La caja empieza subiendo
            movLateral = true;
            rotacionTotal = 0f;

            //----------musica
            musicaFondo = new TgcMp3Player();
            musicaFondo.FileName = GModel.MediaDir + "\\Sound\\LavTown.mp3";
            musicaFondo.play(true);

        }

        public void Update()
        {

            //Intento de hacer que la caja3 se traslade entre dos limites de tiempo
            if (subir)
            {
                traslacionCaja3 += velocidadTraslacionCaja * GModel.ElapsedTime;
                tiempoDeSubida -= GModel.ElapsedTime;
                if (tiempoDeSubida <= 0)
                {
                    subir = false;
                }
            }
            else
            {
                traslacionCaja3 -= velocidadTraslacionCaja * GModel.ElapsedTime;
                tiempoDeSubida += GModel.ElapsedTime;
                if (tiempoDeSubida >= 5)
                {
                    subir = true;
                }
            }

            //Movimiento caja4
            if (movLateral)
            {
                traslacionCaja4 += velocidadTraslacionCaja * GModel.ElapsedTime;
                tiempoDeMovLateral -= GModel.ElapsedTime;
                if (tiempoDeMovLateral <= 0)
                {
                    movLateral = false;
                }
            }
            else
            {
                traslacionCaja4 -= velocidadTraslacionCaja * GModel.ElapsedTime;
                tiempoDeMovLateral += GModel.ElapsedTime;
                if (tiempoDeMovLateral >= 10)
                {
                    movLateral = true;
                }
            }

            desplazamientoCaja3 = new TGCVector3(0, traslacionCaja3, 0);
            desplazamientoCaja4 = new TGCVector3(traslacionCaja4, 0, 0);
            //El problema de plantearlo asi es que la caja que rota sobre un eje Y no seria facil de calcular...
            desplazamientoCaja5 = new TGCVector3(0, 1, 0);

            //En base a la traslacion que quiero se actualiza la matriz de traslación         
            movimientoTraslacionY = TGCMatrix.Translation(desplazamientoCaja3);
            movimientoTraslacionX = TGCMatrix.Translation(desplazamientoCaja4);

            //Movimiento caja 5
            rotacionTotal += velocidadRotacionCaja * GModel.ElapsedTime;
            movimientoRotacionY = TGCMatrix.RotationY(rotacionTotal);

        }
        public void Render()
        {


            foreach (var plano in piso)
            {
                plano.Render();
            }

            foreach (var box in pared1)
            {
                box.Render();
            }
            foreach (var box in pared2)
            {
                box.Render();
            }

            caja1.Render();
            caja2.Render();
            //Posiciono la caja 3, la escalo para el tamaño que quiero y le aplico la matriz de desplazamiento en Y
            var transformacionesCaja3 = posicionamientoCaja3 * escalaBaseParaCajas * movimientoTraslacionY;
            caja3.Transform = transformacionesCaja3;
            //Las transformaciones deben aplicarse tambien a las boundingBox
            caja3.BoundingBox.transform(transformacionesCaja3);

            caja3.Render();

            var transformacionesCaja4 = posicionamientoCaja4 * escalaBaseParaCajas * movimientoTraslacionX;
            caja4.Transform = transformacionesCaja4;
            //Las transformaciones deben aplicarse tambien a las boundingBox
            caja4.BoundingBox.transform(transformacionesCaja4);

            caja4.Render();

            var transformacionesCaja5 = radioCaja5 * movimientoRotacionY * pivoteCaja5 * escalaBaseParaCajas;
            caja5.Transform = transformacionesCaja5;
            //Las transformaciones deben aplicarse tambien a las boundingBox
            caja5.BoundingBox.transform(transformacionesCaja5);

            caja5.Render();

            barril1.Render();
            calabera.Render();
            esqueleto.Render();


            foreach (var pilar in pilares)
            {
                pilar.Render();

            }

            foreach (var mueble1 in muebles)
            {
                mueble1.Render();

            }
            pilarCaido1.Render();


            pilarCaido2.Render();

            //-------prueba pltaforma fija
            plaFija1.Transform = posicionamientoPla1;
            plaFija1.BoundingBox.transform(posicionamientoPla1);
            plaFija1.Render();

            plaFija2.Transform = posicionamientoPla2;
            plaFija2.BoundingBox.transform(posicionamientoPla2);
            plaFija2.Render();

            plaFija3.Transform = posicionamientoPla3;
            plaFija3.BoundingBox.transform(posicionamientoPla3);
            plaFija3.Render();

            plaFija4.Transform = posicionamientoPla4;
            plaFija4.BoundingBox.transform(posicionamientoPla4);
            plaFija4.Render();

            plaFija5.Transform = posicionamientoPla5;
            plaFija5.BoundingBox.transform(posicionamientoPla5);
            plaFija5.Render();

            var transformacionesPlataformaMovil = movimientoTraslacionY * posPlaMovil1;
            plaMovil1.Transform = transformacionesPlataformaMovil;
            plaMovil1.BoundingBox.transform(transformacionesPlataformaMovil);

            plaMovil1.Render();



        }
        public void Dispose()
        {

            foreach (var plano in piso)
            {
                plano.Dispose();
            }

            foreach (var box in pared1)
            {
                box.Dispose();
            }
            foreach (var box in pared2)
            {
                box.Dispose();
            }


            caja1.Dispose();
            caja2.Dispose();
            caja3.Dispose();
            caja4.Dispose();
            caja5.Dispose();
            barril1.Dispose();
            calabera.Dispose();
            esqueleto.Dispose();
            foreach (var pilar in pilares)
            {
                pilar.Dispose();

            }

            foreach (var mueble1 in muebles)
            {
                mueble1.Dispose();

            }
            pilarCaido1.Dispose();
            pilarCaido2.Dispose();

            plaFija1.Dispose();
            plaFija2.Dispose();
            plaFija3.Dispose();

            plaFija4.Dispose();

            plaFija5.Dispose();


            plaMovil1.Dispose();
        }
        public List<TgcBoundingAxisAlignBox> getAABBDelEscenario()
        {
            return aabbDelEscenario;
        }
        public List<TgcBoundingAxisAlignBox> getPlataformasDelEscenario()
        {
            return plataformasDelEscenario;
        }
        public TGCVector3 desplazamientoDePlataforma(TgcBoundingAxisAlignBox plataforma)
        {
            int indice;
            indice = plataformasDelEscenario.IndexOf(plataforma);
            return desplazamientosDePlataformasDelEscenario.ElementAt(indice);
        }
        public List<TgcPlane> getPiso()
        {
            return piso;
        }

        public void DrawBoundingBox()
        {
            foreach (var boundingBox in aabbDelEscenario)
            {
                boundingBox.Render();
            }
            foreach (var boundingBox in plataformasDelEscenario)
            {
                boundingBox.Render();
            }

        }
    }
}
