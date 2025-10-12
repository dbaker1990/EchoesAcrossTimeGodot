shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;
uniform int color_depth : hint_range(2, 256) = 16;
uniform bool dither : hint_default(true) = true;

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

void fragment() {
    vec4 col = texture(SCREEN_TEXTURE, SCREEN_UV);

    if (dither) {
        float dither_val = hash(FRAGCOORD.xy) / float(color_depth);
        col.rgb += dither_val - 0.5 / float(color_depth);
    }

    float levels = float(color_depth);
    col.rgb = floor(col.rgb * levels) / levels;
    COLOR = col;
}