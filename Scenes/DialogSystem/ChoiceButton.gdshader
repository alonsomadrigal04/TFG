shader_type canvas_item;

uniform float swirl_strength  : hint_range(0.0, 2.0) = 0.30;
uniform float fog_strength    : hint_range(0.0, 1.0) = 0.40;
uniform float pulse_strength  : hint_range(0.0, 1.0) = 0.12;
uniform float speed = 0.35;
uniform float hover : hint_range(0.0, 1.0) = 0.0;
uniform float click : hint_range(0.0, 1.0) = 0.0;

// Gradiente suave en lugar de hash directo → menos artefactos
vec2 grad(vec2 p) {
    p = fract(p * vec2(127.1, 311.7));
    p += dot(p, p + 43.21);
    return fract(vec2(p.x * p.y, p.x + p.y)) * 2.0 - 1.0;
}

// Perlin 2D → mucho más suave que value noise
float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    // quintic interpolation — elimina artefactos de derivada
    vec2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

    float a = dot(grad(i),               f);
    float b = dot(grad(i + vec2(1,0)),   f - vec2(1,0));
    float c = dot(grad(i + vec2(0,1)),   f - vec2(0,1));
    float d = dot(grad(i + vec2(1,1)),   f - vec2(1,1));

    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y) * 0.5 + 0.5;
}

float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    mat2 rot = mat2(vec2(0.8, 0.6), vec2(-0.6, 0.8)); // rota cada octava → rompe repetición
    for (int i = 0; i < 5; i++) {
        v += noise(p) * a;
        p  = rot * p * 2.1;
        a *= 0.48;
    }
    return v;
}

void fragment() {
    vec2 uv     = UV;
    vec4 base   = texture(TEXTURE, uv);
    vec2 center = vec2(0.5);
    vec2 p      = uv - center;
    float dist  = length(p);
    float angle = atan(p.y, p.x);

    float t = TIME * speed;

    // Swirl suavizado con smoothstep para evitar el corte brusco en bordes
    float swirl = (0.5 - dist) * swirl_strength;
    swirl *= smoothstep(0.55, 0.1, dist); // desvanece el swirl en el borde exterior
    angle += swirl;

    vec2 swirl_uv = vec2(cos(angle), sin(angle)) * dist;

    // Dos capas de fbm con velocidades distintas
    float fog1 = fbm(swirl_uv * 3.5 + vec2( t * 0.40, -t * 0.25));
    float fog2 = fbm(swirl_uv * 6.0 - vec2( t * 0.15,  t * 0.35));

    // Multiplicar da niebla más dramática y contrastada
    float mist = fog1 * fog2;
    // Suavizar el resultado final del mist
    mist = smoothstep(0.0, 1.0, mist * 1.4 - 0.1);

    // Máscara de centro con borde suave más amplio
    float center_mask = smoothstep(0.52, 0.0, dist);

    // Respiración lenta
    float pulse = sin(TIME * 1.1) * 0.5 + 0.5;

    // Bonus hover: intensifica el efecto al pasar el ratón
    float hover_boost = 1.0 + hover * 0.5 + click * 0.3;

    float effect = mist * center_mask * (1.0 + pulse * pulse_strength) * hover_boost;

    // Oscurecimiento suave
    vec3 darkened = mix(base.rgb, base.rgb * 0.60, effect * fog_strength);

    // Tinte violeta muy sutil encima (solo en hover)
    vec3 tint = vec3(0.45, 0.20, 0.75);
    vec3 final_rgb = mix(darkened, darkened + tint * 0.08, hover * effect);

    // Brillo en click
    final_rgb = mix(final_rgb, final_rgb + vec3(0.6, 0.4, 1.0) * 0.12, click * center_mask);

    COLOR = vec4(final_rgb, base.a);
}