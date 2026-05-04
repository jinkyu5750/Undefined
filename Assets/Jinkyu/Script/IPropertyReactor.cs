using UnityEngine;

public interface IPropertyReactor
{

    void OnPropertyInjected_Static(StaticPropertyType property);

    void OnPropertyInjected_Dynamic(DynamicPropertyType property);
}
