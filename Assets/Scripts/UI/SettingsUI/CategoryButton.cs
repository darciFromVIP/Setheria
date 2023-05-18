using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CategoryButton : MonoBehaviour
{
    public GameObject window;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(CategoryClicked);
    }

    private void CategoryClicked()
    {
        GetComponentInParent<WindowWithCategories>().OpenAnotherWindow(window);
    }
}
