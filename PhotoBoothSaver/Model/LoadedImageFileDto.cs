using SixLabors.ImageSharp;

namespace PhotoBoothSaver;

public class LoadedImageFileDto : ImageFileDto
{
    public Image File { get; set; }
}