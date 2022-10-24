using System;
using System.Linq;
using UnityEngine;

namespace ThunderWire.Input
{
    public class CrossPlatformSprites : ScriptableObject
    {
        [Serializable]
        public struct PS4Sprites
        {
            public Sprite PS4Gamepad;
            public Sprite DpadUp;
            public Sprite DpadDown;
            public Sprite DpadLeft;
            public Sprite DpadRight;
            public Sprite Triangle;
            public Sprite Circle;
            public Sprite Cross;
            public Sprite Square;
            public Sprite LeftStick;
            public Sprite LeftStickPress;
            public Sprite RightStick;
            public Sprite RightStickPress;
            public Sprite L1;
            public Sprite R1;
            public Sprite L2;
            public Sprite R2;
            public Sprite Options;
            public Sprite Share;
            public Sprite Touchpad;
        }

        [Serializable]
        public struct XboxOneSprites
        {
            public Sprite XboxGamepad;
            public Sprite DpadUp;
            public Sprite DpadDown;
            public Sprite DpadLeft;
            public Sprite DpadRight;
            public Sprite Y;
            public Sprite B;
            public Sprite A;
            public Sprite X;
            public Sprite LeftStick;
            public Sprite LeftStickPress;
            public Sprite RightStick;
            public Sprite RightStickPress;
            public Sprite LB;
            public Sprite RB;
            public Sprite LT;
            public Sprite RT;
            public Sprite Menu;
            public Sprite ChangeView;
        }

        [Serializable]
        public struct MouseSprites
        {
            public Sprite MouseLeft;
            public Sprite MouseMiddle;
            public Sprite MouseRight;
            public Sprite Mouse;
        }

        [Serializable]
        public struct KeyboardSprites
        {
            public Sprite None;

            //Digits
            public Sprite Key0;
            public Sprite Key1;
            public Sprite Key2;
            public Sprite Key3;
            public Sprite Key4;
            public Sprite Key5;
            public Sprite Key6;
            public Sprite Key7;
            public Sprite Key8;
            public Sprite Key9;

            //Alphabet
            public Sprite A;
            public Sprite B;
            public Sprite C;
            public Sprite D;
            public Sprite E;
            public Sprite F;
            public Sprite G;
            public Sprite H;
            public Sprite I;
            public Sprite J;
            public Sprite K;
            public Sprite L;
            public Sprite M;
            public Sprite N;
            public Sprite O;
            public Sprite P;
            public Sprite Q;
            public Sprite R;
            public Sprite S;
            public Sprite T;
            public Sprite U;
            public Sprite V;
            public Sprite W;
            public Sprite X;
            public Sprite Y;
            public Sprite Z;

            //Arrows
            public Sprite Arrow_UP;
            public Sprite Arrow_DOWN;
            public Sprite Arrow_LEFT;
            public Sprite Arrow_RIGHT;

            //F-Keys
            public Sprite F1;
            public Sprite F2;
            public Sprite F3;
            public Sprite F4;
            public Sprite F5;
            public Sprite F6;
            public Sprite F7;
            public Sprite F8;
            public Sprite F9;
            public Sprite F10;
            public Sprite F11;
            public Sprite F12;

            //Others
            public Sprite Esc;
            public Sprite Tab;
            public Sprite CapsLock;
            public Sprite Shift;
            public Sprite Ctrl;
            public Sprite Alt;
            public Sprite Space;
            public Sprite Enter;
            public Sprite Backspace;
            public Sprite Insert;
            public Sprite Home;
            public Sprite PGUP;
            public Sprite Delete;
            public Sprite End;
            public Sprite PGDN;
            public Sprite Asterisk;
            public Sprite Semicolon;
            public Sprite Bracket_L;
            public Sprite Bracket_R;
            public Sprite Quote;
            public Sprite Comma;
            public Sprite Period;
            public Sprite Backslash;
            public Sprite EqualsKey;
            public Sprite Backquote;
            public Sprite Context;

            //Numpad
            public Sprite NumpadSlash;
            public Sprite NumpadMinus;
            public Sprite NumpadPlus;
            public Sprite NumpadEnter;
            public Sprite NumpadPeriod;
        }

        public PS4Sprites PS4;
        public XboxOneSprites XboxOne;
        public MouseSprites Mouse;
        public KeyboardSprites Keyboard;

        /// <summary>
        /// Get sprite texture for specific binding path.
        /// </summary>
        public Sprite GetSprite(string bindingPath, InputHandler.Device device = InputHandler.Device.None)
        {
            string deviceName = bindingPath.Contains("/") ? bindingPath.Split('/')[0] : string.Empty;

            if (string.IsNullOrEmpty(deviceName) && bindingPath != InputHandler.NULL_INPUT)
                throw new NullReferenceException("The specified binding path is incorrect! The binding path should be in [<Device>/Input] format.\nBinding Path: " + bindingPath);

            if (deviceName.Equals(InputHandler.DUALSHOCK_BINDING))
            {
                return GetDualshockSprite(bindingPath);
            }
            else if (deviceName.Equals(InputHandler.XINPUT_BINDING))
            {
                return GetXInputSprite(bindingPath);
            }
            else if (deviceName.Equals(InputHandler.KEYBOARD_BINDING))
            {
                return GetKeyboardSprite(bindingPath);
            }
            else if (deviceName.Equals(InputHandler.MOUSE_BINDING))
            {
                return GetMouseSprite(bindingPath);
            }
            else if(device != InputHandler.Device.None)
            {
                var pathArray = bindingPath.Split('/').ToList();
                pathArray.RemoveAt(0);
                string path = string.Join("", pathArray);

                if(device == InputHandler.Device.DualshockGamepad)
                {
                    return GetDualshockSprite(InputHandler.DUALSHOCK_BINDING + "/" + path);
                }
                else if(device == InputHandler.Device.XboxGamepad)
                {
                    return GetXInputSprite(InputHandler.XINPUT_BINDING + "/" + path);
                }
            }

            return Keyboard.None;
        }

        public Sprite GetDualshockSprite(string bindingPath)
        {
            switch (bindingPath)
            {
                case InputHandler.DUALSHOCK_BINDING + "/dpad/up":
                    return PS4.DpadUp;
                case InputHandler.DUALSHOCK_BINDING + "/dpad/down":
                    return PS4.DpadDown;
                case InputHandler.DUALSHOCK_BINDING + "/dpad/left":
                    return PS4.DpadLeft;
                case InputHandler.DUALSHOCK_BINDING + "/dpad/right":
                    return PS4.DpadRight;
                case InputHandler.DUALSHOCK_BINDING + "/buttonNorth":
                    return PS4.Triangle;
                case InputHandler.DUALSHOCK_BINDING + "/buttonEast":
                    return PS4.Circle;
                case InputHandler.DUALSHOCK_BINDING + "/buttonSouth":
                    return PS4.Cross;
                case InputHandler.DUALSHOCK_BINDING + "/buttonWest":
                    return PS4.Square;
                case InputHandler.DUALSHOCK_BINDING + "/leftStick":
                    return PS4.LeftStick;
                case InputHandler.DUALSHOCK_BINDING + "/leftStickPress":
                    return PS4.LeftStickPress;
                case InputHandler.DUALSHOCK_BINDING + "/rightStick":
                    return PS4.RightStick;
                case InputHandler.DUALSHOCK_BINDING + "/rightStickPress":
                    return PS4.RightStickPress;
                case InputHandler.DUALSHOCK_BINDING + "/leftShoulder":
                    return PS4.L1;
                case InputHandler.DUALSHOCK_BINDING + "/rightShoulder":
                    return PS4.R1;
                case InputHandler.DUALSHOCK_BINDING + "/leftTrigger":
                    return PS4.L2;
                case InputHandler.DUALSHOCK_BINDING + "/rightTrigger":
                    return PS4.R2;
                case InputHandler.DUALSHOCK_BINDING + "/start":
                    return PS4.Options;
                case InputHandler.DUALSHOCK_BINDING + "/select":
                    return PS4.Share;
                case InputHandler.DUALSHOCK_BINDING + "/touchpadButton":
                    return PS4.Touchpad;
                default:
                    break;
            }

            return Keyboard.None;
        }

        public Sprite GetXInputSprite(string bindingPath)
        {
            switch (bindingPath)
            {
                case InputHandler.XINPUT_BINDING + "/dpad/up":
                    return XboxOne.DpadUp;
                case InputHandler.XINPUT_BINDING + "/dpad/down":
                    return XboxOne.DpadDown;
                case InputHandler.XINPUT_BINDING + "/dpad/left":
                    return XboxOne.DpadLeft;
                case InputHandler.XINPUT_BINDING + "/dpad/right":
                    return XboxOne.DpadRight;
                case InputHandler.XINPUT_BINDING + "/buttonNorth":
                    return XboxOne.Y;
                case InputHandler.XINPUT_BINDING + "/buttonEast":
                    return XboxOne.B;
                case InputHandler.XINPUT_BINDING + "/buttonSouth":
                    return XboxOne.A;
                case InputHandler.XINPUT_BINDING + "/buttonWest":
                    return XboxOne.X;
                case InputHandler.XINPUT_BINDING + "/leftStick":
                    return XboxOne.LeftStick;
                case InputHandler.XINPUT_BINDING + "/leftStickPress":
                    return XboxOne.LeftStickPress;
                case InputHandler.XINPUT_BINDING + "/rightStick":
                    return XboxOne.RightStick;
                case InputHandler.XINPUT_BINDING + "/rightStickPress":
                    return XboxOne.RightStickPress;
                case InputHandler.XINPUT_BINDING + "/leftShoulder":
                    return XboxOne.LB;
                case InputHandler.XINPUT_BINDING + "/rightShoulder":
                    return XboxOne.RB;
                case InputHandler.XINPUT_BINDING + "/leftTrigger":
                    return XboxOne.LT;
                case InputHandler.XINPUT_BINDING + "/rightTrigger":
                    return XboxOne.RT;
                case InputHandler.XINPUT_BINDING + "/start":
                    return XboxOne.Menu;
                case InputHandler.XINPUT_BINDING + "/select":
                    return XboxOne.ChangeView;
                default:
                    break;
            }

            return Keyboard.None;
        }

        public Sprite GetKeyboardSprite(string bindingPath)
        {
            switch (bindingPath)
            {
                case InputHandler.KEYBOARD_BINDING + "/0":
                case InputHandler.KEYBOARD_BINDING + "/numpad0":
                    return Keyboard.Key0;
                case InputHandler.KEYBOARD_BINDING + "/1":
                case InputHandler.KEYBOARD_BINDING + "/numpad1":
                    return Keyboard.Key1;
                case InputHandler.KEYBOARD_BINDING + "/2":
                case InputHandler.KEYBOARD_BINDING + "/numpad2":
                    return Keyboard.Key2;
                case InputHandler.KEYBOARD_BINDING + "/3":
                case InputHandler.KEYBOARD_BINDING + "/numpad3":
                    return Keyboard.Key3;
                case InputHandler.KEYBOARD_BINDING + "/4":
                case InputHandler.KEYBOARD_BINDING + "/numpad4":
                    return Keyboard.Key4;
                case InputHandler.KEYBOARD_BINDING + "/5":
                case InputHandler.KEYBOARD_BINDING + "/numpad5":
                    return Keyboard.Key5;
                case InputHandler.KEYBOARD_BINDING + "/6":
                case InputHandler.KEYBOARD_BINDING + "/numpad6":
                    return Keyboard.Key6;
                case InputHandler.KEYBOARD_BINDING + "/7":
                case InputHandler.KEYBOARD_BINDING + "/numpad7":
                    return Keyboard.Key7;
                case InputHandler.KEYBOARD_BINDING + "/8":
                case InputHandler.KEYBOARD_BINDING + "/numpad8":
                    return Keyboard.Key8;
                case InputHandler.KEYBOARD_BINDING + "/9":
                case InputHandler.KEYBOARD_BINDING + "/numpad9":
                    return Keyboard.Key9;
                case InputHandler.KEYBOARD_BINDING + "/a":
                    return Keyboard.A;
                case InputHandler.KEYBOARD_BINDING + "/b":
                    return Keyboard.B;
                case InputHandler.KEYBOARD_BINDING + "/c":
                    return Keyboard.C;
                case InputHandler.KEYBOARD_BINDING + "/d":
                    return Keyboard.D;
                case InputHandler.KEYBOARD_BINDING + "/e":
                    return Keyboard.E;
                case InputHandler.KEYBOARD_BINDING + "/f":
                    return Keyboard.F;
                case InputHandler.KEYBOARD_BINDING + "/g":
                    return Keyboard.G;
                case InputHandler.KEYBOARD_BINDING + "/h":
                    return Keyboard.H;
                case InputHandler.KEYBOARD_BINDING + "/i":
                    return Keyboard.I;
                case InputHandler.KEYBOARD_BINDING + "/j":
                    return Keyboard.J;
                case InputHandler.KEYBOARD_BINDING + "/k":
                    return Keyboard.K;
                case InputHandler.KEYBOARD_BINDING + "/l":
                    return Keyboard.L;
                case InputHandler.KEYBOARD_BINDING + "/m":
                    return Keyboard.M;
                case InputHandler.KEYBOARD_BINDING + "/n":
                    return Keyboard.N;
                case InputHandler.KEYBOARD_BINDING + "/o":
                    return Keyboard.O;
                case InputHandler.KEYBOARD_BINDING + "/p":
                    return Keyboard.P;
                case InputHandler.KEYBOARD_BINDING + "/q":
                    return Keyboard.Q;
                case InputHandler.KEYBOARD_BINDING + "/r":
                    return Keyboard.R;
                case InputHandler.KEYBOARD_BINDING + "/s":
                    return Keyboard.S;
                case InputHandler.KEYBOARD_BINDING + "/t":
                    return Keyboard.T;
                case InputHandler.KEYBOARD_BINDING + "/u":
                    return Keyboard.U;
                case InputHandler.KEYBOARD_BINDING + "/v":
                    return Keyboard.V;
                case InputHandler.KEYBOARD_BINDING + "/w":
                    return Keyboard.W;
                case InputHandler.KEYBOARD_BINDING + "/x":
                    return Keyboard.X;
                case InputHandler.KEYBOARD_BINDING + "/y":
                    return Keyboard.Y;
                case InputHandler.KEYBOARD_BINDING + "/z":
                    return Keyboard.Z;
                case InputHandler.KEYBOARD_BINDING + "/leftArrow":
                    return Keyboard.Arrow_LEFT;
                case InputHandler.KEYBOARD_BINDING + "/rightArrow":
                    return Keyboard.Arrow_RIGHT;
                case InputHandler.KEYBOARD_BINDING + "/upArrow":
                    return Keyboard.Arrow_UP;
                case InputHandler.KEYBOARD_BINDING + "/downArrow":
                    return Keyboard.Arrow_DOWN;
                case InputHandler.KEYBOARD_BINDING + "/f1":
                    return Keyboard.F1;
                case InputHandler.KEYBOARD_BINDING + "/f2":
                    return Keyboard.F2;
                case InputHandler.KEYBOARD_BINDING + "/f3":
                    return Keyboard.F3;
                case InputHandler.KEYBOARD_BINDING + "/f4":
                    return Keyboard.F4;
                case InputHandler.KEYBOARD_BINDING + "/f5":
                    return Keyboard.F5;
                case InputHandler.KEYBOARD_BINDING + "/f6":
                    return Keyboard.F6;
                case InputHandler.KEYBOARD_BINDING + "/f7":
                    return Keyboard.F7;
                case InputHandler.KEYBOARD_BINDING + "/f8":
                    return Keyboard.F8;
                case InputHandler.KEYBOARD_BINDING + "/f9":
                    return Keyboard.F9;
                case InputHandler.KEYBOARD_BINDING + "/f10":
                    return Keyboard.F10;
                case InputHandler.KEYBOARD_BINDING + "/f11":
                    return Keyboard.F11;
                case InputHandler.KEYBOARD_BINDING + "/f12":
                    return Keyboard.F12;
                case InputHandler.KEYBOARD_BINDING + "/escape":
                    return Keyboard.Esc;
                case InputHandler.KEYBOARD_BINDING + "/tab":
                    return Keyboard.Tab;
                case InputHandler.KEYBOARD_BINDING + "/capsLock":
                    return Keyboard.CapsLock;
                case InputHandler.KEYBOARD_BINDING + "/leftShift":
                    return Keyboard.Shift;
                case InputHandler.KEYBOARD_BINDING + "/rightShift":
                    return Keyboard.Shift;
                case InputHandler.KEYBOARD_BINDING + "/leftCtrl":
                    return Keyboard.Ctrl;
                case InputHandler.KEYBOARD_BINDING + "/rightCtrl":
                    return Keyboard.Ctrl;
                case InputHandler.KEYBOARD_BINDING + "/leftAlt":
                    return Keyboard.Alt;
                case InputHandler.KEYBOARD_BINDING + "/rightAlt":
                    return Keyboard.Alt;
                case InputHandler.KEYBOARD_BINDING + "/space":
                    return Keyboard.Space;
                case InputHandler.KEYBOARD_BINDING + "/enter":
                    return Keyboard.Enter;
                case InputHandler.KEYBOARD_BINDING + "/backspace":
                    return Keyboard.Backspace;
                case InputHandler.KEYBOARD_BINDING + "/insert":
                    return Keyboard.Insert;
                case InputHandler.KEYBOARD_BINDING + "/home":
                    return Keyboard.Home;
                case InputHandler.KEYBOARD_BINDING + "/pageUp":
                    return Keyboard.PGUP;
                case InputHandler.KEYBOARD_BINDING + "/delete":
                    return Keyboard.Delete;
                case InputHandler.KEYBOARD_BINDING + "/end":
                    return Keyboard.End;
                case InputHandler.KEYBOARD_BINDING + "/pageDown":
                    return Keyboard.PGDN;
                case InputHandler.KEYBOARD_BINDING + "/minus":
                case InputHandler.KEYBOARD_BINDING + "/numpadMinus":
                    return Keyboard.NumpadMinus;
                case InputHandler.KEYBOARD_BINDING + "/semicolon":
                    return Keyboard.Semicolon;
                case InputHandler.KEYBOARD_BINDING + "/quote":
                    return Keyboard.Quote;
                case InputHandler.KEYBOARD_BINDING + "/slash":
                    return Keyboard.NumpadSlash;
                case InputHandler.KEYBOARD_BINDING + "/leftBracket":
                    return Keyboard.Bracket_L;
                case InputHandler.KEYBOARD_BINDING + "/rightBracket":
                    return Keyboard.Bracket_R;
                case InputHandler.KEYBOARD_BINDING + "/comma":
                    return Keyboard.Comma;
                case InputHandler.KEYBOARD_BINDING + "/period":
                    return Keyboard.Period;
                case InputHandler.KEYBOARD_BINDING + "/backslash":
                    return Keyboard.Backslash;
                case InputHandler.KEYBOARD_BINDING + "/equals":
                case InputHandler.KEYBOARD_BINDING + "/numpadEquals":
                    return Keyboard.EqualsKey;
                case InputHandler.KEYBOARD_BINDING + "/numpadDivide":
                    return Keyboard.NumpadSlash;
                case InputHandler.KEYBOARD_BINDING + "/numpadMultiply":
                    return Keyboard.Asterisk;
                case InputHandler.KEYBOARD_BINDING + "/numpadPlus":
                    return Keyboard.NumpadPlus;
                case InputHandler.KEYBOARD_BINDING + "/numpadEnter":
                    return Keyboard.NumpadEnter;
                case InputHandler.KEYBOARD_BINDING + "/numpadPeriod":
                    return Keyboard.NumpadPeriod;
                case InputHandler.KEYBOARD_BINDING + "/backquote":
                    return Keyboard.Backquote;
                case InputHandler.KEYBOARD_BINDING + "/contextMenu":
                    return Keyboard.Context;
                default:
                    break;
            }

            return Keyboard.None;
        }

        public Sprite GetMouseSprite(string bindingPath)
        {
            switch (bindingPath)
            {
                case InputHandler.MOUSE_BINDING + "/leftButton":
                    return Mouse.MouseLeft;
                case InputHandler.MOUSE_BINDING + "/middleButton":
                    return Mouse.MouseMiddle;
                case InputHandler.MOUSE_BINDING + "/rightButton":
                    return Mouse.MouseRight;
                case InputHandler.MOUSE_BINDING + "/delta":
                    return Mouse.Mouse;
                default:
                    break;
            }

            return Keyboard.None;
        }
    }
}