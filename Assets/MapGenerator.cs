
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;




public class MapGenerator : MonoBehaviour
{
	public static int mapWidth = 30;
	public static int mapHeight = 30;
	public int PercentAreWalls = 45;

	public int iterations = 2;

	public GameObject wallPrefab;
	public GameObject floorPrefab;

	public GameObject bearPrefab;
	public GameObject exitPrefab;


	int[,] map;

	List<(int, int)> freespace  = new List<(int, int)>();
	List<(int, int)> maincave = new List<(int, int)>();

	public bool testing = false;

    // Start is called before the first frame update
  void Start()
  {
		generateMap(1);




  }

  // Update is called once per frame
  void Update()
  {

  }

	public void generateMap(int level){
		map = new int[mapWidth, mapHeight];

		if(testing){
			var  fileName = "TestMap";
			string line;
			int i = 0;

			System.IO.StreamReader file =
			    new System.IO.StreamReader(fileName);
			for(i = 0; i<mapWidth;i++)
			{
				line = file.ReadLine();

				int j = 0;
				foreach (char c in line){

					if (c == '1') map[i,j] = 1;
					if (c == '0') map[i,j] = 0;
					j++;
				}


			}

		}

		if (!testing){

			bool done = false;
			int outerIts = 0;
			//break if suitable map generated or 10 iterations
			while(!done && outerIts<10){

				outerIts ++;

				//clear maps
				map = new int[mapWidth, mapHeight];
				freespace  = new List<(int, int)>();
				maincave = new List<(int, int)>();

				//generate cave map
				RandomFillMap();
				for(int i = 0;i<iterations;i++){
					MakeCaverns(false);
				}
				for(int i = 0;i<iterations-2;i++){
					MakeCaverns(true);
				}
				FillOuterWalls();
				printArray("MyRawMap.txt");


				//collect floortiles
				for(int i = 0 ;i<mapWidth;i++){
					for(int j = 0 ;j<mapHeight;j++){
						if(map[i,j] == 0)
							freespace.Add((i, j));
					}
				}

				int its = 0;
				int[,] cavemap;
				do{
					//get random floor tile
					(int, int) tile = freespace[Random.Range(0,freespace.Count)];

					//get Cave for that floor tile
					cavemap = GetRegionTiles(tile.Item1, tile.Item2);

					Debug.Log("tile: " + tile );
					Debug.Log("its: "+ outerIts + " " + its);
					Debug.Log("cave size: "+ maincave.Count);
					Debug.Log("percentage: "+(maincave.Count *1.0f) /(mapWidth*mapHeight)*100  );
					its++;

				}while((maincave.Count *1.0f) /(mapWidth*mapHeight)*100  < ((100-45)*1.0f / 2) && its<=10);

				if(outerIts == 1)continue;
				if(its<=10){
					map = cavemap;
					printArray("MyMap.txt");
					done = true;
				}

			}
			if(!done)
				Debug.Log("didnt found suitable cave");

		}

		//rest freespace, because it still includes caves whihc now no longer exist
		freespace  = new List<(int, int)>();
		//draw map
		for(int i = 0 ;i<mapWidth;i++){
			for(int j = 0 ;j<mapHeight;j++){
				if( map[i,j] == 1){
					 GameObject tile = Instantiate(wallPrefab, new Vector3(i,j,0) ,  Quaternion.identity);
					 GameManager.instance.everything.Add(tile);
					}
				else{
					GameObject tile = Instantiate(floorPrefab, new Vector3(i,j,0) ,  Quaternion.identity);
					GameManager.instance.everything.Add(tile);
					freespace.Add((i, j));
					}
			}
		}

		var spot = (0,0);
		//spawn the player
		spot = freespace[0];
		GameObject player = GameObject.FindGameObjectsWithTag("Player")[0];
		player.transform.position = new Vector3(spot.Item1, spot.Item2, 0);
		freespace.Remove(spot);

		//spawn enemies
		int boarCount = (int)Mathf.Ceil(((float)level/2)) *2;
		int wolfCount = (int)Mathf.Ceil((float)(level-1)/2);
		int bearCount = (int)Mathf.Ceil(((float)level - 4)/2);

		Debug.Log("boarCount: "+boarCount);
		Debug.Log("wolfCount: "+wolfCount);
		Debug.Log("bearCount: "+bearCount);

		for(int i = 0 ;i<boarCount;i++){
			spot = freespace[Random.Range(0,freespace.Count)];
			GameObject boar = Instantiate(bearPrefab,new Vector3(spot.Item1, spot.Item2, 0),  Quaternion.identity);
			GameManager.instance.everything.Add(boar);
			boar.GetComponent<Enemy>().SetSpecies("Boar");
			freespace.Remove(spot);
		}

		for(int i = 0 ;i<bearCount;i++){
			spot = freespace[Random.Range(0,freespace.Count)];
			GameObject bear = Instantiate(bearPrefab,new Vector3(spot.Item1, spot.Item2, 0),  Quaternion.identity);
			GameManager.instance.everything.Add(bear);
			bear.GetComponent<Enemy>().SetSpecies("Bear");
			freespace.Remove(spot);
		}
		for(int i = 0 ;i<wolfCount;i++){
			spot = freespace[Random.Range(0,freespace.Count)];
			GameObject wolf = Instantiate(bearPrefab,new Vector3(spot.Item1, spot.Item2, 0),  Quaternion.identity);
			GameManager.instance.everything.Add(wolf);
			wolf.GetComponent<Enemy>().SetSpecies("Wolf");
			freespace.Remove(spot);
		}


		//spawn exit
		spot = freespace[freespace.Count - 1];
		GameObject exit = Instantiate(exitPrefab,new Vector3(spot.Item1, spot.Item2, 0),  Quaternion.identity);
		GameManager.instance.everything.Add(exit);
		freespace.Remove(spot);

	}

	//initializes Array with zeroes
	void initArray(){
		print("init");
		for (int i=0;i<mapWidth;i++){
			for(int j=0;j<mapHeight;j++){
				map[i,j] = 0;

			}
		}
	}

	void printArray(string fileName){

		var sr = File.CreateText(fileName);


		for (int i=0;i<mapWidth;i++){
			string line = "";
			for(int j=0;j<mapHeight;j++){
				line+=map[i,j];
			}
			sr.WriteLine(line);
		}

		sr.Close();
	}
	//cell automata map
	public void RandomFillMap(){


		for(int column=0, row=0; row< mapHeight; row++){
			for(column = 0; column<mapWidth; column++){
					//keep middle free, cause reasons

						int r = Random.Range(0, 100);
						map[column,row] = r <= PercentAreWalls ? 1:0;


			}
		}
	}

	public void MakeCaverns(bool smoothing){
		for(int column=0, row=0; row <= mapHeight-1; row++)
		{
			for(column = 0; column <= mapWidth-1; column++)
			{
						map[column,row] = PlaceWallLogic(column,row, smoothing);

			}
		}
	}

	public void FillOuterWalls(){
		for(int column=0, row=0; row< mapHeight; row++){
			for(column = 0; column<mapWidth; column++){
				//fill edge
				if(column ==0){
					map[column, row]=1;
				}
				else if(row ==0){
					map[column, row]=1;
				}
				else if(column == mapWidth-1){
					map[column, row]=1;
				}
				else if(row ==mapHeight-1){
					map[column, row]=1;
				}
				//else fill tile randomly

			}
		}
	}

	//cell automata logic
	public int PlaceWallLogic(int x,int y, bool smoothing)
		{
			int numWalls = GetAdjacentWalls(x,y,1,1);
			int numWalls2 = GetAdjacentWalls(x,y,2,2);

			//if wall, die only if less than 2 walls in neighborhood
			if(map[x,y]==1 ){
				if(numWalls<=2){
					return 0;
				}
				else
					return 1;
			}

			//if not wall, resurrect if 5 in neighborhood
			else{
				if(!smoothing)
					if(numWalls>=5  || numWalls2<=1)
					{
						return 1;
					}
				else
					if(numWalls>=5 )
					{
						return 1;
					}

			}

			return 0;
		}

		public int GetAdjacentWalls(int x,int y,int scopeX,int scopeY)
		{
			int startX = x - scopeX;
			int startY = y - scopeY;
			int endX = x + scopeX;
			int endY = y + scopeY;

			int iX = startX;
			int iY = startY;

			int wallCounter = 0;

			for(iY = startY; iY <= endY; iY++) {
				for(iX = startX; iX <= endX; iX++)
				{
					if(!(iX==x && iY==y))
					{
						if(IsWall(iX,iY))
						{
							wallCounter += 1;
						}
					}
				}
			}
			return wallCounter;
		}

		bool IsWall(int x,int y)
	{
		// Consider out-of-bound a wall
		if( IsOutOfBounds(x,y) )
		{
			return true;
		}

		if( map[x,y]==1	 )
		{
			return true;
		}

		if( map[x,y]==0	 )
		{
			return false;
		}
		return false;
	}

	bool IsOutOfBounds(int x, int y)
	{
		if( x<0 || y<0 )
		{
			return true;
		}
		else if( x>mapWidth-1 || y>mapHeight-1 )
		{
			return true;
		}
		return false;
	}

	//attempt on floodfilling
	public void FillingCave(int x, int y){

		if(IsWall(x,y)){

			return;
		}
		if(!maincave.Contains((x,y)) )
			maincave.Add((x,y));

		//repeat with neighbors
		FillingCave(x+1,y);
		FillingCave(x-1,y);
		FillingCave(x,y-1);
		FillingCave(x,y+1);
	}

	//get Encapsulated Rooms
	// List<List<(int,int)>> GetRegions(int tileType){
	// 	List<List<(int,int)>> regions = new List<List<(int,int)>> ();
	//
	// 	//initialize zero array
	// 	int[,] mapFlags = new int[mapWidth, mapHeight];
	//
	// 	for(int x = 0; x<mapWidth; x++){
	// 		for(int y = 0; y<mapHeight; y++){
	// 			if(mapFlags[x,y] == 0 && map[x,y] == tileType){
	// 				List<(int,int)> newRegion = GetRegionTiles(x,y);
	// 				regions.Add(newRegion);
	//
	// 				foreach ((int,int) tile in newRegion){
	// 					mapFlags[tile.Item1, tile.Item2] = 1;
	// 				}
	// 			}
	// 		}
	// 	}
	// 	return regions;
	// }

	//get all tiles for a room
	int[,] GetRegionTiles(int startX, int startY){
		List<(int,int)> tiles = new List<(int,int)> ();
		int[,] mapFlags = new int[mapWidth,mapHeight];
		for(int x  = 0; x<mapWidth; x++){
			for(int y = 0; y<mapHeight; y++){
				mapFlags[x,y] = 1;
			}
		}
		int tileType = map[startX, startY];
		Debug.Log("tileType: "+tileType);

		Queue<(int,int)> queue = new Queue<(int,int)>();
		queue.Enqueue ((startX, startY));
		mapFlags[startX, startY] = 0;

		while(queue.Count > 0){
			(int,int) tile = queue.Dequeue();
			tiles.Add(tile);


			for(int x = tile.Item1 -1 ; x<= tile.Item1+1; x++){
				for(int y = tile.Item2 -1 ; y<= tile.Item2 +1; y++){
					if(IsInMapRange(x,y) && (x == tile.Item1 || y == tile.Item2)){
						if(mapFlags[x,y] == 1 && map[x,y] == tileType){

							mapFlags[x,y] = 0;
							queue.Enqueue((x,y));
						}
					}
				}
			}
		}
		maincave = tiles;
		return mapFlags;
	}

	bool IsInMapRange(int x, int y) {
		return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
	}
}


//end cell automata map
