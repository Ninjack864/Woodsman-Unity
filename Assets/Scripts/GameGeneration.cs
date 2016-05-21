﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TerEdge;
using LibNoise.Unity;
using LibNoise.Unity.Generator;

public class GameGeneration : MonoBehaviour {

	private int TREES_PER_UNIT = 10;

	private Terrain terrain;

	private float HEIGHT = 0.15f;
	private float ALPHA = 0.17f;
	private int SEED = 1;
	private float FREQ = 1.7f;
	private int LACUNARITY = 3;
	private float PERSISTENCE = 0.5f;
	private int OCTAVES = 3;

	void Start() {
		Unit treeUnit = new Unit((GameObject)Resources.Load("Tree"), new Vector2(10, 10), TREES_PER_UNIT);
		Unit outsideTreeUnit = new Unit((GameObject)Resources.Load("Tree"), new Vector2(10, 10), 3);

		GameObject treeParent = new GameObject();
		treeParent.name = "Tree Parent";
		treeParent.tag = "TreeParent";
		GameObject bgTrees = new GameObject();
		bgTrees.name = "Background Tree Parent";
		bgTrees.tag = "BGTrees";
		GenerateTerrain();
		SpawnUnitInRange(treeUnit, new Vector2(0,0), new Vector2(100,100), "TreeParent");
		SpawnUnitInRange(outsideTreeUnit, new Vector2(-100,-100), new Vector2(0,200), "BGTrees");
		SpawnUnitInRange(outsideTreeUnit, new Vector2(0,-100), new Vector2(100,0), "BGTrees");
		SpawnUnitInRange(outsideTreeUnit, new Vector2(0, 100), new Vector2(100, 200), "BGTrees");
		SpawnUnitInRange(outsideTreeUnit, new Vector2(100,-100), new Vector2(200,200), "BGTrees");

		Instantiate(Resources.Load("Cloud"));
		Instantiate(Resources.Load("Cloud"));
		Instantiate(Resources.Load("Cloud"));

		gameObject.SetActive(false);
	}

	public void SpawnUnitInRange(Unit unit, Vector2 startPoint, Vector2 endPoint, string tag = "Untagged") {
		for (float xx = startPoint.x; xx < endPoint.x; xx += unit.getSize().x) {
			
			for (float yy = startPoint.y; yy < endPoint.y; yy += unit.getSize().y) {
				GameObject treeUnitParent = (GameObject) Instantiate(Resources.Load("UnitParent"));

				treeUnitParent.name = "Unit_" + ((int)xx).ToString("000") + "_" + ((int)yy).ToString("000");
				treeUnitParent.transform.position = new Vector3(xx, 0f, yy);
				treeUnitParent.GetComponent<UnitParent>().unit = unit;

				foreach (Vector2 pos in UnitScatter(unit, new Vector2(xx,yy))) {
					
					GameObject tree = (GameObject) Instantiate(unit.getObj(),
						new Vector3(pos.x,
							terrain.SampleHeight(new Vector3(pos.x, 0f, pos.y)) - 0.1f,
							pos.y),
						Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f))
					);

					if (tag != "Untagged") {
						tree.transform.parent = treeUnitParent.transform;
					}
					treeUnitParent.GetComponent<UnitParent>().AddChild(tree);
				}
				treeUnitParent.transform.parent = GameObject.FindGameObjectWithTag(tag).transform;
			}
		}
	}

	private void GenerateTerrain() {
		TerrainData tdata = new TerrainData();
		tdata.size = new Vector3(400, 600, 400);

		SplatPrototype[] textures = new SplatPrototype[1];
		textures[0] = new SplatPrototype();
		textures[0].texture = (Texture2D) Resources.Load("grass");

		tdata.splatPrototypes = textures;

		GameObject terrainObj = Terrain.CreateTerrainGameObject(tdata);
		terrainObj.transform.position = new Vector3(-100, 0, -100);
		terrain = terrainObj.GetComponent<Terrain>();

		SEED = Random.Range(1, 65535);

		ModuleBase mb = new Perlin(FREQ, LACUNARITY, PERSISTENCE, OCTAVES, SEED, QualityMode.High);
		teFunc.generateHeightmap(terrainObj, mb, HEIGHT, ALPHA);

		// TODO-W procedurally apply textures to terrain
	}

	private Vector2[] UnitScatter(Unit unit, Vector2 startPoint) {
		Vector2[] scattered = new Vector2[unit.getAmount()];

		for (int i = 0; i < unit.getAmount(); i++) {
			bool nextXYIsValid = false;
			float xx = 0f;
			float yy = 0f;

			while (!nextXYIsValid) {
				xx = Random.Range(startPoint.x, startPoint.x + unit.getSize().x);
				yy = Random.Range(startPoint.y, startPoint.y + unit.getSize().y);
				nextXYIsValid = true;
				foreach (Vector2 coord in scattered) {
					if (Mathf.Abs(coord.x - xx) <= 0.6f && Mathf.Abs(coord.y - yy) <= 0.6f) {
						nextXYIsValid = false;
					}
				}
			}

			scattered[i] = new Vector2(xx, yy);
		}

		return scattered;
	}
}