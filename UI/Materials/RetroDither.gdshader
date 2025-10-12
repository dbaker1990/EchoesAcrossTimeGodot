shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;
uniform int dither_size : hint_range(1, 8) = 2;
uniform float threshold : hint_range(0.0, 1.0) = 0.5;

// Bayer matrix 8x8
const int bayer[64] = int[](
0, 32, 8, 40, 2, 34, 10, 42,
48, 16, 56, 24, 50, 18, 58, 26,
12, 44, 4, 36, 14, 46, 6, 38,
60, 28, 52, 20, 62, 30, 54, 22,
3, 35, 11, 43, 1, 33, 9, 41,
51, 19, 59, 27, 49, 17, 57, 25,
15, 47, 7, 39, 13, 45, 5, 37,
63, 31, 55, 23, 61, 29, 53, 21
);

void fragment() {
    vec4 col = texture(SCREEN_TEXTURE, SCREEN_UV);

    ivec2 pixel = ivec2(FRAGCOORD.xy) / dither_size;
    int index = (pixel.x % 8) + (pixel.y % 8) * 8;
    float dither_threshold = float(bayer[index]) / 64.0;

    vec3 gray = vec3(dot(col.rgb, vec3(0.299, 0.587, 0.114)));
    col.rgb = step(dither_threshold + threshold - 0.5, gray);

    COLOR = col;
}