using Checkers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace 西洋跳棋
{
    public partial class Checkers : Form
    {
        public Checkers()
        {
            InitializeComponent();
        }

        public Boards newbd;
        public static int sletedX = -1;
        public static int sletedY = -1;
        public static bool mustEat = false;
        public static int eatX = -1;
        public static int eatY = -1;
        public static int bluenum = 0;
        public static int rednum = 0;
        public static int bluekingnum = 0;
        public static int redkingnum = 0;
        private bool startGame = false;
        private static ArrayList copyBlue = new ArrayList();
        private static ArrayList copyRed = new ArrayList();
        private static int[,] copyBoard = new int[8, 8];
        private bool go = false;
        private bool backOnce = false;
        public static SoundPlayer moveSound = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checkers.Resources.moveDirection.wav"));
        public static SoundPlayer clickSound = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checkers.Resources.click.wav"));
        public static SoundPlayer winSound = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checkers.Resources.win.wav"));
        public static SoundPlayer overSound = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checkers.Resources.over.wav"));

        private void res_Click(object sender, EventArgs e)
        {
            clickSound.Play();
            newbd.resetBoards();
            copyBlue.Clear();
            copyRed.Clear();
            Boards.DrawBoard();
        }

        private void over_Click(object sender, EventArgs e)
        {
            clickSound.Play();
            this.Close();
        }

        private void back_Click(object sender, EventArgs e)
        {
            clickSound.Play();
            if (!go || (copyBlue.Count == 0) || Boards.winflag)
            {
                MessageBox.Show(this, "请先走棋！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (backOnce)
            {
                MessageBox.Show(this, "只能悔棋一次！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            AI.initForNextLevel(ref copyBlue, ref Boards.blueArray,
            ref copyRed, ref Boards.redArray, ref copyBoard, ref Boards.boardState);

            Boards.DrawBoard();
            backOnce = true;
        }

        private void Checkers_Paint(object sender, PaintEventArgs e)
        {
            if (!startGame)
                return;

            Boards.DrawBoard();
        }

        private void Checkers_Load(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.settings.setMode("loop", true);
            axWindowsMediaPlayer1.URL = Application.StartupPath + "\\resource\\moon.mp3";
            //axWindowsMediaPlayer1.URL = Assembly.GetExecutingAssembly().GetManifestResourceStream("Checkers.Resources.moon.wav");
            //SoundPlayer bgMusic = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checkers.Resources.moon.wav"));
            //bgMusic.PlayLooping();
            newbd = new Boards(this.CreateGraphics());
            //foreach (Chess c in Boards.blueArray)
            //{
            //    int index = Boards.blueArray.IndexOf(c);
            //    ((Chess)Boards.blueArray[Boards.blueArray.IndexOf(c)]).Alive = false;
            //}
        }

        public static void updateCheckerNum()
        {
            blueNum.Text = bluenum.ToString();
            bluekingNum.Text = bluekingnum.ToString();
            redNum.Text = rednum.ToString();
            redkingNum.Text = redkingnum.ToString();
        }

        private void Checkers_MouseDown(object sender, MouseEventArgs e)
        {
            if ((Boards.playerTurn == false) || (Boards.winflag == true) || (startGame == false))
                return;

            int x = e.X, y = e.Y;
            if (e.X <= 630 && e.Y <= 620 && e.X >= 30 && e.Y >= 20)
            {
                int m = ((x - 30) / 75);
                int n = ((y - 20) / 75);
                int X, Y, xx, yy, xxx, yyy;
                if (mustEat)
                {
                    foreach (Chess c in Boards.blueArray)
                    {
                        if ((eatX == c.CurrentX) && (eatY == c.CurrentY) && c.Alive)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                X = AI.w[i, 0];
                                Y = AI.w[i, 1];
                                xx = eatX + X;
                                yy = eatY + Y;
                                xxx = eatX + 2 * X;
                                yyy = eatY + 2 * Y;
                                // c.upDateCanEat(ref Boards.boardState);
                                if ((m == xxx) && (n == yyy) && (c.CanEat) && c.eatDirection[i] && c.Alive)
                                {
                                    moveSound.Play();
                                    Boards.boardState[n, m] = 0;
                                    Boards.boardState[yy, xx] = -1;
                                    Boards.boardState[c.CurrentY, c.CurrentX] = -1;
                                    c.CurrentX = m;
                                    c.CurrentY = n;
                                    foreach (Chess cs in Boards.redArray)
                                    {
                                        if ((cs.CurrentX == xx) && (cs.CurrentY == yy) && cs.Alive)
                                        {
                                            cs.Alive = false;

                                            break;
                                        }
                                    }
                                    c.upDateCanEat(ref Boards.boardState);
                                    if (c.CanEat)
                                    {
                                        mustEat = true;
                                        eatX = c.CurrentX;
                                        eatY = c.CurrentY;
                                        newbd.DrawMustEat(eatX, eatY);
                                        return;
                                    }
                                    mustEat = false;
                                    c.updateKing(ref Boards.boardState);
                                    Boards.DrawBoard();
                                    eatX = -1;
                                    eatY = -1;
                                    Boards.playerTurn = false;
                                    int win = AI.whoWin(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                    if (Boards.winflag)
                                    {
                                        switch (win)
                                        {
                                            case -1:
                                                {
                                                    overSound.Play();
                                                    MessageBox.Show(this, "电脑胜利！", "游戏结束", MessageBoxButtons.OK);
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    winSound.Play();
                                                    MessageBox.Show(this, "玩家胜利！", "游戏结束", MessageBoxButtons.OK);
                                                    break;
                                                }
                                            case 0:
                                                {
                                                    winSound.Play();
                                                    MessageBox.Show(this, "平局！", "游戏结束", MessageBoxButtons.OK);
                                                    break;
                                                }
                                        }

                                        return;
                                    }
                                    AI.AIMove(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                    win = AI.whoWin(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                    if (Boards.winflag)
                                    {
                                        switch (win)
                                        {
                                            case -1:
                                                {
                                                    overSound.Play();
                                                    MessageBox.Show(this, "电脑胜利！", "游戏结束", MessageBoxButtons.OK);
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    winSound.Play();
                                                    MessageBox.Show(this, "玩家胜利！", "游戏结束", MessageBoxButtons.OK);
                                                    break;
                                                }
                                            case 0:
                                                {
                                                    winSound.Play();
                                                    MessageBox.Show(this, "平局！", "游戏结束", MessageBoxButtons.OK);
                                                    break;
                                                }
                                        }

                                        return;
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (AI.findCanEat(ref Boards.blueArray, ref Boards.boardState, 0) > 0)
                    {
                        foreach (Chess c in Boards.blueArray)
                        {
                            if ((sletedX == c.CurrentX) && (sletedY == c.CurrentY) && (c.CanEat) && c.Alive)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    X = AI.w[i, 0];
                                    Y = AI.w[i, 1];
                                    xx = sletedX + X;
                                    yy = sletedY + Y;
                                    xxx = sletedX + 2 * X;
                                    yyy = sletedY + 2 * Y;
                                    if (c.eatDirection[i] && (m == xxx) && (n == yyy))
                                    {
                                        go = true;
                                        backOnce = false;
                                        AI.initForNextLevel(ref Boards.blueArray, ref copyBlue,
                                        ref Boards.redArray, ref copyRed, ref Boards.boardState, ref copyBoard);
                                        Boards.boardState[n, m] = 0;
                                        Boards.boardState[yy, xx] = -1;
                                        Boards.boardState[c.CurrentY, c.CurrentX] = -1;
                                        c.CurrentX = m;
                                        c.CurrentY = n;
                                        foreach (Chess cs in Boards.redArray)
                                        {
                                            if ((cs.CurrentX == xx) && (cs.CurrentY == yy) && cs.Alive)
                                            {
                                                cs.Alive = false;

                                                break;
                                            }
                                        }
                                        c.upDateCanEat(ref Boards.boardState);
                                        moveSound.Play();
                                        Boards.DrawBoard();
                                        if (c.CanEat)
                                        {
                                            mustEat = true;
                                            eatX = c.CurrentX;
                                            eatY = c.CurrentY;
                                            newbd.DrawMustEat(eatX, eatY);
                                            return;
                                        }
                                        mustEat = false;
                                        c.updateKing(ref Boards.boardState);
                                        Boards.DrawBoard();
                                        sletedX = -1;
                                        sletedY = -1;
                                        eatX = -1;
                                        eatY = -1;
                                        int win = AI.whoWin(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                        if (Boards.winflag)
                                        {
                                            switch (win)
                                            {
                                                case -1:
                                                    {
                                                        overSound.Play();
                                                        MessageBox.Show(this, "电脑胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "玩家胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 0:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "平局！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                            }

                                            return;
                                        }
                                        Boards.playerTurn = false;

                                        AI.AIMove(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                        win = AI.whoWin(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                        if (Boards.winflag)
                                        {
                                            switch (win)
                                            {
                                                case -1:
                                                    {
                                                        overSound.Play();
                                                        MessageBox.Show(this, "电脑胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "玩家胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 0:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "平局！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                            }

                                            return;
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    else if (AI.findCanMove(ref Boards.blueArray, ref Boards.boardState, 0) > 0)
                    {
                        foreach (Chess c in Boards.blueArray)
                        {
                            if ((sletedX == c.CurrentX) && (sletedY == c.CurrentY) && (c.CanMove) && c.Alive)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    X = AI.w[i, 0];
                                    Y = AI.w[i, 1];
                                    xx = sletedX + X;
                                    yy = sletedY + Y;
                                    xxx = sletedX + 2 * X;
                                    yyy = sletedY + 2 * Y;
                                    if ((m == xx) && (n == yy) && c.moveDirection[i])
                                    {
                                        go = true;
                                        backOnce = false;
                                        AI.initForNextLevel(ref Boards.blueArray, ref copyBlue,
                                        ref Boards.redArray, ref copyRed, ref Boards.boardState, ref copyBoard);
                                        Boards.boardState[n, m] = 0;
                                        Boards.boardState[c.CurrentY, c.CurrentX] = -1;
                                        c.CurrentX = m;
                                        c.CurrentY = n;
                                        c.updateKing(ref Boards.boardState);
                                        moveSound.Play();
                                        Boards.DrawBoard();
                                        sletedX = -1;
                                        sletedY = -1;
                                        Boards.playerTurn = false;
                                        int win = AI.whoWin(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                        if (Boards.winflag)
                                        {
                                            switch (win)
                                            {
                                                case -1:
                                                    {
                                                        overSound.Play();
                                                        MessageBox.Show(this, "电脑胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "玩家胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 0:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "平局！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                            }

                                            return;
                                        }
                                        AI.AIMove(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                        win = AI.whoWin(ref Boards.blueArray, ref Boards.redArray, ref Boards.boardState);
                                        if (Boards.winflag)
                                        {
                                            switch (win)
                                            {
                                                case -1:
                                                    {
                                                        overSound.Play();
                                                        MessageBox.Show(this, "电脑胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "玩家胜利！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                                case 0:
                                                    {
                                                        winSound.Play();
                                                        MessageBox.Show(this, "平局！", "游戏结束", MessageBoxButtons.OK);
                                                        break;
                                                    }
                                            }

                                            return;
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                Boards.DrawBoard(m, n);
                sletedX = m;
                sletedY = n;
            }
        }

        private void start_Click(object sender, EventArgs e)
        {
            if (startGame)
                return;
            clickSound.Play();
            startGame = true;

            groupBox1.Visible = true;

            groupBox3.Visible = true;
            start.Visible = false;
            Boards.DrawBoard();
        }

        private void help_Click(object sender, EventArgs e)
        {
            clickSound.Play();
            HelpForm Help = new HelpForm();
            Help.Show(this);
        }

        private void draw_Click(object sender, EventArgs e)
        {
            clickSound.Play();
            if (!go || (copyBlue.Count == 0) || Boards.winflag)
            {
                MessageBox.Show(this, "请先走棋！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Boards.winflag = true;
            MessageBox.Show(this, "玩家申请和棋！", "游戏结束", MessageBoxButtons.OK);
        }

        private void about_Click(object sender, EventArgs e)
        {
            clickSound.Play();
            aboutForm newabout = new aboutForm();
            newabout.Show(this);
        }
    }
}