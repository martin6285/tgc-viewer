using Microsoft.DirectX;
using System.Collections.Generic;
using TGC.Core.Camara;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Optimization.Octree
{
    /// <summary>
    ///     Ejemplo EjemploOctree
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimizacion - Octree
    ///     Muestra como crear y utilizar una Octree para optimizar el renderizado de un escenario por Frustum Culling.
    ///     El escenario es una isla con palmeras, rocas y el suelo. Solo las palmeras y rocas se optimizan con esta tecnica.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploOctree : TGCExampleViewer
    {
        private List<TgcMesh> objetosIsla;
        private Octree octree;
        private TgcSkyBox skyBox;
        private TgcMesh terreno;

        public EjemploOctree(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Optimization";
            Name = "Octree";
            Description =
                "Muestra como crear y utilizar una Octree para optimizar el renderizado de un escenario por Frustum Culling.";
        }

        public override void Init()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new TGCVector3(0, 500, 0);
            skyBox.Size = new TGCVector3(10000, 10000, 10000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            skyBox.Init();

            //Cargar escenario de Isla
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "Isla\\Isla-TgcScene.xml");

            //Separar el Terreno del resto de los objetos
            var list1 = new List<TgcMesh>();
            scene.separeteMeshList(new[] { "Terreno" }, out list1, out objetosIsla);
            terreno = list1[0];

            //Crear Octree
            octree = new Octree();
            octree.create(objetosIsla, scene.BoundingBox);
            octree.createDebugOctreeMeshes();

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(1500, 800, 0), Input);

            Modifiers.addBoolean("showOctree", "Show Octree", false);
            Modifiers.addBoolean("showTerrain", "Show Terrain", true);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            var showOctree = (bool)Modifiers["showOctree"];
            var showTerrain = (bool)Modifiers["showTerrain"];

            skyBox.render();
            if (showTerrain)
            {
                terreno.render();
            }
            octree.render(Frustum, showOctree);

            PostRender();
        }

        public override void Dispose()
        {
            skyBox.dispose();
            terreno.dispose();
            foreach (var mesh in objetosIsla)
            {
                mesh.dispose();
            }
        }
    }
}