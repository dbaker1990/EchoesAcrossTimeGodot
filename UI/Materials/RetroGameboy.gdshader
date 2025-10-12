shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

// Classic Game Boy color palette
const vec3 COLOR_0 = vec3(0.06, 0.22, 0.06);  // Darkest green
const vec3 COLOR_1 = vec3(0.19, 0.38, 0.19);  // Dark green
const vec3 COLOR_2 = vec3(0.54, 0.67, 0.06);  // Light green
const vec3 COLOR_3 = vec3(0.61, 0.73, 0.06);  // Lightest green

void fragment() {
    vec3 col = texture(screen_texture, SCREEN_UV).rgb;

    // Convert to grayscale
    float gray = dot(col, vec3(0.299, 0.587, 0.114));

    // Map to Game Boy palette (4 colors)
    vec3 gb_color;
    if (gray < 0.25) {
        gb_color = COLOR_0;
    } else if (gray < 0.5) {
        gb_color = COLOR_1;
    } else if (gray < 0.75) {
        gb_color = COLOR_2;
    } else {
        gb_color = COLOR_3;
    }

    COLOR = vec4(gb_color, 1.0);
}