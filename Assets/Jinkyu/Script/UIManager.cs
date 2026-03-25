using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    //디버깅용 디스크립션 패널
    [SerializeField] private Image ObjectDiscription;
    private TextMeshProUGUI discription_Name;
    private TextMeshProUGUI discription_StaticProperty;
    private TextMeshProUGUI discription_DynamicProperty;
    //

    [SerializeField] private Image interation;
    private Image interactionGage;

    [SerializeField] private TextMeshProUGUI staticPropertyText;
    [SerializeField] private TextMeshProUGUI dynamicPropertyText;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
      
        interactionGage = interation.transform.GetChild(1).GetComponent<Image>();

        //디버깅용 디스크립션 패널
        discription_Name = ObjectDiscription.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        discription_StaticProperty = ObjectDiscription.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        discription_DynamicProperty = ObjectDiscription.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }


    //디버깅용 디스크립션 패널
    public void SetObjectDiscription(string name, string staticProperty, string DynamicProperty)
    {
        discription_Name.text = "Name : " + name;
        discription_StaticProperty.text = "Static : " + staticProperty;
        discription_DynamicProperty.text = "Dynamic : " + DynamicProperty;

    }
    public void OnOffDiscription(bool active)
    {
        ObjectDiscription.gameObject.SetActive(active);
    }
    //

    public void FillGageSlider(float timer,float time,bool isLeftClick)
    {
     

        if (interation == null || interactionGage == null) Debug.Log("인터랙션이나 게이지 널");


        if (!interactionGage.gameObject.activeSelf) OnOff_InteractionGage(true);

        interactionGage.color = isLeftClick? new Color32(96, 169, 245,255): new Color32(245,102,96,255);
        interactionGage.fillAmount = Mathf.Clamp01(timer / time);
    }


    public void SetPropertyText(ObjectProperties properties)
    {
        staticPropertyText.text = "Current Static : " + properties.staticProperty.ToString();
        dynamicPropertyText.text = "Current Dynamic : " + properties.dynamicProperty.ToString();

    }
  
    public void OnOff_InteractionGage(bool active)
    {
        interactionGage.gameObject.SetActive(active);
    }
    public void OnOff_Interaction(bool active)
    {
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = active;

        interation.gameObject.SetActive(active);
    }

}
