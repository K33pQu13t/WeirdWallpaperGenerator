using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FractalGenerator.BackgroundDrawers
{
    public class PrimeFractalDrawer : IDrawer
    {
        private Bitmap _bitmap;
        private readonly int _width;
        private readonly int _height;
        private int _patternStartY;
        private int _patternEndY;
        private int PatternalWidth => _width / _brushSize;
        /// <summary>
        /// height of bitmap area with pattern 
        /// </summary>
        private int PatternalHeight => (_patternEndY - _patternStartY) / _brushSize;

        private int x;
        private int y;
        /// <summary>
        /// x position of pattern square
        /// </summary>
        private int PatternalX => x / _brushSize;
        /// <summary>
        /// y position of pattern square
        /// </summary>
        private int PatternalY => y / _brushSize;

        private int CountOfPatternsToColor => _width * (_patternEndY-_patternStartY) / (_brushSize * _brushSize);
        private Direction _direction;
        private CornerPosition _startPosition;
        private int _brushSize;

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

        public PrimeFractalDrawer(int height, int width, int brushSize = 0)
        {
            _height = height;
            _width = width;
            SetPattern(height, width, brushSize);

            _fillOutsideColor = Color.Black;
            _fillInsideColor = Color.White;
        }

        /// <param name="height">total height of bitmap</param>
        /// <param name="width">total width of bitmap</param>
        /// <param name="fillOutsideColor">color of outside fill</param>
        /// <param name="fillInsideColor">color of inside fill</param>
        /// <param name="brushSize">the size of each minimal colored square</param>
        /// <param name="startPosition">corner to start from. Actually changing corner flips an image</param>
        public PrimeFractalDrawer(int height, int width,
            Color? fillOutsideColor = null, Color? fillInsideColor = null,
            int brushSize = 0,
            CornerPosition startPosition = CornerPosition.LeftUp)
        {
            _height = height;
            _width = width;
            SetPattern(height, width, brushSize);

            _fillOutsideColor = fillOutsideColor ?? Color.Black;
            _fillInsideColor = fillInsideColor ?? Color.White;

            _startPosition = startPosition;
        }

        /// <param name="height">total height of bitmap</param>
        /// <param name="width">total width of bitmap</param>
        /// <param name="fillOutsideColor">hex color of outside fill</param>
        /// <param name="fillInsideColor">hex color of inside fill</param>
        /// <param name="brushSize">the size of each minimal colored square</param>
        /// <param name="startPosition">corner to start from. Actually changing corner flips an image</param>
        public PrimeFractalDrawer(int height, int width,
            string fillOutsideColor = null, string fillInsideColor = null,
            int brushSize = 0,
            CornerPosition startPosition = CornerPosition.LeftUp)
        {
            _height = height;
            _width = width;
            SetPattern(height, width, brushSize);

            ColorConverter converter = new ColorConverter();
            _fillOutsideColor = !string.IsNullOrEmpty(fillOutsideColor) ? 
                (Color)converter.ConvertFromString(fillOutsideColor) : Color.Black;
            _fillInsideColor = !string.IsNullOrEmpty(fillInsideColor) ?
                (Color)converter.ConvertFromString(fillInsideColor) : Color.White;

            _startPosition = startPosition;
        }

        public string GetConfig()
        {
            return $"{_height}, {_width}, {_fillOutsideColor}, {_fillInsideColor}, {_brushSize}, {_startPosition}";
        }

        private void SetPattern(int height, int width, int brushSize)
        {
            if (brushSize == default)
            {
                _brushSize = GCD(width, height);
                _patternStartY = 0;
                _patternEndY = height - 1;
            }
            else if (GCD(width, height) != brushSize)
            {
                int size;
                int newHeight = height;
                do
                {
                    size = GCD(width, --newHeight);
                    if (newHeight <= 0)
                        throw new Exception("unable to fit such width height and brush size");
                }
                while (size != brushSize);
                _brushSize = brushSize;
                _patternStartY = (height - newHeight) / 2;
                _patternEndY = height - _patternStartY;
            }
            else
            {
                _brushSize = brushSize;
                _patternStartY = 0;
                _patternEndY = height - 1;
            }
        }

        private int GCD(int num1, int num2)
        {
            int gcd = 0;
            for (int i = 1; i < (num2 * num1 + 1); i++)
            {
                if (num1 % i == 0 && num2 % i == 0)
                {
                    gcd = i;
                }
            }
            return gcd;
        }

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
                    x = _width - 1;
                    y = _patternStartY;
                    _direction = Direction.ToLeftDown;
                    break;
                case CornerPosition.RightDown:
                    x = _width - 1;
                    y = _patternEndY - 1;
                    _direction = Direction.ToLeftUp;
                    break;
                case CornerPosition.LeftDown:
                    x = 0;
                    y = _patternEndY - 1;
                    _direction = Direction.ToRightUp;
                    break;
            }
        }

        public Bitmap Draw()
        {
            _bitmap = new Bitmap(_width, _height);
            SetStartPosition(_startPosition);
            
            try
            {
                for (int i = 0; i < CountOfPatternsToColor; i++)
                {
                    DrawInternal();
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _bitmap.Save("failed_try.png");
                throw ex;
            }
            DrawBackground();

            var result = new Bitmap(_bitmap);
            _bitmap.Dispose();
            return result;
        }

        private void DrawInternal()
        {
            //color one brush pattern
            for (int i = x; i < x + _brushSize; i++)
            {
                for (int j = y; j < y + _brushSize; j++)
                {
                    _bitmap.SetPixel(i, j, _currentColor);
                }
            }
            //_bitmap.Save("temp.png");

            //get next coordinate to color
            try
            {
                do
                {
                    SetNextCoordinate();
                }
                while (_bitmap.GetPixel(x, y).A != 0);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }

            //switch next color
            if (_currentColor == _fillInsideColor)
                _currentColor = _fillOutsideColor;
            else
                _currentColor = _fillInsideColor;
        }

        private void DrawBackground()
        {
            if (_patternStartY != default)
            {
                //color upper background
                for (int i = 0; i < _width; i++)
                {
                    for (int j = 0; j < _patternStartY; j++)
                    {
                        _bitmap.SetPixel(i, j, _fillOutsideColor);
                    }
                }

                //color bottom background
                for (int i = 0; i < _width; i++)
                {
                    for (int j = _patternEndY; j < _height; j++)
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
                        //bounce from right bound
                        y += _brushSize;
                        _direction = Direction.ToLeftDown;
                        break;
                    }
                    if (PatternalY >= PatternalHeight - 1)
                    {
                        //bounce from down bound
                        x += _brushSize;
                        _direction = Direction.ToRightUp;
                        break;
                    }
                    //normal move
                    x += _brushSize;
                    y += _brushSize;
                    break;
                case Direction.ToRightUp:
                    if (PatternalX >= PatternalWidth - 1)
                    {
                        //bounce from right bound
                        y -= _brushSize;
                        _direction = Direction.ToLeftUp;
                        break;
                    }
                    if (PatternalY <= 0)
                    {
                        //bounce from up bound
                        x += _brushSize;
                        _direction = Direction.ToRightDown;
                        break;
                    }
                    
                    //normal move
                    x += _brushSize;
                    y -= _brushSize;
                    break;
                case Direction.ToLeftUp:
                    if (PatternalX <= 0)
                    {
                        //bounce from left bound
                        y -= _brushSize;
                        _direction = Direction.ToRightUp;
                        break;
                    }
                    if (PatternalY <= 0)
                    {
                        //bounce from up bound
                        x -= _brushSize;
                        _direction = Direction.ToLeftDown;
                        break;
                    }
                    //normal move
                    x -= _brushSize;
                    y -= _brushSize;
                    break;
                case Direction.ToLeftDown:
                    if (PatternalX <= 0)
                    {
                        //bounce from left bound
                        y += _brushSize;
                        _direction = Direction.ToRightDown;
                        break;
                    }
                    if (PatternalY >= PatternalHeight - 1)
                    {
                        //bounce from down bound
                        x -= _brushSize;
                        _direction = Direction.ToLeftUp;
                        break;
                    }
                    //normal move
                    x -= _brushSize;
                    y += _brushSize;
                    break;
            }
        }
    }

    struct Coordinate
    {
        public int x;
        public int y;
    }
}
