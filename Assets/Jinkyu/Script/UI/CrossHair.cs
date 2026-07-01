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

        ChangeCrossHair();



    }



    public void ChangeCrossHair()
    {

        crossHair[0].gameObject.SetActive(!player.isLookObject);
        crossHair[1].gameObject.SetActive(player.isLookObject);

    }
  



}
