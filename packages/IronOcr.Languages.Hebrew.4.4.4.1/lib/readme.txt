C# Fully Featured OCR Example with Hebrew Language Support:

using IronOcr;

//..

var Ocr = new AdvancedOcr()
{
    CleanBackgroundNoise = true,
    EnhanceContrast = true,
    EnhanceResolution = true,
    Language = IronOcr.Languages.Hebrew.OcrLanguagePack,
    Strategy = IronOcr.AdvancedOcr.OcrStrategy.Advanced,
    ColorSpace = AdvancedOcr.OcrColorSpace.Color,
    DetectWhiteTextOnDarkBackgrounds = true,
    InputImageType = AdvancedOcr.InputTypes.AutoDetect,
    RotateAndStraighten = true,
    ReadBarCodes = true,
    ColorDepth =4
};

var testImage = @"C:\path\to\document\image.png";


var Results = Ocr.Read(testImage);

var Barcodes = Results.Barcodes.Select(b => b.Value);
var TextContent = Results.Text;

Console.WriteLine(TextContent);