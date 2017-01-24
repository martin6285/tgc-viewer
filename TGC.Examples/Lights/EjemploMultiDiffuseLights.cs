using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Interpolation;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploMultiDiffuseLights:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - Iluminacion dinamica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploMultipleLights".
    ///     Muestra como aplicar iluminacion dinamica con PhongShading por pixel en un Pixel Shader.
    ///     Utiliza varias luces para un mismo objeto, en una misma pasada de Shaders.
    ///     Solo calcula el componente Diffuse, para acelerar los circulos.
    ///     Las luces poseen atenuacion por la distancia.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMultiDiffuseLights : TGCExampleViewer
    {
        private Effect effect;
        private InterpoladorVaiven interp;
        private TgcBox[] lightMeshes;
        private TGCVector3[] origLightPos;
        private TgcScene scene;

        public EjemploMultiDiffuseLights(string mediaDir, string shadersDir, TgcUserVars userVars,
            TgcModifiers modifiers) : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Pixel Shaders";
            Name = "Multiple Diffuse Lights";
            Description = "Iluminacion dinamicas con 4 luces Diffuse a la vez para un mismo mesh.";
        }

        public override void Init()
        {
            //Cargar escenario
            var loader = new TgcSceneLoader();
            scene =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(260f, 170f, 390f), 400f, 300f, Input);

            //Cargar Shader personalizado de MultiDiffuseLights
            /*
             * Cargar Shader personalizado de MultiDiffuseLights
             * Este Shader solo soporta TgcMesh con RenderType DIFFUSE_MAP (que son las unicas utilizadas en este ejemplo)
             * El shader toma 4 luces a la vez para iluminar un mesh.
             * Pero como hacer 4 veces los calculos en el shader es costoso, de cada luz solo calcula el componente Diffuse.
             */
            effect = TgcShaders.loadEffect(ShadersDir + "MultiDiffuseLights.fx");

            //Crear 4 mesh para representar las 4 para la luces. Las ubicamos en distintas posiciones del escenario, cada una con un color distinto.
            lightMeshes = new TgcBox[4];
            origLightPos = new TGCVector3[lightMeshes.Length];
            var c = new Color[4] { Color.Red, Color.Blue, Color.Green, Color.Yellow };
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var co = c[i % c.Length];
                lightMeshes[i] = TgcBox.fromSize(new TGCVector3(10, 10, 10), co);
                lightMeshes[i].AutoTransformEnable = true;
                origLightPos[i] = new TGCVector3(-40, 20 + i * 20, 400);
            }

            //Modifiers
            Modifiers.addBoolean("lightEnable", "lightEnable", true);
            Modifiers.addBoolean("lightMove", "lightMove", true);
            Modifiers.addFloat("lightIntensity", 0, 150, 38);
            Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.15f);

            Modifiers.addColor("mEmissive", Color.Black);
            Modifiers.addColor("mDiffuse", Color.White);

            //Interpolador para mover las luces de un lado para el otro
            interp = new InterpoladorVaiven();
            interp.Min = -200f;
            interp.Max = 200f;
            interp.Speed = 100f;
            interp.Current = 0f;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Habilitar luz
            var lightEnable = (bool)Modifiers["lightEnable"];
            Effect currentShader;
            string currentTechnique;
            if (lightEnable)
            {
                //Shader personalizado de iluminacion
                currentShader = effect;
                currentTechnique = "MultiDiffuseLightsTechnique";
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
                currentTechnique = TgcShaders.Instance.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
            }

            //Aplicar a cada mesh el shader actual
            foreach (var mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                mesh.Technique = currentTechnique;
            }

            //Configurar los valores de cada luz
            var move = new TGCVector3(0, 0,
                (bool)Modifiers["lightMove"] ? interp.update(ElapsedTime) : 0);
            var lightColors = new ColorValue[lightMeshes.Length];
            var pointLightPositions = new Vector4[lightMeshes.Length];
            var pointLightIntensity = new float[lightMeshes.Length];
            var pointLightAttenuation = new float[lightMeshes.Length];
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var lightMesh = lightMeshes[i];
                lightMesh.Position = origLightPos[i] + TGCVector3.Scale(move, i + 1);

                lightColors[i] = ColorValue.FromColor(lightMesh.Color);
                pointLightPositions[i] = TGCVector3.Vector3ToVector4(lightMesh.Position);
                pointLightIntensity[i] = (float)Modifiers["lightIntensity"];
                pointLightAttenuation[i] = (float)Modifiers["lightAttenuation"];
            }

            //Renderizar meshes
            foreach (var mesh in scene.Meshes)
            {
                mesh.UpdateMeshTransform();
                if (lightEnable)
                {
                    //Cargar variables de shader
                    mesh.Effect.SetValue("lightColor", lightColors);
                    mesh.Effect.SetValue("lightPosition", pointLightPositions);
                    mesh.Effect.SetValue("lightIntensity", pointLightIntensity);
                    mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);
                    mesh.Effect.SetValue("materialEmissiveColor",
                        ColorValue.FromColor((Color)Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialDiffuseColor",
                        ColorValue.FromColor((Color)Modifiers["mDiffuse"]));
                }

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar meshes de luz
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var lightMesh = lightMeshes[i];
                lightMesh.render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            scene.disposeAll();
            effect.Dispose();
            for (var i = 0; i < lightMeshes.Length; i++)
            {
                var lightMesh = lightMeshes[i];
                lightMesh.dispose();
            }
        }
    }
}