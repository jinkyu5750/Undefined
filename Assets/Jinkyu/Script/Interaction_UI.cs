using UnityEngine;
using UnityEngine.EventSystems;

public class Interaction_UI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    enum Action { Extraction, Injection }

    [SerializeField] private Action action;
    [SerializeField] private float holdTime = 2f;
    private float holdTimer = 0;

    private bool isHolding;
    private bool isLeftClick;

    private PlayerSystem player;
    private ObjectScript objectScript;
    private ObjectData data;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerSystem>();
    }
    private void Update()
    {

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            UIManager.instance.FillGageSlider(holdTimer, holdTime,isLeftClick);

            if (holdTimer >= holdTime)
            {
                switch (action)
                {
                    case Action.Extraction:
                        player.Extraction(data,isLeftClick);                 
                        break;
                    case Action.Injection:
                        objectScript.SetProperties(player.Injection(),isLeftClick);
                        break;

                }

                holdTimer = 0;
                UIManager.instance.OnOff_InteractionGage(false);
                isHolding = false;

            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {

        if(eventData.button == PointerEventData.InputButton.Left)
            isLeftClick = true;
        else if( eventData.button == PointerEventData.InputButton.Right)
            isLeftClick = false;
        

        isHolding = true;

        objectScript = GameObject.Find("Player").GetComponent<PlayerSystem>().GetObjectScript();
        data = objectScript.GetData();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        data = null;
        isHolding = false;
        holdTimer = 0;
        UIManager.instance.OnOff_InteractionGage(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        data = null;
        isHolding = false;
        holdTimer = 0;
        UIManager.instance.OnOff_InteractionGage(false);

    }
}

