﻿using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using Neitri;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyGame.PlanetaryBody;
using System.Drawing;

namespace MyGame
{
	public class ProceduralPlanets : SingletonsPropertyAccesor
	{
		public List<PlanetaryBody.Planet> planets = new List<PlanetaryBody.Planet>();
		readonly SceneSystem scene;

		Camera Cam { get { return scene.MainCamera; } }

		public bool runPlanetLogicInOwnThread = true;

		public ProceduralPlanets(SceneSystem scene)
		{
			this.scene = scene;

			Initialize();

			if (runPlanetLogicInOwnThread)
			{
				var t = new Thread(() =>
				{
					while (true)
					{
						PlanetLogicUpdate();
					}
				});
				t.Name = "planet logic";
				t.Priority = ThreadPriority.Lowest;
				t.IsBackground = true;
				t.Start();
			}

			scene.EventSystem.On<RenderPrepareEnded>(e =>
			{
				if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Set();
				else PlanetLogicUpdate();
			});

			scene.EventSystem.On<FrameEnded>(GPUThreadUpdate);
		}


		public PlanetaryBody.Planet AddPlanet()
		{
			var id = planets.Count + 1;
			var planet = scene.AddEntity("procedural planet #" + id).AddComponent<PlanetaryBody.Planet>();
			planet.ID = id;
			planets.Add(planet);
			return planet;
		}



		void Initialize()
		{

			var biomesAtlas = new BiomesAtlas();

			biomesAtlas.AddBiome(Color.FromArgb(0, 0, 255), "biomes/water"); // blue, 0,0,1
			biomesAtlas.AddBiome(Color.FromArgb(200, 200, 200), "biomes/rock");
			biomesAtlas.AddBiome(Color.FromArgb(128, 128, 128), "biomes/tundra"); // grey,  0.5,0.5,0.5
			biomesAtlas.AddBiome(Color.FromArgb(255, 255, 255), "biomes/snow"); // white, 1,1,1
			biomesAtlas.AddBiome(Color.FromArgb(64, 128, 64), "biomes/tundra2"); // green-vomit,  0.25,0.5,0.25
			biomesAtlas.AddBiome(Color.FromArgb(255, 255, 128), "biomes/sand"); // sand,  1,1,0.5
			biomesAtlas.AddBiome(Color.FromArgb(255, 255, 0), "biomes/sand2"); // yellow, 1,1,0
			biomesAtlas.AddBiome(Color.FromArgb(0, 128, 0), "biomes/grass"); // green, 0,0.5,0

			/*{
				// procedural stars or space dust
				var random = new Random();
				for (int i = 0; i < 1000; i++)
				{
					var e = scene.AddEntity("start dust #" + i);
					var vec = new Vector3d(random.Next(-1.0, 1.0), random.Next(-1.0, 1.0), random.Next(-1.0, 1.0));
					e.Transform.Position = new WorldPos(vec.Normalized() * (600000.0 + random.Next(0, 200000)));
					e.Transform.Scale *= random.Next(0.5f, 1f) * 1000f;
					var r = e.AddComponent<MeshRenderer>();
					r.Mesh = Factory.GetMesh("sphere.obj");
					var m = new MaterialPBR(Factory);
					r.Material = m;
					m.GBufferShader = Factory.GetShader("internal/deferred.gBuffer.PBR.shader");
					m.albedo = new Vector4(10);
				}
			}*/

			/*
            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planet.radius = 150;
            planet.radiusVariation = 7;
            planet.Transform.Position = new Vector3(-2500, 200, 0);
            planet.planetMaterial = planetMaterial;
            planet.Start();
            planets.Add(planet);

            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planet.radius = 100;
            planet.radiusVariation = 5;
            planet.Transform.Position = new Vector3(-2000, 50, 0);
            planet.Start();
            planet.planetMaterial = planetMaterial;
            planets.Add(planet);
            */


			Planet planet1;
			Planet planet2;

			{
				var cfg = new PlanetaryBody.Config();
				cfg.chunkNumberOfVerticesOnEdge = Debug.GetCVar("generation / segment number of vertices on edge", 50);
				cfg.weightNeededToSubdivide = Debug.GetCVar("generation / segment subdivide if weight is bigger than", 0.2f);
				cfg.stopSegmentRecursionAtWorldSize = Debug.GetCVar("generation / segment stop recursion at world size", 100);
				cfg.radiusMin = Debug.GetCVar("generation / planet 1 / radius", 637100); //100000; // 6371000 m is earth radius
				cfg.noiseHeightMultiplier = Debug.GetCVar("generation / planet 1 / noise height multiplier", 9000);
				cfg.noisePositionMultiplier = Debug.GetCVar("generation / planet 1 / noise position multiplier", 0.6371f);
				cfg.seaLevel01 = Debug.GetCVar("generation / planet 1 / sea level 0..1", 0.5f);
				cfg.normalMapDimensions = Debug.GetCVar("generation / normal map dimensions", 128);
				cfg.biomesControlMap = new Texture2D(FileSystem.FindExistingFile("biomes/biomes_control_map.*"))
				{ FilterMode = FilterMode.Point, WrapMode = MyEngine.TextureWrapMode.Clamp, UseMipMaps = false };
				cfg.AddBiomes(biomesAtlas);

				var planetShader = Factory.GetShader("shaders/planet.surface.glsl");
				var planetMaterial = new Material();
				planetMaterial.RenderShader = planetShader;

				var planet = planet1 = AddPlanet();
				planet.Transform.Position = new WorldPos(Program.sunRadius * 5 + cfg.radiusMin * 5, 0, 0);
				planet.SurfaceMaterial = planetMaterial;
				planet.Initialize(cfg);
			}


			{
				var cfg = new PlanetaryBody.Config();
				cfg.chunkNumberOfVerticesOnEdge = Debug.GetCVar("generation / segment number of vertices on edge", 50);
				cfg.weightNeededToSubdivide = Debug.GetCVar("generation / segment subdivide if weight is bigger than", 0.2f);
				cfg.stopSegmentRecursionAtWorldSize = Debug.GetCVar("generation / segment stop recursion at world size", 100);
				cfg.radiusMin = Debug.GetCVar("generation / planet 2 / radius", 100000);
				cfg.noiseHeightMultiplier = Debug.GetCVar("generation / planet 2 / noise height multiplier", 2000);
				cfg.noisePositionMultiplier = Debug.GetCVar("generation / planet 2 / noise position multiplier", 2);
				cfg.seaLevel01 = Debug.GetCVar("generation / planet 2 / sea level 0..1", 0.5f);
				cfg.normalMapDimensions = Debug.GetCVar("generation / normal map dimensions", 128);
				cfg.biomesControlMap = new Texture2D(FileSystem.FindExistingFile("biomes/biomes_control_map.*"))
				{ FilterMode = FilterMode.Point, WrapMode = MyEngine.TextureWrapMode.Clamp, UseMipMaps = false };
				cfg.AddBiomes(biomesAtlas);

				var planetShader = Factory.GetShader("shaders/planet.surface.glsl");
				var planetMaterial = new Material();
				planetMaterial.RenderShader = planetShader;

				var planet = planet2 = AddPlanet();
				planet.Transform.Position = new WorldPos(0, 0, Program.sunRadius * 5 + cfg.radiusMin * 5);
				planet.SurfaceMaterial = planetMaterial;
				planet.Initialize(cfg);
			}



		}

		public PlanetaryBody.Planet GetClosestPlanet(WorldPos pos)
		{
			return planets.OrderBy(p => p.Transform.Position.DistanceSqr(pos) - p.RadiusMin * p.RadiusMin).FirstOrDefault();
		}


		bool FreezeUpdate => scene.Debug.GetCVar("generation / planet segments pause subdivision and visibility updates");
		void PlanetLogicUpdate()
		{
			if (FreezeUpdate) return;

			if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Wait();

			Debug.Tick("generation / planet logic");

			var camPos = Cam.Transform.Position;

			if (Debug.GetCVar("generation / debug / regenerate all planets").EatBoolIfTrue())
				foreach (var p in planets)
					p.MarkForRegeneration();


			foreach (var p in planets)
				p.TrySubdivideOver(camPos);

			if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Reset();
		}


		ManualResetEventSlim canRunNextLogicUpdate = new ManualResetEventSlim();


		void GPUThreadUpdate(FrameTimeEvent r)
		{
			foreach (var p in planets)
			{
				p.GPUThreadTick(r.FrameTime);
			}
		}
	}
}