using SixLabors.ImageSharp;

namespace PhotoBoothSaver;

public class ImageFileDto
{
    public byte[] Content { get; set; }

    public string SavePath { get; set; }
}