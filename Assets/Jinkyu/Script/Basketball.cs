using System.Collections;
using Unity.Cinemachine;
using Unity.FPS.Gameplay;
using UnityEngine;

public class Basketball : ObjectBase
{

    PlayerCharacterController controller;
    [SerializeField]
    CinemachineCamera basketballCam;

    bool isConverted = false;


    public override void Start()
    {
        base.Start();
        controller = GameObject.Find("Player").GetComponent<PlayerCharacterController>();
    }
    public override void SetIsLifted(bool on)
    {
        base.SetIsLifted(on);

        if (!isConverted)
        {
            isConverted = true;
            StartCoroutine(ConvertCam());
        }
    }
    public IEnumerator ConvertCam()
    {

        int camPriority = basketballCam.Priority;

    
        basketballCam.Priority += 10;
        controller.canMove = false;
        yield return new WaitForSeconds(2f);
        basketballCam.Priority = camPriority;
        controller.canMove = true;
    }

}
