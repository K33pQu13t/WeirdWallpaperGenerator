using System;
using System.Drawing;
using WeirdWallpaperGenerator.DTO;
using WeirdWallpaperGenerator.Helpers;

namespace WeirdWallpaperGenerator.Services.Drawers
{
    public class PrimeFractalDrawer : IDrawer
    {
        private Bitmap _bitmap;
        /// <summary>
        /// image total width
        /// </summary>
        private readonly int _width;
        /// <summary>
        /// image total height
        /// </summary>
        private readonly int _height;
        /// <summary>
        /// size of minimal pattern square to color in pixels
        /// </summary>
        private int _brushSize;

        /// <summary>
        /// pixel Y index where patterns start (included)
        /// </summary>
        private int _patternStartY;
        /// <summary>
        /// pixel Y index where patterns end (included)
        /// </summary>
        private int _patternEndY;

        /// <summary>
        /// how many patterns in width
        /// </summary>
        private int PatternalWidth => _width / _brushSize;
        /// <summary>
        /// how many patterns in height
        /// </summary>
        private int PatternalHeight => (_patternEndY - _patternStartY + 1) / _brushSize;

        /// <summary>
        /// current pixel X position
        /// </summary>
        private int x;
        /// <summary>
        /// current pixel Y position
        /// </summary>
        private int y;

        /// <summary>
        /// X position of pattern
        /// </summary>
        private int PatternalX => x / _brushSize;
        /// <summary>
        /// Y position of pattern. -1 if out of pattern
        /// </summary>
        private int PatternalY {
            get
            {
                if (y < TopOffset)
                    return -1;

                int result = (y - TopOffset) / _brushSize;
                if (result > PatternalHeight)
                    return -1;

                return result;
            }
        } 

        /// <summary>
        /// count of pixels offsetted from the top of the image
        /// </summary>
        private int TopOffset => _patternStartY;

        /// <summary>
        /// total count of patterns in image
        /// </summary>
        private int CountOfPatternsToColor => PatternalWidth * PatternalHeight;

        private Direction _direction;
        private CornerPosition _startPosition;

        private enum Direction
        {
            ToLeftUp,
            ToRightUp,
            ToRightDown,
            ToLeftDown
        }
        public enum CornerPosition
        {
            LeftUp,
            RightUp,
            RightDown,
            LeftDown
        }

        private Color _currentColor;
        private Color _fillInsideColor;
        private Color _fillOutsideColor;

        public PrimeFractalDrawer(PrimeFractalConfigDTO config)
        {
            _height = config.Height.Value;
            _width = config.Width.Value;
            _brushSize = config.BrushSize;

            if (_width % _brushSize != 0)
                throw ExceptionHelper.GetException(
                    nameof(PrimeFractalDrawer),
                    "Constructor",
                    "width must can be divided by brush size without remainder");
            if (_brushSize > _width || _brushSize > _height)
                throw ExceptionHelper.GetException(
                    nameof(PrimeFractalDrawer),
                    "Constructor", 
                    "brush size can't be bigger than picture area");

            _fillInsideColor = config.FillInsideColor;
            _fillOutsideColor = config.FillOutsideColor;
            _startPosition = config.StartPosition;
            SetPattern();
        }

        public string GetConfig()
        {
            return $"{GetType().Name} {_height}, {_width}, {_fillOutsideColor.ToHex()}, {_fillInsideColor.ToHex()}, {_brushSize}, {_startPosition}";
        }

        /// <summary>
        /// sets brush size and determines patterned area
        /// </summary>
        private void SetPattern()
        {
            if (_brushSize == default)
            {
                _brushSize = _width.GCD(_height);
                _patternStartY = 0;
                _patternEndY = _height - 1;
            }
            else if (_width.GCD(_height) != _brushSize)
            {
                int size;
                int newHeight = _height;
                do
                {
                    size = _width.GCD(newHeight--);
                    if (newHeight <= 0)
                        throw ExceptionHelper.GetException(
                            nameof(PrimeFractalDrawer),
                            nameof(SetPattern), 
                            "unable to fit such width, height and brush size");
                }
                while (size != _brushSize);

                int difference = (_height - ++newHeight);
                // that would give a bigger space to bottom edge
                _patternStartY = difference / 2;
                _patternEndY = _height - 1 - (int)Math.Ceiling(difference / 2f);
            }
            else
            {
                _patternStartY = 0;
                _patternEndY = _height - 1;
            }
        }

        /// <summary>
        /// sets start coordinates to color from
        /// </summary>
        /// <param name="newStartPosition"></param>
        public void SetStartPosition(CornerPosition newStartPosition)
        {
            _startPosition = newStartPosition;
            _currentColor = _fillOutsideColor;
            switch (_startPosition)
            {
                case CornerPosition.LeftUp:
                    x = 0;
                    y = _patternStartY;
                    _direction = Direction.ToRightDown;
                    break;
                case CornerPosition.RightUp:
                    x = _width - _brushSize;
                    y = _patternStartY;
                    _direction = Direction.ToLeftDown;
                    break;
                case CornerPosition.RightDown:
                    x = _width - _brushSize;
                    y = _patternEndY - _brushSize + 1;
                    _direction = Direction.ToLeftUp;
                    break;
                case CornerPosition.LeftDown:
                    x = 0;
                    y = _patternEndY - _brushSize + 1;
                    _direction = Direction.ToRightUp;
                    break;
            }
        }

        public Bitmap Draw()
        {
            _bitmap = new Bitmap(_width, _height);
            SetStartPosition(_startPosition);
            
            // draw patterned area
            for (int i = 0; i < CountOfPatternsToColor; i++)
            {
                DrawInternal();
            }
            // draw top and bottom free space
            DrawBackground();

            var result = new Bitmap(_bitmap);
            _bitmap.Dispose();
            return result;
        }

        private void DrawInternal()
        {
            ColorCurrentPattern();

            try
            {
                do
                {
                    //_bitmap.Save($"error {GetConfig()}.png");
                    SetNextCoordinate();
                }
                while (_bitmap.GetPixel(x, y).A != 0);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }

            SwitchColor();
        }
        /// <summary>
        /// colors one brush pattern. Start X and Y is left-up corner of pattern
        /// </summary>
        private void ColorCurrentPattern()
        {
            for (int i = x; i < x + _brushSize; i++)
            {
                for (int j = y; j < y + _brushSize; j++)
                {
                    //if (i >= _width || j >= _height)
                    //{
                    //    //_bitmap.Save($"error {GetConfig()}.png");
                    //    Console.WriteLine();
                    //}
                    //_bitmap.Save($"error {GetConfig()}.png");
                    _bitmap.SetPixel(i, j, _currentColor);
                }
            }
        }

        private void DrawBackground()
        {
            if (PatternalHeight != _height)
            {
                // color upper background
                for (int i = 0; i < _width; i++)
                {
                    for (int j = 0; j < _patternStartY; j++)
                    {
                        _bitmap.SetPixel(i, j, _fillOutsideColor);
                    }
                }

                // color bottom background
                for (int i = 0; i < _width; i++)
                {
                    for (int j = _patternEndY + 1; j < _height; j++)
                    {
                        _bitmap.SetPixel(i, j, _fillOutsideColor);
                    }
                }
            }
        }

        private void SetNextCoordinate()
        {
            switch (_direction)
            {
                case Direction.ToRightDown:
                    if (PatternalX >= PatternalWidth - 1)
                    {
                        // bounce from right bound
                        y += _brushSize;
                        _direction = Direction.ToLeftDown;
                        break;
                    }
                    if (PatternalY >= PatternalHeight - 1)
                    {
                        // bounce from down bound
                        x += _brushSize;
                        _direction = Direction.ToRightUp;
                        break;
                    }
                    // normal move
                    x += _brushSize;
                    y += _brushSize;
                    break;
                case Direction.ToRightUp:
                    if (PatternalX >= PatternalWidth - 1)
                    {
                        // bounce from right bound
                        y -= _brushSize;
                        _direction = Direction.ToLeftUp;
                        break;
                    }
                    if (PatternalY <= 0)
                    {
                        // bounce from up bound
                        x += _brushSize;
                        _direction = Direction.ToRightDown;
                        break;
                    }
                    // normal move
                    x += _brushSize;
                    y -= _brushSize;
                    break;
                case Direction.ToLeftUp:
                    if (PatternalX <= 0)
                    {
                        // bounce from left bound
                        y -= _brushSize;
                        _direction = Direction.ToRightUp;
                        break;
                    }
                    if (PatternalY <= 0)
                    {
                        // bounce from up bound
                        x -= _brushSize;
                        _direction = Direction.ToLeftDown;
                        break;
                    }
                    // normal move
                    x -= _brushSize;
                    y -= _brushSize;
                    break;
                case Direction.ToLeftDown:
                    if (PatternalX <= 0)
                    {
                        // bounce from left bound
                        y += _brushSize;
                        _direction = Direction.ToRightDown;
                        break;
                    }
                    if (PatternalY >= PatternalHeight - 1)
                    {
                        // bounce from down bound
                        x -= _brushSize;
                        _direction = Direction.ToLeftUp;
                        break;
                    }
                    // normal move
                    x -= _brushSize;
                    y += _brushSize;
                    break;
            }
        }

        private void SwitchColor()
        {
            if (_currentColor == _fillInsideColor)
                _currentColor = _fillOutsideColor;
            else
                _currentColor = _fillInsideColor;
        }
    }
}
