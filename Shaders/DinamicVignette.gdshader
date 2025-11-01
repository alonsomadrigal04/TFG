shader_type canvas_item;

uniform float vignette_size : hint_range(0.0, 1.0) = 0.5; // Tamaño del hueco central
uniform float vignette_softness : hint_range(0.0, 1.0) = 0.3; // Suavidad del borde
uniform float wobble_strength : hint_range(0.0, 0.5) = 0.1; // Fuerza del tambaleo
uniform float wobble_speed : hint_range(0.0, 10.0) = 2.0; // Velocidad del tambaleo
uniform vec4 vignette_color : source_color = vec4(0.0, 0.0, 0.0, 0.8); // Color del borde
uniform bool circular = true; // Forma circular o cuadrada
uniform float particle_intensity : hint_range(0.0, 1.0) = 0.3; // Cantidad de partículas
uniform int noise_octaves : hint_range(1, 6) = 4; // Capas de ruido para suavizar
uniform float noise_scale : hint_range(1.0, 30.0) = 10.0; // Escala base del ruido

// ----------------------
// Ruido pseudoaleatorio
// ----------------------
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

float noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(a, b, u.x) +
           (c - a) * u.y * (1.0 - u.x) +
           (d - b) * u.x * u.y;
}

// ----------------------
// fBM: varias capas de ruido suavizado
// ----------------------
float fbm(vec2 st) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;

    for (int i = 0; i < noise_octaves; i++) {
        value += amplitude * noise(st * frequency);
        frequency *= 2.0;
        amplitude *= 0.5;
    }

    return value;
}

// ----------------------
// Fragment Shader
// ----------------------
void fragment() {
    vec2 uv = UV;
    vec2 center = vec2(0.5);

    float dist;
    if (circular) {
        dist = distance(uv, center);
    } else {
        vec2 d = abs(uv - center);
        dist = max(d.x, d.y) * 1.414;
    }

    // Ruido suavizado para el borde (fbm)
    float t = TIME * wobble_speed;
    float distortion = (fbm(uv * noise_scale + t) - 0.5) * wobble_strength;
    dist += distortion;

    // Borde con transición suave
    float vignette = smoothstep(vignette_size, vignette_size + vignette_softness, dist);

    // Efecto de partículas orgánico
    float particle_noise = fbm(uv * (noise_scale * 4.0) + vec2(t * 0.5, -t * 0.25));
    float particle_mask = step(1.0 - particle_intensity, particle_noise) * vignette;

    vec4 edge_color = vignette_color;
    edge_color.a += particle_mask * 0.3;

    COLOR = mix(vec4(0.0), edge_color, vignette);
}
