shader_type canvas_item;

// Necesario en Godot 4 para acceder a la pantalla
uniform sampler2D screen_texture : hint_screen_texture;

void fragment() {
    // Obtiene el color del píxel
    vec4 col = texture(screen_texture, SCREEN_UV);

    // Convierte a blanco y negro usando la fórmula de luminancia
    float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));

    // Devuelve el color en escala de grises, manteniendo alpha
    COLOR = vec4(vec3(gray), col.a);
}
