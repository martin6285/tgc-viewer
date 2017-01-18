using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.PortalRendering
{
    /// <summary>
    ///     Herramienta para optimizar la visibilidad de objetos mediante PortalRendering.
    ///     Determina que objetos se ven y cuales no en base a celdas y portales.
    ///     Esta herramienta implementa una estrategia de Portal Rendering muy b�sica.
    ///     A�n posee muchos aspectos para pulir y optimizar.
    /// </summary>
    public class TgcPortalRenderingManager
    {
        private readonly TgcConvexPolyhedron frustumConvexPolyhedon;

        private readonly TgcScene scene;

        /// <summary>
        ///     Crear TexturesManager
        /// </summary>
        /// <param name="scene">Escenario a administrar</param>
        public TgcPortalRenderingManager(TgcScene scene)
        {
            this.scene = scene;
            Cells = new List<TgcPortalRenderingCell>();
            Portals = new List<TgcPortalRenderingPortal>();
            frustumConvexPolyhedon = new TgcConvexPolyhedron();
        }

        /// <summary>
        ///     Celdas
        /// </summary>
        public List<TgcPortalRenderingCell> Cells { get; }

        /// <summary>
        ///     Portales
        /// </summary>
        public List<TgcPortalRenderingPortal> Portals { get; }

        /// <summary>
        ///     Actualiza la visibilidad de todos los modelos de las celdas.
        ///     Las modelos visibles se cargan como Enable = true, mientras que el
        ///     resto se deshabilita.
        /// </summary>
        /// <param name="cameraPos">Posici�n de la c�mara</param>
        public void updateVisibility(TGCVector3 cameraPos, TgcFrustum frustum)
        {
            //Armar Frustum para uso internor, en base al Frustum actual
            var currentFrustumPlanes = new TGCPlane[6];
            currentFrustumPlanes = new TGCPlane[6];
            currentFrustumPlanes[0] = frustum.NearPlane;
            currentFrustumPlanes[1] = frustum.FarPlane;
            currentFrustumPlanes[2] = frustum.LeftPlane;
            currentFrustumPlanes[3] = frustum.RightPlane;
            currentFrustumPlanes[4] = frustum.BottomPlane;
            currentFrustumPlanes[5] = frustum.TopPlane;

            //Deshabilitar todas las celdas
            foreach (var cell in Cells)
            {
                cell.Visited = false;
                foreach (var connection in cell.Connections)
                {
                    connection.Portal.Visited = false;
                }
            }

            //Buscar la celda actual en la que se encuentra la c�mara
            var currentCell = findCellFromPoint(cameraPos);
            if (currentCell == null)
            {
                return;
            }

            //Recorrer grafo de celdas desde la celda actual
            currentCell.Visited = true;
            traverseCellGraph(cameraPos, currentFrustumPlanes, currentCell);
        }

        /// <summary>
        ///     Renderiza todos los meshes habilitados, y los vuelve a marcar como inhabilitados
        ///     para el pr�ximo cuadro.
        ///     Debe ejecutarse luego de haber llamado a updateVisibility()
        /// </summary>
        public void render()
        {
            foreach (var mesh in scene.Meshes)
            {
                mesh.render();
                mesh.Enabled = false;
            }
        }

        /// <summary>
        ///     Recorrer el grafo de celdas y portales
        /// </summary>
        private void traverseCellGraph(TGCVector3 cameraPos, TGCPlane[] currentFrustumPlanes, TgcPortalRenderingCell cell)
        {
            //Habilitar modelos visibles de esta celda
            findVisibleMeshes(cell, currentFrustumPlanes);

            //Recorrer todas la conexiones de esta celda
            foreach (var connection in cell.Connections)
            {
                //Si el portal ya fue visitado, ignorar
                //TODO: Hay una configuraci�n extrema de celdas y portales que no es tenida en cuenta con este atajo. Analizar en m�s detalle.
                if (connection.Portal.Visited)
                    continue;

                //TODO: quiz�s convendria hacer un test Frustum-BoundingSphere del Portal para descartar m�s r�pido los que no se ven

                //Hacer clipping entre el Frustum y el pol�gono del portal
                var clippedPortalVerts = doPortalClipping(currentFrustumPlanes, connection.Polygon);

                //Si qued� alg�n remanente luego de hacer clipping, avanzar hacia esa celda
                if (clippedPortalVerts != null)
                {
                    //Crear nuevo Frustum recortado por el portal
                    var clippedFrustumPlanes = createFrustumPlanes(cameraPos, currentFrustumPlanes, clippedPortalVerts,
                        connection.Plane);

                    //Avanzar sobre la celda que conecta este portal, utilizando el Frustum recortado
                    connection.NextCell.Visited = true;
                    connection.Portal.Visited = true;
                    traverseCellGraph(cameraPos, clippedFrustumPlanes, connection.NextCell);
                }
            }
        }

        /// <summary>
        ///     Crear un nuevo Frustum acotado usando como base el portal recorado.
        ///     La cantidad de planos del nuevo Frustum no tiene por qu� ser 6.
        ///     Depende de la forma que haya quedado en el portal recortado.
        /// </summary>
        private TGCPlane[] createFrustumPlanes(TGCVector3 cameraPos, TGCPlane[] currentFrustumPlanes, TGCVector3[] portalVerts,
            TGCPlane portalPlane)
        {
            //Hay un plano por cada v�rtice del pol�gono + 2 por el near y far plane
            var frustumPlanes = new TGCPlane[2 + portalVerts.Length];

            //Cargar near y far plane originales
            //TODO: habria que usar el portalPlane para acercar el NearPlane hasta el portal
            frustumPlanes[0] = currentFrustumPlanes[0];
            frustumPlanes[1] = currentFrustumPlanes[1];

            //Generar los planos laterales en base al pol�gono remanente del portal
            //Vamos tomando de a dos puntos del pol�gono + la posici�n de la c�mara y creamos un plano
            var lastP = portalVerts[portalVerts.Length - 1];
            for (var i = 0; i < portalVerts.Length; i++)
            {
                var nextP = portalVerts[i];

                //Armar el plano para que la normal apunte hacia adentro del Frustum
                var a = lastP - cameraPos;
                var b = nextP - cameraPos;
                var plane = TGCPlane.FromPointNormal(cameraPos, TGCVector3.Cross(b, a));

                //Guardar despu�s del near y far plane
                frustumPlanes[i + 2] = plane;

                lastP = nextP;
            }

            return frustumPlanes;
        }

        /// <summary>
        ///     Recorta el portal en base al frustum.
        ///     Este m�todo se realiza haciendo un clipping del Frustum contra la cara del portal.
        ///     El recorte se hace en 3D.
        ///     Existen t�cnicas m�s eficientes para realizar el clipping en 2D, utilizando la proyecci�n del BoundingBox del
        ///     portal.
        ///     Ver Cap�tulo 13 - Portal Rendering, del libro Core Techniques and Algorithms in Game Programming, para optimizar la
        ///     estrategia.
        /// </summary>
        private TGCVector3[] doPortalClipping(TGCPlane[] frustumPlanes, TgcConvexPolygon portalPoly)
        {
            var clippedPortalVerts = portalPoly.BoundingVertices;
            foreach (var plane in frustumPlanes)
            {
                //Clipping con algoritmo de Sutherland-Hodgman
                if (!TgcCollisionUtils.clipConvexPolygon(clippedPortalVerts, plane, out clippedPortalVerts))
                {
                    return null;
                }
            }

            return clippedPortalVerts;
        }

        /// <summary>
        ///     Habilitar los modelos visibles de esta celda, seg�n el Frustum restringido
        /// </summary>
        private void findVisibleMeshes(TgcPortalRenderingCell cell, TGCPlane[] currentFrustumPlanes)
        {
            //El Frustum puede tener m�s de 6 planos, asi que lo tratamos como un cuerpo convexo general.
            frustumConvexPolyhedon.Planes = currentFrustumPlanes;
            foreach (var mesh in cell.Meshes)
            {
                if (mesh.Enabled == false)
                {
                    if (TgcCollisionUtils.classifyConvexPolyhedronAABB(frustumConvexPolyhedon, mesh.BoundingBox) !=
                        TgcCollisionUtils.ConvexPolyhedronResult.OUTSIDE)
                    {
                        mesh.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Busca y devuelve la celda que contenga al punto q.
        ///     Devuelve null en caso de que ninguna celda lo contenga.
        /// </summary>
        /// <param name="q">Punto buscado</param>
        /// <returns>Celda que lo contiene o null</returns>
        public TgcPortalRenderingCell findCellFromPoint(TGCVector3 q)
        {
            foreach (var cell in Cells)
            {
                var c = TgcCollisionUtils.classifyPointConvexPolyhedron(q, cell.ConvexPolyhedron);
                if (c == TgcCollisionUtils.ConvexPolyhedronResult.INSIDE)
                {
                    return cell;
                }
            }

            return null;
        }

        /// <summary>
        ///     Crear meshes de debug para renderizar portales
        /// </summary>
        /// <param name="portalColor">Color de portales</param>
        public void createDebugPortals(Color portalColor)
        {
            foreach (var cell in Cells)
            {
                foreach (var connection in cell.Connections)
                {
                    connection.Polygon.Color = portalColor;
                    connection.Polygon.updateValues();
                }
            }
        }

        /// <summary>
        ///     Renderizar meshes de debug de Portales visitados
        /// </summary>
        public void renderPortals()
        {
            foreach (var cell in Cells)
            {
                foreach (var connection in cell.Connections)
                {
                    if (connection.Portal.Visited)
                    {
                        connection.Polygon.render();
                    }
                }
            }
        }

        /// <summary>
        ///     Libera todos los recursos
        /// </summary>
        public void dispose()
        {
            foreach (var cell in Cells)
            {
                foreach (var connection in cell.Connections)
                {
                    connection.Polygon.dispose();
                }
            }
        }
    }
}