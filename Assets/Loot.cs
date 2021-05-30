using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    private BoxCollider2D boxCollider;

    public int arrows = 3;
    public int potions = 1;
    // Start is called before the first frame update
    void Start()
    {
      boxCollider = GetComponent <BoxCollider2D> ();
    }

    // Update is called once per frame
    void Update()
    {

    }

}
