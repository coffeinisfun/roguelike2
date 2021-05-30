using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

	private BoxCollider2D boxCollider;
	private Transform target;

	public LayerMask blockingLayer;

	public double maxDist = 8.0;
	public int health = 2;

	public bool dead = false;
	public bool isTurn= true;
    // Start is called before the first frame update
    void Start()
    {
				GameManager.instance.enemies.Add(this);
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

		public void Turn(){

			
			//move only every other turn
			// if(!isTurn){
			// 	isTurn = true;
			// 	return;
			// }

			//check distance
			double dist = Math.Sqrt( Math.Pow((transform.position.x - target.position.x),2)  + Math.Pow((transform.position.y - target.position.y),2) );


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

			if(Math.Abs(transform.position.x - target.position.x) > Math.Abs(transform.position.y - target.position.y)){

			xDir = target.position.x > transform.position.x ? 1 : -1;
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
					hitPlayer.health --;


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
