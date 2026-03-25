using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
public class CrossHair : MonoBehaviour
{
  
    [SerializeField] GameObject[] crossHair = new GameObject[2];
    [SerializeField] private float rayDistance;
    private ObjectScript objectScript;
    private bool isLookObject = false;


    private void Update()
    {

        if (Camera.main == null || UIManager.instance == null) return;

        if (!isLookObject)
            UIManager.instance.OnOffDiscription(false);


        ChangeCrossHair();
        OnOff_Interaction();
        DetectObject();

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * rayDistance, Color.red);

    }


    public void DetectObject()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            objectScript = hit.transform.GetComponent<ObjectScript>();
            if (objectScript != null)
            {
                ObjectData data = objectScript.GetData();
                if (data == null)
                {
                    UIManager.instance.OnOffDiscription(false);
                    return;
                }
                isLookObject = true;

                //디버깅용 디스크립션 패널
                UIManager.instance.OnOffDiscription(true);
                UIManager.instance.SetObjectDiscription(
                    data.name,
                    data.properties.staticProperty.ToString(),
                    data.properties.dynamicProperty.ToString()
                );
                return;
            }
        }

        isLookObject = false;

    }

    public void OnOff_Interaction()
    {
        if (isLookObject)
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

        crossHair[0].gameObject.SetActive(!isLookObject);
        crossHair[1].gameObject.SetActive(isLookObject);

    }
    public ObjectScript GetObjectScript()
    {
        return objectScript;
    }

}
