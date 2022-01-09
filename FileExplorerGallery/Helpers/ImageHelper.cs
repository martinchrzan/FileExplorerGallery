using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorerGallery.Helpers
{
    [Export(typeof(ImageHelper))]
    public class ImageHelper
    {
        public Task<bool> SaveRotatedImage(string pathToImage, int rotationInAngles)
        {
            return Task.Run(() =>
            {
                var tempFile = Path.GetTempFileName();
                using (Image image = Image.FromFile(pathToImage))
                {
                    var success = true;
                    var sourceFormat = image.RawFormat;
                    
                    EncoderParameters encoderParams = null;
                    try
                    {
                        if (sourceFormat.Guid == ImageFormat.Jpeg.Guid)
                        {
                            long rotation = 0;

                            if (rotationInAngles % 270 == 0)
                            {
                                rotation = (long)EncoderValue.TransformRotate90;
                            }
                            else if (rotationInAngles % 180 == 0)
                            {
                                rotation = (long)EncoderValue.TransformRotate180;
                            }
                            else if (rotationInAngles % 90 == 0)
                            {
                                rotation = (long)EncoderValue.TransformRotate270;
                            }

                            encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Transformation, rotation);
                            image.Save(tempFile, GetEncoder(sourceFormat), encoderParams);
                        }
                        else
                        {
                            if (rotationInAngles % 270 == 0)
                            {
                                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            }
                            else if (rotationInAngles % 180 == 0)
                            {
                                image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            }
                            else if (rotationInAngles % 90 == 0)
                            {
                                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            }

                            image.Save(tempFile, sourceFormat);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Failed to save rotated image " + pathToImage + " to temp file: " + tempFile, ex);
                        success = false;
                    }
                    finally
                    {
                        if (encoderParams != null)
                        {
                            encoderParams.Dispose();
                        }
                    }

                    if (!success)
                    {
                        return false;
                    }
                }

                try
                {
                    // cannot save into the same image, remove the original and replace with the temp one
                    File.Delete(pathToImage);
                    File.Move(tempFile, pathToImage);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to remove selected image and replace it with rotated version. Original image: " + pathToImage + " rotated temp image: " + tempFile, ex);
                    return false;
                }
                return true;
            });
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            foreach (var info in ImageCodecInfo.GetImageEncoders())
                if (info.FormatID == format.Guid)
                    return info;
            return null;
        }
    }
}
