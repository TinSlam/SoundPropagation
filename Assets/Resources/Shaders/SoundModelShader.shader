Shader "SoundModel"{
    SubShader{
        Pass{
            Blend SrcAlpha OneMinusSrcAlpha
            BlendOp Max

            CGPROGRAM
                #pragma vertex vertexShader
                #pragma fragment fragmentShader

                    struct VertexShaderInput{
                    float4 position: POSITION;
                    fixed4 color: COLOR0;
                };

                struct VertexShaderOutput{
                    float4 position: SV_POSITION;
                    fixed4 color: COLOR0;
                };

                VertexShaderOutput vertexShader(VertexShaderInput input){
                    VertexShaderOutput output;
                    output.position = UnityObjectToClipPos(input.position);
                    output.color = input.color;
                    return output;
                }

                fixed4 fragmentShader(VertexShaderOutput input): SV_Target{
                    return input.color;
                }
            ENDCG
        }
    }
}
