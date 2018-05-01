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

namespace TGC.Group.Model
{
    public class Escenario1
    {
        private GameModel GModel;
        private List<TGCBox> pared1;
        private List<TGCBox> pared2;
        private List<TgcPlane> piso;
        private TGCBox caja1,caja2,caja3;
        private TgcMesh barril1,calabera,esqueleto;
        TGCMatrix escalaBaseParaCajas;
        TGCMatrix posicionamientoCaja3;
        TGCMatrix movimientoTraslacionY;
        float traslacionCaja3 = 0f;
        float velocidadTraslacionCaja = 10f;
        float tiempoDeSubida;
        bool subir;

        private List<TgcBoundingAxisAlignBox> aabbDelEscenario;


        public void Init(GameModel gameModel) {
            GModel = gameModel;
            var pisoTextura = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "Texturas\\largerblock3b3dim.jpg");
            var planeSize = new TGCVector3(500, 0, 500);
            var pisoOrientacion = TgcPlane.Orientations.XZplane;

            var texturaTipo1 = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "\\Texturas\\blocks9.jpg");


            var boxSize = new TGCVector3(20, 150, 500);

            pared1 = new List<TGCBox>();
            pared2 = new List<TGCBox>();
            piso = new List<TgcPlane>();

            aabbDelEscenario = new List<TgcBoundingAxisAlignBox>();

            for (var i = 0; i < 8; i++)
            {
                var plane = new TgcPlane(new TGCVector3(0, 0, planeSize.Z * i), planeSize, pisoOrientacion, pisoTextura);
                piso.Add(plane);

            }
            for (var i = 0; i < 8; i++)
            {
                var center = new TGCVector3(planeSize.X, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);
                
                box.AutoTransform = true;
                pared1.Add(box);
                aabbDelEscenario.Add(box.BoundingBox);

            }
            for (var i = 0; i < 8; i++)
            {
                var center = new TGCVector3(0, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);
                box.AutoTransform = true;
                pared2.Add(box);
                aabbDelEscenario.Add(box.BoundingBox);
            }
            var texturaCaja = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "\\Texturas\\cajaMadera4.jpg");
            var sizeCaja = new TGCVector3(50,50,50);
            //Caja1 posicion = 100, 50/2, 150
            caja1 = TGCBox.fromSize(new TGCVector3(100,sizeCaja.Y/2,150),sizeCaja,texturaCaja);
            caja1.AutoTransform = true;
            aabbDelEscenario.Add(caja1.BoundingBox);


            caja2 = TGCBox.fromSize(new TGCVector3(400, sizeCaja.Y / 2, 2500), sizeCaja, texturaCaja);
            caja2.AutoTransform = true;
            aabbDelEscenario.Add(caja2.BoundingBox);


            //Caja3 sin autoTransform
            var centro = new TGCVector3(0f,0f,0f);
            var tamanio = new TGCVector3(5f, 5f, 5f);
            caja3 = TGCBox.fromSize(centro, tamanio, texturaCaja);
            caja3.AutoTransform = false;
            aabbDelEscenario.Add(caja3.BoundingBox);


            //---------------------
            var loader = new TgcSceneLoader();
            var sceneBarril = loader.loadSceneFromFile(GModel.MediaDir+ "\\MeshCreator\\Meshes\\Objetos\\BarrilPolvora\\BarrilPolvora-TgcScene.xml");
            barril1 = sceneBarril.Meshes[0];
            barril1.AutoTransform = true;
            barril1.Position = new TGCVector3(100,0,300);
            aabbDelEscenario.Add(barril1.BoundingBox);


            var sceneCalabera= loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\Calabera\\Calabera-TgcScene.xml");
            var sceneEsqueleto = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\EsqueletoHumano\\Esqueleto-TgcScene.xml");
            calabera = sceneCalabera.Meshes[0];
            calabera.AutoTransform = true;
            calabera.Position= new TGCVector3(400, 0, 1000);
            aabbDelEscenario.Add(calabera.BoundingBox);

            esqueleto = sceneEsqueleto.Meshes[0];
            esqueleto.AutoTransform = true;
            esqueleto.Position = new TGCVector3(30, 0, 1500);
            aabbDelEscenario.Add(esqueleto.BoundingBox);


            //Matrices para el manejo de cajas
            escalaBaseParaCajas = TGCMatrix.Scaling(10f, 10f, 10f);
            posicionamientoCaja3 = TGCMatrix.Translation(20, caja3.Size.Y / 2, 20);

            tiempoDeSubida = 5f; //Equivalente a 5 segundos?
            subir = true; //La caja empieza subiendo
        }

        public void Update() {
            //Intento de hacer que la caja3 se traslade entre dos limites de tiempo
            if(subir)
            {
                traslacionCaja3 += velocidadTraslacionCaja * GModel.ElapsedTime;
                tiempoDeSubida -= GModel.ElapsedTime;
                if(tiempoDeSubida <= 0)
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

            //En base a la traslacion que quiero se actualiza la matriz de traslación
            movimientoTraslacionY = TGCMatrix.Translation(0, traslacionCaja3, 0);
        }
        public void Render() {


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
            caja3.Transform = posicionamientoCaja3 * escalaBaseParaCajas * movimientoTraslacionY;
            caja3.Render();
            barril1.Render();
            calabera.Render();
            esqueleto.Render();

        }
        public void Dispose() {

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
            barril1.Dispose();
            calabera.Dispose();
            esqueleto.Dispose();
        }
        public List<TgcBoundingAxisAlignBox> getAABBDelEscenario()
        {
            return aabbDelEscenario;
        }
        public List<TgcPlane> getPiso()
        {
            return piso;
        }

    }
}
