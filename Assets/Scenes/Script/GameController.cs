using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController _current;
    private void Awake()//use Singleton to ensure be read everywhere
    {
        if(_current == null)
        {
            _current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Event Flip;
}
