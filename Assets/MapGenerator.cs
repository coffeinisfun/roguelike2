
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;



namespace Generator{
public class MapGenerator : MonoBehaviour
{
	public static int mapWidth = 50;
	public static int mapHeight = 50;
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
				Debug.Log("done parsing");
			}

			if (!testing){
				RandomFillMap();
				for(int i = 0;i<iterations;i++){
					MakeCaverns(false);
				}
				for(int i = 0;i<iterations-2;i++){
					MakeCaverns(true);
				}


				FillOuterWalls();
				printArray();
				Debug.Log("1");

			}

			for(int i = 0 ;i<mapWidth;i++){
				for(int j = 0 ;j<mapHeight;j++){
					if( map[i,j] == 1) Instantiate(wallPrefab, new Vector3(i,j,0) ,  Quaternion.identity);
					else{
						Instantiate(floorPrefab, new Vector3(i,j,0) ,  Quaternion.identity);
						freespace.Add((i, j));
						}
				}
			}
			Debug.Log("3");




			//spawn a bear
			var spot = (0,0);
			int bearCount = 3;
			for(int i = 0 ;i<bearCount;i++){
				spot = freespace[Random.Range(0,freespace.Count)];
				Instantiate(bearPrefab,new Vector3(spot.Item1, spot.Item2, 0),  Quaternion.identity);
				freespace.Remove(spot);
			}

			//spawn the player
			spot = freespace[Random.Range(0,freespace.Count)];
			GameObject player = GameObject.FindGameObjectsWithTag("Player")[0];
			player.transform.position = new Vector3(spot.Item1, spot.Item2, 0);

			spot = freespace[Random.Range(0,freespace.Count)];
			Instantiate(exitPrefab,new Vector3(spot.Item1, spot.Item2, 0),  Quaternion.identity);
			freespace.Remove(spot);

    }

    // Update is called once per frame
    void Update()
    {

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

		void printArray(){
			var  fileName = "MyMap.txt";
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

	public void FillingCave(int x, int y){
		Debug.Log("called FillingCave");
		if(IsWall(x,y)){
			Debug.Log("found Wall at " +x +" "+y );
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

}

}
//end cell automata map
