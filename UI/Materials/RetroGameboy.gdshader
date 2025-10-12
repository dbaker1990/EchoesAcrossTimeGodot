shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest;

// Classic GameBoy green palette
const vec3 COLORS[4] = {
vec3(0.06, 0.22, 0.06),   // Darkest
vec3(0.19, 0.38, 0.19),   // Dark
vec3(0.55, 0.67, 0.06),   // Light
vec3(0.61, 0.73, 0.06)    // Lightest
};

void fragment() {
    vec4 col = texture(SCREEN_TEXTURE, SCREEN_UV);
    float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));

    int color_index = int(gray * 3.0);
    COLOR = vec4(COLORS[color_index], 1.0);
}