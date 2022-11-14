///////////////////////////////////
/// Create and edit by QerO
/// 09.2018
/// lidan-357@mail.ru
///////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenerator
{
    public class PlayerController : MonoBehaviour
    {

        //Settings
        public float player_setup_move_walk_speed;
        public float player_setup_move_run_speed;
        public float player_setup_move_crouch_speed;
        public float player_setup_move_left_and_right_speed_mod;
        public float player_setup_crouch_speed;
        public float player_setup_crouch_height_modifier;
        public float player_setup_ground_forse;
        public float player_setup_stamina;
        public float player_setup_mouse_sensitivity;
        public float player_setup_mouse_head_min_x;
        public float player_setup_mouse_head_max_x;
        public GameObject player_setup_head;
        //	public GameObject player_setup_body;

        //Private vars
        private float player_current_speed;
        private Vector2 player_input;
        private float player_input_horizontal;
        private float player_input_vertical;
        private Vector3 player_input_move;
        private Vector3 player_move;
        private CharacterController player_chr_cntr;
        private float player_camera_x;
        private float player_camera_y;
        public Vector3 player_move_rotation;
        public Vector3 player_camera_rotation;
        private float player_setup_crouch_height;
        private float player_setup_height;
        private bool player_crouch_mode;
        private bool player_run_mode;



        void Start()
        {
            player_chr_cntr = GetComponent<CharacterController>();
            player_current_speed = player_setup_move_walk_speed;

            player_setup_crouch_height = player_chr_cntr.height * player_setup_crouch_height_modifier;
            player_setup_height = player_chr_cntr.height;
            player_crouch_mode = false;
            player_run_mode = false;
        }

        void Update()
        {
            Player_Crouch_and_Run();
        }

        void FixedUpdate()
        {

            Get_Input();
            player_input_move = transform.forward * player_input.y + transform.right * player_setup_move_left_and_right_speed_mod * player_input.x;

            if (player_crouch_mode == true)
            {
                player_current_speed = player_setup_move_crouch_speed;
            }
            else if (player_run_mode == true)
            {
                player_current_speed = player_setup_move_run_speed;
            }
            else
            {
                player_current_speed = player_setup_move_walk_speed;
            }

            player_move.x = player_input_move.x * player_current_speed;
            player_move.z = player_input_move.z * player_current_speed;
            player_move.y = -player_setup_ground_forse;

            player_chr_cntr.Move(player_move * Time.fixedDeltaTime);

            player_camera_x = Input.GetAxis("Mouse X") * player_setup_mouse_sensitivity;
            player_camera_y = Input.GetAxis("Mouse Y") * player_setup_mouse_sensitivity;
            player_move_rotation += new Vector3(0, player_camera_x, 0);
            player_camera_rotation += new Vector3(-player_camera_y, 0, 0);
            player_camera_rotation.x = Mathf.Clamp(player_camera_rotation.x, player_setup_mouse_head_min_x, player_setup_mouse_head_max_x);
            gameObject.transform.localEulerAngles = player_move_rotation;
            player_setup_head.transform.localEulerAngles = player_camera_rotation;
        }

        void Get_Input()
        {
            player_input_horizontal = Input.GetAxis("Horizontal");
            player_input_vertical = Input.GetAxis("Vertical");

            player_input = new Vector2(player_input_horizontal, player_input_vertical);

            if (player_input.sqrMagnitude > 1)
            {
                player_input.Normalize();
            }
        }

        void Player_Crouch_and_Run()
        {
            if (Input.GetButton("Crouch") && player_chr_cntr.height > player_setup_crouch_height)
            {
                Player_Crouch(player_setup_crouch_speed * -1);
                player_crouch_mode = true;
                player_run_mode = false;
            }
            else if (!(Input.GetButton("Crouch")) && player_chr_cntr.height < player_setup_height)
            {
                Player_Crouch(player_setup_crouch_speed);
                player_crouch_mode = false;
            }
            else if (Input.GetButton("Run"))
            {
                player_run_mode = true;
            }
            else
            {
                player_run_mode = false;
            }
        }

        void Player_Crouch(float crouch_mod)
        {
            if (crouch_mod > 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + crouch_mod * Time.fixedDeltaTime, transform.position.z);
            }
            player_chr_cntr.height += crouch_mod * Time.fixedDeltaTime;
     		//player_setup_body.transform.localPosition = new Vector3 (0, player_setup_body.transform.localPosition.y + crouch_mod * Time.fixedDeltaTime * 0.5f);
            player_setup_head.transform.localPosition = new Vector3(0, player_setup_head.transform.localPosition.y + crouch_mod * Time.fixedDeltaTime * 0.5f);
        }
    }
}
