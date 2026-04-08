shader_type spatial;
render_mode unshaded, cull_disabled, blend_mix;

uniform vec4 color_main : source_color = vec4(0.6, 0.6, 0.6, 1.0);
uniform vec4 color_shadow : source_color = vec4(0.45, 0.45, 0.45, 1.0);

uniform float facet_steps = 4.0;

uniform float noise_scale = 6.0;
uniform float wobble_strength = 0.08;
uniform float time_scale = 6.0;

varying vec3 world_pos;

// ruido simple
float hash(vec3 p) {
    return fract(sin(dot(p, vec3(127.1, 311.7, 91.7))) * 43758.5453);
}

float noise(vec3 p) {
    vec3 i = floor(p);
    vec3 f = fract(p);

    float a = hash(i);
    float b = hash(i + vec3(1,0,0));
    float c = hash(i + vec3(0,1,0));
    float d = hash(i + vec3(1,1,0));

    vec2 u = f.xy * f.xy * (3.0 - 2.0 * f.xy);

    return mix(a, b, u.x) +
           (c - a) * u.y * (1.0 - u.x) +
           (d - b) * u.x * u.y;
}

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;

    // vibración más "seca" (no blobby)
    float n = noise(world_pos * noise_scale + TIME * time_scale);
    float snap = floor(n * 3.0) / 3.0; // cuantiza → look más duro

    VERTEX += NORMAL * (snap * wobble_strength);
}

void fragment() {
    // cuantizar la iluminación para simular lowpoly
    float light = dot(NORMAL, vec3(0.0, 1.0, 0.0)) * 0.5 + 0.5;
    light = floor(light * facet_steps) / facet_steps;

    vec3 color = mix(color_shadow.rgb, color_main.rgb, light);

    // alpha más sólido (menos burbuja)
    float alpha = 0.8;

    ALBEDO = color;
    ALPHA = alpha * color_main.a;
}