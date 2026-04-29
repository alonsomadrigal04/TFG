shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_tex : hint_screen_texture, filter_nearest_mipmap;

uniform float intensity : hint_range(0.0, 1.0) = 0.85;
uniform float speed : hint_range(0.0, 3.0) = 1.0;

uniform float warp : hint_range(0.0, 0.2) = 0.06;
uniform float shake : hint_range(0.0, 1.0) = 0.45;
uniform float chromatic : hint_range(0.0, 0.02) = 0.004;

uniform float blur_amount : hint_range(0.0, 1.0) = 0.25;
uniform float blur_mip : hint_range(0.0, 6.0) = 1.5;

uniform float grain : hint_range(0.0, 1.0) = 0.20;
uniform float scanline_strength : hint_range(0.0, 1.0) = 0.10;
uniform float vignette_strength : hint_range(0.0, 1.0) = 0.38;

uniform float color_drift : hint_range(0.0, 1.0) = 0.55;
uniform float ghosting : hint_range(0.0, 1.0) = 0.12;
uniform float line_jitter : hint_range(0.0, 0.01) = 0.0015;

vec2 rot(vec2 p, float a) {
	float s = sin(a);
	float c = cos(a);
	return mat2(vec2(c, -s), vec2(s, c)) * p;
}

float hash12(vec2 p) {
p = fract(p * vec2(123.34, 456.21));
p += dot(p, p + 34.345);
return fract(p.x * p.y);
}

void fragment() {
vec2 uv = SCREEN_UV;
vec2 center = vec2(0.5);
vec2 p = uv - center;
float r = length(p);
float t = TIME * speed;

// Deriva lenta, como si la pantalla respirara.
vec2 drift = vec2(sin(t * 0.73), cos(t * 0.61));
uv += drift * 0.003 * intensity;

// Micro-desfase por líneas: simula “dejoncillos” y vibración.
float line_id = floor(uv.y * 280.0 + t * 20.0);
float j = hash12(vec2(line_id, floor(t * 12.0))) - 0.5;
uv.x += j * line_jitter * intensity * (0.2 + r);

// Warp tipo sueño / mareo.
float barrel = warp * (1.0 + 0.65 * sin(t * 0.5 + r * 6.0));
uv = center + (uv - center) * (1.0 + barrel * (r * r * 1.9 + 0.15 * sin(t * 0.9 + p.y * 9.0)));

// Ligera rotación flotante.
uv = center + rot(uv - center, 0.01 * sin(t * 0.33 + r * 3.0) * intensity);

// Wobble lateral y vertical.
uv += vec2(
	sin(uv.y * 22.0 + t * 1.6 + sin(t * 0.4)),
	cos(uv.x * 18.0 - t * 1.3)
) * 0.0018 * shake;

// Cromática.
vec2 ca_dir = normalize((uv - center) + vec2(1e-4));
float ca = chromatic * (0.7 + 2.2 * r) * (0.6 + 0.4 * sin(t * 1.2));

vec2 uv_r = clamp(uv + ca_dir * ca, vec2(0.0), vec2(1.0));
vec2 uv_g = clamp(uv, vec2(0.0), vec2(1.0));
vec2 uv_b = clamp(uv - ca_dir * ca, vec2(0.0), vec2(1.0));

vec3 col;
col.r = textureLod(screen_tex, uv_r, blur_mip).r;
col.g = textureLod(screen_tex, uv_g, blur_mip).g;
col.b = textureLod(screen_tex, uv_b, blur_mip).b;

// Fantasma suave para simular estela.
vec2 ghost_offset = vec2(sin(t * 0.9), cos(t * 0.77)) * (0.002 + 0.006 * r) * ghosting;
vec3 ghost = textureLod(screen_tex, clamp(uv + ghost_offset, vec2(0.0), vec2(1.0)), blur_mip + 1.0).rgb;
float ghost_mix = blur_amount * (0.15 + 0.25 * (0.5 + 0.5 * sin(t * 0.8 + r * 12.0)));
col = mix(col, ghost, ghost_mix);

// Cambio de temperatura / color onírico.
vec3 warm = vec3(1.04, 0.97, 0.93);
vec3 cool = vec3(0.93, 1.01, 1.07);
float drift_phase = 0.5 + 0.5 * sin(t * 0.35 + r * 5.0);
col *= mix(warm, cool, drift_phase * color_drift);

// Muy pequeño “hue wobble” para que no se quede plano.
col = mix(col, col.bgr, 0.03 * intensity * (0.5 + 0.5 * sin(t * 2.0 + p.x * 6.0)));

// Scanlines sutiles.
float scan = 0.5 + 0.5 * sin((uv.y + t * 0.35) * 900.0);
col *= 1.0 - scanline_strength * 0.08 * scan;

// Grano.
float n = hash12(uv * vec2(1920.0, 1080.0) + t * 60.0) - 0.5;
col += n * grain * 0.06;

// Viñeta.
float vig = 1.0 - smoothstep(0.38, 0.95, r);
col *= mix(1.0, vig, vignette_strength);

// Niebla soñada.
vec3 haze = vec3(0.02, 0.015, 0.03) * (0.5 + 0.5 * sin(t * 0.5));
col += haze * intensity;

COLOR = vec4(clamp(col, 0.0, 1.0), 1.0);

}