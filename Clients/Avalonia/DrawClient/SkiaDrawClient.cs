﻿namespace Rogue.App.DrawClient
{
    using Avalonia.Controls;
    using Avalonia.Media.Imaging;
    using Avalonia.Threading;
    using MoreLinq;
    using Rogue.Resources;
    using Rogue.Types;
    using Rogue.View.Interfaces;
    using SkiaSharp;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class SkiaDrawClient : IDrawClient
    {
        private static float YUnit = 20f;
        private static float XUnit = 11.5625f;
        private WriteableBitmap ViewportBitmap;
        private Image control;

        SKBitmap drawingBitmap;
        private SKBitmap DrawingBitmap
        {
            get
            {
                if (drawingBitmap == default)
                {
                    drawingBitmap = new SKBitmap(1157, 700, SKColorType.Bgra8888, SKAlphaType.Premul);
                }

                return this.drawingBitmap;
            }

            set
            {
                this.drawingBitmap = value;
            }
        }

        public SkiaDrawClient(WriteableBitmap viewportBitmap, Image image)
        {
            this.ViewportBitmap = viewportBitmap;
            this.control = image;
        }

        private static readonly ConcurrentDictionary<string, SKBitmap> tilesetsCache = new ConcurrentDictionary<string, SKBitmap>();

        private static SKBitmap TileSetByName(string tilesetName)
        {
            if (!tilesetsCache.TryGetValue(tilesetName, out var bitmap))
            {
                var stream = ResourceLoader.Load(tilesetName, tilesetName);
                bitmap = SKBitmap.Decode(stream);

                tilesetsCache.TryAdd(tilesetName, bitmap);
            }

            return bitmap;
        }

        /// <summary>
        /// API клиента отображения
        /// </summary>
        /// <param name="drawSessions"></param>
        public void Draw(IEnumerable<IDrawSession> drawSessions)
        {
            if (drawSessions.Count() == 0)
                return;

            var bitmap = DrawingBitmap;
            var canvas = new SKCanvas(DrawingBitmap);
          
            float fontSize = 20f;
            var font = CommonFont;// SKTypeface.FromFamilyName("Lucida Console");

            foreach (var session in drawSessions)
            {
                if (session.Drawables != null)
                {
                    DrawTiles(canvas, session.Drawables);
                }

                if (session.TextContent != null && session.TextContent.Any())
                {
                    DrawText(canvas, fontSize, font, session);
                }
            }

            canvas.Dispose();
            font.Dispose();

            this.InternalDraw(GetBounds(drawSessions));
        }

        private static void DrawPath(SKCanvas canvas, IDrawablePath path)
        {
            var paint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(path.ForegroundColor.R, path.ForegroundColor.G, path.ForegroundColor.B, path.ForegroundColor.A),
                Style = path.Fill ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
                StrokeWidth = path.Depth
            };

            if (path.PathPredefined == View.Enums.PathPredefined.RoundedRectangle)
            {
                canvas.DrawRoundRect(path.Region.Xf, path.Region.Yf, path.Region.Widthf, path.Region.Heightf, path.Angle, path.Angle, paint);
            }
            else if (path.PathPredefined == View.Enums.PathPredefined.Rectangle)
            {
                canvas.DrawRect(path.Region.Xf, path.Region.Yf, path.Region.Widthf, path.Region.Heightf, paint);
            }
            else if (path.Path.Count() == 2)
            {
                var from = path.Path.First();
                var to = path.Path.Last();

                canvas.DrawLine(new SKPoint(from.Xf, from.Yf), new SKPoint(to.Xf, to.Yf),paint);
            }
            else
            {
                var skPath = new SKPath();
                foreach (var point in path.Path)
                {
                    skPath.LineTo(new SKPoint
                    {
                        X = point.Xf,
                        Y = point.Yf
                    });
                }
                canvas.DrawPath(skPath, paint);
            }
        }

        /// <summary>
        /// Рисование тайлов
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="drawables"></param>
        private static void DrawTiles(SKCanvas canvas, IEnumerable<IDrawable> drawables)
        {
            foreach (var drawable in drawables)
            {
                if (drawable.Container)
                    continue;

                var y = drawable.Region.Y * 24 + 3;
                var x = drawable.Region.X * 24 - 3;

                var tileset = TileSetByName(drawable.Tileset);

                var tileSize = new SKSize
                {
                    Height = (float)drawable.TileSetRegion.Height,
                    Width = (float)drawable.TileSetRegion.Width
                };

                var tilePos = new SKRect
                {
                    Left = drawable.TileSetRegion.Xf,
                    Top = drawable.TileSetRegion.Yf,
                    Size = tileSize
                };

                canvas.DrawBitmap(tileset, tilePos, new SKRect
                {
                    Top = ((float)y) - 24,
                    Left = (float)x,
                    Size = new SKSize
                    {
                        Height = (float)(drawable.Region.Height * 24),
                        Width = (float)(drawable.Region.Width * 24)
                    }
                }, new SKPaint { IsAntialias = true });
            }
        }

        /// <summary>
        /// Рисование текста
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="fontSize"></param>
        /// <param name="font"></param>
        /// <param name="YUnit"></param>
        /// <param name="XUnit"></param>
        /// <param name="session"></param>
        private static void DrawText(SKCanvas canvas, float fontSize, SKTypeface font, IDrawSession session)
        {
            if (session.AutoClear)
            {
                ClearRegion(canvas, session);
            }

            double y = session.SessionRegion.Y * 24 + 3;
            double x = session.SessionRegion.X * 24 - 3;

            foreach (var line in session.TextContent)
            {
                if (line.Region != null)
                {
                    DrawPositionalText(canvas, fontSize, font, line);
                }
                else
                {
                    DrawNonPositionalText(canvas, fontSize, font, y, x, line);
                    y += line.Size;
                }
            }
        }

        /// <summary>
        /// Рисование надписи без собственного позиционирования (как консоль)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="fontSize"></param>
        /// <param name="font"></param>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <param name="line"></param>
        private static void DrawNonPositionalText(SKCanvas canvas, float fontSize, SKTypeface font, double y, double x, IDrawText line)
        {
            foreach (var lne in line.Data)
            {
                foreach (var range in lne.Data)
                {
                    DrawTextRanges(canvas, fontSize, font, y, x, range);
                }
            }
        }

        /// <summary>
        /// Рисование надписи с собственным позиционированием
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="fontSize"></param>
        /// <param name="font"></param>
        /// <param name="drawText"></param>
        private static void DrawPositionalText(SKCanvas canvas, float fontSize, SKTypeface font, IDrawText drawText)
        {
            double y = drawText.Region.Y * 24 + 3;
            double x = drawText.Region.X * 24 - 3;

            foreach (var range in drawText.Data)
            {
                DrawTextRanges(canvas, fontSize, font, y, x, range);
                x += range.Length * range.LetterSpacing;
            }
        }

        /// <summary>
        /// Рисование внутренних отрезков в тексте (отрезки с собственным форматированием)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="fontSize"></param>
        /// <param name="font"></param>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <param name="range"></param>
        private static void DrawTextRanges(SKCanvas canvas, float fontSize, SKTypeface font, double y, double x, IDrawText range)
        {
            var textpaint = new SKPaint
            {
                Typeface = font,
                TextSize = range.Size,
                IsAntialias = true,
                Color = new SKColor(range.ForegroundColor.R, range.ForegroundColor.G, range.ForegroundColor.B, range.ForegroundColor.A),
                Style = SKPaintStyle.Fill
            };

            foreach (var @char in range.StringData)
            {
                canvas.DrawText(@char.ToString(), (float)x, (float)y, textpaint);
                canvas.DrawText(@char.ToString(), (float)x, (float)y, textpaint);

                x += range.LetterSpacing;
            }
        }

        /// <summary>
        /// Очищение региона (закрашивание чёрным)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="YUnit"></param>
        /// <param name="XUnit"></param>
        /// <param name="session"></param>
        private static void ClearRegion(SKCanvas canvas, IDrawSession session)
        {
            var blackPaint = new SKPaint { Color = new SKColor(0, 0, 0, 255) };

            if (session.SessionRegion != null)
            {
                var rect = new SKRect
                {
                    Location = new SKPoint
                    {
                        Y = ((float)session.SessionRegion.Y * 24 + 3),
                        X = ((float)session.SessionRegion.X * 24+3)
                    },
                    Size = new SKSize
                    {
                        Height = (float)(session.SessionRegion.Height * 24),
                        Width = (float)(session.SessionRegion.Width * 24)
                    }
                };

                canvas.DrawRect(rect, blackPaint);
            }
        }
                
        private unsafe void InternalDraw(SKRect bitmapReplaceRegion)
        {
            var width = ViewportBitmap.PixelSize.Width;
            var height = ViewportBitmap.PixelSize.Height;

            var px = (int)(0 * width);
            var py = (int)(0 * height);

            using (var buf = ViewportBitmap.Lock())
            {
                var w = Math.Min(width - px, DrawingBitmap.Width);
                var h = Math.Min(height - py, DrawingBitmap.Height);

                var ptr = (uint*)buf.Address;

                for (var i = (int)bitmapReplaceRegion.Left; i < (int)bitmapReplaceRegion.Left + bitmapReplaceRegion.Size.Width; i++)
                {
                    for (var j = (int)bitmapReplaceRegion.Top; j < (int)bitmapReplaceRegion.Top+ bitmapReplaceRegion.Size.Height; j++)
                    {

                        var pix = DrawingBitmap.GetPixel(i, j);
                        
                        if (pix.Alpha > 200)
                        {
                            var pixPtr = ptr + (j + py) * width + i + px;
                            *pixPtr = (uint)(pix.Blue | pix.Green << 8 | pix.Red << 16 | byte.MaxValue << 24);
                        }
                    }
                }
            }

            control.InvalidateVisual();
        }

        public void Animate(IAnimationSession animationSession)
        {
            void invalidate() { Dispatcher.UIThread.InvokeAsync(() => control.InvalidateVisual()); }

            //var wait = animationSession.Frames.Count() * 25;

            Task.Run(() =>
            {
                foreach (var frame in animationSession.Frames)
                {
                    var bitmap = DrawingBitmap;
                    var canvas = new SKCanvas(DrawingBitmap);
                    DrawTiles(canvas, frame);
                    canvas.Dispose();

                    //if (!PublishManager.IsBlocked)
                    //{
                        InternalDraw(GetBounds(frame));
                        Thread.Sleep(animationSession.Speed);
                    //}
                    //invalidate();
                }

                animationSession.End();
            });
        }

        private static SKRect GetBounds(IEnumerable<IDrawable> drawables)
        {
            var leftUpdate = drawables.Min(session =>
            {
                return session.Region.X;
            });

            var topUpdate = drawables.Min(session =>
            {
                return session.Region.Y;
            });


            var maxX = drawables.MaxBy(session =>
            {
                return session.Region.X;
            }).First();

            var widthUpdate = maxX.Region.X + maxX.Region.Width;

            var maxY = drawables.MaxBy(session =>
            {
                    return session.Region.Y;
            }).First();

            var heightUpdate = maxY.Region.Y + maxY.Region.Height;


            topUpdate -= 1;

            return new SKRect
            {
                Top = (float)(topUpdate * 24),
                Left = (float)(leftUpdate * 24),
                Size = new SKSize
                {
                    Height = (float)((heightUpdate - topUpdate) * 24),
                    Width = (float)((widthUpdate - leftUpdate) * 24)
                }
            };

            //height - абсолютная величина СКОЛЬКО надо нарисовать
            //какой-то уебанский косяк, чес слово
            //return new SKRect
            //{
            //    Top = topUpdate * 24 -24+3,
            //    Left = leftUpdate * 24,
            //    Size = new SKSize
            //    {
            //        Height = heightUpdate-24+3,
            //        Width = widthUpdate
            //    }
            //};
        }

        private static SKRect GetBounds(IEnumerable<IDrawSession> drawSessions)
        {
            var leftUpdate = drawSessions.Min(session =>
            {
                if (session.Drawables.IsNotEmpty())
                {
                    return session.Drawables.Min(x => x.Region.X);
                }
                else
                {
                    return session.SessionRegion.X;
                }
            });

            var topUpdate = drawSessions.Min(session =>
            {
                if (session.Drawables.IsNotEmpty())
                {
                    return session.Drawables.Min(x => x.Region.Y);
                }
                else
                {
                    return session.SessionRegion.Y;
                }
            });


            var maxX = drawSessions.MaxBy(session =>
            {
                if (session.Drawables.IsNotEmpty())
                {
                    var f = session.Drawables.MaxBy(x => x.Region.X + x.Region.Width).First();
                    return f.Region.X + f.Region.Width;
                }
                else
                {
                    return session.SessionRegion.X+session.SessionRegion.Width;
                }
            }).First();

            double endOfX = 0f;

            if (maxX.Drawables.IsNotEmpty())
            {
                var maxXDrawable = maxX.Drawables.MaxBy(x => x.Region.X).FirstOrDefault();
                endOfX = maxXDrawable.Region.X + maxXDrawable.Region.Width;
            }
            else
            {
                endOfX = maxX.SessionRegion.X + maxX.SessionRegion.Width;
            }

            var maxY = drawSessions.MaxBy(session =>
            {
                if (session.Drawables.IsNotEmpty())
                {
                    return session.Drawables.MaxBy(x => x.Region.Y).First().Region.Y;
                }
                else
                {
                    return session.SessionRegion.Y + session.SessionRegion.Height;
                }
            }).First();

            double heightUpdate = 0f;
            if (maxY.Drawables.IsNotEmpty())
            {
                var maxYDrawable = maxY.Drawables.MaxBy(x => x.Region.Y).First();
                heightUpdate = maxYDrawable.Region.Y+ maxYDrawable.Region.Height;
            }
            else
            {
                heightUpdate = maxY.SessionRegion.Y+ maxY.SessionRegion.Height;
            }

            if (topUpdate != 0)
            {
                topUpdate -= 1;
            }

            return new SKRect
            {
                Top = ((float)topUpdate * 24),
                Left = ((float)leftUpdate * 24),
                Size = new SKSize
                {
                    Height = (float)((heightUpdate - topUpdate) * 24),
                    Width = (float)((endOfX - leftUpdate) * 24)
                }
            };
        }

        public void SetScene(IScene scene)
        {
            throw new NotImplementedException();
        }

        public Point MeasureText(IDrawText drawText)
        {
            throw new NotImplementedException();
        }

        public void MoveCamera(Direction direction)
        {
            throw new NotImplementedException();
        }

        public void MoveCamera(Direction direction, bool stop)
        {
            throw new NotImplementedException();
        }

        public void SetCameraSpeed(double speed)
        {
            throw new NotImplementedException();
        }

        public void ResetCamera()
        {
            throw new NotImplementedException();
        }

        public Point MeasureImage(string image)
        {
            throw new NotImplementedException();
        }

        public void SaveObject(ISceneObject sceneObject, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveObject(ISceneObject sceneObject, string path, Point offset)
        {
            throw new NotImplementedException();
        }

        public void SaveObject(ISceneObject sceneObject, string path, Point offset, string runtimeCacheName = "")
        {
            throw new NotImplementedException();
        }

        public void SetCamera(double x, double y)
        {
            throw new NotImplementedException();
        }

        public void StopMoveCamera()
        {
            throw new NotImplementedException();
        }

        public void Drag(ISceneObject @object, ISceneObject area = null)
        {
            throw new NotImplementedException();
        }

        public void Drop()
        {
            throw new NotImplementedException();
        }

        public void SetCursor(string texture)
        {
            throw new NotImplementedException();
        }

        public void CacheObject(ISceneObject @object)
        {
            throw new NotImplementedException();
        }

        public static SKTypeface CommonFont
        {
            get
            {
                SKTypeface result;

                var fontStream = ResourceLoader.Load("Rogue.Resources.Fonts.Common.otf");

                result = SKTypeface.FromStream(fontStream);                
                return result;
            }
        }

        public double CameraOffsetX { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double CameraOffsetY { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double CameraOffsetLimitX => throw new NotImplementedException();

        public double CameraOffsetLimitY => throw new NotImplementedException();
    }
}