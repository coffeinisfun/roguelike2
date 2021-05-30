using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager instance = null;

  public List<Enemy> enemies;

  public GameObject lootPrefab;

  public float turnDelay = 0.1f;

  public bool playerTurn = true;
  public bool enemiesMoving = false;

  public bool gameOver = false;
  public bool gameOverMode = false;
  //singleton shit
  void Awake(){
    if (instance == null)
      instance = this;

              else if (instance != this)
                  Destroy(gameObject);
  }
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

    if(gameOver && !gameOverMode){
      gameOverMode = true;
      print("Game Over");
    }
    //remove dead enemies
    for (int i = enemies.Count - 1; i >= 0; i--)
		{
		  if (enemies[i].dead){
  			Enemy enemy = enemies[i];
        Vector3 deathPos = enemy.gameObject.transform.position;
  			enemies.RemoveAt(i);
  			enemy.selfDestruct();
  			print("destroyed");

        //create loot on tile where enemy died
        Instantiate(lootPrefab, deathPos, Quaternion.identity) ;

			}
		}
    //call enemies if not players turn and not already being called
    if(!playerTurn && !enemiesMoving)
    {

      StartCoroutine(callEnemies ());
    }
  }

  IEnumerator callEnemies(){
		enemiesMoving = true;

		yield return new WaitForSeconds(turnDelay);

		foreach(Enemy enemy in enemies){
			enemy.Turn();
		}
		enemiesMoving = false;
		playerTurn = true;
	}
}
