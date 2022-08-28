Shader "Unlit/cave"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WallTex ("Wall_Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _WallTex;
            float4 _WallTex_ST;

            int _num_hallways;
            float _hallways[20];
            float _hallways_width[10];

            int _num_rooms;
            float _room_positions[20];
            float _room_sizes[10];
            float _room_smooth[10];
            float _room_square[10];


            float smoothMin(float a, float b, float c)
            {
                if (c == 0.0)
                {
                    return min(a, b);
                }
                else {
                    float h = max(c - abs(a - b), 0.0) / c;

                }
                return min(a, b) - h * h * h * c * (1.0 / 6.0);
            }

            float getValueHallway(int index, float2 coord) {
                int room1 = floor(_hallways[2 * index] + .1);
                room1 *= 2;
                int room2 = floor(_hallways[2 * index+1] + .1);
                room2 *= 2;

                float2 r1pos = float2(_room_positions[room1], _room_positions[room1 + 1]);
                float2 r2pos = float2(_room_positions[room2], _room_positions[room2 + 1]);

                float2 li = r1pos - r2pos;
                float2 closestPoint;
                float pointOnLine = dot(normalize(li), coord - r2pos);
                pointOnLine = clamp(pointOnLine, 0.0, length(li));
                closestPoint = pointOnLine * normalize(li) + r2pos;

                return length(closestPoint - coord) / _hallways_width[index] - 1.0;


            }

            float getValueRoom(int index, float2 coord) 
            {
                float2 diff = coord - float2(_room_positions[2*index], _room_positions[2*index+1]);
                float value1 = pow(pow(abs(diff.x), _room_square[index]) + pow(abs(diff.y), _room_square[index]), 1 / _room_square[index]) / _room_sizes[index] - 1;
                return value1;
            }



            float getRooms(float2 coord) {
                if (_num_rooms == 0) {
                    return -1.0;
                }
                float value = getValueRoom(0.0, coord);
                if (_num_rooms == 1) {
                    return value;
                }

                for (int i = 1; i < _num_rooms; i++)
                {
                    value = smoothMin(value, getValueRoom(i, coord), 0.0);
                }
                return value;
            }
            float getHallways(float2 coord, float value) {


                if (_num_hallways == 0) {
                    return value;
                }

                for (int i = 0; i < _num_hallways; i++) {
                    value = smoothMin(value, getValueHallway(i, coord), 0.0);
                }
                return value;
            }


            float getValueAtPos(float2 coord)
            {
                float value = getRooms(coord);
                return getHallways(coord, value);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = getValueAtPos(i.worldPos.xy) < 0 ? tex2D( _MainTex, i.worldPos.xy) : tex2D(_WallTex, i.worldPos.xy);
                
                return col;
            }
            ENDCG
        }
    }
}
