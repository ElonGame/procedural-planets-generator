﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyEngine;
using MyEngine.Events;
using MyEngine.Components;

namespace MyGame
{
    public class DrawBounds : MonoBehaviour
    {
        public Transform drawForThisTransform;

        public DrawBounds(Entity entity) : base(entity)
        {
            Entity.EventSystem.Register<GraphicsUpdate>(e => Update(e.DeltaTime));
        }

        public static void ForEntity(Entity entity)
        {
            var bb = entity.Scene.AddEntity();
            var ft = bb.AddComponent<DrawBounds>();
            ft.drawForThisTransform = entity.Transform;
            var mr = bb.AddComponent<MeshRenderer>();
            mr.Mesh = Factory.GetMesh("internal/cube.obj");
        }
        void Update(double deltaTime)
        {
            var r = drawForThisTransform.Entity.GetComponent<MeshRenderer>();
            if (r != null)
            {
                Entity.Transform.Scale = r.Mesh.bounds.Extents;

                Entity.Transform.Rotation = drawForThisTransform.Rotation;
                Entity.Transform.Position = drawForThisTransform.Position + r.Mesh.bounds.center.RotateBy(Entity.Transform.Rotation);                
            }
            else
            {
                Entity.Transform.Position = drawForThisTransform.Position;
                Entity.Transform.Rotation = drawForThisTransform.Rotation;
            }
        }

    }
}