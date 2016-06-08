using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Collision.ElipsoidCollision;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo ElipsoidCollision
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminaci�n - SkyBox
    ///     # Unidad 6 - Detecci�n de Colisiones - Estrategia Integral
    ///     Ejemplo de nivel avanzado.
    ///     Se recomienda leer primero SphereCollision
    ///     Muestra una posible implementaci�n de la Estrategia Integral de colisiones de un Elipsoide con gravedad y sliding,
    ///     explicada en el apunte de Detecci�n de Colisiones.
    ///     Es una mejora del algoritmo utilizado en SphereCollision, pero utilizando un Elipsoide y m�todos de colisi�n
    ///     mas robustos.
    ///     Se utilizan distintos volumenes de colision (llamados Colliders): basados en triangulos y basados en BoundingBox.
    ///     Tener Colliders del tipo Triangulos permite armar terrenos con desniveles irregulares.
    ///     En cambio SphereCollision solo soporta BoundingBox.
    ///     Aun esta en estado BETA.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class ElipsoidCollision : TgcExample
    {
        private readonly List<Collider> objetosColisionables = new List<Collider>();
        private TgcElipsoid characterElipsoid;
        private ElipsoidCollisionManager collisionManager;
        private TgcArrow collisionNormalArrow;
        private TgcBox collisionPoint;
        private TgcArrow directionArrow;
        private TgcScene escenario;
        private bool jumping;
        private float jumpingElapsedTime;
        private TgcSkeletalMesh personaje;
        private TgcSkyBox skyBox;

        public ElipsoidCollision(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Collision";
            Name = "Elipsoid collision";
            Description =
                "Colisi�n de un Elipsoide contra todo un escenario, aplicando gravedad y sliding. Movimiento con W, A, S, D, Space.";
        }

        public override void Init()
        {
            //Cargar escenario espec�fico para este ejemplo. Este escenario tiene dos layers: objetos normales y objetos con colisi�n a nivel de tri�ngulo.
            //La colisi�n a nivel de tri�ngulos es costosa. Solo debe utilizarse para objetos puntuales (como el piso). Y es recomendable dividirlo en varios
            //meshes (y no hacer un �nico piso que ocupe todo el escenario)
            var loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(MediaDir + "\\MeshCreator\\Scenes\\Mountains\\Mountains-TgcScene.xml");

            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Walk-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Jump-TgcSkeletalAnim.xml"
                    });

            //Configurar animacion inicial
            personaje.playAnimation("StandBy", true);
            //Escalarlo porque es muy grande
            personaje.Position = new Vector3(0, 1000, -150);
            //Rotarlo 180� porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));

            //BoundingSphere que va a usar el personaje
            personaje.AutoUpdateBoundingBox = false;
            characterElipsoid = new TgcElipsoid(personaje.BoundingBox.calculateBoxCenter() + new Vector3(0, 0, 0),
                new Vector3(12, 28, 12));
            jumping = false;

            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                //Los objetos del layer "TriangleCollision" son colisiones a nivel de triangulo
                if (mesh.Layer == "TriangleCollision")
                {
                    objetosColisionables.Add(TriangleMeshCollider.fromMesh(mesh));
                }
                //El resto de los objetos son colisiones de BoundingBox. Las colisiones a nivel de triangulo son muy costosas asi que deben utilizarse solo
                //donde es extremadamente necesario (por ejemplo en el piso). El resto se simplifica con un BoundingBox
                else
                {
                    objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                }
            }

            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.GravityEnabled = true;

            //Crear linea para mostrar la direccion del movimiento del personaje
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 0.4f;
            directionArrow.HeadSize = new Vector2(5, 10);

            //Linea para normal de colision
            collisionNormalArrow = new TgcArrow();
            collisionNormalArrow.BodyColor = Color.Blue;
            collisionNormalArrow.HeadColor = Color.Yellow;
            collisionNormalArrow.Thickness = 0.4f;
            collisionNormalArrow.HeadSize = new Vector2(2, 5);

            //Caja para marcar punto de colision
            collisionPoint = TgcBox.fromSize(new Vector3(4, 4, 4), Color.Red);

            //Configurar camara en Tercer Persona
            Camara = new TgcThirdPersonCamera();
            ((TgcThirdPersonCamera)Camara).setTargetOffsets(personaje.Position, 20, -120);
            ((TgcThirdPersonCamera)Camara).TargetDisplacement = new Vector3(0, 45, 0);

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox3\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "Up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "Down.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "Left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "Right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "Back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "Front.jpg");
            skyBox.updateValues();

            //Modifier para ver BoundingBox
            Modifiers.addBoolean("Collisions", "Collisions", true);
            Modifiers.addBoolean("showBoundingBox", "Bouding Box", true);

            //Modifiers para desplazamiento del personaje
            Modifiers.addFloat("VelocidadCaminar", 0, 20, 2);
            Modifiers.addFloat("VelocidadRotacion", 1f, 360f, 150f);
            Modifiers.addBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            Modifiers.addVertex3f("Gravedad", new Vector3(-50, -50, -50), new Vector3(50, 50, 50),
                new Vector3(0, -4, 0));
            Modifiers.addFloat("SlideFactor", 0f, 2f, 1f);
            Modifiers.addFloat("Pendiente", 0f, 1f, 0.72f);
            Modifiers.addFloat("VelocidadSalto", 0f, 50f, 10f);
            Modifiers.addFloat("TiempoSalto", 0f, 2f, 0.5f);

            UserVars.addVar("Movement");
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = (bool)Modifiers.getValue("showBoundingBox");

            //obtener velocidades de Modifiers
            var velocidadCaminar = (float)Modifiers.getValue("VelocidadCaminar");
            var velocidadRotacion = (float)Modifiers.getValue("VelocidadRotacion");
            var velocidadSalto = (float)Modifiers.getValue("VelocidadSalto");
            var tiempoSalto = (float)Modifiers.getValue("TiempoSalto");

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var d3dInput = TgcD3dInput.Instance;
            var moving = false;
            var rotating = false;
            float jump = 0;

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Jump
            if (!jumping && d3dInput.keyPressed(Key.Space))
            {
                //Se puede saltar solo si hubo colision antes
                if (collisionManager.Result.collisionFound)
                {
                    jumping = true;
                    jumpingElapsedTime = 0f;
                    jump = 0;
                }
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personaje.rotateY(rotAngle);
                ((TgcThirdPersonCamera)Camara).rotateY(rotAngle);
            }

            //Saltando
            if (jumping)
            {
                //Activar animacion de saltando
                personaje.playAnimation("Jump", true);
            }
            //Si hubo desplazamiento
            else if (moving)
            {
                //Activar animacion de caminando
                personaje.playAnimation("Walk", true);
            }
            //Si no se esta moviendo ni saltando, activar animacion de Parado
            else
            {
                personaje.playAnimation("StandBy", true);
            }

            //Actualizar salto
            if (jumping)
            {
                //El salto dura un tiempo hasta llegar a su fin
                jumpingElapsedTime += ElapsedTime;
                if (jumpingElapsedTime > tiempoSalto)
                {
                    jumping = false;
                }
                else
                {
                    jump = velocidadSalto * (tiempoSalto - jumpingElapsedTime);
                }
            }

            //Vector de movimiento
            var movementVector = Vector3.Empty;
            if (moving || jumping)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new Vector3(
                    FastMath.Sin(personaje.Rotation.Y) * moveForward,
                    jump,
                    FastMath.Cos(personaje.Rotation.Y) * moveForward
                    );
            }

            //Actualizar valores de gravedad
            collisionManager.GravityEnabled = (bool)Modifiers["HabilitarGravedad"];
            collisionManager.GravityForce = (Vector3)Modifiers["Gravedad"] /** elapsedTime*/;
            collisionManager.SlideFactor = (float)Modifiers["SlideFactor"];
            collisionManager.OnGroundMinDotValue = (float)Modifiers["Pendiente"];

            //Si esta saltando, desactivar gravedad
            if (jumping)
            {
                collisionManager.GravityEnabled = false;
            }

            //Mover personaje con detecci�n de colisiones, sliding y gravedad
            if ((bool)Modifiers["Collisions"])
            {
                //Aca se aplica toda la l�gica de detecci�n de colisiones del CollisionManager. Intenta mover el Elipsoide
                //del personaje a la posici�n deseada. Retorna la verdadera posicion (realMovement) a la que se pudo mover
                var realMovement = collisionManager.moveCharacter(characterElipsoid, movementVector,
                    objetosColisionables);
                personaje.move(realMovement);

                //Cargar desplazamiento realizar en UserVar
                UserVars.setValue("Movement", TgcParserUtils.printVector3(realMovement));
            }
            else
            {
                personaje.move(movementVector);
            }

            /*
            //Si estaba saltando y hubo colision de una superficie que mira hacia abajo, desactivar salto
            if (jumping && collisionManager.Result.collisionNormal.Y < 0)
            {
                jumping = false;
            }
            */

            //Hacer que la camara siga al personaje en su nueva posicion
            ((TgcThirdPersonCamera)Camara).Target = personaje.Position;

            //Actualizar valores de la linea de movimiento
            directionArrow.PStart = characterElipsoid.Center;
            directionArrow.PEnd = characterElipsoid.Center + Vector3.Multiply(movementVector, 50);
            directionArrow.updateValues();

            //Actualizar valores de normal de colision
            if (collisionManager.Result.collisionFound)
            {
                collisionNormalArrow.PStart = collisionManager.Result.collisionPoint;
                collisionNormalArrow.PEnd = collisionManager.Result.collisionPoint +
                                            Vector3.Multiply(collisionManager.Result.collisionNormal, 80);
                ;
                collisionNormalArrow.updateValues();
                collisionNormalArrow.render();

                collisionPoint.Position = collisionManager.Result.collisionPoint;
                collisionPoint.render();
            }

            //Render de mallas
            foreach (var mesh in escenario.Meshes)
            {
                mesh.render();
            }

            //Render personaje
            personaje.animateAndRender(ElapsedTime);
            if (showBB)
            {
                characterElipsoid.render();
            }

            //Render linea
            directionArrow.render();

            //Render SkyBox
            skyBox.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            escenario.disposeAll();
            personaje.dispose();
            skyBox.dispose();
            collisionNormalArrow.dispose();
            directionArrow.dispose();
            characterElipsoid.dispose();
        }
    }
}