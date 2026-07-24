using DG.Tweening;
using System.Linq;
using UnityEngine;

public class Door_Tutorial_04 : ObjectBase
{



    private DoorScript.Door door;



    public void TryOpenDoor()
    {
     
            door = GetComponentInParent<DoorScript.Door>();
            if (door != null)
                door.OpenDoor();
        

    }
}
