
/***************************************************************************
*                                                                          *
*  Copyright (c) Raphaël Ernaelsten (@RaphErnaelsten)                      *
*  All Rights Reserved.                                                    *
*                                                                          *
*  NOTICE: Aura 2 is a commercial project.                                 * 
*  All information contained herein is, and remains the property of        *
*  Raphaël Ernaelsten.                                                     *
*  The intellectual and technical concepts contained herein are            *
*  proprietary to Raphaël Ernaelsten and are protected by copyright laws.  *
*  Dissemination of this information or reproduction of this material      *
*  is strictly forbidden.                                                  *
*                                                                          *
***************************************************************************/

using System;
using UnityEngine;
using UnityEditor;

namespace Aura2API
{
    public static class Menus
    {
        /// <summary>
        /// Opens the Discord server URL
        /// </summary>
        [MenuItem("Window/Aura 2/Support", priority = 100)]
        public static void SendSupportRequestEmail()
        {
            Application.OpenURL("mailto:help@oniric-studio.com?subject=Aura%202%20support%20request");
        }
    }
}
