﻿using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            //string[] args = System.Environment.GetCommandLineArgs();
            using (var engine = new EngineMain())
            {

                var scene = engine.AddScene();

                {

                    var entity = scene.AddEntity();
                    entity.AddComponent<FirstPersonCamera>();

                    var cam = scene.mainCamera = entity.AddComponent<Camera>();
                    //cam.AddPostProcessEffect(Factory.GetShader("postProcessEffects/bloom.shader"));

                    string skyboxName = "skybox/generated/";
                    engine.skyboxCubeMap = Factory.GetCubeMap(new [] {
                        skyboxName + "left.png",
                        skyboxName + "right.png",
                        skyboxName + "top.png",
                        skyboxName + "bottom.png",
                        skyboxName + "front.png",
                        skyboxName + "back.png"
                    });

          
                    entity.Transform.Position = (new Vector3(1, 1, 1)) * 100;
                    entity.Transform.LookAt(new Vector3(0, 0, 100));

                    //engine.camera.entity.AddComponent<SSAO>();
                    
                }



                /*
                {
                    var entity = scene.AddEntity();
                    var mr = entity.AddComponent<MeshRenderer>();
                    mr.Mesh = Factory.GetMesh("cube.obj");
                    entity.Transform.Scale = new Vector3(100, 10, 10);
                }
                */



                var proceduralPlanets = new ProceduralPlanets(scene);


                Entity sunEntity;

                {
                    var entity = sunEntity = scene.AddEntity();
                    entity.Transform.Scale *= 1000;
                    entity.Transform.Position = new Vector3(-2000, -2000, 100);

                    var renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = Factory.GetMesh("sphere.obj");

                    var mat = renderer.Material = new Material();
                    mat.GBufferShader = Factory.GetShader("shaders/sun.glsl");
                    mat.Uniforms.Set("param_colorGradient", Factory.GetTexture2D("textures/fire_gradient.png"));
                    mat.Uniforms.Set("param_turbulenceMap", Factory.GetTexture2D("textures/turbulence_map.png"));                    
                }

                {
                    var entity = scene.AddEntity();
                    entity.Transform.Position = sunEntity.Transform.Position;

                    var light = entity.AddComponent<Light>();
                    light.LighType = LightType.Directional;
                    light.color = Vector3.One * 1f;
                    light.Shadows = LightShadows.None;

                    scene.EventSystem.Register((MyEngine.Events.GraphicsUpdate e) =>
                    {
                        entity.Transform.LookAt(scene.mainCamera.Transform.Position);
                    });
                }

                    //var proceduralSkybox = new ProceduralSpaceSkybox();
                    //engine.skyboxCubeMap = proceduralSkybox.cubemap;

                    engine.Run();

            }
        }
    }   
}
