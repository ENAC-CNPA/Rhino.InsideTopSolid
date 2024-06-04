using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;


namespace EPFL.GrasshopperTopSolid
{

}

namespace Rhino.DocObjects
{
    static class ViewportInfoExtension
    {
        public static void SetScreenPortFromFrustum(this ViewportInfo vport, double unitsPerInch, double scale = 0.01, int DPI = 72)
        {
            scale *= unitsPerInch;

            int width = (int)Math.Round(vport.FrustumWidth * scale * DPI);
            int height = (int)Math.Round(vport.FrustumHeight * scale * DPI);

            vport.ScreenPort = new System.Drawing.Rectangle(0, 0, Math.Max(1, width), Math.Max(1, height));
        }


        public static bool SetExtents(this ViewportInfo vport, int direction, Geometry.Interval extents)
        {
            if (vport.GetFrustum(out var left, out var right, out var bottom, out var top, out var near, out var far))
            {
                switch (direction)
                {
                    case 0: return vport.SetFrustum(extents.T0, extents.T1, bottom, top, near, far);
                    case 1: return vport.SetFrustum(left, right, extents.T0, extents.T1, near, far);
                    case 2: return vport.SetFrustumNearFar(extents.T0, extents.T1);
                }
            }

            return false;
        }

        public static Geometry.Plane GetCameraFrameAt(this ViewportInfo vport, double depth = 0.0) =>
          new Geometry.Plane(vport.CameraLocation - vport.CameraZ * depth, vport.CameraX, vport.CameraY);

#if !RHINO8_OR_GREATER
        public static double[] GetViewScale(this ViewportInfo vport)
        {
            var scale = vport.ViewScale;
            return new double[] { scale.Width, scale.Height, 1.0 };
        }


#endif

        internal static Geometry.Point3d[] GetFramePlaneCorners(this ViewportInfo vport, double depth, Geometry.Interval width, Geometry.Interval height)
        {
            var plane = GetCameraFrameAt(vport, depth);
            var s = vport.IsPerspectiveProjection ? depth / vport.FrustumNear : 1.0;

            var scale = vport.GetViewScale();
            var x = 1.0 / scale[0];
            var y = 1.0 / scale[1];

            return new Geometry.Point3d[]
            {
        plane.PointAt(s * x * width.T0, s * y * height.T0),
        plane.PointAt(s * x * width.T1, s * y * height.T0),
        plane.PointAt(s * x * width.T0, s * y * height.T1),
        plane.PointAt(s * x * width.T1, s * y * height.T1),
            };
        }

        public static Geometry.Rectangle3d GetFrustumRectangle(this ViewportInfo vport, double depth)
        {
            var width = new Geometry.Interval(vport.FrustumLeft, vport.FrustumRight);
            var height = new Geometry.Interval(vport.FrustumBottom, vport.FrustumTop);
            var s = vport.IsPerspectiveProjection ? depth / vport.FrustumNear : 1.0;

            var scale = vport.GetViewScale();
            var x = 1.0 / scale[0];
            var y = 1.0 / scale[1];

            return new Geometry.Rectangle3d
            (
              vport.GetCameraFrameAt(depth),
              new Geometry.Interval(s * x * width.T0, s * y * width.T1),
              new Geometry.Interval(s * x * height.T0, s * y * height.T1)
            );
        }
    }
}

namespace Rhino.DocObjects.Tables
{
    static class NamedConstructionPlaneTableExtension
    {
        public static int Add(this NamedConstructionPlaneTable table, ConstructionPlane cplane)
        {
            if (table.Document != RhinoDoc.ActiveDoc)
                throw new InvalidOperationException("Invalid Rhino Active Document");

            if (table.Find(cplane.Name) < 0)
            {
                var previous = table.Document.Views.ActiveView.MainViewport.GetConstructionPlane();

                try
                {
                    table.Document.Views.ActiveView.MainViewport.SetConstructionPlane(cplane);
                    //table.Document.Views.ActiveView.MainViewport.PushConstructionPlane(cplane);
                    if (RhinoApp.RunScript($"_-NamedCPlane _Save \"{cplane.Name}\" _Enter", false))
                        return table.Count;
                }
                finally
                {
                    //table.Document.Views.ActiveView.MainViewport.PopConstructionPlane();
                    table.Document.Views.ActiveView.MainViewport.SetConstructionPlane(previous);
                }
            }

            return -1;
        }

        public static bool Modify(this NamedConstructionPlaneTable table, ConstructionPlane cplane, int index, bool quiet)
        {
            if (table.Document != RhinoDoc.ActiveDoc)
                throw new InvalidOperationException("Invalid Rhino Active Document");

            if (index <= table.Count)
            {
                var previous = table.Document.Views.ActiveView.MainViewport.GetConstructionPlane();

                try
                {
                    //table.Document.Views.ActiveView.MainViewport.PushConstructionPlane(cplane);
                    table.Document.Views.ActiveView.MainViewport.SetConstructionPlane(cplane);

                    var current = table[index];
                    if (current.Name != cplane.Name)
                        return RhinoApp.RunScript($"_-NamedCPlane _Rename \"{current.Name}\" \"{cplane.Name}\" _Save \"{cplane.Name}\" _Enter", !quiet);
                    else
                        return RhinoApp.RunScript($"_-NamedCPlane _Save \"{cplane.Name}\" _Enter", !quiet);
                }
                finally
                {
                    //table.Document.Views.ActiveView.MainViewport.PopConstructionPlane();
                    table.Document.Views.ActiveView.MainViewport.SetConstructionPlane(previous);
                }
            }

            return false;
        }
    }
}

namespace Rhino.Display
{
    static class RhinoViewExtension
    {
        public static bool BringToFront(this RhinoView view)
        {
            var viewWindow = (WindowHandle)view.Handle;
            if (!viewWindow.IsZero)
            {
                var topMost = viewWindow;
                while (!topMost.Parent.IsZero)
                {
                    topMost = topMost.Parent;
                    //if (view.Floating) break;
                    if (!viewWindow.Parent.Owner.IsZero) break;
                }

                if (topMost.Visible == false) topMost.Visible = true;
                return topMost.BringToFront();
            }

            return false;
        }

        public static bool SetClientSize(this RhinoView view, System.Drawing.Size clientSize)
        {
            var viewWindow = (WindowHandle)view.Handle;
            if (!viewWindow.IsZero)
            {
                if (view.Floating)
                    viewWindow.Parent.ClientSize = clientSize;
                else
                    viewWindow.ClientSize = clientSize;

                return true;
            }

            return false;
        }
    }

    static class RhinoViewportExtension
    {
        internal static bool SetViewportInfo(this RhinoViewport viewport, DocObjects.ViewportInfo vport)
        {
            var vportInfo = vport;

            if (vport.ScreenPortAspect < RhinoMath.SqrtEpsilon)
            {
                viewport.GetScreenPort(out var left, out var right, out var top, out var bottom, out var _, out var _);
                vportInfo = new DocObjects.ViewportInfo(vport)
                {
                    FrustumAspect = viewport.FrustumAspect,
                    ScreenPort = new System.Drawing.Rectangle(left, top, right - left, top - bottom)
                };
            }

            return viewport.SetViewProjection(vportInfo, !vportInfo.TargetPoint.IsValid);
        }

        public static Geometry.Vector2d PixelsPerUnit(this RhinoViewport viewport, Geometry.Point3d point)
        {
            if (viewport.GetCameraFrame(out var cameraFrame))
            {
                var worldToScrren = viewport.GetTransform(DocObjects.CoordinateSystem.World, DocObjects.CoordinateSystem.Screen);
                var screen = worldToScrren * point;
                return new Geometry.Vector2d
                (
                  Math.Abs(screen.X - (worldToScrren * (point + cameraFrame.XAxis)).X),
                  Math.Abs(screen.Y - (worldToScrren * (point + cameraFrame.YAxis)).Y)
                );
            }

            return new Geometry.Vector2d(1.0, 1.0);
        }

        public static bool Scale(this RhinoViewport viewport, double scaleFactor)
        {
            var scaleTransform = Rhino.Geometry.Transform.Scale(Geometry.Point3d.Origin, scaleFactor);
            var projection = new Rhino.DocObjects.ViewportInfo(viewport);
            projection.TransformCamera(scaleTransform);

            if (projection.IsParallelProjection)
            {
                projection.GetFrustum(out var left, out var right, out var bottom, out var top, out var near, out var far);
                projection.SetFrustum(left * scaleFactor, right * scaleFactor, bottom * scaleFactor, top * scaleFactor, near * scaleFactor, far * scaleFactor);
            }

            if (!viewport.SetViewProjection(projection, updateTargetLocation: true))
                return false;

            var cplane = viewport.GetConstructionPlane();
            cplane.Plane.Transform(scaleTransform);
            cplane.GridSpacing *= scaleFactor;
            cplane.SnapSpacing *= scaleFactor;
            viewport.SetConstructionPlane(cplane);
            return true;
        }
    }
}
