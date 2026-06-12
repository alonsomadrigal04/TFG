shader_type spatial;

render_mode blend_mix, depth_draw_opaque, cull_back, specular_schlick_ggx;

uniform sampler2D albedo_texture : source_color;
uniform sampler2D roughness_texture : source_color;

uniform vec4 highlight_color : source_color = vec4(1.0);
uniform vec4 mid1_color : source_color = vec4(0.8, 0.8, 0.8, 1.0);
uniform vec4 mid2_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform vec4 shadow_color : source_color = vec4(0.25, 0.25, 0.25, 1.0);

uniform float highlight_threshold : hint_range(0.0, 1.0) = 0.85;
uniform float mid1_threshold : hint_range(0.0, 1.0) = 0.6;
uniform float mid2_threshold : hint_range(0.0, 1.0) = 0.35;
uniform float smoothness : hint_range(0.0, 0.5) = 0.1;

uniform float roughness = 1.0;
uniform int roughness_channel : hint_range(0, 3) = 0;

void fragment() {
    vec4 tex = texture(albedo_texture, UV);
    ALBEDO = tex.rgb;
    ALPHA = tex.a;

    vec4 rough_tex = texture(roughness_texture, UV);
    float raw = roughness;

    if (roughness_channel == 0) raw = rough_tex.r * roughness;
    else if (roughness_channel == 1) raw = rough_tex.g * roughness;
    else if (roughness_channel == 2) raw = rough_tex.b * roughness;
    else if (roughness_channel == 3) raw = rough_tex.a * roughness;

    ROUGHNESS = clamp(raw, 0.2, 0.8);
}

void light() {

    // LIGHT = dirección de la luz REAL
    float NdotL = max(dot(NORMAL, LIGHT), 0.0);

    float h = smoothstep(highlight_threshold - smoothness,
                         highlight_threshold + smoothness, NdotL);

    float m1 = smoothstep(mid1_threshold - smoothness,
                          mid1_threshold + smoothness, NdotL);

    float m2 = smoothstep(mid2_threshold - smoothness,
                          mid2_threshold + smoothness, NdotL);

    vec3 col = shadow_color.rgb;

    col = mix(col, mid2_color.rgb, m2);
    col = mix(col, mid1_color.rgb, m1);
    col = mix(col, highlight_color.rgb, h);

    DIFFUSE_LIGHT += col * LIGHT_COLOR * ATTENUATION;
}