using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    //������ ��ũ���� �г�
    [SerializeField] private Image ObjectDiscription;
    private TextMeshProUGUI discription_Name;
    private TextMeshProUGUI discription_StaticProperty;
    private TextMeshProUGUI discription_DynamicProperty;
    //

    [SerializeField] private Image throwSlider;
    private Image slider;

    [SerializeField] private TextMeshProUGUI staticPropertyText;
    [SerializeField] private TextMeshProUGUI dynamicPropertyText;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
      
        slider = throwSlider.transform.GetChild(0).GetComponent<Image>();

        //������ ��ũ���� �г�
        discription_Name = ObjectDiscription.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        discription_StaticProperty = ObjectDiscription.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        discription_DynamicProperty = ObjectDiscription.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }


    //������ ��ũ���� �г�
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

    public void FillSlider(float timer,float time,bool isLift ,bool isLeftClick = true)
    {
     

        if (throwSlider == null || slider == null) Debug.Log("NULL");

        if (!throwSlider.gameObject.activeSelf) OnOff_Slider(true);

        if (isLift)
            slider.color = new Color32(91, 91, 91, 255);
        else
        {
            slider.color = isLeftClick ? new Color32(96, 169, 245, 255) : new Color32(245, 102, 96, 255);
        }
        slider.fillAmount = Mathf.Clamp01(timer / time);
    }


    public void SetPropertyText(ObjectProperties properties)
    {
        staticPropertyText.text = "Current Static : " + properties.staticProperty.ToString();
        dynamicPropertyText.text = "Current Dynamic : " + properties.dynamicProperty.ToString();

    }
  
    public void OnOff_Slider(bool active)
    {
        throwSlider.gameObject.SetActive(active);
    }
/*    public void OnOff_Interaction(bool active)
    {
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = active;

        interation.gameObject.SetActive(active);
    }*/

}
