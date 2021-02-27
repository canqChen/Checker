using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace 西洋跳棋
{
    public class AI
    {
        //博弈树搜索深度
        public const int SEARCH_DEPTH = 8;

        //评估棋局的分数
        public const int CHESS_VALUE = 8;

        public const int KING_VALUE = 10;
        public const int WILLBEKING_VALUE = 2;
        public const int CANMOVE_VALUE = 3;
        public const int CANEAT_VALUE = 5;
        public const int MAX_VALUE = 30000;
        public const int MIN_VALUE = -30000;
        public const int EMPTYMAX_VALUE = 25000;
        public const int EMPTYMIN_VALUE = -25000;
        public const int NOMOVEMAX_VALUE = 20000;
        public const int NOMOVEMIN_VALUE = -20000;

        //最后确定可走棋子的坐标
        public static int slectedX = -1;

        public static int slectedY = -1;

        //最后确定可走棋子移动目标点的坐标
        public static int moveToX = -1;

        public static int moveToY = -1;

        public static int[,] w = new int[4, 2] { { -1, -1 }, { 1, -1 }, { -1, 1 }, { 1, 1 } };

        /// <summary>
        /// 判断赢家
        /// </summary>
        /// <param name="blue">蓝方（玩家）包含所有棋子信息的数组列表</param>
        /// <param name="red">红方（AI）包含所有棋子信息的数组列表</param>
        /// <param name="board">当前棋盘状态</param>
        /// <returns>返回赢家，-1为未达胜利条件，1为蓝方赢，0为红方赢</returns>
        public static int whoWin(ref ArrayList blue, ref ArrayList red, ref int[,] board)
        {
            int win = -2;   //判断胜利的标志
            int blueNum = calCheckerNum(ref blue);
            int redNum = calCheckerNum(ref red);
            int canMoveBlueNum = findCanMove(ref blue, ref board, 0);
            int canMoveRedNum = findCanMove(ref red, ref board, 1);
            int canEatBlueNum = findCanEat(ref blue, ref board, 0);
            int canEatRedNum = findCanEat(ref red, ref board, 1);
            if ((blueNum == 0) || ((canEatBlueNum == 0) && (canMoveBlueNum == 0)))
            {
                Boards.winflag = true;
                win = -1;
            }
            else if ((redNum == 0) || ((canEatRedNum == 0) && (canMoveRedNum == 0)))
            {
                Boards.winflag = true;
                win = 1;
            }
            if (((blueNum == 0) || ((canEatBlueNum == 0) && (canMoveBlueNum == 0)))
                && ((redNum == 0) || ((canEatRedNum == 0) && (canMoveRedNum == 0))))
            {
                Boards.winflag = true;
                win = 0;
            }
            return win;
        }

        //计算王棋的数量，用于评估棋局
        public static int calKingNum(ref ArrayList al)
        {
            int num = 0;
            foreach (Chess c in al)
            {
                if (c.Alive)
                {
                    if (c.IsKing)
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        //计算棋子的数量，用于评估棋局
        public static int calCheckerNum(ref ArrayList al)
        {
            int num = 0;
            foreach (Chess c in al)
            {
                if (c.Alive)
                {
                    num++;
                }
            }
            return num;
        }

        /// <summary>
        /// 检查某一方是否已无棋子
        /// </summary>
        /// <param name="al">包含某一方所有棋子的数组列表</param>
        /// <returns></returns>
        public static bool checkEmpty(ref ArrayList al)
        {
            foreach (Chess c in al)
            {
                if (c.Alive)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 计算即将成为王棋的数量，用于评估棋局
        /// </summary>
        /// <param name="al">包含某一方所有棋子的数组列表</param>
        /// <param name="board">当前棋盘，包含棋局信息</param>
        /// <param name="type">需要统计的一方，0位玩家，1为AI</param>
        /// <returns>返回统计数量</returns>
        public static int calBeKingNum(ref ArrayList al, ref int[,] board, int type)
        {
            int num = 0;
            foreach (Chess c in al)
            {
                if ((!c.IsKing) && c.Alive)
                {
                    switch (type)
                    {
                        case 0:
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    int x = w[i, 0];
                                    int y = w[i, 1];
                                    int xx = c.CurrentX + x;
                                    int yy = c.CurrentY + y;
                                    if ((xx >= 0) && (xx <= 7) && (yy == 7) && (board[yy, xx] == -1))
                                    {
                                        num++;
                                    }
                                }

                                break;
                            }
                        case 1:
                            {
                                for (int i = 2; i < 4; i++)
                                {
                                    int x = w[i, 0];
                                    int y = w[i, 1];
                                    int xx = c.CurrentX + x;
                                    int yy = c.CurrentY + y;
                                    if ((xx >= 0) && (xx <= 7) && (yy == 0) && (board[yy, xx] == -1))
                                    {
                                        num++;
                                    }
                                }

                                break;
                            }
                    }
                }
            }
            return num;
        }

        /// <summary>
        /// 计算可走棋子的数量，用于评估棋局
        /// </summary>
        /// <param name="al">包含某一方所有棋子的数组列表</param>
        /// <param name="board">当前棋盘，包含棋局信息</param>
        /// <param name="type">需要统计的一方，0位玩家，1为AI</param>
        /// <returns>返回统计数量</returns>
        public static int findCanMove(ref ArrayList al, ref int[,] board, int type)
        {
            int num = 0;
            int x, y, X, Y, XX, YY;
            switch (type)
            {
                case 0:
                    {
                        foreach (Chess c in al)
                        {
                            if (c.Alive)
                            {
                                X = c.CurrentX;
                                Y = c.CurrentY;
                                if (c.IsKing)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        x = w[i, 0];
                                        y = w[i, 1];
                                        XX = X + x;
                                        YY = Y + y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7)
                                            && (YY >= 0) && (board[YY, XX] == -1))
                                        {
                                            c.moveDirection[i] = true;
                                        }
                                        else
                                            c.moveDirection[i] = false;
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < 2; i++)
                                    {
                                        x = w[i, 0];
                                        y = w[i, 1];
                                        XX = X + x;
                                        YY = Y + y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7) && (YY >= 0)
                                            && (board[YY, XX] == -1))
                                        {
                                            c.moveDirection[i] = true;
                                        }
                                        else
                                            c.moveDirection[i] = false;
                                    }
                                    c.moveDirection[2] = c.moveDirection[3] = false;
                                }
                                if ((c.moveDirection[0] == true) || (c.moveDirection[1] == true)
                                    || (c.moveDirection[2] == true) || (c.moveDirection[3] == true))
                                {
                                    c.CanMove = true;
                                    num++;
                                }
                                else
                                    c.CanMove = false;
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        foreach (Chess c in al)
                        {
                            if (c.Alive)
                            {
                                X = c.CurrentX;
                                Y = c.CurrentY;
                                if (c.IsKing)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        x = AI.w[i, 0];
                                        y = AI.w[i, 1];
                                        XX = X + x;
                                        YY = Y + y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7)
                                            && (YY >= 0) && (board[YY, XX] == -1))
                                        {
                                            c.moveDirection[i] = true;
                                        }
                                        else
                                            c.moveDirection[i] = false;
                                    }
                                }
                                else
                                {
                                    for (int i = 2; i < 4; i++)
                                    {
                                        x = AI.w[i, 0];
                                        y = AI.w[i, 1];
                                        XX = X + x;
                                        YY = Y + y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7) && (YY >= 0)
                                            && (board[YY, XX] == -1))
                                        {
                                            c.moveDirection[i] = true;
                                        }
                                        else
                                            c.moveDirection[i] = false;
                                    }
                                    c.moveDirection[0] = c.moveDirection[1] = false;
                                }
                                if ((c.moveDirection[0] == true) || (c.moveDirection[1] == true)
                                    || (c.moveDirection[2] == true) || (c.moveDirection[3] == true))
                                {
                                    c.CanMove = true;
                                    num++;
                                }
                                else
                                    c.CanMove = false;
                            }
                        }
                        break;
                    }
            }

            return num;
        }

        /// <summary>
        /// 计算某一方可吃敌方棋子的数量，用于评估棋局
        /// </summary>
        /// <param name="al">包含某一方所有棋子的数组列表</param>
        /// <param name="board">当前棋盘，包含棋局信息</param>
        /// <param name="type">需要统计的一方，0位玩家，1为AI</param>
        /// <returns>返回统计数量</returns>
        public static int findCanEat(ref ArrayList al, ref int[,] board, int type)
        {
            int num = 0;
            int x, y, X, Y, XX, YY;
            switch (type)
            {
                case 0:
                    {
                        foreach (Chess c in al)
                        {
                            if (c.Alive)
                            {
                                X = c.CurrentX;
                                Y = c.CurrentY;
                                if (c.IsKing)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        x = AI.w[i, 0];
                                        y = AI.w[i, 1];
                                        XX = X + 2 * x;
                                        YY = Y + 2 * y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7)
                                            && (YY >= 0) && (board[YY, XX] == -1) && (board[Y + y, X + x] == 1))
                                        {
                                            c.eatDirection[i] = true;
                                        }
                                        else
                                            c.eatDirection[i] = false;
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < 2; i++)
                                    {
                                        x = AI.w[i, 0];
                                        y = AI.w[i, 1];
                                        XX = X + 2 * x;
                                        YY = Y + 2 * y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7) && (YY >= 0)
                                            && (board[YY, XX] == -1) && (board[Y + y, X + x] == 1))
                                        {
                                            c.eatDirection[i] = true;
                                        }
                                        else
                                            c.eatDirection[i] = false;
                                    }
                                    c.eatDirection[2] = c.eatDirection[3] = false;
                                }
                                if ((c.eatDirection[0] == true) || (c.eatDirection[1] == true)
                                    || (c.eatDirection[2] == true) || (c.eatDirection[3] == true))
                                {
                                    c.CanEat = true;
                                    num++;
                                }
                                else
                                    c.CanEat = false;
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        foreach (Chess c in al)
                        {
                            if (c.Alive)
                            {
                                X = c.CurrentX;
                                Y = c.CurrentY;
                                if (c.IsKing)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        x = AI.w[i, 0];
                                        y = AI.w[i, 1];
                                        XX = X + 2 * x;
                                        YY = Y + 2 * y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7)
                                            && (YY >= 0) && (board[YY, XX] == -1) && (board[Y + y, X + x] == 0))
                                        {
                                            c.eatDirection[i] = true;
                                        }
                                        else
                                            c.eatDirection[i] = false;
                                    }
                                }
                                else
                                {
                                    for (int i = 2; i < 4; i++)
                                    {
                                        x = AI.w[i, 0];
                                        y = AI.w[i, 1];
                                        XX = X + 2 * x;
                                        YY = Y + 2 * y;
                                        if ((XX >= 0) && (XX <= 7) && (YY <= 7) && (YY >= 0)
                                            && (board[YY, XX] == -1) && (board[Y + y, X + x] == 0))
                                        {
                                            c.eatDirection[i] = true;
                                        }
                                        else
                                            c.eatDirection[i] = false;
                                    }
                                    c.eatDirection[0] = c.eatDirection[1] = false;
                                }
                                if ((c.eatDirection[0] == true) || (c.eatDirection[1] == true)
                                    || (c.eatDirection[2] == true) || (c.eatDirection[3] == true))
                                {
                                    c.CanEat = true;
                                    num++;
                                }
                                else
                                    c.CanEat = false;
                            }
                        }
                        break;
                    }
            }

            return num;
        }

        /// <summary>
        /// 复制当前双方棋子状态信息以及棋盘信息，为传递到下一层博弈树做准备
        /// </summary>
        /// <param name="oldBlue"></param>
        /// <param name="newBlue"></param>
        /// <param name="oldRed"></param>
        /// <param name="newRed"></param>
        /// <param name="oldboard"></param>
        /// <param name="newboard"></param>
        public static void initForNextLevel(ref ArrayList oldBlue, ref ArrayList newBlue,
            ref ArrayList oldRed, ref ArrayList newRed, ref int[,] oldboard, ref int[,] newboard)
        {
            newBlue.Clear();
            newRed.Clear();
            foreach (Chess c in oldBlue)
            {
                Chess temp = new Chess(c.CurrentY, c.CurrentX, c.Type, c.Alive, c.IsKing, c.CanEat, c.CanMove);
                for (int i = 0; i < 4; i++)
                {
                    temp.eatDirection[i] = c.eatDirection[i];
                    temp.moveDirection[i] = c.moveDirection[i];
                }
                newBlue.Add(temp);
            }
            foreach (Chess c in oldRed)
            {
                Chess temp = new Chess(c.CurrentY, c.CurrentX, c.Type, c.Alive, c.IsKing, c.CanEat, c.CanMove);
                for (int i = 0; i < 4; i++)
                {
                    temp.eatDirection[i] = c.eatDirection[i];
                    temp.moveDirection[i] = c.moveDirection[i];
                }
                newRed.Add(temp);
            }
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    newboard[i, j] = oldboard[i, j];
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="al"></param>
        /// <param name="board"></param>
        /// <param name="type"></param>
        public static void doEat(Chess cs, ref ArrayList al, ref int[,] board, int type)
        {
            int x, y, XX, YY;

            for (int i = 0; i < 4; i++)
            {
                x = w[i, 0];
                y = w[i, 1];
                XX = cs.CurrentX + 2 * x;
                YY = cs.CurrentY + 2 * y;
                if ((XX >= 0) && (XX <= 7) && (YY >= 0) && (YY <= 7) && (board[YY, XX] == -1)
                && (board[cs.CurrentY + y, cs.CurrentX + x] == 1 - type))
                {
                    board[cs.CurrentY + y, cs.CurrentX + x] = -1;
                    board[cs.CurrentY, cs.CurrentX] = -1;
                    foreach (Chess c in al)
                    {
                        if ((c.CurrentX == cs.CurrentX + x) && (c.CurrentY == cs.CurrentY + y) && c.Alive)
                        {
                            c.Alive = false;
                            break;
                        }
                    }
                    cs.CurrentX = XX;
                    cs.CurrentY = YY;
                    board[cs.CurrentY, cs.CurrentX] = type;
                    cs.upDateCanEat(ref board);
                    if (cs.CanEat)
                    {
                        doEat(cs, ref al, ref board, type);
                    }
                    else
                    {
                        cs.updateKing(ref board);
                    }
                    break;
                }
            }
        }

        public static void doEat(Chess cs, ref ArrayList al, ref int[,] board, int movetoX, int movetoY)
        {
            int x, y, XX, YY;

            if ((movetoX != -1) && (moveToY != -1))
            {
                for (int i = 0; i < 4; i++)
                {
                    x = w[i, 0];
                    y = w[i, 1];
                    XX = cs.CurrentX + 2 * x;
                    YY = cs.CurrentY + 2 * y;
                    if ((XX == movetoX) && (YY == movetoY))
                    {
                        board[cs.CurrentY + y, cs.CurrentX + x] = -1;
                        board[cs.CurrentY, cs.CurrentX] = -1;
                        foreach (Chess c in al)
                        {
                            if ((c.CurrentX == cs.CurrentX + x) && (c.CurrentY == cs.CurrentY + y) && c.Alive)
                            {
                                c.Alive = false;

                                break;
                            }
                        }
                        cs.CurrentX = XX;
                        cs.CurrentY = YY;
                        board[YY, XX] = 1;
                        Checkers.moveSound.Play();
                        Thread.Sleep(100);
                        Boards.DrawBoard();
                        cs.upDateCanEat(ref board);

                        if (cs.CanEat)
                        {
                            doEat(cs, ref al, ref board, -1, -1);
                        }
                        else
                        {
                            cs.updateKing(ref board);
                            Boards.DrawBoard();
                        }
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    x = w[i, 0];
                    y = w[i, 1];
                    XX = cs.CurrentX + 2 * x;
                    YY = cs.CurrentY + 2 * y;
                    if (cs.eatDirection[i])
                    {
                        board[cs.CurrentY + y, cs.CurrentX + x] = -1;
                        board[cs.CurrentY, cs.CurrentX] = -1;
                        foreach (Chess c in al)
                        {
                            if ((c.CurrentX == cs.CurrentX + x) && (c.CurrentY == cs.CurrentY + y) && c.Alive)
                            {
                                c.Alive = false;

                                break;
                            }
                        }
                        cs.CurrentX = XX;
                        cs.CurrentY = YY;
                        board[YY, XX] = 1;
                        Checkers.moveSound.Play();
                        Thread.Sleep(400);
                        Boards.DrawBoard();

                        cs.upDateCanEat(ref board);

                        if (cs.CanEat)
                        {
                            doEat(cs, ref al, ref board, -1, -1);
                        }
                        else
                        {
                            cs.updateKing(ref board);
                            Boards.DrawBoard();
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 博弈树模型（核心算法），递归寻找AI下一步最佳走棋
        /// </summary>
        /// <param name="blue"></param>
        /// <param name="red"></param>
        /// <param name="board"></param>
        public static void AlphaBetaSearch(ref ArrayList blue, ref ArrayList red, ref int[,] board)
        {
            int x, y, xx, yy;
            int X, Y;
            int gameValue = MIN_VALUE;
            int eatNum = 0;
            int moveNum = 0;
            int alpha = MIN_VALUE;
            int beta = MAX_VALUE;
            int depth = 0;
            int value;
            int blueTotal = -1, redTotal = -1;
            int nextBlueTotal = -1, nextRedTotal = -1;
            ArrayList nextBlue = new ArrayList();
            ArrayList nextRed = new ArrayList();

            int[,] nextBoard = new int[8, 8];
            eatNum = findCanEat(ref red, ref board, 1);
            if (eatNum > 0)
            {
                foreach (Chess c in red)
                {
                    if (c.CanEat && c.Alive)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (c.eatDirection[i])
                            {
                                initForNextLevel(ref blue, ref nextBlue, ref red, ref nextRed, ref board, ref nextBoard);

                                x = w[i, 0];
                                y = w[i, 1];
                                X = c.CurrentX;
                                Y = c.CurrentY;
                                xx = c.CurrentX + 2 * x;
                                yy = c.CurrentY + 2 * y;

                                int index = red.IndexOf(c);

                                ((Chess)nextRed[index]).CurrentX = xx;
                                ((Chess)nextRed[index]).CurrentY = yy;
                                nextBoard[Y, X] = -1;
                                nextBoard[yy, xx] = 1;
                                nextBoard[Y + y, X + x] = -1;
                                foreach (Chess cs in nextBlue)
                                {
                                    if ((cs.CurrentX == x + X) && (cs.CurrentY == y + Y) && cs.Alive)
                                    {
                                        cs.Alive = false;
                                        break;
                                    }
                                }

                                doEat((Chess)nextRed[index], ref nextBlue, ref nextBoard, 1);
                                value = minLayer(ref nextBlue, ref nextRed, ref nextBoard, alpha, beta, depth + 1, ref nextBlueTotal, ref nextRedTotal);
                                Random rd = new Random();
                                if (
                                    (gameValue < value)
                                    || ((gameValue == value) && (blueTotal > nextBlueTotal) && (redTotal == nextRedTotal))
                                    || ((gameValue == value) && (redTotal < nextRedTotal))
                                    || ((gameValue == value) && (blueTotal == nextBlueTotal) && (redTotal == nextRedTotal) && (rd.Next(0, 1) == 0))
                                    )
                                {
                                    gameValue = value;
                                    blueTotal = nextBlueTotal;
                                    redTotal = nextRedTotal;

                                    slectedX = X;
                                    slectedY = Y;
                                    moveToX = xx;
                                    moveToY = yy;
                                    if (gameValue > alpha)
                                    {
                                        alpha = gameValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                moveNum = findCanMove(ref red, ref board, 1);
                if (moveNum > 0)
                {
                    foreach (Chess c in red)
                    {
                        if (c.CanMove && c.Alive)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (c.moveDirection[i])
                                {
                                    x = w[i, 0];
                                    y = w[i, 1];
                                    xx = c.CurrentX + x;
                                    yy = c.CurrentY + y;
                                    initForNextLevel(ref blue, ref nextBlue, ref red, ref nextRed, ref board, ref nextBoard);

                                    nextBoard[c.CurrentY, c.CurrentX] = -1;
                                    int index = red.IndexOf(c);
                                    ((Chess)nextRed[index]).CurrentX = xx;
                                    ((Chess)nextRed[index]).CurrentY = yy;
                                    nextBoard[yy, xx] = 1;
                                    value = minLayer(ref nextBlue, ref nextRed, ref nextBoard, alpha, beta, depth + 1, ref nextBlueTotal, ref nextRedTotal);
                                    Random rd = new Random();
                                    if (
                                        (gameValue < value)
                                        || ((gameValue == value) && (blueTotal > nextBlueTotal) && (redTotal == nextRedTotal))
                                    || ((gameValue == value) && (redTotal < nextRedTotal))
                                    || ((gameValue == value) && (blueTotal == nextBlueTotal) && (redTotal == nextRedTotal) && (rd.Next(0, 1) == 0))
                                        )
                                    {
                                        gameValue = value;

                                        slectedX = c.CurrentX;
                                        slectedY = c.CurrentY;
                                        moveToX = xx;
                                        moveToY = yy;
                                        if (gameValue > alpha)
                                        {
                                            alpha = gameValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 博弈树中的最大层
        /// </summary>
        /// <param name="blue"></param>
        /// <param name="red"></param>
        /// <param name="board"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <param name="blueTotal"></param>
        /// <param name="redTotal"></param>
        /// <returns></returns>
        public static int maxLayer(ref ArrayList blue, ref ArrayList red, ref int[,] board, int alpha, int beta, int depth, ref int blueTotal, ref int redTotal)
        {
            if (depth == SEARCH_DEPTH)
            {
                return calValue(ref blue, ref red, ref board, ref blueTotal, ref redTotal);
            }
            int eatNum = findCanEat(ref red, ref board, 1);
            int moveNum = findCanMove(ref red, ref board, 1);
            if (checkEmpty(ref red))
            {
                return EMPTYMIN_VALUE;
            }
            else if ((eatNum == 0) && (moveNum == 0))
            {
                return NOMOVEMIN_VALUE;
            }
            int gameValue = MIN_VALUE;
            int x, y, xx, yy, value, X, Y;
            ArrayList nextBlue = new ArrayList();
            ArrayList nextRed = new ArrayList();
            int[,] nextBoard = new int[8, 8];
            int nextBlueTotal = -1, nextRedTotal = -1;
            System.Windows.Forms.Application.DoEvents();
            if (eatNum > 0)
            {
                foreach (Chess c in red)
                {
                    if (c.CanEat && c.Alive)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (c.eatDirection[i])
                            {
                                x = w[i, 0];
                                y = w[i, 1];
                                X = c.CurrentX;
                                Y = c.CurrentY;
                                xx = c.CurrentX + 2 * x;
                                yy = c.CurrentY + 2 * y;
                                initForNextLevel(ref blue, ref nextBlue, ref red, ref nextRed, ref board, ref nextBoard);

                                int index = red.IndexOf(c);

                                ((Chess)nextRed[index]).CurrentX = xx;
                                ((Chess)nextRed[index]).CurrentY = yy;
                                nextBoard[Y, X] = -1;
                                nextBoard[yy, xx] = 1;
                                nextBoard[Y + y, X + x] = -1;
                                foreach (Chess cs in nextBlue)
                                {
                                    if ((cs.CurrentX == x + X) && (cs.CurrentY == y + Y) && cs.Alive)
                                    {
                                        cs.Alive = false;
                                        break;
                                    }
                                }

                                doEat((Chess)nextRed[index], ref nextBlue, ref nextBoard, 1);
                                value = minLayer(ref nextBlue, ref nextRed, ref nextBoard, alpha, beta, depth + 1, ref nextBlueTotal, ref nextRedTotal);

                                if (
                                    (gameValue < value)
                                     || ((gameValue == value) && (blueTotal > nextBlueTotal) && (redTotal == nextRedTotal))
                                    || ((gameValue == value) && (redTotal < nextRedTotal))
                                    )
                                {
                                    gameValue = value;

                                    if (gameValue >= beta)
                                    {
                                        return gameValue;
                                    }
                                    if (gameValue > alpha)
                                    {
                                        alpha = gameValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (moveNum > 0)
            {
                foreach (Chess c in red)
                {
                    if (c.CanMove && c.Alive)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (c.moveDirection[i])
                            {
                                x = w[i, 0];
                                y = w[i, 1];
                                xx = c.CurrentX + x;
                                yy = c.CurrentY + y;
                                initForNextLevel(ref blue, ref nextBlue, ref red, ref nextRed, ref board, ref nextBoard);
                                nextBoard[c.CurrentY, c.CurrentX] = -1;
                                int index = red.IndexOf(c);
                                ((Chess)nextRed[index]).CurrentX = xx;
                                ((Chess)nextRed[index]).CurrentY = yy;
                                nextBoard[yy, xx] = 1;
                                value = minLayer(ref nextBlue, ref nextRed, ref nextBoard, alpha, beta, depth + 1, ref nextBlueTotal, ref nextRedTotal);
                                if (
                                   (gameValue < value)
                                    || ((gameValue == value) && (blueTotal > nextBlueTotal) && (redTotal == nextRedTotal))
                                    || ((gameValue == value) && (redTotal < nextRedTotal))
                                   )
                                {
                                    gameValue = value;

                                    if (gameValue >= beta)
                                    {
                                        return gameValue;
                                    }
                                    if (gameValue > alpha)
                                    {
                                        alpha = gameValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return gameValue;
        }

        /// <summary>
        /// 博弈树中的最小层
        /// </summary>
        /// <param name="blue"></param>
        /// <param name="red"></param>
        /// <param name="board"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <param name="blueTotal"></param>
        /// <param name="redTotal"></param>
        /// <returns></returns>
        public static int minLayer(ref ArrayList blue, ref ArrayList red, ref int[,] board, int alpha, int beta, int depth, ref int blueTotal, ref int redTotal)
        {
            if (depth == SEARCH_DEPTH)
            {
                return calValue(ref blue, ref red, ref board, ref blueTotal, ref redTotal);
            }
            int eatNum = findCanEat(ref blue, ref board, 0);
            int moveNum = findCanMove(ref blue, ref board, 0);
            if (checkEmpty(ref blue))
            {
                return EMPTYMAX_VALUE;
            }
            else if ((eatNum == 0) && (moveNum == 0))
            {
                return NOMOVEMAX_VALUE;
            }
            int gameValue = MAX_VALUE;
            int x, y, xx, yy, value, X, Y;
            int nextBlueTotal = -1, nextRedTotal = -1;
            ArrayList nextBlue = new ArrayList();
            ArrayList nextRed = new ArrayList();
            int[,] nextBoard = new int[8, 8];
            System.Windows.Forms.Application.DoEvents();
            if (eatNum > 0)
            {
                foreach (Chess c in blue)
                {
                    if (c.CanEat && c.Alive)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (c.eatDirection[i])
                            {
                                x = w[i, 0];
                                y = w[i, 1];
                                X = c.CurrentX;
                                Y = c.CurrentY;
                                xx = c.CurrentX + 2 * x;
                                yy = c.CurrentY + 2 * y;
                                initForNextLevel(ref blue, ref nextBlue, ref red, ref nextRed, ref board, ref nextBoard);

                                int index = blue.IndexOf(c);

                                ((Chess)nextBlue[index]).CurrentX = xx;
                                ((Chess)nextBlue[index]).CurrentY = yy;
                                nextBoard[Y, X] = -1;
                                nextBoard[yy, xx] = 0;
                                nextBoard[Y + y, X + x] = -1;
                                foreach (Chess cs in nextRed)
                                {
                                    if ((cs.CurrentX == x + X) && (cs.CurrentY == y + Y) && cs.Alive)
                                    {
                                        cs.Alive = false;
                                        break;
                                    }
                                }

                                doEat((Chess)nextBlue[index], ref nextRed, ref nextBoard, 0);
                                value = maxLayer(ref nextBlue, ref nextRed, ref nextBoard, alpha, beta, depth + 1, ref nextBlueTotal, ref nextRedTotal);
                                if (
                                    (gameValue > value)
                                     || ((gameValue == value) && (blueTotal == nextBlueTotal) && (redTotal > nextRedTotal))
                                    || ((gameValue == value) && (blueTotal < nextBlueTotal))
                                    )
                                {
                                    gameValue = value;

                                    if (gameValue <= alpha)
                                    {
                                        return gameValue;
                                    }
                                    if (gameValue < beta)
                                    {
                                        beta = gameValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (moveNum > 0)
            {
                foreach (Chess c in blue)
                {
                    if (c.CanMove && c.Alive)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (c.moveDirection[i])
                            {
                                x = w[i, 0];
                                y = w[i, 1];
                                xx = c.CurrentX + x;
                                yy = c.CurrentY + y;
                                initForNextLevel(ref blue, ref nextBlue, ref red, ref nextRed, ref board, ref nextBoard);
                                nextBoard[c.CurrentY, c.CurrentX] = -1;
                                int index = blue.IndexOf(c);
                                ((Chess)nextBlue[index]).CurrentX = xx;
                                ((Chess)nextBlue[index]).CurrentY = yy;
                                nextBoard[yy, xx] = 0;
                                value = maxLayer(ref nextBlue, ref nextRed, ref nextBoard, alpha, beta, depth + 1, ref nextBlueTotal, ref nextRedTotal);
                                if (
                                    (gameValue > value)
                                     || ((gameValue == value) && (blueTotal == nextBlueTotal) && (redTotal > nextRedTotal))
                                    || ((gameValue == value) && (blueTotal < nextBlueTotal))
                                    )
                                {
                                    gameValue = value;

                                    if (gameValue < alpha)
                                    {
                                        return gameValue;
                                    }
                                    if (gameValue < beta)
                                    {
                                        beta = gameValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return gameValue;
        }

        /// <summary>
        /// 统计AI当前棋局得分，用于确定AI下一步走棋
        /// </summary>
        /// <param name="blue"></param>
        /// <param name="red"></param>
        /// <param name="board"></param>
        /// <param name="blueTotal"></param>
        /// <param name="redTotal"></param>
        /// <returns></returns>
        public static int calValue(ref ArrayList blue, ref ArrayList red, ref int[,] board, ref int blueTotal, ref int redTotal)
        {
            int canMoveBlueNum = findCanMove(ref blue, ref board, 0);
            int canMoveRedNum = findCanMove(ref red, ref board, 1);
            if (canMoveBlueNum == 0)
                return NOMOVEMAX_VALUE;
            else if (canMoveRedNum == 0)
                return NOMOVEMIN_VALUE;

            int value;
            int blueNum = calCheckerNum(ref blue);
            int redNum = calCheckerNum(ref red);
            blueTotal = blueNum;
            redTotal = redNum;
            int blueCanEatNum = findCanEat(ref blue, ref board, 0);
            int redCanEatNum = findCanEat(ref red, ref board, 1);
            int blueKingNum = calKingNum(ref blue);
            int redKingNum = calKingNum(ref red);
            int blueBeKingNum = calBeKingNum(ref blue, ref board, 0);
            int redBeKingNum = calBeKingNum(ref red, ref board, 1);
            value = CHESS_VALUE * (redNum - blueNum) + KING_VALUE * (redKingNum - blueKingNum) + WILLBEKING_VALUE * (redBeKingNum - blueBeKingNum)
               + CANMOVE_VALUE * (canMoveRedNum - canMoveBlueNum) + CANEAT_VALUE * ((redNum - blueCanEatNum) - (blueNum - redCanEatNum));
            // + CANEAT_VALUE * (redCanEatNum - blueCanEatNum)
            //  + WILLBEKING_VALUE * (redBeKingNum - blueBeKingNum);
            //+CANMOVE_VALUE * (canMoveRedNum - canMoveBlueNum)
            //+ CANEAT_VALUE * (redCanEatNum - blueCanEatNum);
            return value;
        }

        /// <summary>
        /// AI走棋
        /// </summary>
        /// <param name="blue"></param>
        /// <param name="red"></param>
        /// <param name="board"></param>
        public static void AIMove(ref ArrayList blue, ref ArrayList red, ref int[,] board)
        {
            AlphaBetaSearch(ref blue, ref red, ref board);
            foreach (Chess c in red)
            {
                if ((c.CurrentX == slectedX) && (c.CurrentY == slectedY) && c.Alive)
                {
                    if (c.CanEat)
                    {
                        doEat(c, ref blue, ref board, moveToX, moveToY);
                    }
                    else if (c.CanMove)
                    {
                        board[c.CurrentY, c.CurrentX] = -1;
                        board[moveToY, moveToX] = 1;
                        c.CurrentX = moveToX;
                        c.CurrentY = moveToY;
                        c.updateKing(ref board);
                        Checkers.moveSound.Play();
                        Thread.Sleep(100);
                        Boards.DrawBoard();
                    }
                    break;
                }
            }
            slectedX = -1;
            slectedY = -1;
            moveToX = -1;
            moveToY = -1;
            Boards.playerTurn = true;
        }
    }
}