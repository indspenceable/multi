//C# Example

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

public class MapLoader: EditorWindow {
	[Serializable]
	public class JsonMapLayer {
		public int[] data;
		public int[] objects;
		public string name;
		public int width;
		public int height;
	}
	[Serializable]
	public class JsonMapData {
		public JsonMapLayer[] layers;
	}

	[MenuItem ("Window/Map Loader")]

	public static void  ShowWindow () {
		EditorWindow.GetWindow(typeof(MapLoader));
	}

	public int width = 25;
	public int height = 20;
	public GameObject tile = null;
//	public Sprite[] tiles;

	void OnGUI () {
		width = EditorGUILayout.IntField("Width:", width);
		height = EditorGUILayout.IntField("Height:",height);
		tile = EditorGUILayout.ObjectField("Tile", tile, typeof(GameObject), false) as GameObject;
//		tiles = EditorGUILayout.PropertyField(tiles);

		if (GUILayout.Button("Load map")) {
//			string path = EditorUtility.OpenFilePanel("","","json");
			string path = "/Users/dspencer/dev/mv/Assets/level_maps/campsite.json";
			if (path == null) return;
			string fileContents = System.IO.File.ReadAllText(path);
			createGameObject(
				JsonUtility.FromJson<JsonMapData>(fileContents)
			);
		}
	}

	// True if OK, false otherwise
	private bool checkDimensions(int totalWidth, int totalHeight) {
		bool ok = true;
		if (totalWidth == -1) {
			Debug.LogError("No layer named \"Layout\". Aborting.");
			ok = false;
		} else if(tile == null) {
			Debug.LogError("You must select a tile prefab. Aborting.");
		} else {
			if ((totalWidth % (width-1) != 1) || totalWidth < width) {
				Debug.LogError("Width (" + totalWidth +") doesn't neatly fit into rooms of wsize (" + width + "). Aborting.");
				ok = false;
			}
			if ((totalHeight % (height-1) != 1) || totalHeight < height) {
				Debug.LogError("Height (" + totalHeight +") doesn't neatly fit into rooms of hsize (" + height + "). Aborting.");
				ok = false;
			}
		}
		return ok;
	}

	private void createGameObject(JsonMapData mapData) {
		// Check the widths and such.
		// Assume a well-formed mapData
		int totalWidth = -1;
		int totalHeight = -1;
		for (int i = 0; i < mapData.layers.Length; i+= 1) {
			if (mapData.layers[i].name == "Layout") {
//				Debug.Log("Found layout");
//				Debug.Log(mapData.layers[i].width);
				totalWidth = mapData.layers[i].width;
				totalHeight = mapData.layers[i].height;
				// end the loop lol.
				i = 9999;
			}
		}
		if (!checkDimensions(totalWidth, totalHeight)) {
			return;
		}

		EditorCoroutines.start(buildArea(mapData, totalWidth, totalHeight));
	}

//	Dictionary<int, Dictionary<int, GameObject>> coordsToRooms;
//	Dictionary<int, Dictionary<int, GameObject>> coordsToPrefabs;

	IEnumerator buildArea(JsonMapData mapData, int totalWidth, int totalHeight) {
//		GameObject areaGameObject = new GameObject("Area");
		int numberOfRoomsAcross = (totalWidth - 1) / (width-1);
		int numberOfRoomsTall = (totalHeight - 1) / (height-1);
		Dictionary<int, Dictionary<int, GameObject>> coordsToRooms = new Dictionary<int, Dictionary<int, GameObject>>();
		Dictionary<int, Dictionary<int, GameObject>> coordsToPrefabs = new Dictionary<int, Dictionary<int, GameObject>>();

		// For each Room + each layer. Create room + place tiles + make exits
		for (int roomX = 0; roomX < numberOfRoomsAcross; roomX+=1) {
			for (int roomY = 0; roomY < numberOfRoomsTall; roomY+=1) {
				GameObject roomGameObject = new GameObject("Room (" + roomX + ", " + roomY + ")");
				GameObject tilesContainer = new GameObject("Tiles");
				tilesContainer.transform.parent = roomGameObject.transform;

				for (int currentLayerIndex = 0; currentLayerIndex < mapData.layers.Length; currentLayerIndex+=1) {
					JsonMapLayer currentLayer = mapData.layers[currentLayerIndex];
					if (currentLayer.name == "Objects") {
						continue;
					}
//					Debug.Log("currentLayer.data is: " + currentLayer.name);
					int currentLayerZ = 0;

					for (int x = 0; x < width; x+=1) {
						for (int y = 0; y < height; y+=1) {
//							Debug.Log("Rx:" + roomX + ", Ry:" + roomY + ", x:" + x + ", y:" + y);
							int correctlyOffsetX = x + (roomX * (width-1));
							int correctlyOffsetY = y + (roomY * (height-1));
							int tileLocation = (correctlyOffsetY * totalWidth) + correctlyOffsetX;
//							Debug.Log("CX: " + correctlyOffsetX + ", CY: " +correctlyOffsetY + ", final: " + tileLocation);
//							Debug.Log("currentLayer.data has: " + currentLayer.data.Length);
							int tileData = currentLayer.data[tileLocation];
							if (tileData == 0) {
							} else {
								GameObject currentTile = GameObject.Instantiate(tile) as GameObject;
//								currentTile.GetComponent<SpriteRenderer>().sprite = tiles[tileData];
								currentTile.transform.parent = tilesContainer.transform;
								currentTile.transform.localPosition = new Vector3(x-((width-1)/2f), -y+((height-1)/2f), currentLayerZ);
							}
						}
					}
					yield return null;
				}
				if (!coordsToRooms.ContainsKey(roomX))
					coordsToRooms.Add(roomX, new Dictionary<int, GameObject>());
				coordsToRooms[roomX][roomY] = roomGameObject;
			}
		}

//		yield return EditorCoroutines.start(
		for (int roomX = 0; roomX < numberOfRoomsAcross; roomX+=1) {
			for (int roomY = 0; roomY < numberOfRoomsTall; roomY+=1) {
				GameObject roomGameObject = coordsToRooms[roomX][roomY];
				string prefabPath = "Assets/Resources/Prefabs/Room" + roomX + "_" + roomY + ".prefab";
				GameObject currentPrefab = PrefabUtility.CreatePrefab(prefabPath, roomGameObject, ReplacePrefabOptions.ConnectToPrefab) as GameObject;
				//				currentPrefab = PrefabUtility.ReplacePrefab(roomGameObject, currentPrefab);
//				Debug.Log(currentPrefab);

				if (!coordsToPrefabs.ContainsKey(roomX))
					coordsToPrefabs.Add(roomX, new Dictionary<int, GameObject>());
				coordsToPrefabs[roomX][roomY] = currentPrefab;
				yield return null;
			}
		}//		);
//		yield return EditorCoroutines.start(
		Debug.Log("Create exits.");
		for (int roomX = 0; roomX < numberOfRoomsAcross; roomX+=1) {
			for (int roomY = 0; roomY < numberOfRoomsTall; roomY+=1) {

				GameObject roomGameObject = coordsToRooms[roomX][roomY];
				GameObject roomPrefab = coordsToPrefabs[roomX][roomY];
				GameObject exitsContainer = new GameObject("Exits");
				exitsContainer.transform.parent = roomGameObject.transform;

//				Debug.Log(roomPrefab);
				//				Debug.Log(coordsToPrefabs[roomX+1][roomY]);
//				Debug.Log("-----");

				if (roomX < numberOfRoomsAcross-1) {
					GameObject rightExit = new GameObject("Right Exit");
					rightExit.transform.parent = exitsContainer.transform;
					rightExit.transform.localPosition = new Vector3((width+1)/2f, 0);
					rightExit.transform.localScale = new Vector3(1, height, 1);

					RoomExit re = rightExit.AddComponent<RoomExit>();
					re.destination = coordsToPrefabs[roomX+1][roomY];
					re.offset = new Vector3(width, 0);
					re.time = 0.75f;

					rightExit.AddComponent<BoxCollider2D>();
					rightExit.layer = LayerMask.NameToLayer("Exits");
				}
				if (roomX > 0) {
					GameObject leftExit = new GameObject("Left Exit");
					leftExit.transform.parent = exitsContainer.transform;
					leftExit.transform.localPosition = new Vector3(-(width+1)/2f, 0);
					leftExit.transform.localScale = new Vector3(1, height, 1);

					RoomExit re = leftExit.AddComponent<RoomExit>();
					re.destination = coordsToPrefabs[roomX-1][roomY];
					re.offset = new Vector3(-width, 0);
					re.time = 0.75f;

					leftExit.AddComponent<BoxCollider2D>();
					leftExit.layer = LayerMask.NameToLayer("Exits");
				}
				if (roomY > 0) {
					GameObject upExit = new GameObject("Up Exit");
					upExit.transform.parent = exitsContainer.transform;
					upExit.transform.localPosition = new Vector3(0, (height+1)/2f);
					upExit.transform.localScale = new Vector3(width, 1, 1);

					RoomExit re = upExit.AddComponent<RoomExit>();
					re.destination = coordsToPrefabs[roomX][roomY-1];
					re.offset = new Vector3(0, height);
					re.time = 0.75f;

					upExit.AddComponent<BoxCollider2D>();
					upExit.layer = LayerMask.NameToLayer("Exits");
				}
				if (roomY < numberOfRoomsTall-1) {
					GameObject downExit = new GameObject("Down Exit");
					downExit.transform.parent = exitsContainer.transform;
					downExit.transform.localPosition = new Vector3(0, -(height+1)/2f);
					downExit.transform.localScale = new Vector3(width, 1, 1);

					RoomExit re = downExit.AddComponent<RoomExit>();
					re.destination = coordsToPrefabs[roomX][roomY+1];
					re.offset = new Vector3(0, -height);
					re.time = 0.75f;

					downExit.AddComponent<BoxCollider2D>();
					downExit.layer = LayerMask.NameToLayer("Exits");
				}
				// TODO danny make up and down exits

				PrefabUtility.ReplacePrefab(roomGameObject, roomPrefab, ReplacePrefabOptions.ConnectToPrefab);
				yield return null;
			}
		}
//		);
		for (int roomX = 0; roomX < numberOfRoomsAcross; roomX+=1) {
			for (int roomY = 0; roomY < numberOfRoomsTall; roomY+=1) {
				GameObject roomGameObject = coordsToRooms[roomX][roomY];
				DestroyImmediate(roomGameObject);
				yield return null;
			}
		}
	}
	void createExits(int numberOfRoomsAcross, int numberOfRoomsTall, Dictionary<int, Dictionary<int, GameObject>> coordsToRooms, Dictionary<int, Dictionary<int, GameObject>> coordsToPrefabs) { 
		
	}

	void createPrefabs(int numberOfRoomsAcross, int numberOfRoomsTall, Dictionary<int, Dictionary<int, GameObject>> coordsToRooms, Dictionary<int, Dictionary<int, GameObject>> coordsToPrefabs) {
		
	}

	void createExits(int numberOfRoomsAcross, int numberOfRoomsTall, Dictionary<int, Dictionary<int, GameObject>> coordsToRooms) {

	}
}
