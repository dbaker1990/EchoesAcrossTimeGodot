shader_type canvas_item;
render_mode unshaded, blend_mix;

// --- Tunables ---
uniform float blur_radius = 2.0;        // 1.6–2.8 is good; pixels
uniform float corner = 0.12;            // 0..0.5, rounded corner relative to rect size
uniform vec4 tint = vec4(0.06,0.08,0.10,0.55);  // glass tint (rgb) + alpha
uniform vec4 border_color = vec4(1.0,1.0,1.0,0.18);
uniform float border_thickness = 0.012; // relative (0..0.2)
uniform float noise_amount = 0.02;      // tiny sparkle
uniform float gloss_amount = 0.15;      // soft top-left shine

// --- Helpers ---
float hash(vec2 p){ return fract(sin(dot(p, vec2(127.1,311.7))) * 43758.5453); }

float sd_round_rect(vec2 uv, vec2 half, float r){
    // uv in 0..1; half = vec2(0.5)
    vec2 q = abs(uv - 0.5) - (half - vec2(r));
    return length(max(q, 0.0)) - r;
}

vec4 blur13(vec2 uv, float px){
    vec2 t = SCREEN_PIXEL_SIZE * px;
    vec4 c = texture(SCREEN_TEXTURE, uv) * 0.19648255;
    c += texture(SCREEN_TEXTURE, uv + vec2( 1.4117647,  1.4117647)*t) * 0.29690696;
    c += texture(SCREEN_TEXTURE, uv + vec2(-1.4117647,  1.4117647)*t) * 0.29690696;
    c += texture(SCREEN_TEXTURE, uv + vec2( 1.4117647, -1.4117647)*t) * 0.29690696;
    c += texture(SCREEN_TEXTURE, uv + vec2(-1.4117647, -1.4117647)*t) * 0.29690696;
    return c;
}

void fragment(){
    // Rounded-rect mask in UV space
    float r = clamp(corner, 0.0, 0.5);
    float d = sd_round_rect(UV, vec2(0.5), r);
    float aa = fwidth(d);
    float mask = smoothstep(0.0, aa, -d);

    // Border ring
    float inner = smoothstep(0.0, aa, -(d + border_thickness));
    float ring = clamp(mask - inner, 0.0, 1.0);

    // Blur whatever is already drawn behind this control
    vec4 b = blur13(SCREEN_UV, blur_radius);

    // Glass tint
    vec4 glass = mix(b, vec4(tint.rgb, 1.0), tint.a);

    // Gloss (soft top-left highlight) + micro noise
    float gloss = gloss_amount * smoothstep(0.0, 1.0, 1.0 - (UV.y*0.85 + UV.x*0.15));
    glass.rgb += gloss;
    glass.rgb += (hash(UV*1200.0)-0.5) * noise_amount;

    // Compose: glass + border
    vec4 col = glass * mask;
    col = mix(col, vec4(border_color.rgb, border_color.a), ring);

    COLOR = col;
}
