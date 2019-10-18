#version 330 core

in vec2 frag_UV;
flat in ivec3 frag_Normals;
flat in int frag_ShowNormals;

uniform sampler2D tex;

out vec4 out_Color;

void main()
{
	if (frag_ShowNormals == 1)
	{
		out_Color = vec4(vec3(frag_Normals) + 1 / 2.0, 1.0);
	}
	else
	{
		vec4 mask = vec4(0.7);
		
		if (frag_Normals.y > 0)
			mask = vec4(1);
		else if(frag_Normals.y < 0)
			mask = vec4(0.4);
		mask.w = 1.0;
		
		out_Color = texture(tex, vec2(frag_UV.x, 1.0-frag_UV.y)) * mask;
	}
}