shader_type canvas_item;

uniform float corner_scale: hint_range(0., 1.) = 0.5;
uniform float edge_thickness: hint_range(0., 0.2) = 0.02; // grosor del borde
uniform float speed: hint_range(0., 5.) = 1.0; // velocidad del movimiento
uniform float intensity: hint_range(0., 1.) = 0.5; // cuán fuerte vibra el borde

// Función simple de ruido 2D (pseudo-random)
float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

// Calcula borde dinámico
float dynamicEdge(vec2 uv, float s) {
    vec2 offset = vec2(
        sin(TIME * speed + uv.x * 10.0) * intensity,
        cos(TIME * speed + uv.y * 10.0) * intensity
    ) * edge_thickness;

    float dist = pow(uv.x - 0.5 + offset.x, 2.0) + pow(uv.y - 0.5 + offset.y, 2.0);
    float radius = pow(s, 2.0);

    return smoothstep(radius, radius + edge_thickness, dist);
}

void fragment() {
    float s = corner_scale * 0.5;

    // calcula la máscara con el borde dinámico
    float mask = dynamicEdge(UV, s);

    COLOR.a *= 1.0 - mask;
}
