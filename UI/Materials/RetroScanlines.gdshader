shader_type canvas_item;
render_mode blend_mix;

uniform float scanline_count : hint_range(100.0, 1000.0) = 480.0;
uniform float scanline_strength : hint_range(0.0, 1.0) = 0.5;
uniform float scanline_speed : hint_range(0.0, 2.0) = 0.1;
uniform vec3 tint_color : source_color = vec3(1.0, 1.0, 1.0);

void fragment() {
    // Get base color
    vec4 color = texture(TEXTURE, UV);
    
    // Animated scanline position
    float scanline_offset = TIME * scanline_speed;
    
    // Calculate scanline
    float scanline = sin((UV.y + scanline_offset) * scanline_count * 3.14159) * 0.5 + 0.5;
    scanline = mix(1.0, scanline, scanline_strength);
    
    // Apply scanline darkening
    color.rgb *= scanline;
    
    // Apply color tint
    color.rgb *= tint_color;
    
    COLOR = color;
}