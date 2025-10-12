shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest;
uniform int pixel_size : hint_range(1, 32) = 4;

void fragment() {
    vec2 screen_size = vec2(textureSize(SCREEN_TEXTURE, 0));
    vec2 pixel_uv = floor(SCREEN_UV * screen_size / float(pixel_size)) * float(pixel_size) / screen_size;
    COLOR = texture(SCREEN_TEXTURE, pixel_uv);
}