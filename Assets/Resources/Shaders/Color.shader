Shader "Color"{
    Properties{
        _color("Color", Color) = (1, 1, 0, 1)
    }

    SubShader{
        Pass{
            CGPROGRAM
                #pragma vertex vertexShader
                #pragma fragment fragmentShader

                uniform fixed4 _color;

                float4 vertexShader(float4 position: POSITION): SV_POSITION{
                    return UnityObjectToClipPos(position);
                }

                fixed4 fragmentShader(): SV_Target{
                    return _color;
                }
            ENDCG
        }
    }
}
