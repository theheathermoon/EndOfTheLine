using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

//using TMPro;

public class WeaponWheelButtonController : MonoBehaviour
{
    public int Id;
    private Animator anim;
  //  public string itemName;
   // public TextMeshProGUI itemText;
   //public Image SelectedItem;
    private bool selected = false;
   // public Sprite icon;




    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (selected)
        {
           // SelectedItem.Sprite = icon;
           // itemText.text = itemName;
        }

    }



    public void Selected ()
    {
        selected = true;
    }

    public void Deselected()
    {
        selected = false;
    }


    public void HoverEnter()
    {
        anim.SetBool("Hover", true);
     //   itemText.text = itemName;

    }

    public void HoverExit()
    {
        anim.SetBool("Hover", false);
     //   itemText.text = "";

    }


}
