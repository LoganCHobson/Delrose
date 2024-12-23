using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ColorDropdown : MonoBehaviour
{
    public TMP_Dropdown colorDropdown; 
    public CustomizationManager character;      

    private readonly Color[] colors = {
        Color.red,    
        Color.blue,   
        Color.green, 
    };

    private void Start()
    {
        colorDropdown.onValueChanged.AddListener(OnColorChanged);
    }

    private void OnColorChanged(int index)
    {
        if (index >= 0 && index < colors.Length)
        {
            character.character.GetComponent<MeshRenderer>().material.color = colors[index];
        }
        character.modelCustomizationData.color = colors[index];
    }
}
