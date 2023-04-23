using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TalentTreeCategory : MonoBehaviour
{
    public GameObject window;
    public TalentTreeType treeType;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(CategoryClicked);
    }

    private void CategoryClicked()
    {
        GetComponentInParent<TalentScreen>().OpenAnotherWindow(window, treeType);
    }
}
