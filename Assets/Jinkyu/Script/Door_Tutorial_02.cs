using DG.Tweening;
using System.Linq;
using UnityEngine;

public class Door_Tutorial_02 : ObjectBase
{


    [SerializeField] private int password;
    private int password_Input;
    private DoorScript.Door door;



    public void TryOpenDoor()
    {
        if (password_Input == password)
        {
            door = GetComponentInParent<DoorScript.Door>();
            if (door != null)
                door.OpenDoor();
        }

    }
}
