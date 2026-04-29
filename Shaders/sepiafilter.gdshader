shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform float sepia_strength : hint_range(0.0, 1.0) = 1.0;
uniform float time_scale = 0.5;
uniform float wave_intensity = 0.03;
uniform float grain_strength : hint_range(0.0, 0.2) = 0.05;
uniform float vignette_strength : hint_range(0.0, 1.0) = 0.4;

float random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898,78.233))) * 43758.5453);
}

void fragment() {
    vec2 uv = SCREEN_UV;
    float t = TIME * time_scale;

    // Subtle memory distortion
    uv.y += sin(uv.x * 10.0 + t) * wave_intensity;
    uv.x += cos(uv.y * 8.0 + t * 0.7) * wave_intensity;

    vec4 col = texture(SCREEN_TEXTURE, uv);

    // Sepia
    float r = dot(col.rgb, vec3(0.393, 0.769, 0.189));
    float g = dot(col.rgb, vec3(0.349, 0.686, 0.168));
    float b = dot(col.rgb, vec3(0.272, 0.534, 0.131));
    vec3 sepia = vec3(r, g, b);

    // Memory pulse
    float pulse = 0.85 + 0.15 * sin(t * 1.5);
    sepia *= pulse;

    // Film grain
    float grain = random(uv * t) * grain_strength;
    sepia += grain;

    // Vignette
    vec2 dist = uv - vec2(0.5);
    float vignette = smoothstep(0.8, vignette_strength, length(dist));
    sepia *= vignette;

    col.rgb = mix(col.rgb, sepia, sepia_strength);
    COLOR = col;
}
