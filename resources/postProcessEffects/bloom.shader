[include internal/prependAll.shader]

[VertexShader] // pass thru vertex shader

layout(location = 0) in vec3 in_position;

void main()
{
	gl_Position = vec4(in_position,1);
}





[FragmentShader]

uniform vec3 testColor;

layout(location = 0) out vec4 out_color;



//http://www.walterzorn.de/en/grapher/grapher_e.htm
// 0.5*exp(-( x*x / pow(6/4, 2) ))
float gausian(float x, float totalWidth) 
{
	return exp(-( (x*x)/(totalWidth/4)*(totalWidth/4) ));	
}


// help from http://prideout.net/archive/bloom/
void main()
{


	vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;

	GBufferPerPixel gBuffer = GetGBufferPerPixel(screenCoord);

	// skybox, just pass thru color
	if (gBuffer.normal == vec3(0, 0, 0)) {
		out_color = vec4(gBuffer.color, 1);
		return;
	}
	

	//out_color = vec4(pow(gBuffer.final,vec3(gBuffer.depth)), 1);
	//out_color = vec4(gBuffer.final, 1);
	//out_color = vec4(testColor, 1);	


	vec3 col = vec3(gBuffer.final);

	vec3 final;
	float intensity = (col.r + col.g + col.b) / 3;
	//if(intensity > 1) col = col / intensity;

	#define ADD(X, Y, MULT) \
		final = GBufferGetFinal(screenCoord + vec2(xScale*X, yScale*Y)); \
		if(any(greaterThan(final, vec3(1)))) { \
			col += final * MULT; \
		} \


	float xScale = 1.5/engine.screenSize.x;
	float yScale = 1.5/engine.screenSize.y;

	int size = 5;

	for(int x=-size; x<size; x++) {
		for(int y=-size; y<size; y++) {
			float v =  1 - sqrt(x*x + y*y) / sqrt(size * size * 5);
			v = clamp(v,0,1)*0.01;
			ADD(x,y,v);
		}
	}

	/*
	ADD(0,+1,1)
	ADD(0,-1,1)
	ADD(+1,0,1)
	ADD(-1,0,1)
	ADD(+1,+1,0.5)
	ADD(-1,+1,0.5)
	ADD(+1,-1,0.5)		
	ADD(-1,-1,0.5)
	*/


	out_color = vec4(col, 1);

	//DEBUG
	//out_color = vec4(1,0,0, 1);
		
}
	

