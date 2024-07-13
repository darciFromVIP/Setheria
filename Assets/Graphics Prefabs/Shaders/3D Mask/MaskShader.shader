Shader "Custom/MaskShader"
{
    SubShader
    {
       Tags{ "Queue" = "Transparent+2"}

       Pass{
           Blend Zero One
       }
    }
}
