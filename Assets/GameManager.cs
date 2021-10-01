using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
  public static GameManager instance = null;

  public List<GameObject> everything;
  public List<Enemy> enemies;
  public List<Loot> loots;

  public GameObject lootPrefab;

  private MapGenerator mg;
  private Player player;

  public GameObject gameOverScreen;
  public GameObject levelScreen;
  public Text levelText;
  public Text UIText;

  public float turnDelay = 0.1f;

  public bool playerTurn = true;
  public bool enemiesMoving = false;

  public int level = 1;
  public bool gameOver = false;
  public bool gameOverMode = false;
  public bool levelFinished = false;
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
    mg = gameObject.GetComponent<MapGenerator>();
    player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Player>();
    Debug.Log("MapGenerator: " + mg);
  }

  // Update is called once per frame
  void Update()
  {
    //quit game
    if(Input.GetKey(KeyCode.Escape)){
      Application.Quit();
    }
    //Update HUD
    UIText.text = "HEALTH: "+ player.health +"/"+player.maxHealth+"\n"+
      "ARROWS: "+player.arrowCount+"\n"+
      "POTIONS: "+player.potionCount+"\n";

    //if exit is reached by player, load next level
    if(levelFinished){
      levelFinished = false;
      StartCoroutine(nextLevel());

    }
    if(gameOverMode){
      if(Input.GetKeyDown(KeyCode.R)){
        SceneManager.LoadScene(1);
      }
    }

    //start gameover routine
    if(gameOver && !gameOverMode){
      gameOverMode = true;
      gameOverScreen.SetActive(true);
      print("Game Over");
      return;
    }
    //remove dead enemies
    for (int i = enemies.Count - 1; i >= 0; i--)
		{
		  if (enemies[i].dead){
  			Enemy enemy = enemies[i];
        Vector3 deathPos = enemy.gameObject.transform.position;
        int arrowDrop = enemy.arrowDrop;
        int potionDrop = enemy.potionDrop;
  			enemies.RemoveAt(i);
  			enemy.selfDestruct();
  			print("destroyed");

        //create loot on tile where enemy died
        createLoot((int)deathPos.x, (int)deathPos.y, arrowDrop, potionDrop);

			}
		}
    //call enemies if not players turn and not already being called
    if(!playerTurn && !enemiesMoving)
    {

      StartCoroutine(callEnemies ());
    }
  }

  //create loot on spot
  public void createLoot(int x, int y, int arrows = 0, int potions = 0){
    //check if loot already exists on spot
    foreach (Loot loot in loots){
      if(loot.transform.position.x == x && loot.transform.position.y == y ){
        loot.arrows += arrows;
        loot.potions += potions;
        return;
      }
    }
    //if not create a new one
    GameObject lootObject = Instantiate(lootPrefab, new Vector3(x,y,0), Quaternion.identity) ;
    Loot lootscript = lootObject.GetComponent<Loot>();
    lootscript.arrows = arrows;
    lootscript.potions = potions;
    loots.Add(lootscript);

  }

  //call all enemies and let them make their turm
  IEnumerator callEnemies(){
		enemiesMoving = true;

		yield return new WaitForSeconds(turnDelay);

		foreach(Enemy enemy in enemies){
			enemy.Turn();
		}
		enemiesMoving = false;
		playerTurn = true;
	}

  //load next level
  IEnumerator nextLevel(){
    //destroy everything
    foreach (GameObject go in everything){
      Destroy(go);
    }
    //clear all Lists
    enemies.Clear();
    loots.Clear();
    level ++;

    levelText.text = "LEVEL "+ level;
    levelScreen.SetActive(true);
    mg.generateMap(level);
    yield return new WaitForSeconds(1f);
    levelScreen.SetActive(false);

  }
}
