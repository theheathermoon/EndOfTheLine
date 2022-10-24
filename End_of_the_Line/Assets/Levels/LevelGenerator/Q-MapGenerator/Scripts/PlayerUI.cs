///////////////////////////////////
/// Create and edit by QerO
/// 09.2018
/// lidan-357@mail.ru
///////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MapGenerator
{
    public class PlayerUI : MonoBehaviour
    {

        public Text red_keys_text;
        static public int red_keys;
        public Text green_keys_text;
        static public int green_keys;
        public Text blue_keys_text;
        static public int blue_keys;

        public Slider load_bar;
        public Image load_screen;

        static public PlayerUI PlUi;
        private Animator anim;

        void Start()
        {
            PlUi = gameObject.GetComponent<PlayerUI>();
            red_keys = 0;
            green_keys = 0;
            blue_keys = 0;
            Show_Keys_Count();
            anim = gameObject.GetComponent<Animator>();
        }

        public static void Pickup_Key(string color) //This funcion called by "PlayerPickup" script when player pickup key;
        {
            Debug.Log("Received key");
            if (color == "Red")
            {
                Debug.Log("Red");
                red_keys++;
                PlUi.anim.SetTrigger("Red_Key");
            }
            else if (color == "Green")
            {
                Debug.Log("Green");
                green_keys++;
                PlUi.anim.SetTrigger("Green_Key");
            }
            else if (color == "Blue")
            {
                Debug.Log("Blue");
                blue_keys++;
                PlUi.anim.SetTrigger("Blue_Key");
            }
            PlUi.Show_Keys_Count();
        }

        public static void Load_Bar_Max(int slider_max) //This funcion use "MapGenerator" script.
        {
            PlUi.load_bar.maxValue = slider_max;
        }

        public static void Load_Bar_Progress() //This funcion use "MapGenerator" script.
        {
            PlUi.load_bar.value++;
        }

        public static void Player_UI_GameStart() //This funcion use "MapGenerator" script.
        {
            PlUi.load_bar.gameObject.SetActive(false);
            PlUi.load_screen.gameObject.SetActive(false);
        }


        public void Show_Keys_Count()
        {
            red_keys_text.text = ("Red keys : " + red_keys);
            green_keys_text.text = ("Green keys : " + green_keys);
            blue_keys_text.text = ("Blue keys : " + blue_keys);
        }

    }
}
