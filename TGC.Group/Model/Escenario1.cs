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
namespace TGC.Group.Model
{
    public class Escenario1
    {
        private GameModel GModel;
        private List<TGCBox> pared1;
        private List<TGCBox> pared2;
        private List<TgcPlane> piso;
        private TGCBox caja1,caja2;
        private TgcMesh barril1,calabera,esqueleto;


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

            }
            for (var i = 0; i < 8; i++)
            {
                var center = new TGCVector3(0, boxSize.Y / 2, (boxSize.Z / 2) + boxSize.Z * i);
                var box = TGCBox.fromSize(center, boxSize, texturaTipo1);
                box.AutoTransform = true;
                pared2.Add(box);

            }
            var texturaCaja = TgcTexture.createTexture(D3DDevice.Instance.Device, GModel.MediaDir + "\\Texturas\\cajaMadera4.jpg");
            var sizeCaja = new TGCVector3(50,50,50);
            caja1 = TGCBox.fromSize(new TGCVector3(100,sizeCaja.Y/2,150),sizeCaja,texturaCaja);
            caja1.AutoTransform = true;

            caja2 = TGCBox.fromSize(new TGCVector3(400, sizeCaja.Y / 2, 2500), sizeCaja, texturaCaja);
            caja2.AutoTransform = true;
            //---------------------
            var loader = new TgcSceneLoader();
            var sceneBarril = loader.loadSceneFromFile(GModel.MediaDir+ "\\MeshCreator\\Meshes\\Objetos\\BarrilPolvora\\BarrilPolvora-TgcScene.xml");
            barril1 = sceneBarril.Meshes[0];
            barril1.AutoTransform = true;
            barril1.Position = new TGCVector3(100,0,300);

            var sceneCalabera= loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\Calabera\\Calabera-TgcScene.xml");
            var sceneEsqueleto = loader.loadSceneFromFile(GModel.MediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\EsqueletoHumano\\Esqueleto-TgcScene.xml");
            calabera = sceneCalabera.Meshes[0];
            calabera.AutoTransform = true;
            calabera.Position= new TGCVector3(400, 0, 1000);

            esqueleto = sceneEsqueleto.Meshes[0];
            esqueleto.AutoTransform = true;
            esqueleto.Position = new TGCVector3(30, 0, 1500);




        }
        public void Update() { }
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
            barril1.Dispose();
            calabera.Dispose();
            esqueleto.Dispose();
        }


    }
}
