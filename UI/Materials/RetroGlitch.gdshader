shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;
uniform float separation_amount : hint_range(0.0, 0.05) = 0.01;
uniform float glitch_speed : hint_range(0.0, 10.0) = 2.0;
uniform float glitch_intensity : hint_range(0.0, 1.0) = 0.5;

float random(float seed) {
    return fract(sin(seed) * 43758.5453123);
}

void fragment() {
    vec2 uv = SCREEN_UV;

    // Random glitch blocks
    float block = floor(uv.y * 20.0 + TIME * glitch_speed);
    float glitch = step(0.95, random(block)) * glitch_intensity;

    // RGB separation
    vec2 offset = vec2(separation_amount * glitch, 0.0);
    float r = texture(SCREEN_TEXTURE, uv + offset).r;
    float g = texture(SCREEN_TEXTURE, uv).g;
    float b = texture(SCREEN_TEXTURE, uv - offset).b;

    // Random line displacement
    if (glitch > 0.0) {
        uv.x += (random(block) - 0.5) * 0.1 * glitch;
    }

    COLOR = vec4(r, g, b, 1.0);
}