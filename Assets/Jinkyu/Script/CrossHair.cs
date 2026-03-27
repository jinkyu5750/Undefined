using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
public class CrossHair : MonoBehaviour
{

    [SerializeField] GameObject[] crossHair = new GameObject[2];
    PlayerSystem player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerSystem>();
    }

    private void Update()
    {


        if (!player.isLookObject)
            UIManager.instance.OnOffDiscription(false);

        //  OnOff_Interaction();
        ChangeCrossHair();


     //   Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * rayDistance, Color.red);

    }



    public void OnOff_Interaction()
    {
        if (player.isLookObject)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
                UIManager.instance.OnOff_Interaction(true);
            else
                UIManager.instance.OnOff_Interaction(false);

        }
        else
            UIManager.instance.OnOff_Interaction(false);


    }

    public void ChangeCrossHair()
    {

        crossHair[0].gameObject.SetActive(!player.isLookObject);
        crossHair[1].gameObject.SetActive(player.isLookObject);

    }
  



}
