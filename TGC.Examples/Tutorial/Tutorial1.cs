using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;
using TGC.Core.Textures;
using TGC.Core.Utils;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;

namespace TGC.Examples.Tutorial
{
	/// <summary>
	///     Tutorial 1:
	///     Unidades Involucradas:
	///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
	/// 	# Unidad 4 - Texturas e iluminacion - Texturas
	///     Muestra como crear una caja 3D de color y como mostrarla por pantalla.
	/// 	Muestra como crear una caja 3D con una imagen 2D como textura para darle color.
	/// 	Muestra como crear una caja 3D con textura que se traslada y rota en cada cuadro.
	/// 	Muestra como crear una caja 3D que se mueve cuando las flechas del teclado.
	///     Autor: Mat�as Leone
	/// </summary>
	public class Tutorial1 : TGCExampleViewer
    {
		//Constantes para velocidades de movimiento
		private const float ROTATION_SPEED = 1f;
		private const float MOVEMENT_SPEED = 5f;

        //Variables para las cajas 3D
        private TgcBox box1;
		private TgcBox box2;
		private TgcBox box3;

		//Variable direccion de movimiento
		private float currentMoveDir = 1f;

        public Tutorial1(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Tutorial";
            Name = "Tutorial 1";
            Description = "Ejemplos de Creaci�n de Cajas 3D con color, con imagen 2D como Textura, con traslaci�n y con rotaci�n";
        }

        /// <summary>
        ///     M�todo en el que se deben crear todas las cosas que luego se van a querer usar.
        ///     Es invocado solo una vez al inicio del ejemplo.
        /// </summary>
        public override void Init()
        {
			//Todos los recursos que se van a necesitar (objetos 3D, meshes, texturas, etc) se deben cargar en el metodo init().
			//Crearlos cada vez en el metodo render() es un error grave. Destruye la performance y suele provocar memory leaks.

			//Creamos una caja 3D de color rojo, ubicada en el origen y lado 10
			var center = TGCVector3.Empty;
            var size = new TGCVector3(10, 10, 10);
            var color = Color.Red;
            box1 = TgcBox.fromSize(size, color);
			box1.Transform = TGCMatrix.Translation(center);

			//Cargamos una textura una textura es una im�gen 2D que puede dibujarse arriba de un pol�gono 3D para darle color.
			//Es muy �til para generar efectos de relieves y superficies.
			//Puede ser cualquier im�gen 2D (jpg, png, gif, etc.) y puede ser editada con cualquier editor
			//normal (photoshop, paint, descargada de goole images, etc).
			//El framework viene con un mont�n de texturas incluidas y organizadas en categorias (texturas de
			//madera, cemento, ladrillo, pasto, etc). Se encuentran en la carpeta del framework: Media\MeshCreator\Textures
			//Podemos acceder al path de la carpeta "Media" utilizando la variable "this.MediaDir".
			//Esto evita que tengamos que hardcodear el path de instalaci�n del framework.
			var texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Madera\\cajaMadera3.jpg");

			//Creamos una caja 3D ubicada en (10, 0, 0) y la textura como color.
			center = new TGCVector3(15, 0, 0);
			box2 = TgcBox.fromSize(size, texture);
			box2.Transform = TGCMatrix.Translation(center);

			//Creamos una caja 3D con textura
			center = new TGCVector3(-15, 0, 0);
			texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Metal\\cajaMetal.jpg");
			box3 = TgcBox.fromSize(center, size, texture);
			box3.AutoTransformEnable = true;

			//Ubicar la camara del framework mirando al centro de este objeto.
			//La camara por default del framework es RotCamera, cuyo comportamiento es
			//centrarse sobre un objeto y permitir rotar y hacer zoom con el mouse.
			//Con clic izquierdo del mouse se rota la c�mara, con el derecho se traslada y con la rueda se hace zoom.
			//Otras c�maras disponibles (a modo de ejemplo) son: FpsCamera (1ra persona) y ThirdPersonCamera (3ra persona).
			Camara = new TgcRotationalCamera(box1.BoundingBox.calculateBoxCenter(),
                box1.BoundingBox.calculateBoxRadius() * 5, Input);
        }

        public override void Update()
        {
            PreUpdate();

			//En cada cuadro de render rotamos la caja con cierta velocidad (en radianes)
			//Siempre tenemos que multiplicar las velocidades por el elapsedTime.
			//De esta forma la velocidad de rotacion es independiente de la potencia del CPU.
			//Sino en computadoras con CPU m�s r�pido la caja girar�a mas r�pido que en computadoras mas lentas.
			box3.rotateY(ROTATION_SPEED * ElapsedTime);

			//Aplicamos una traslaci�n en Y. Hacemos que la caja se mueva en forma intermitente en el intervalo [0, 3] de Y. 
			//Cuando llega a uno de los l�mites del intervalo invertimos la direcci�n del movimiento.
			//Tambien tenemos que multiplicar la velocidad por el elapsedTime
			box3.move(0, MOVEMENT_SPEED * currentMoveDir * ElapsedTime, 0);
			if (FastMath.Abs(box3.Position.Y) > 3f)
			{
				currentMoveDir *= -1;
			}
		}

        /// <summary>
        ///     M�todo que se invoca todo el tiempo. Es el render-loop de una aplicaci�n gr�fica.
        ///     En este m�todo se deben dibujar todos los objetos que se desean mostrar.
        ///     Antes de llamar a este m�todo el framework limpia toda la pantalla.
        ///     Por lo tanto para que un objeto se vea hay volver a dibujarlo siempre.
        ///     La variable elapsedTime indica la cantidad de segundos que pasaron entre esta invocaci�n
        ///     y la anterior de render(). Es �til para animar e interpolar valores.
        /// </summary>
        public override void Render()
        {
            //Iniciamoss la escena
            PreRender();

            //Dibujar las cajas en pantalla
            box1.render();
			box2.render();
			box3.render();

            //Finalizamos el renderizado de la escena
            PostRender();
        }

        /// <summary>
        ///     M�todo que se invoca una sola vez al finalizar el ejemplo.
        ///     Se debe liberar la memoria de todos los recursos utilizados.
        /// </summary>
        public override void Dispose()
        {
            //Liberar memoria de las cajas 3D.
            //Por mas que estamos en C# con Garbage Collector igual hay que liberar la memoria de los recursos gr�ficos.
            //Porque est�n utilizando memoria de la placa de video (y ah� no hay Garbage Collector).
            box1.dispose();
			box2.dispose();
			box3.dispose();
        }
    }
}