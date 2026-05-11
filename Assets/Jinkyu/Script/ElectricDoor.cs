using UnityEngine;
public class ElectricDoor : ObjectBase
{

    [SerializeField] private bool isOpen;
    private Animator ani;

    private void Start()
    {
        ani = GetComponent<Animator>();
    }

    public override void OnPropertyInjected_Dynamic(DynamicPropertyType property)
    {
        base.OnPropertyInjected_Dynamic(property);

        if (property == DynamicPropertyType.Engine)
        {
            isOpen = true;
        }

    }


    public void TryOpenDoor()
    {
        if ((!isOpen)) return;

        ani.SetTrigger("Open");
            
        
    }
}
