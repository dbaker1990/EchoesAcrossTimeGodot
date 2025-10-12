shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear;
uniform float distortion_strength : hint_range(0.0, 0.1) = 0.02;
uniform float noise_strength : hint_range(0.0, 0.5) = 0.1;
uniform float scanline_strength : hint_range(0.0, 1.0) = 0.3;
uniform float tape_speed : hint_range(0.0, 5.0) = 1.0;

float random(vec2 st) {
    return fract(sin(dot(st, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    vec2 uv = SCREEN_UV;

    // Horizontal distortion (like tape warping)
    float wave = sin(uv.y * 20.0 + TIME * tape_speed) * distortion_strength;
    uv.x += wave;

    // Random line jumps
    float line_jump = step(0.99, random(vec2(TIME * 0.5, floor(uv.y * 100.0))));
    uv.x += line_jump * 0.05;

    vec3 col = texture(SCREEN_TEXTURE, uv).rgb;

    // Scanlines
    float scanline = sin(uv.y * 800.0) * 0.5 + 0.5;
    col *= mix(1.0, scanline, scanline_strength);

    // VHS noise
    float noise = random(uv + TIME) * noise_strength;
    col += noise;

    // Color bleeding
    col.r = texture(SCREEN_TEXTURE, uv + vec2(0.002, 0.0)).r;
    col.b = texture(SCREEN_TEXTURE, uv - vec2(0.002, 0.0)).b;

    COLOR = vec4(col, 1.0);
}