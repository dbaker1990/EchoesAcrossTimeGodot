shader_type canvas_item;
render_mode unshaded, blend_mix;

// Declare the screen texture explicitly for Godot 4.5+
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

// --- CRT Effect Parameters ---
uniform float scanline_strength : hint_range(0.0, 1.0) = 0.3;
uniform float scanline_count : hint_range(100.0, 1000.0) = 400.0;
uniform float curvature : hint_range(0.0, 0.3) = 0.05;
uniform float vignette_strength : hint_range(0.0, 1.0) = 0.4;
uniform float chromatic_aberration : hint_range(0.0, 0.01) = 0.002;
uniform float noise_amount : hint_range(0.0, 0.1) = 0.02;
uniform float brightness : hint_range(0.5, 1.5) = 1.0;
uniform vec3 phosphor_color : source_color = vec3(1.0, 0.95, 0.85);

// Flicker effect
uniform float flicker_speed : hint_range(0.0, 10.0) = 2.0;
uniform float flicker_strength : hint_range(0.0, 0.1) = 0.02;

// --- Helper Functions ---
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

// Screen curvature distortion
vec2 curve_uv(vec2 uv, float amount) {
    uv = uv * 2.0 - 1.0;
    vec2 offset = abs(uv.yx) / vec2(amount);
    uv = uv + uv * offset * offset;
    uv = uv * 0.5 + 0.5;
    return uv;
}

void fragment() {
    // Apply screen curvature
    vec2 curved_uv = curve_uv(SCREEN_UV, curvature);

    // Check if outside curved screen bounds
    bool out_of_bounds = curved_uv.x < 0.0 || curved_uv.x > 1.0 || curved_uv.y < 0.0 || curved_uv.y > 1.0;

    vec3 col;

    if (out_of_bounds) {
        // Black border for curved edges
        col = vec3(0.0);
    } else {
        // Chromatic aberration (RGB split)
        float r = texture(screen_texture, curved_uv + vec2(chromatic_aberration, 0.0)).r;
        float g = texture(screen_texture, curved_uv).g;
        float b = texture(screen_texture, curved_uv - vec2(chromatic_aberration, 0.0)).b;
        col = vec3(r, g, b);

        // Scanlines
        float scanline = sin(curved_uv.y * scanline_count * 3.14159) * 0.5 + 0.5;
        scanline = mix(1.0, scanline, scanline_strength);
        col *= scanline;

        // Phosphor glow effect
        col *= phosphor_color;

        // Vignette effect
        vec2 vignette_uv = curved_uv * (1.0 - curved_uv);
        float vignette = vignette_uv.x * vignette_uv.y * 15.0;
        vignette = pow(vignette, vignette_strength);
        col *= vignette;

        // Add noise/static
        float noise = hash(curved_uv * 1000.0 + TIME * 10.0) - 0.5;
        col += noise * noise_amount;

        // Flicker effect
        float flicker = sin(TIME * flicker_speed) * flicker_strength;
        col *= 1.0 + flicker;

        // Brightness adjustment
        col *= brightness;
    }

    COLOR = vec4(col, 1.0);
}