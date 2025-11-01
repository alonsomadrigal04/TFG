shader_type canvas_item;

uniform float vignette_size : hint_range(0.0, 1.0) = 0.5; // Tamaño del área transparente
uniform float vignette_softness : hint_range(0.0, 1.0) = 0.3; // Transición del borde
uniform float wobble_strength : hint_range(0.0, 0.5) = 0.1; // Amplitud del tambaleo
uniform float wobble_speed : hint_range(0.0, 10.0) = 2.0; // Velocidad de la animación
uniform vec4 vignette_color : source_color = vec4(0.0, 0.0, 0.0, 0.8); // Color del borde
uniform bool circular = true; // Circular o cuadrado
uniform float particle_intensity : hint_range(0.0, 1.0) = 0.3; // Cantidad de partículas

// --- Ruido mejorado (suavizado y multi-octava) ---

float random(vec2 st) {
    return fract(sin(dot(st, vec2(12.9898,78.233))) * 43758.5453123);
}

float smooth_noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));
    vec2 u = smoothstep(0.0, 1.0, f);
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

// Fractal noise (suma de varias escalas de ruido)
float fractal_noise(vec2 st) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    for (int i = 0; i < 4; i++) {
        value += smooth_noise(st * frequency) * amplitude;
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

void fragment() {
    vec2 uv = UV;
    vec2 center = vec2(0.5);

    // Distancia desde el centro
    float dist;
    if (circular) {
        dist = distance(uv, center);
    } else {
        vec2 d = abs(uv - center);
        dist = max(d.x, d.y) * 1.414;
    }

    // Ruido animado más suave
    float t = TIME * wobble_speed;
    float distortion = (fractal_noise(uv * 6.0 + vec2(t, t * 0.3)) - 0.5) * wobble_strength;

    dist += distortion;

    // Borde suave de la viñeta
    float vignette = smoothstep(vignette_size, vignette_size + vignette_softness, dist);

    // Partículas más naturales usando ruido de alta frecuencia
    float particle_noise = fractal_noise(uv * 30.0 + vec2(t * 0.2, -t * 0.1));
    float particle_mask = step(1.0 - particle_intensity, particle_noise) * vignette;

    // Color del borde
    vec4 edge_color = vignette_color;
    edge_color.a += particle_mask * 0.25;

    // Transparente en el centro
    COLOR = mix(vec4(0.0), edge_color, vignette);
}
