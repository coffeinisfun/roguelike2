
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

	private BoxCollider2D boxCollider;
	private SpriteRenderer spriteRenderer;
	private Transform target;

	public LayerMask blockingLayer;

	public Sprite wolfImage;
	public Sprite boarImage;
	public Sprite bearImage;

	private Sprite sprite;
	private string species;

	public double maxDist = 8.0; //view radius
	public int health = 2; //current Health
	public int attackDmg = 1; //how much damage it dreals per hit
	public int arrowDrop = 1;
	public int potionDrop = 1;

	public bool dead = false;
	public bool isTurn= true;


    // Start is called before the first frame update
    void Start()
    {
				GameManager.instance.enemies.Add(this);
				spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
				Debug.Log("SpriteRenderer: "+spriteRenderer);
				spriteRenderer.sprite = sprite;
				boxCollider = GetComponent <BoxCollider2D> ();
				target = GameObject.FindGameObjectWithTag ("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
				if(health<=0){
					dead=true;
				}
    }

		public void SetSpecies(string name){
			switch (name) {
				case "Bear":
					maxDist = 8;
					health = Random.Range(5, 7);
					attackDmg = 3;
					arrowDrop = health;
					potionDrop = 1;
					sprite = bearImage;
					break;
				case "Boar":
					maxDist = 8;
					health = Random.Range(2, 5);
					attackDmg = 1;
					arrowDrop = health;
					potionDrop = 0;
					sprite = boarImage;
					break;
				case "Wolf":
					maxDist = 10;
					health = Random.Range(1, 4);
					attackDmg = 2;
					arrowDrop = health;
					potionDrop = 0;
					sprite = wolfImage;
					break;
				default:
					maxDist = 8;
					health = 4;
					attackDmg = 1;
					arrowDrop = health;
					potionDrop = 0;
					sprite = boarImage;
					break;
			}
			species = name;
			//spriteRenderer.sprite = sprite;
		}

		public void Turn(){


			//move only every other turn
			// if(!isTurn){
			// 	isTurn = true;
			// 	return;
			// }

			//check distance
			double dist = Mathf.Sqrt( Mathf.Pow((transform.position.x - target.position.x),2)  + Mathf.Pow((transform.position.y - target.position.y),2) );


			//if distance < max
			if(dist<=maxDist){
				//attemptmove
				MoveEnemy();
			}
			isTurn = false;


		}
		void MoveEnemy(){
			int xDir = 0;
								int yDir = 0;

			if(Mathf.Abs(transform.position.x - target.position.x) > Mathf.Abs(transform.position.y - target.position.y)){

				xDir = target.position.x > transform.position.x ? 1 : -1;
				if(xDir == 1) spriteRenderer.flipX = true;
				else spriteRenderer.flipX = false;
				}

			else{
				yDir = target.position.y > transform.position.y ? 1 : -1;
			}

			AttemptMove(xDir, yDir);
		}

		void AttemptMove(int xDir, int yDir){


			boxCollider.enabled = false;
			RaycastHit2D hit = Physics2D.Linecast (transform.position, transform.position + new Vector3(xDir,yDir,0), blockingLayer);
			boxCollider.enabled = true;
			if (hit.transform != null ){

				if(hit.collider.gameObject.tag == "Player"){
					StartCoroutine(PlayerHit(transform.position, new Vector3(xDir,yDir,0)));
					Player hitPlayer = hit.collider.gameObject.GetComponent<Player>();
					hitPlayer.health -= attackDmg;


				}
			}
			else{

				transform.position = transform.position + new Vector3(xDir,yDir,0);
			}
		}

		IEnumerator PlayerHit(Vector3 start, Vector3 moveDir) {

			transform.position = start + 0.1f*moveDir;
			//audioSource.Play();
			yield return new WaitForSeconds(0.1f);
			transform.position = start;
		}

		public void selfDestruct(){
			Destroy(gameObject);
		}
}
