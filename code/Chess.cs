using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace 西洋跳棋
{
    public class Chess
    {
        private int _currentY, _currentX;
        private int _oldY, _oldX;
        private bool _canMove;
        private bool _isKing;
        private bool _canEat;
        private bool _alive;
        public bool[] moveDirection = new bool[4] { false, false, false, false };
        public bool[] eatDirection = new bool[4] { false, false, false, false };

        //private enum chessColor { BLUE, Red };

        //private chessColor cc;
        private int _type;   //0是玩家棋子，1是电脑棋子

        public Chess(int Y, int X, int type, bool a = true, bool ik = false, bool ce = false, bool cm = false)
        {
            _currentY = Y;
            _currentX = X;
            _type = type;
            _alive = a;
            _isKing = ik;
            _canEat = ce;
            _canMove = cm;
        }

        public int CurrentY
        {
            get { return _currentY; }
            set { _currentY = value; }
        }

        public int Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int CurrentX
        {
            get { return _currentX; }
            set { _currentX = value; }
        }

        public int OldY
        {
            get { return _oldY; }
            set { _oldY = value; }
        }

        public int OldX
        {
            get { return _oldX; }
            set { _oldX = value; }
        }

        public bool CanMove
        {
            get { return _canMove; }
            set { _canMove = value; }
        }

        public bool IsKing
        {
            get { return _isKing; }
            set { _isKing = value; }
        }

        public bool CanEat
        {
            get { return _canEat; }
            set { _canEat = value; }
        }

        public bool Alive
        {
            get { return _alive; }
            set { _alive = value; }
        }

        public void updateKing(ref int[,] board)
        {
            if (_isKing == true)
            {
                return;
            }
            switch (_type)
            {
                case 0:
                    {
                        if (_currentY == 0)
                        {
                            _isKing = true;
                        }
                        break;
                    }
                case 1:
                    {
                        if (_currentY == 7)
                        {
                            _isKing = true;
                        }
                        break;
                    }
            }
        }

        public void upDateCanEat(ref int[,] board)
        {
            int x, xx, yy, y;
            switch (_type)
            {
                case 0:
                    {
                        if (_isKing)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                x = AI.w[i, 0];
                                y = AI.w[i, 1];
                                xx = _currentX + 2 * x;
                                yy = _currentY + 2 * y;
                                if ((xx >= 0) && (xx <= 7) && (yy <= 7)
                                    && (yy >= 0) && (board[yy, xx] == -1) && (board[_currentY + y, _currentX + x] == 1))
                                {
                                    eatDirection[i] = true;
                                }
                                else
                                    eatDirection[i] = false;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                x = AI.w[i, 0];
                                y = AI.w[i, 1];
                                xx = _currentX + 2 * x;
                                yy = _currentY + 2 * y;
                                if ((xx >= 0) && (xx <= 7) && (yy <= 7) && (yy >= 0)
                                    && (board[yy, xx] == -1) && (board[_currentY + y, _currentX + x] == 1))
                                {
                                    eatDirection[i] = true;
                                }
                                else
                                    eatDirection[i] = false;
                            }
                            eatDirection[2] = eatDirection[3] = false;
                        }
                        if ((eatDirection[0] == true) || (eatDirection[1] == true)
                                    || (eatDirection[2] == true) || (eatDirection[3] == true))
                        {
                            CanEat = true;
                        }
                        else
                            CanEat = false;
                        break;
                    }
                case 1:
                    {
                        if (_isKing)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                x = AI.w[i, 0];
                                y = AI.w[i, 1];
                                xx = _currentX + 2 * x;
                                yy = _currentY + 2 * y;
                                if ((xx >= 0) && (xx <= 7) && (yy <= 7) && (yy >= 0)
                                    && (board[yy, xx] == -1) && (board[_currentY + y, _currentX + x] == 0))
                                {
                                    eatDirection[i] = true;
                                }
                                else
                                    eatDirection[i] = false;
                            }
                        }
                        else
                        {
                            for (int i = 2; i < 4; i++)
                            {
                                x = AI.w[i, 0];
                                y = AI.w[i, 1];
                                xx = _currentX + 2 * x;
                                yy = _currentY + 2 * y;
                                if ((xx >= 0) && (xx <= 7) && (yy <= 7) && (yy >= 0)
                                    && (board[yy, xx] == -1) && (board[_currentY + y, _currentX + x] == 0))
                                {
                                    eatDirection[i] = true;
                                }
                                else
                                    eatDirection[i] = false;
                            }
                            eatDirection[0] = eatDirection[1] = false;
                        }
                        if ((eatDirection[0] == true) || (eatDirection[1] == true)
                                   || (eatDirection[2] == true) || (eatDirection[3] == true))
                        {
                            CanEat = true;
                        }
                        else
                            CanEat = false;
                        break;
                    }
            }
        }
    }
}