shader_type spatial;

render_mode cull_front, unshaded, depth_draw_never;

uniform sampler2D albedo_texture : source_color;

uniform float darken_factor : hint_range(0.0, 1.0) = 0.35;
uniform vec4 outline_tint : source_color = vec4(0.0, 0.0, 0.0, 1.0);

uniform float outline_size = 0.03;

void vertex() {
    VERTEX += NORMAL * outline_size;
}

void fragment() {

    vec4 tex = texture(albedo_texture, UV);

    float luminance = dot(tex.rgb, vec3(0.299, 0.587, 0.114));

    vec3 dark_col = tex.rgb * (1.0 - darken_factor);

    vec3 final_col = mix(dark_col, outline_tint.rgb, 0.2);

    ALBEDO = final_col;
    ALPHA = tex.a;
}