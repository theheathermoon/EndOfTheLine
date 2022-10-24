 Shader "Invisible/InvisibleShadowCaster" {
     SubShader {
         Tags { 
             "Queue"="Transparent"
             "RenderType"="Transparent" 
         }
         CGPROGRAM
         #pragma surface surf Lambert alpha addshadow
 
         struct Input {
             float nothing; // Just a dummy because surf expects something
         };
 
         void surf (Input IN, inout SurfaceOutput o) {
             o.Alpha = 0;
         }
         ENDCG
     } 
     FallBack "Diffuse"
 }