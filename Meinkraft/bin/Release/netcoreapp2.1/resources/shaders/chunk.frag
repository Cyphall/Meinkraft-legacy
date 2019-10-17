#version 330 core

in vec2 frag_UV;
uniform sampler2D tex;

out vec4 out_Color;

void main()
{
	out_Color = texture(tex, vec2(frag_UV.x, 1.0-frag_UV.y));
}