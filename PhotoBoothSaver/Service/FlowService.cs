using System.Text;
using System.Threading.Tasks.Dataflow;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace PhotoBoothSaver.Service;

public class FlowService
{
    private readonly TransformBlock<FormFileDto, ImageFileDto> _formTransformBlock;

    private static readonly ExecutionDataflowBlockOptions Options = new()
    {
        BoundedCapacity = 1000,
        MaxDegreeOfParallelism = Environment.ProcessorCount,
    };

    private static readonly Rectangle Crop = new ((1920-960)/2, 0, 960, 1080);
    
    public FlowService(ILogger<FlowService> logger)
    {
        ILogger<FlowService> logger1 = logger;
        
        _formTransformBlock = new TransformBlock<FormFileDto, ImageFileDto>(dto =>
        {
            var form = dto.File;
            
            using var memStream = new MemoryStream(form);
            
            memStream.Position = 0;
                        
            byte[] contents = memStream.ToArray();
            
            var imageDto = new ImageFileDto
            {
                Content = contents,
                SavePath = dto.SavePath
            };
            return imageDto;
        }, Options);
        
        TransformBlock<ImageFileDto, LoadedImageFileDto> imageLoadingBlock = new(dto =>
        {
            var image = new LoadedImageFileDto
            {
                Content = dto.Content,
                SavePath = dto.SavePath,
                File = Image.Load(new DecoderOptions(), new ReadOnlySpan<byte>(dto.Content))
            };
            return image;
        }, Options);
        
        TransformBlock<LoadedImageFileDto, LoadedImageFileDto> imageProcessingBlock = new(img =>
        {
            img.File.Mutate(x => x.Crop(cropRectangle: Crop));
            return img;
        }, Options);
        
        
        ActionBlock<LoadedImageFileDto> imageSaveBlock = new(item =>
        {
            Directory.CreateDirectory(item.SavePath);
            item.File.SaveAsWebp(item.SavePath);
        }, Options);
        
        _formTransformBlock.LinkTo(imageLoadingBlock);
        imageLoadingBlock.LinkTo(imageProcessingBlock);
        imageProcessingBlock.LinkTo(imageSaveBlock);
    }

    public void Queue(FormFileDto dto)
    {
        _formTransformBlock.Post(dto);
    }
}