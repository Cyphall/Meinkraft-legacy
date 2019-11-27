#version 460 core

layout(location = 0) in uvec3 in_Vertex;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in ivec3 in_Normals;

layout(location = 3) uniform mat4 modelViewProjection;
layout(location = 4) uniform int showNormals;

out vec2 frag_UV;
flat out ivec3 frag_Normals;
flat out int frag_ShowNormals;

void main()
{
	gl_Position = modelViewProjection * vec4(in_Vertex, 1);

    frag_UV = in_UV;
    frag_Normals = in_Normals;
    frag_ShowNormals = showNormals;
}