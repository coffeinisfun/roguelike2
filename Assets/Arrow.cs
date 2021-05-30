using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

	private BoxCollider2D boxCollider;
	public bool stop = false;
	public List<(int,int)> arrowPath;

    // Start is called before the first frame update
    void Start()
    {
			Debug.Log("arrow start");
			boxCollider = GetComponent <BoxCollider2D> ();
			StartCoroutine(Travel(arrowPath));
    }

    // Update is called once per frame
    void Update()
    {

    }

		void OnTriggerEnter2D(Collider2D col){
			if(col.gameObject.layer == 8 && col.gameObject.tag != "Player"){
				stop = true;
				if(col.gameObject.tag == "Enemy"){
					Enemy hitEnemy = col.gameObject.GetComponent<Enemy>();
					hitEnemy.health--;
					print(hitEnemy.health);
				}
				Destroy(gameObject);

			}
		}

		IEnumerator Travel(List<(int,int)> path){
			path.RemoveAt(0);
			foreach((int,int) point  in path){
				if (stop) break;
				yield return new WaitForSeconds(0.1f);
				Vector3 pos = new Vector3(point.Item1,point.Item2,0);
				transform.position = pos;
			}
			stop = true;
			Debug.Log(stop);
		}

		//Bresenham for digitalize way of arrow
		private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

		public List<(int,int)> BresenhamLine((int,int) start, (int,int) end)	{

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
				Swap<int>(ref x1, ref y1);
				Swap<int>(ref x2, ref y2);
			}
			//Swap start and end points if necessary and store swap state
			bool swapped = false;
			if (x1 >x2){
				Swap<int>(ref x1, ref x2);
				Swap<int>(ref y1, ref y2);
				swapped = true;
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
