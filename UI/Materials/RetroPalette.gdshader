shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform int color_depth : hint_range(2, 256) = 16;

vec3 reduceColorDepth(vec3 color, int depth) {
    float levels = float(depth);
    return floor(color * levels) / levels;
}

void fragment() {
    vec3 col = texture(screen_texture, SCREEN_UV).rgb;
    col = reduceColorDepth(col, color_depth);
    COLOR = vec4(col, 1.0);
}