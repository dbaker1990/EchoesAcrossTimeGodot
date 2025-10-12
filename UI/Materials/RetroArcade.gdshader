shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;
uniform float pixel_grid_strength : hint_range(0.0, 1.0) = 0.5;
uniform float scanline_intensity : hint_range(0.0, 1.0) = 0.3;
uniform float bloom : hint_range(0.0, 2.0) = 0.3;

void fragment() {
    vec2 uv = SCREEN_UV;
    vec3 col = texture(SCREEN_TEXTURE, uv).rgb;

    // Pixel grid (RGB subpixel pattern)
    vec2 pixel_pos = fract(uv * vec2(textureSize(SCREEN_TEXTURE, 0)) * 0.5);
    vec3 grid = vec3(
    smoothstep(0.5, 0.6, pixel_pos.x),
    smoothstep(0.3, 0.5, pixel_pos.x) * smoothstep(0.5, 0.7, pixel_pos.x),
    smoothstep(0.0, 0.4, pixel_pos.x)
    );
    col *= mix(vec3(1.0), grid, pixel_grid_strength);

    // Scanlines
    float scanline = sin(uv.y * 600.0) * 0.5 + 0.5;
    col *= mix(1.0, scanline, scanline_intensity);

    // Bloom/glow
    col += col * bloom;

    COLOR = vec4(col, 1.0);
}