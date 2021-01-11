# MUI
Modification of the Unity builtin UI shader that fixes the edges of quads being visible on text characters 

### Example

##### Before
![before](https://cdn.discordapp.com/attachments/498962324670119937/581792260707057674/Unity_j27ahbqXnF.png)
##### After
![after](https://cdn.discordapp.com/attachments/498962324670119937/581792290939338752/Unity_O3oMnJ7bKM.png)

### Why text rendering gets broken and what this does
When you use MSAA as a solution for anti-aliasing in games, it comes with a less well known side effect. That is, if you have a value getting interpolated across a surface and some MSAA samples fall outside of the bounds of the rasterized area on the edges, the interpolated values will get extrapolated. This means that if you have a quad with texture coordinates going from (0, 0) in the top left to (1, 1) in the bottom right, it is entirely valid to have some pixels that receive extrapolated values at the edges of (1.1, 0.5) for instance.

This is not an issue most of the time, but in this case, Unity only has 1 pixel of padding on the texture atlas they generate for text glyphs. Because of this, if you overshoot the texture coordinates by even 1 pixel in the atlas, you have a good chance of sampling a neighboring glyph in the atlas. 

This small modification to the builtin UI shader fixes that using a technique used by Valve for fixing extrapolation of mesh normals. The description of their fix can be found [on slide 44 of this slideshow](http://media.steampowered.com/apps/valve/2015/Alex_Vlachos_Advanced_VR_Rendering_GDC2015.pdf). But basically, this will switch to centroid interpolation which does not extrapolate if it detects a sample will fall out of the bounds of a given triangle. Centroid interpolation does not have valid derivatives, so you generally do not want to use it unless you need to. 
