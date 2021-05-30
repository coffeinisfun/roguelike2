using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Player : MonoBehaviour
{


	public LayerMask blockingLayer;
	public LayerMask floorLayer;



	private BoxCollider2D boxCollider;
	private SpriteRenderer spriteRenderer;

	private bool selectionMode;
	private bool movable;

	public GameObject SelecterPrefab;
	public GameObject ArrowPrefab;
	public float shootForce;

	private GameObject selecter;
	private Vector3 targetPos;


	public float speed = 1f;

	public int arrowCount = 0;
	public int potionCount = 0;

	public int maxHealth = 5;
	public int health;


	private bool steepArrow = false;
	private bool flippedArrow = false;
    // Start is called before the first frame update
    void Start()
    {
				health = maxHealth;
        boxCollider = GetComponent <BoxCollider2D> ();
				spriteRenderer = GetComponent <SpriteRenderer> ();

				selectionMode = false;
				movable = true;
    }

    // Update is called once per frame
    void Update()
    {
		if(health<=0){
			GameManager.instance.gameOver = true;
			movable = false;
			//Destroy(gameObject);
		}

	//while in selection mode
		if(selectionMode){

		Vector3 startSel = selecter.transform.position;
		Vector3 Dir = new Vector3(0,0,0);
		if (Input.GetKeyDown("up"))
		{

			Dir = new Vector3(0,speed,0);
			selecter.transform.position = startSel+Dir;

		}

		if (Input.GetKeyDown("down"))
		{

			Dir = new Vector3(0,-speed,0);
			selecter.transform.position = startSel+Dir;
		}

		if (Input.GetKeyDown("right"))
		{

			Dir = new Vector3(speed,0,0);
			selecter.transform.position = startSel+Dir;

		}
		if (Input.GetKeyDown("left"))
		{

			Dir = new Vector3(-speed,0,0);
			selecter.transform.position = startSel+Dir;


		}
		//select target and shoot, automatically quit mode
		if (Input.GetKeyDown(KeyCode.Return)){
			//can't shoot on place where player is standing
			if(selecter.transform.position == transform.position){
				return;
			}

			arrowCount--;

			targetPos = selecter.transform.position;
			Destroy(selecter);


			selectionMode = false;
			movable =false;


			List<(int,int)> arrowPath = BresenhamLine( ((int)Mathf.Floor(transform.position.x), (int)Mathf.Floor(transform.position.y)), ((int)Mathf.Floor(targetPos.x), (int)Mathf.Floor(targetPos.y)));

			GameObject shot = GameObject.Instantiate(ArrowPrefab, new Vector3 (arrowPath[0].Item1,arrowPath[0].Item2,0), transform.rotation);

			//arrow Rotation
			if(steepArrow){
				//v
				if(flippedArrow) {
					shot.transform.Rotate(new Vector3(0, 0, 90));
					Debug.Log("down");
				}
				//^
				else{
					shot.transform.Rotate(new Vector3(0, 0, -90));
					Debug.Log("up");
				}
			}
			else{
				//<
				if(flippedArrow){
					Debug.Log("left");
					shot.transform.Rotate(new Vector3(0, 0, 0));
					}
				//>
				else{
					Debug.Log("right");
					shot.transform.Rotate(new Vector3(0, 0, 180));
					}
			}

			Arrow arrow = shot.GetComponent<Arrow>();
			arrow.arrowPath = arrowPath;





			StartCoroutine(WaitandTurn0n(arrow));

			return;
		}
		//quit target selection mode
		if(Input.GetKeyDown(KeyCode.T)){
			Destroy(selecter);
			selectionMode = false;
		}

		return;
	}
	Vector3 start = transform.position;
	Vector3 moveDir = new Vector3(0,0,0);
	bool move_attempt = false;


	//enter target selection for shooting
	if(Input.GetKeyDown(KeyCode.T)){
		if(arrowCount == 0){
			print("OOA");
			//GameManager.instance.playerTurn = false;
		}
		else{
			selecter = GameObject.Instantiate(SelecterPrefab, transform.position, transform.rotation);
			selectionMode = true;
		}
	}

	//shoot arrow -TEST-
	if(Input.GetKeyDown(KeyCode.F)){
		GameObject shot = GameObject.Instantiate(ArrowPrefab, transform.position, transform.rotation);
		shot.GetComponent<Rigidbody2D>().AddForce(transform.right * shootForce);
		GameManager.instance.playerTurn = false;
	}

	//drink potion
	if(Input.GetKeyDown(KeyCode.D)){
		if(potionCount <= 0){
			Debug.Log("OO potions");
		}
		else{
			potionCount--;
			health = Mathf.Min(maxHealth, health+1);
			Debug.Log("You drank one potion. You're health is now " + health +".");
		}
	}
	//movement
	if(!movable) return;
        if (Input.GetKeyDown("up"))
        {

					moveDir = new Vector3(0,speed,0);
					move_attempt = true;

        }

        if (Input.GetKeyDown("down"))
        {

					moveDir = new Vector3(0,-speed,0);
					move_attempt = true;

        }

        if (Input.GetKeyDown("right"))
        {

					moveDir = new Vector3(speed,0,0);
					move_attempt = true;
					spriteRenderer.flipX = false;

        }
        if (Input.GetKeyDown("left"))
        {

					moveDir = new Vector3(-speed,0,0);
					move_attempt = true;
					spriteRenderer.flipX = true;


	}
	Vector3 end = start + moveDir;


	//transform.position = end;

	if(move_attempt){
		boxCollider.enabled = false;
		//to-do dual raycast for multiple layers
		RaycastHit2D hit = Physics2D.Linecast (start, end, blockingLayer);

		boxCollider.enabled = true;
		if (hit.transform != null ){

			if (hit.collider.gameObject.tag == "Enemy"){
				StartCoroutine(EnemyHit(start, moveDir));
				Enemy hitEnemy = hit.collider.gameObject.GetComponent<Enemy>();
				hitEnemy.health --;
			}



			if(hit.collider.gameObject.tag == "Exit"){

				Debug.Log("You reached the exit!");
				transform.position = end;
			}

		}
		else{
			hit = Physics2D.Linecast (start, end, floorLayer);
			if (hit.transform != null ){
				if (hit.collider.gameObject.tag == "Loot"){
					print("hit loot");
					Loot loot = hit.collider.gameObject.GetComponent<Loot>();
					arrowCount += loot.arrows;
					potionCount += loot.potions;
					Destroy(hit.collider.gameObject);
					transform.position = end;
				}
				if (hit.collider.gameObject.tag == "Arrow"){
					print("hit arrow");
					arrowCount += 1;
					Destroy(hit.collider.gameObject);
					transform.position = end;
				}


			}
			else transform.position = end;

		}
		GameManager.instance.playerTurn = false;
	}
    }
	//Coroutines
	IEnumerator WaitandTurn0n(Arrow arrow)  {
      yield return new WaitUntil(() => arrow.stop);
			movable = true;
			GameManager.instance.playerTurn = false;
    }
	IEnumerator EnemyHit(Vector3 start, Vector3 moveDir) {

				transform.position = start + 0.1f*moveDir;
				//audiosource.clip = monsterSound;
				//audiosource.Play();
				yield return new WaitForSeconds(0.1f);
				transform.position = start;
		}

		private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

		public List<(int,int)> BresenhamLine((int,int) start, (int,int) end)	{
			steepArrow = false;
			flippedArrow = false;

			List<(int,int)> points = new List<(int,int)>();

			int x1 = start.Item1;
			int y1 = start.Item2;
			int x2 = end.Item1;
			int y2 = end.Item2;

			int dx = x2-x1;
			int dy = y2-y1;

			// Determine how steep the line is
			bool is_steep =(Mathf.Abs(dy) > Mathf.Abs(dx));
			//rotate line
			if (is_steep){
				steepArrow = true;
				Swap<int>(ref x1, ref y1);
				Swap<int>(ref x2, ref y2);
			}
			//Swap start and end points if necessary and store swap state
			bool swapped = false;
			if (x1 >x2){
				Swap<int>(ref x1, ref x2);
				Swap<int>(ref y1, ref y2);
				swapped = true;
				flippedArrow = true;
			}

			//Recalculate differentials
			dx = x2-x1;
			dy = y2-y1;

			// calc error
			int err = (int)(dx/2);
			int ystep = y1 < y2 ? 1 : -1;

			//Iterate over bounding box generating points between start and end
			int y = y1;

			for (int x  = x1 ; x <= x2; x++){
				(int, int) coord = is_steep ? (y,x) : (x,y) ;
				points.Add(coord);
				err -= Mathf.Abs(dy);
				if(err < 0){
					y+=ystep;
					err += dx;
				}
			}
			if(swapped)
				points.Reverse();

			points.RemoveAt(0);
			return points;

		}


}
