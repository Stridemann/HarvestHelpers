using System;
using System.Drawing;
using System.IO;
using ExileCore.Shared.Enums;
using Color = SharpDX.Color;
using Graphics = ExileCore.Graphics;
using RectangleF = SharpDX.RectangleF;
using Vector2 = System.Numerics.Vector2;

namespace HarvestHelpers
{
    public class MapController
    {
        private readonly Graphics _graphics;
        private RectangleF _imageDrawFrame;
        private float _scale;
        private readonly string _bgImg;
        private Size _bmSize = new Size(42, 42);
        public bool DrawDispensers { get; set; } = true;
        public bool DrawPylons { get; set; } = true;
        public bool DrawCollectors { get; set; } = true;
        public bool DrawStorage { get; set; } = true;
        public bool DrawLinks { get; set; } = true;
        public bool DrawSeeds { get; set; } = true;

        public MapController(Graphics graphics, string directoryFullName)
        {
            _graphics = graphics;
            _bgImg = Path.Combine(directoryFullName, "BgImage.png");
            _graphics.InitImage(_bgImg, false);
        }

        public void Draw(RectangleF windowDrawFrame)
        {
            var bmSizeWidth = _bmSize.Width;
            var bmSizeHeight = _bmSize.Height;

            var scaleX = windowDrawFrame.Width / bmSizeWidth;
            var scaleY = windowDrawFrame.Height / bmSizeHeight;
            _scale = Math.Min(scaleX, scaleY);

            var width = bmSizeWidth * _scale;
            var height = bmSizeHeight * _scale;

            _imageDrawFrame = new RectangleF(windowDrawFrame.X, windowDrawFrame.Y, width, height);

            const int lines = 42;
            var stepY = _imageDrawFrame.Height / lines;

            for (var i = 1; i < lines; i++)
            {
                var drawPosY = _imageDrawFrame.Y + i * stepY;
                _graphics.DrawLine(new Vector2(_imageDrawFrame.X, drawPosY),
                    new Vector2(_imageDrawFrame.Right, drawPosY), 1, Color.Gray);
            }


            var stepX = _imageDrawFrame.Width / lines;
            for (var i = 1; i < lines; i++)
            {
                var drawPosX = _imageDrawFrame.X + i * stepX;
                _graphics.DrawLine(new Vector2(drawPosX, _imageDrawFrame.Y),
                    new Vector2(drawPosX, _imageDrawFrame.Bottom), 1, Color.Gray);
            }

            _graphics.DrawImage(Path.GetFileName(_bgImg), _imageDrawFrame, Color.White);
        }


        public RectangleF DrawBoxOnMap(SharpDX.Vector2 screenPos, float size, Color color)
        {
            var sizeScaled = size * _scale;
            var rectangleF = new RectangleF(screenPos.X - sizeScaled / 2, screenPos.Y - sizeScaled / 2, sizeScaled, sizeScaled);
            _graphics.DrawBox(
                rectangleF,
                color);
            return rectangleF;
        }

        public void DrawFrameOnMap(SharpDX.Vector2 screenPos, float size, int border, Color color)
        {
            var sizeScaled = size * _scale;
            _graphics.DrawFrame(
                new RectangleF(screenPos.X - sizeScaled / 2, screenPos.Y - sizeScaled / 2, sizeScaled, sizeScaled), color,
                border);
        }

        public void DrawTextOnMap(string text, SharpDX.Vector2 screenPos, Color color, int height, FontAlign align = FontAlign.Left)
        {
            if (align == FontAlign.Center)
            {
                screenPos.Y -= _graphics.MeasureText(text, height).Y / 2;
            }

            _graphics.DrawText(text, screenPos, color, height, align);
        }

        public void DrawLine(SharpDX.Vector2 p1, SharpDX.Vector2 p2, float lineWidth, Color color)
        {
            _graphics.DrawLine(p1, p2, lineWidth, color);
        }

        public SharpDX.Vector2 GridPosToMapPos(SharpDX.Vector2 gridPos)
        {
            gridPos.X -= Constants.IMAGE_CUTOFF_LEFT;
            gridPos.Y -= Constants.IMAGE_CUTOFF_BOT;

            gridPos /= Constants.GRID_WIDTH;

            gridPos.Y = 1 - gridPos.Y;

            return new SharpDX.Vector2(_imageDrawFrame.X + _imageDrawFrame.Width * gridPos.X,
                _imageDrawFrame.Y + _imageDrawFrame.Height * gridPos.Y);
        }
    }
}