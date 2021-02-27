using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace 西洋跳棋
{
    /// <summary>
    /// 棋盘类，包含所有棋盘信息
    /// </summary>
    public class Boards
    {
        public static bool backstone = false;    //用于限定只能悔一次棋
        public static int[,] boardState = new int[8, 8];      //模拟棋盘，二维数组，8x8

        //以下为C#绘图信息
        private static Graphics mg;

        private static Bitmap bufferBmp;
        public static ArrayList blueArray = new ArrayList(12);
        public static ArrayList redArray = new ArrayList(12);

        private static Bitmap bs;
        private static Bitmap rs;
        private static Bitmap bks;
        private static Bitmap rks;
        private static Bitmap bd;
        private static Bitmap cursor;
        private static Bitmap currentChecker;

        //用于标记轮到哪方（蓝(0)或红方(1)）下棋
        public static bool playerTurn = true;

        //胜负标志
        public static bool winflag = false;

        public Boards(Graphics g)
        {
            mg = g;
            resetBoards();

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream bStream = myAssembly.GetManifestResourceStream("Checkers.Resources.blue.png");
            Stream rStream = myAssembly.GetManifestResourceStream("Checkers.Resources.red.png");
            Stream bkStream = myAssembly.GetManifestResourceStream("Checkers.Resources.blueking.png");
            Stream rkStream = myAssembly.GetManifestResourceStream("Checkers.Resources.redking.png");
            bs = new Bitmap(bStream);
            rs = new Bitmap(rStream);
            bks = new Bitmap(bkStream);
            rks = new Bitmap(rkStream);

            Stream myStream = myAssembly.GetManifestResourceStream("Checkers.Resources.board.png");
            bd = new Bitmap(myStream);
            myStream.Close();

            Stream csStream = myAssembly.GetManifestResourceStream("Checkers.Resources.kh.png");
            cursor = new Bitmap(csStream);
            csStream.Close();

            Stream ccStream = myAssembly.GetManifestResourceStream("Checkers.Resources.choose.png");
            currentChecker = new Bitmap(ccStream);
            ccStream.Close();

            bStream.Close();
            rStream.Close();
            bkStream.Close();
            rkStream.Close();
        }

        /// <summary>
        /// 重设棋盘
        /// </summary>
        public void resetBoards()
        {
            winflag = false;
            blueArray.Clear();
            redArray.Clear();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    boardState[i, j] = -1;
                }
            }
            for (int i = 1; i < 8; i += 2)
            {
                boardState[0, i] = 1;
                redArray.Add(new Chess(0, i, 1));
            }
            for (int i = 0; i < 8; i += 2)
            {
                boardState[1, i] = 1;
                redArray.Add(new Chess(1, i, 1));
            }
            for (int i = 1; i < 8; i += 2)
            {
                boardState[2, i] = 1;
                redArray.Add(new Chess(2, i, 1));
            }

            for (int i = 0; i < 8; i += 2)
            {
                boardState[5, i] = 0;
                blueArray.Add(new Chess(5, i, 0));
            }
            for (int i = 1; i < 8; i += 2)
            {
                boardState[6, i] = 0;
                blueArray.Add(new Chess(6, i, 0));
            }
            for (int i = 0; i < 8; i += 2)
            {
                boardState[7, i] = 0;
                blueArray.Add(new Chess(7, i, 0));
            }
        }

        /// <summary>
        /// 绘制棋盘
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void DrawBoard(int x = -1, int y = -1)
        {
            int eat = AI.findCanEat(ref Boards.blueArray, ref Boards.boardState, 0);
            AI.findCanMove(ref Boards.blueArray, ref Boards.boardState, 0);
            Checkers.rednum = 0;
            Checkers.bluenum = 0;
            Checkers.redkingnum = 0;
            Checkers.bluekingnum = 0;
            bufferBmp = new Bitmap(630, 620);
            Graphics g = Graphics.FromImage(bufferBmp);

            g.DrawImage(bd, 30, 20, bd.Width, bd.Height);

            foreach (Chess c in blueArray)
            {
                if (c.Alive)
                {
                    if (!c.IsKing)
                    {
                        g.DrawImage(bs, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, bs.Width, bs.Height);
                        Checkers.bluenum++;
                    }
                    else
                    {
                        g.DrawImage(bks, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, bs.Width, bs.Height);
                        Checkers.bluekingnum++;
                    }
                    if (eat > 0)
                    {
                        if (c.CanEat)
                        {
                            if (c.CurrentX == x && c.CurrentY == y)
                                g.DrawImage(currentChecker, x * 75 + 30, y * 75 + 20, bs.Width, bs.Height);
                            else
                                g.DrawImage(cursor, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, cursor.Width, cursor.Height);
                        }
                    }
                    else if (c.CanMove)
                    {
                        if (c.CurrentX == x && c.CurrentY == y)
                            g.DrawImage(currentChecker, x * 75 + 30, y * 75 + 20, bs.Width, bs.Height);
                        else
                            g.DrawImage(cursor, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, cursor.Width, cursor.Height);
                    }
                }
            }
            foreach (Chess c in Boards.redArray)
            {
                if (c.Alive)
                {
                    if (!c.IsKing)
                    {
                        g.DrawImage(rs, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, rs.Width, rs.Height);
                        Checkers.rednum++;
                    }
                    else
                    {
                        g.DrawImage(rks, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, bs.Width, bs.Height);
                        Checkers.redkingnum++;
                    }
                }
            }
            Checkers.updateCheckerNum();
            mg.DrawImage(bufferBmp, new Point(0, 0));   //将bufferBmp中的内容画到屏幕上

            g.Dispose();
        }

        /// <summary>
        /// 绘制必须吃子棋子的光标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawMustEat(int x = -1, int y = -1)
        {
            bufferBmp = new Bitmap(630, 620);
            Graphics g = Graphics.FromImage(bufferBmp);
            Checkers.rednum = 0;
            Checkers.bluenum = 0;
            Checkers.redkingnum = 0;
            Checkers.bluekingnum = 0;
            g.DrawImage(bd, 30, 20, bd.Width, bd.Height);

            foreach (Chess c in Boards.blueArray)
            {
                if (c.Alive)
                {
                    if (!c.IsKing)
                    {
                        g.DrawImage(bs, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, bs.Width, bs.Height);
                        Checkers.bluenum++;
                    }
                    else
                    {
                        g.DrawImage(bks, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, bs.Width, bs.Height);
                        Checkers.bluekingnum++;
                    }
                    if (c.CurrentX == x && c.CurrentY == y)
                        g.DrawImage(currentChecker, x * 75 + 30, y * 75 + 20, bs.Width, bs.Height);
                }
            }
            foreach (Chess c in Boards.redArray)
            {
                if (c.Alive)
                {
                    if (!c.IsKing)
                    {
                        g.DrawImage(rs, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, rs.Width, rs.Height);
                        Checkers.rednum++;
                    }
                    else
                    {
                        g.DrawImage(rks, c.CurrentX * 75 + 30, c.CurrentY * 75 + 20, bs.Width, bs.Height);
                        Checkers.redkingnum++;
                    }
                }
            }
            Checkers.updateCheckerNum();
            mg.DrawImage(bufferBmp, new Point(0, 0));   //将bufferBmp中的内容画到屏幕上

            g.Dispose();
        }
    }
}