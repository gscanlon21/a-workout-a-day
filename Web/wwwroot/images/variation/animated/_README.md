# Animated Images

## License

These photos are licensed for use by ascallion21 only.

## Adding Animated Images

License a stock video from a stock video site (sa. https://stock.adobe.com).
- Try to find a video with 30-60fps.
- Try to find a video that naturally loops.
- Try to find a video with adaquate lighting.
- Try to find a video less than 10s in length.
- Try to find a video that is not extreme slo-mo.
- Try to find a video where the whole subject is in frame.
- Try to find a video where the subject's face is visible.
- Try to find a video with little background movement. Only the subject should be moving.

### Converting a .mov video to a .webp animated image

#### Using FastFlix (https://github.com/cdgriffith/FastFlix)

1. Select the source file. 
1. Select the standard profile "WebP".
1. Set the "Resolution"'s "Width" to 400. Keep the aspect ratio.
1. Set the qscale to 90. All the other compression/quality/fps presets should be their default value.
1. In the "Custom ffmpeg options" input field, put "-loop 0" to infinitely loop the animated image.
1. Convert the file.
1. If the file is over 10MB, lower the qscale by 5 and try again.

