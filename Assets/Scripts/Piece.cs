using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;

    public Piece(Piece p)
    {
        isWhite = p.isWhite;
    }

    public Piece(bool isItWhite)
    {
        isWhite = isItWhite;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
