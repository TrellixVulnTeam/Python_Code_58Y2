﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading;

namespace MChess
{
    public class Searching
    {
        private Board brd ,brd1 , brd2;
        public state st =new state();
        public state st1 = new state();
        public state st2 = new state();
        internal Evaluator Eval;
        private TranspositionTable TransTable;
        public HistoryTable HisTable ;
        public HistoryTable HisTable1;
        public HistoryTable HisTable2;
        public int[,] mtb = new int[3,3]; // biến lưu trữ material ballance , tính ngay sau mỗi nước đi giúp tránh được việc phải duyệt hết cả bàn cờ 
        public Move next_move;             
        public int so_nuoc_di = 0;          //do re nhanh
        EventWaitHandle wh = new AutoResetEvent(false);        
    
        public int TTHitCount = 0;          //so lan tim thay mot the co trong Transposition Table
        //public int QuiescenceNode = 0;      //so nut da thuc hien Quiescence
        public int QuiescenceDepth = 0;
        Board b3 = new Board();
        Board b4 = new Board();
        public static int EVAL_THRESHOLD;
        
        //constructor
       /* public int materialbalace(int color)
        {
            int x, y;
            x = y = 0;
            for (int i = 0; i <= 9; i++)
            {
                if (st.QuanXanh[i].value == 0)
                    x = x + 1000;
                if (st.QuanDo[i].value == 0)
                    y = y + 1000;
                if (st.QuanXanh[i].value > 0)
                    x = x + st.QuanXanh[i].value;
                if (st.QuanDo[i].value > 0)
                    y = y + st.QuanDo[i].value;
            }
            if (color == 1) return (x - y);
            else return (y - x);
        }*/
        public Searching(Board b)
        {
            brd = new Board();
            brd1 = new Board();
            brd2 = new Board();
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 11; j++)
                {
                    this.brd.setSquare(b.getInfo(i, j), i, j);
                    this.brd1.setSquare(b.getInfo(i, j), i, j);
                    this.brd2.setSquare(b.getInfo(i, j), i, j);
                }
            //brd = new Board();
            //UpdateBoard(b);
            Eval = new Evaluator();
            next_move = new Move();
            TransTable = new TranspositionTable();
            HisTable = HistoryTable.GetInstance();
            HisTable1 = HistoryTable.GetInstance();
            HisTable2 = HistoryTable.GetInstance();
            
        }

        //cap nhat ban co
        public void UpdateBoard(Board b)
        {
            int k;
            mtb[0,1] = mtb[0,2] = 0;
            for (int i = 0; i <= 9; i++)
            {
                st.set(ref st.QuanDo[i], -1);
                st.set(ref st.QuanXanh[i], -1);
                st1.set(ref st1.QuanDo[i], -1);
                st1.set(ref st1.QuanXanh[i], -1);
                st2.set(ref st2.QuanDo[i], -1);
                st2.set(ref st2.QuanXanh[i], -1);
            }
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 11; j++)
                {
                    k = b.getInfo(i, j);
                    this.brd.setSquare(k, i, j);
                    this.brd1.setSquare(k, i, j);
                    this.brd2.setSquare(k, i, j);
                    if (k!=-1)
                            if (k < 10) // quan xanh
                            {
                                st.set(ref st.QuanXanh[k], i, j, k);
                                st1.set(ref st1.QuanXanh[k], i, j, k);
                                st2.set(ref st2.QuanXanh[k], i, j, k);
                                mtb[0,1] = mtb[0,1] + k;
                            }
                            else  // quan do
                            {
                                st.set(ref st.QuanDo[k - 10], i, j, k - 10);
                                st1.set(ref st1.QuanDo[k - 10], i, j, k - 10);
                                st2.set(ref st2.QuanDo[k - 10], i, j, k - 10);
                                mtb[0,2] = mtb[0,2] + k - 10;
                            }


                }
            mtb[1, 1] = mtb[0, 1];
            mtb[1, 2] = mtb[0, 2];
            mtb[2, 1] = mtb[0, 1];
            mtb[2, 2] = mtb[0, 2];
        }

        //thuc hien nuoc di tu x1,y1 den x2,y2, da an quan value neu capture = true
        public bool MakeMove(int color, Move m,int depth)
        {
            int temp = brd.getInfo(m.x1, m.y1);
                if (!m.capture)
                {
                    brd.setSquare(temp, m.x2, m.y2);
                    brd.setSquare(-1, m.x1, m.y1);
                    if (color == 1)
                        st.set(ref st.QuanXanh[temp % 10],m.x2,m.y2);
                    else
                        st.set(ref st.QuanDo[temp % 10],m.x2,m.y2);
                    return false;
                }
                else
                    {
                        if (m.value < 10)
                        {
                            st.set(ref st.QuanXanh[m.value] ,-1);
                            st.set(ref st.QuanDo[temp - 10],m.x2,m.y2);
                        }
                        else
                        {
                            st.set(ref st.QuanDo[m.value - 10],-1);
                            st.set(ref st.QuanXanh[temp],m.x2,m.y2);
                        }
                        brd.setSquare(temp, m.x2, m.y2);
                        brd.setSquare(-1, m.x1, m.y1);
                        mtb[0,color] = mtb[0,color] + (m.value%10);
                        if (m.value % 10 == 0)
                        {
                            mtb[0,color] = mtb[0,color] + 1000 +50 * depth;
                            return true;
                        }
                        else
                            return false;
                    }

                }
        public bool MakeMove1(int color, Move m, int depth)
        {
            int temp = brd1.getInfo(m.x1, m.y1);
            if (!m.capture)
            {
                brd1.setSquare(temp, m.x2, m.y2);
                brd1.setSquare(-1, m.x1, m.y1);
                if (color == 1)
                    st1.set(ref st1.QuanXanh[temp % 10], m.x2, m.y2);
                else
                    st1.set(ref st1.QuanDo[temp % 10], m.x2, m.y2);
                return false;
            }
            else
            {
                if (m.value < 10)
                {
                    st1.set(ref st1.QuanXanh[m.value], -1);
                    st1.set(ref st1.QuanDo[temp - 10], m.x2, m.y2);
                }
                else
                {
                    st1.set(ref st1.QuanDo[m.value - 10], -1);
                    st1.set(ref st1.QuanXanh[temp], m.x2, m.y2);
                }
                brd1.setSquare(temp, m.x2, m.y2);
                brd1.setSquare(-1, m.x1, m.y1);
                mtb[1,color] = mtb[1,color] + (m.value % 10);
                if (m.value % 10 == 0)
                {
                    mtb[1,color] = mtb[1,color] + 1000 + 50 * depth;
                    return true;
                }
                else
                    return false;
            }

        }
        public bool MakeMove2(int color, Move m, int depth)
        {
            int temp = brd2.getInfo(m.x1, m.y1);
            if (!m.capture)
            {
                brd2.setSquare(temp, m.x2, m.y2);
                brd2.setSquare(-1, m.x1, m.y1);
                if (color == 1)
                    st2.set(ref st2.QuanXanh[temp % 10], m.x2, m.y2);
                else
                    st2.set(ref st2.QuanDo[temp % 10], m.x2, m.y2);
                return false;
            }
            else
            {
                if (m.value < 10)
                {
                    st2.set(ref st2.QuanXanh[m.value], -1);
                    st2.set(ref st2.QuanDo[temp - 10], m.x2, m.y2);
                }
                else
                {
                    st2.set(ref st2.QuanDo[m.value - 10], -1);
                    st2.set(ref st2.QuanXanh[temp], m.x2, m.y2);
                }
                brd2.setSquare(temp, m.x2, m.y2);
                brd2.setSquare(-1, m.x1, m.y1);
                mtb[2,color] = mtb[2,color] + (m.value % 10);
                if (m.value % 10 == 0)
                {
                    mtb[2,color] = mtb[2,color] + 1000 + 50 * depth;
                    return true;
                }
                else
                    return false;
            }

        }


        //bo thuc hien nuoc di tu x1,y1 den x2,y2, da an quan gt value neu capture = true
        public bool UnMakeMove(int color,Move m,int depth)
        {
            int temp = brd.getInfo(m.x2, m.y2);
                if (m.capture)
                {
                    if (m.value >= 10)
                    {
                        st.set(ref st.QuanDo[m.value-10],m.x2,m.y2,m.value-10 );
                        st.set(ref st.QuanXanh[temp],m.x1,m.y1);
                    }
                    else
                    {
                        st.set(ref st.QuanXanh[m.value],m.x2,m.y2,m.value);
                        st.set(ref st.QuanDo[temp - 10],m.x1,m.y1);
                    }
                        
                    brd.setSquare(temp, m.x1, m.y1);
                    brd.setSquare(m.value, m.x2, m.y2);
                    mtb[0,color] = mtb[0,color] - (m.value%10);
                    if ((m.value % 10) == 0) mtb[0,color] = mtb[0,color] - 1000 -50 * depth;
                    return true;
                }
                else
                {
                    if (temp >= 10)
                        st.set(ref st.QuanDo[temp - 10],m.x1,m.y1);
                    else
                        st.set(ref st.QuanXanh[temp],m.x1,m.y1);


                    brd.setSquare(temp, m.x1, m.y1);
                    brd.setSquare(-1, m.x2, m.y2);
                    return true;
                }
            
        }
        public bool UnMakeMove1(int color, Move m, int depth)
        {
            int temp = brd1.getInfo(m.x2, m.y2);
            if (m.capture)
            {
                if (m.value >= 10)
                {
                    st1.set(ref st1.QuanDo[m.value - 10], m.x2, m.y2, m.value - 10);
                    st1.set(ref st1.QuanXanh[temp], m.x1, m.y1);
                }
                else
                {
                    st1.set(ref st1.QuanXanh[m.value], m.x2, m.y2, m.value);
                    st1.set(ref st1.QuanDo[temp - 10], m.x1, m.y1);
                }

                brd1.setSquare(temp, m.x1, m.y1);
                brd1.setSquare(m.value, m.x2, m.y2);
                mtb[1,color] = mtb[1,color] - (m.value % 10);
                if ((m.value % 10) == 0) mtb[1,color] = mtb[1,color] - 1000 - 50 * depth;
                return true;
            }
            else
            {
                if (temp >= 10)
                    st1.set(ref st1.QuanDo[temp - 10], m.x1, m.y1);
                else
                    st1.set(ref st1.QuanXanh[temp], m.x1, m.y1);


                brd1.setSquare(temp, m.x1, m.y1);
                brd1.setSquare(-1, m.x2, m.y2);
                return true;
            }

        }
        public bool UnMakeMove2(int color, Move m, int depth)
        {
            int temp = brd2.getInfo(m.x2, m.y2);
            if (m.capture)
            {
                if (m.value >= 10)
                {
                    st2.set(ref st2.QuanDo[m.value - 10], m.x2, m.y2, m.value - 10);
                    st2.set(ref st2.QuanXanh[temp], m.x1, m.y1);
                }
                else
                {
                    st2.set(ref st2.QuanXanh[m.value], m.x2, m.y2, m.value);
                    st2.set(ref st2.QuanDo[temp - 10], m.x1, m.y1);
                }

                brd2.setSquare(temp, m.x1, m.y1);
                brd2.setSquare(m.value, m.x2, m.y2);
                mtb[2,color] = mtb[2,color] - (m.value % 10);
                if ((m.value % 10) == 0) mtb[2,color] = mtb[2,color] - 1000 - 50 * depth;
                return true;
            }
            else
            {
                if (temp >= 10)
                    st2.set(ref st2.QuanDo[temp - 10], m.x1, m.y1);
                else
                    st2.set(ref st2.QuanXanh[temp], m.x1, m.y1);


                brd2.setSquare(temp, m.x1, m.y1);
                brd2.setSquare(-1, m.x2, m.y2);
                return true;
            }

        }
                

        //tim kiem nuoc di theo alpha-beta pruning, do sau tim kiem la depth
        public int alpha_beta(int color, int alpha, int beta, int depth)
        {
            int best, i, value;

            Move[] moveable = new Move[200];
            int number = 0;


            //------------------------------------------------

            if (depth == 0)
            {

                return Quiescence(color, alpha, beta,0);
            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }                
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;
                   
                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                */
                HisTable.SortMoveList2(color, moveable, number);
                HisTable.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number)
                {
                    if (best > alpha) alpha = best;
                    if (MakeMove(color, moveable[i],depth))
                        value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                    else
                        value = -alpha_beta(3 - color, -beta, -alpha, depth - 1);
                    UnMakeMove(color, moveable[i],depth);

                    if (value > best)
                    {
                        best = value;
                        //neu ply=0 thi luu lai nuoc di
                        if (depth == MChess.MAX_PLY)
                        {
                            next_move = moveable[i];
                        }
                        //neu co the cat nhanh
                        if (best >= beta)
                        {
                            HisTable.AddCount(color, moveable[i]);
                            return best;
                        }
                    }
                    i++;
                }
                return best;
            }
        }
        public int alpha_beta2(int color, int alpha, int beta, int depth)
        {
            int best, i, value;

            Move[] moveable = new Move[200];
            int number = 0;


            //------------------------------------------------

            if (depth == 0)
            {

                //ancestor_value = Quiescence(color, alpha, beta, 0);
                //out_number = in_number;
                //wh.Set();
                return Quiescence2(color, alpha, beta, 0);
            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st2.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st2.move(brd2, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd2.getInfo(i, c.y));
                                }
                                else
                                    if (st2.capture(brd2, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd2.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st2.move(brd2, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd2.getInfo(c.x, i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd2.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd2.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd2.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd2.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd2.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st2.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st2.move(brd2, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd2.getInfo(i, c.y));
                                }
                                else
                                    if (st2.capture(brd2, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd2.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st2.move(brd2, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd2.getInfo(c.x, i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd2.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd2.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd2.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd2.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd2.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }                
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;

                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                */
                HisTable2.SortMoveList2(color, moveable, number);
                HisTable2.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number)
                {
                    if (best > alpha) alpha = best;
                    if (MakeMove2(color, moveable[i], depth))
                        value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                    else
                        value = -alpha_beta2(3 - color, -beta, -alpha, depth - 1);
                    UnMakeMove2(color, moveable[i], depth);

                    if (value > best)
                    {
                        best = value;
                        //neu ply=0 thi luu lai nuoc di
                        if (depth == MChess.MAX_PLY)
                        {
                            next_move = moveable[i];
                        }
                        //neu co the cat nhanh
                        if (best >= beta)
                        {
                            HisTable2.AddCount(color, moveable[i]);
                            //ancestor_value = best;
                            //out_number = in_number;
                            //wh.Set();
                            return best;
                        }
                    }
                    i++;
                }
                //ancestor_value = best;
                //out_number = in_number;
                //wh.Set();
                return best;
            }
        }
        public int alpha_beta1(int color, int alpha, int beta, int depth)
        {
            int best, i, value;

            Move[] moveable = new Move[200];
            int number = 0;


            //------------------------------------------------

            if (depth == 0)
            {
                return Quiescence1(color, alpha, beta, 0);
            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st1.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st1.move(brd1, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd1.getInfo(i, c.y));
                                }
                                else
                                    if (st1.capture(brd1, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd1.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st1.move(brd1, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd1.getInfo(c.x, i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd1.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd1.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd1.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd1.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd1.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st1.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st1.move(brd1, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd1.getInfo(i, c.y));
                                }
                                else
                                    if (st1.capture(brd1, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd1.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st1.move(brd1, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd1.getInfo(c.x, i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd1.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd1.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd1.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd1.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd1.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                /*dem = 0;
                for (int l = 0; l < 9; l++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd1.getInfo(l, j);
                        if (temp != -1 && brd1.getColor(l, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, l, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != l)
                                {
                                    if (quan_co.capture(brd1, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, true, brd1.getInfo(k, j));
                                    }

                                    if (quan_co.move(brd1, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd1, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, true, brd1.getInfo(l, k));
                                    }

                                    if (quan_co.move(brd1, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(l, j); k <= Math.Min(8 - l, 10 - j); k++)
                            {
                                if (quan_co.capture(brd1, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, true, brd1.getInfo(l + k, j + k));
                                }

                                if (quan_co.move(brd1, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-l, j - 10); k <= Math.Min(j, 8 - l); k++)
                            {
                                if (quan_co.capture(brd1, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, true, brd1.getInfo(l + k, j - k));
                                }

                                if (quan_co.move(brd1, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }*/
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;

                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                */
                HisTable1.SortMoveList2(color, moveable, number);
                HisTable1.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number)
                {
                    if (best > alpha) alpha = best;
                    if (MakeMove1(color, moveable[i], depth))
                        value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                    else
                        value = -alpha_beta1(3 - color, -beta, -alpha, depth - 1);
                    UnMakeMove1(color, moveable[i], depth);

                    if (value > best)
                    {
                        best = value;
                        //neu ply=0 thi luu lai nuoc di
                        if (depth == MChess.MAX_PLY)
                        {
                            next_move = moveable[i];
                        }
                        //neu co the cat nhanh
                        if (best >= beta)
                        {
                            HisTable1.AddCount(color, moveable[i]);                            
                            return best;
                        }
                    }
                    i++;
                }                
                return best;
            }
        }
        public void alpha_beta(int color, int alpha, int beta, int depth , out int ancestor_value,int in_number , out int out_number)
        {
            int temp = in_number;
            int best, i, value;            
            Move[] moveable = new Move[200];
            int number = 0;



            //------------------------------------------------

            if (depth == 0)
            {

                ancestor_value = Quiescence(color, alpha, beta, 0);
                out_number = temp;
                wh.Set();                
                return;
            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;

                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                */
                HisTable.SortMoveList2(color, moveable, number);
                HisTable.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number)
                {
                    if (best > alpha) alpha = best;
                    if (MakeMove(color, moveable[i], depth))
                        value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                    else
                        value = -alpha_beta(3 - color, -beta, -alpha, depth - 1);
                    UnMakeMove(color, moveable[i], depth);

                    if (value > best)
                    {
                        best = value;
                        //neu ply=0 thi luu lai nuoc di
                        if (depth == MChess.MAX_PLY)
                        {
                            next_move = moveable[i];
                        }
                        //neu co the cat nhanh
                        if (best >= beta)
                        {
                            HisTable.AddCount(color, moveable[i]);
                            ancestor_value = best;
                            out_number = temp;
                            wh.Set();                            
                            return ;
                        }
                    }
                    i++;
                }
                ancestor_value = best;
                out_number = temp;
                wh.Set();                
                return ;                
            }
        }
        public void alpha_beta1(int color, int alpha, int beta, int depth, out int ancestor_value, int in_number, out int out_number,out bool stt)
        {
            int temp = in_number;
            int best, i, value;
            Move[] moveable = new Move[200];
            int number = 0;


            //------------------------------------------------

            if (depth == 0)
            {

                ancestor_value = Quiescence(color, alpha, beta, 0);
                out_number = temp;
                stt = false;
                //wh.Set();
                return;
            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st1.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st1.move(brd1, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd1.getInfo(i, c.y));
                                }
                                else
                                    if (st1.capture(brd1, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd1.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st1.move(brd1, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd1.getInfo(c.x, i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd1.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd1.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd1.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd1.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd1.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st1.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st1.move(brd1, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd1.getInfo(i, c.y));
                                }
                                else
                                    if (st1.capture(brd1, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd1.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st1.move(brd1, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd1.getInfo(c.x, i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd1.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd1.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd1.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st1.move(brd1, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd1.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st1.capture(brd1, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd1.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                /*dem = 0;
                for (int l = 0; l < 9; l++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd1.getInfo(l, j);
                        if (temp != -1 && brd1.getColor(l, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, l, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != l)
                                {
                                    if (quan_co.capture(brd1, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, true, brd1.getInfo(k, j));
                                    }

                                    if (quan_co.move(brd1, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd1, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, true, brd1.getInfo(l, k));
                                    }

                                    if (quan_co.move(brd1, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(l, j); k <= Math.Min(8 - l, 10 - j); k++)
                            {
                                if (quan_co.capture(brd1, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, true, brd1.getInfo(l + k, j + k));
                                }

                                if (quan_co.move(brd1, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-l, j - 10); k <= Math.Min(j, 8 - l); k++)
                            {
                                if (quan_co.capture(brd1, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, true, brd1.getInfo(l + k, j - k));
                                }

                                if (quan_co.move(brd1, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }*/
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;

                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                */
                HisTable1.SortMoveList2(color, moveable, number);
                HisTable1.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number)
                {
                    if (best > alpha) alpha = best;
                    if (MakeMove1(color, moveable[i], depth))
                        value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                    else
                        value = -alpha_beta1(3 - color, -beta, -alpha, depth - 1);
                    UnMakeMove1(color, moveable[i], depth);

                    if (value > best)
                    {
                        best = value;
                        //neu ply=0 thi luu lai nuoc di
                        if (depth == MChess.MAX_PLY)
                        {
                            next_move = moveable[i];
                        }
                        //neu co the cat nhanh
                        if (best >= beta)
                        {
                            HisTable1.AddCount(color, moveable[i]);
                            ancestor_value = best;
                            out_number = temp;
                            //wh.Set();
                            stt = false;
                            return;
                        }
                    }
                    i++;
                }
                ancestor_value = best;
                out_number = temp;
                //wh.Set();
                stt = false;
                return;
            }
        }
        public void alpha_beta2(int color, int alpha, int beta, int depth, out int ancestor_value, int in_number, out int out_number , out bool stt)
        {
            int temp = in_number;
            int best, i, value;
            Move[] moveable = new Move[200];
            int number = 0;


            //------------------------------------------------

            if (depth == 0)
            {

                ancestor_value = Quiescence(color, alpha, beta, 0);
                out_number = temp;
                //wh.Set();
                stt = false;
                return;
            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st2.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st2.move(brd2, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd2.getInfo(i, c.y));
                                }
                                else
                                    if (st2.capture(brd2, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd2.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st2.move(brd2, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd2.getInfo(c.x, i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd2.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd2.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd2.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd2.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd2.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st2.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st2.move(brd2, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd2.getInfo(i, c.y));
                                }
                                else
                                    if (st2.capture(brd2, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd2.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st2.move(brd2, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd2.getInfo(c.x, i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd2.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd2.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd2.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st2.move(brd2, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd2.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st2.capture(brd2, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd2.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                /*dem = 0;
                for (int l = 0; l < 9; l++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd2.getInfo(l, j);
                        if (temp != -1 && brd2.getColor(l, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, l, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != l)
                                {
                                    if (quan_co.capture(brd2, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, true, brd2.getInfo(k, j));
                                    }

                                    if (quan_co.move(brd2, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd2, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, true, brd2.getInfo(l, k));
                                    }

                                    if (quan_co.move(brd2, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(l, j); k <= Math.Min(8 - l, 10 - j); k++)
                            {
                                if (quan_co.capture(brd2, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, true, brd2.getInfo(l + k, j + k));
                                }

                                if (quan_co.move(brd2, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-l, j - 10); k <= Math.Min(j, 8 - l); k++)
                            {
                                if (quan_co.capture(brd2, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, true, brd2.getInfo(l + k, j - k));
                                }

                                if (quan_co.move(brd2, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }*/
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;

                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                */
                HisTable2.SortMoveList2(color, moveable, number);
                HisTable2.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number)
                {
                    if (best > alpha) alpha = best;
                    if (MakeMove2(color, moveable[i], depth))
                        value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                    else
                        value = -alpha_beta2(3 - color, -beta, -alpha, depth - 1);
                    UnMakeMove2(color, moveable[i], depth);

                    if (value > best)
                    {
                        best = value;
                        //neu ply=0 thi luu lai nuoc di
                        if (depth == MChess.MAX_PLY)
                        {
                            next_move = moveable[i];
                        }
                        //neu co the cat nhanh
                        if (best >= beta)
                        {
                            HisTable2.AddCount(color, moveable[i]);
                            ancestor_value = best;
                            out_number = temp;
                            //wh.Set();
                            stt = false;
                            return;
                        }
                    }
                    i++;
                }
                ancestor_value = best;
                out_number = temp;
                //wh.Set();
                stt = false;
                return;
            }
        }
        
        public int parallel_alpha_beta(int color, int alpha, int beta, int depth, int in_number, out int out_number)
        {
            int tmp = in_number;
            int i = 0;
            int best, value;
            object locker = new object();
            Move[] moveable = new Move[200];
            int number = 0;
            int move_number1  = 0;
            int move_number2 = 0;
            int move_number = 0;
            int temp,temp1,temp2;
            bool stt1 = false;
            bool stt2 = false;
            EventWaitHandle Wh2 = new AutoResetEvent(false);
            //Thread th1 = new Thread(delegate() { alpha_beta1(3 - color, -beta, -alpha, depth - 1, out value, i, out move_number); UnMakeMove1(color, moveable[move_number], depth); });
            //Thread th2 = new Thread(delegate() { alpha_beta2(3 - color, -beta, -alpha, depth - 1, out value, i, out move_number); UnMakeMove2(color, moveable[move_number], depth); });
            
            Board b1 = new Board();
            Board b2 = new Board();

            for (int i1 = 0; i1 < 9; i1++)
                for (int j = 0; j < 11; j++)
                {
                    b1.setSquare(brd1.getInfo(i1, j), i1, j);
                }
            for (int i1 = 0; i1 < 9; i1++)
                for (int j = 0; j < 11; j++)
                {
                    b2.setSquare(brd2.getInfo(i1, j), i1, j);
                }
            for (int i1 = 0; i1 < 9; i1++)
                for (int j = 0; j < 11; j++)
                {
                    b3.setSquare(brd1.getInfo(i1, j), i1, j);
                }
            for (int i1 = 0; i1 < 9; i1++)
                for (int j = 0; j < 11; j++)
                {
                    b4.setSquare(brd2.getInfo(i1, j), i1, j);
                }
            //------------------------------------------------

            if (depth == 0)
            {

                out_number = tmp;
                wh.Set();
                return Quiescence(color, alpha, beta, 0);

            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                /*dem = 0;
                for (int l = 0; l < 9; l++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd.getInfo(l, j);
                        if (temp != -1 && brd.getColor(l, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, l, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != l)
                                {
                                    if (quan_co.capture(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, true, brd.getInfo(k, j));
                                    }

                                    if (quan_co.move(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, true, brd.getInfo(l, k));
                                    }

                                    if (quan_co.move(brd, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(l, j); k <= Math.Min(8 - l, 10 - j); k++)
                            {
                                if (quan_co.capture(brd, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, true, brd.getInfo(l + k, j + k));
                                }

                                if (quan_co.move(brd, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-l, j - 10); k <= Math.Min(j, 8 - l); k++)
                            {
                                if (quan_co.capture(brd, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, true, brd.getInfo(l + k, j - k));
                                }

                                if (quan_co.move(brd, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }*/
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;

                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                */
                HisTable.SortMoveList2(color, moveable, number);
                HisTable.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                value = best;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number )
                {
                    if (i < number + 1)
                    {
                        if (best > alpha) alpha = best;
                        if (i == 1)
                        {
                            if (MakeMove(color, moveable[i], depth))
                            {
                                value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                                UnMakeMove(color, moveable[i], depth);
                            }
                            else
                            {

                                MakeMove1(color, moveable[i], depth);
                                MakeMove2(color, moveable[i], depth);
                                temp2 = i;
                                value = -parallel_alpha_beta(3 - color, -beta, -alpha, depth - 1, temp2, out move_number);
                                UnMakeMove(color, moveable[i], depth);
                                UnMakeMove1(color, moveable[i], depth);
                                UnMakeMove2(color, moveable[i], depth);
                            }
                        }
                        else
                            if (!stt1)
                            {
                                if (MakeMove1(color, moveable[i], depth))
                                {
                                    value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                                    UnMakeMove1(color, moveable[i], depth);
                                }
                                else
                                {
                                    stt1 = true;
                                    move_number1 = -1;
                                    temp1 = i;
                                    new Thread(delegate() { alpha_beta1(3 - color, -beta, -alpha, depth - 1, out value, temp1, out move_number1, out stt1); UnMakeMove1(color, moveable[move_number1], depth); wh.Set(); }).Start();
                                }
                            }
                            else if (!stt2)
                            {
                                if (MakeMove2(color, moveable[i], depth))
                                {
                                    value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                                    UnMakeMove2(color, moveable[i], depth);
                                }
                                else
                                {
                                    stt1 = true;
                                    move_number2 = -1;
                                    temp2 = i;
                                    new Thread(delegate() { alpha_beta2(3 - color, -beta, -alpha, depth - 1, out value, temp2, out move_number2, out stt2); UnMakeMove2(color, moveable[move_number2], depth); wh.Set(); }).Start();
                                }
                            }
                        Wh2.Set();
                    }

                    if (i!=2)
                        wh.WaitOne();
                    Wh2.WaitOne();
                    if (move_number1 != -1)
                        temp = move_number1;
                    else if (move_number2 != -1)
                        temp = move_number2;
                    else
                    temp = move_number;
                    lock (locker)
                    {

                        if (value > best)
                        {
                            best = value;
                            //neu ply=0 thi luu lai nuoc di
                            if (depth == MChess.MAX_PLY)
                            {
                                next_move = moveable[temp];
                            }
                            //neu co the cat nhanh
                            if (best >= beta)
                            {
                                HisTable.AddCount(color, moveable[temp]);
                                out_number = tmp;
                                wh.Set();
                                return best;
                            }
                        }
                    }
                    i++;
                }
                out_number = tmp;
                wh.Set();
                return best;
            }
        }
        #region parallel_alpha_beta(int color, int alpha, int beta, int depth) - không dùng
        /*public int parallel_alpha_beta(int color, int alpha, int beta, int depth)
        {
            int i = 0;
            int best, value;
            object locker = new object();
            Move[] moveable = new Move[200];
            int number = 0;
            int move_number = 0;
            int temp;
            bool stt1 = false;
            bool stt2 = false;
            //Thread th1 = new Thread(delegate() { alpha_beta1(3 - color, -beta, -alpha, depth - 1, out value, i, out move_number); UnMakeMove1(color, moveable[move_number], depth); });
            //Thread th2 = new Thread(delegate() { alpha_beta2(3 - color, -beta, -alpha, depth - 1, out value, i, out move_number); UnMakeMove2(color, moveable[move_number], depth); });
            //Board b1 = new Board();
            for (int i1 = 0; i1 < 9; i1++)
                for (int j = 0; j < 11; j++)
                {
                    b1.setSquare(brd1.getInfo(i1, j), i1, j);
                }
            Board b2 = new Board();
            for (int i1 = 0; i1 < 9; i1++)
                for (int j = 0; j < 11; j++)
                {
                    b2.setSquare(brd2.getInfo(i1, j), i1, j);
                }
            //------------------------------------------------

            if (depth == 0)
            {

                //out_number = in_number;
                wh.Set();
                return Quiescence(color, alpha, beta, 0);

            }
            else
            {
                //#region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                else
                    foreach (state.chess c in st.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                /*dem = 0;
                for (int l = 0; l < 9; l++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd.getInfo(l, j);
                        if (temp != -1 && brd.getColor(l, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, l, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != l)
                                {
                                    if (quan_co.capture(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, true, brd.getInfo(k, j));
                                    }

                                    if (quan_co.move(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, true, brd.getInfo(l, k));
                                    }

                                    if (quan_co.move(brd, l, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(l, j, l, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(l, j); k <= Math.Min(8 - l, 10 - j); k++)
                            {
                                if (quan_co.capture(brd, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, true, brd.getInfo(l + k, j + k));
                                }

                                if (quan_co.move(brd, l + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-l, j - 10); k <= Math.Min(j, 8 - l); k++)
                            {
                                if (quan_co.capture(brd, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, true, brd.getInfo(l + k, j - k));
                                }

                                if (quan_co.move(brd, l + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(l, j, l + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;

                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate?
                //Sap xep lai mang moveable[1..number]?
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan?
                /*
                if (depth > 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                
                HisTable.SortMoveList2(color, moveable, number);
                HisTable.SortMoveList(color, moveable, number);
                //------------------------------------------------------

                best = Int32.MinValue + 1;
                value = best;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number + 1)
                {
                    if (i < number )
                    {
                        if (best > alpha) alpha = best;
                        if (i == 1)
                        {
                            if (MakeMove(color, moveable[i], depth))
                            {
                                value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                                UnMakeMove(color, moveable[i], depth);
                            }
                            else
                            {
                                MakeMove1(color, moveable[i], depth) ;
                                MakeMove2(color, moveable[i], depth);
                                value = -parallel_alpha_beta(3 - color, -beta, -alpha, depth - 1, i, out move_number);
                                UnMakeMove(color, moveable[i], depth);
                                UnMakeMove1(color, moveable[i], depth);
                                UnMakeMove2(color, moveable[i], depth);
                            }
                        }
                        else
                            if (!stt1)
                            {
                                if (MakeMove1(color, moveable[i], depth))
                                {
                                    value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                                    UnMakeMove1(color, moveable[i], depth);
                                }
                                else
                                {
                                    stt1 = true;
                                    new Thread(delegate() { alpha_beta1(3 - color, -beta, -alpha, depth - 1, out value, i, out move_number, out stt1); UnMakeMove1(color, moveable[move_number], depth); wh.Set(); }).Start();
                                }
                            }
                            else if (!stt2)
                            {
                                if (MakeMove2(color, moveable[i], depth))
                                {
                                    value = value = 100000 + 5000 * depth;// an quan 0, +depth -Mchess.maxply de xac dinh nuoc nao gan nhat an duoc quan 0
                                    UnMakeMove2(color, moveable[i], depth);
                                }
                                else
                                {
                                    stt1 = true;
                                    new Thread(delegate() { alpha_beta2(3 - color, -beta, -alpha, depth - 1, out value, i, out move_number, out stt2); UnMakeMove2(color, moveable[move_number], depth); wh.Set(); }).Start();
                                }
                            }

                    }
                    if (i > 1)
                        wh.WaitOne();
                    temp = move_number;
                    lock (locker)
                    {

                        if (value > best)
                        {
                            best = value;
                            //neu ply=0 thi luu lai nuoc di
                            if (depth == MChess.MAX_PLY)
                            {
                                next_move = moveable[temp];
                            }
                            //neu co the cat nhanh
                            if (best >= beta)
                            {
                                HisTable.AddCount(color, moveable[temp]);
                                //out_number = in_number;
                                wh.Set();
                                return best;
                            }
                        }
                    }
                    i++;
                }
                //out_number = in_number;
                wh.Set();
                return best;
            }
        }*/
        #endregion
        //Quiescence Search
        //Y tuong Quiescence: Tai nut la, ta xet cac kha nang an quan (khong nhieu) va di theo
        //cac nhanh an quan tim duoc
        //Chu y o day khong gioi han do sau, neu co qua nhieu nuoc an quan lien tiep nhau thi co
        //the Quiescence Search se lam viec lau hon
        //thong so QuiescenceNode cho so nut da thuc hien Quiescence, trong do tat nhien chua toan
        //bo cac nut la khi xet alpha-beta
        //b = do phan nhanh trung binh tai mot nut (tuc so nuoc an quan, b kha nho)
        //Khi do Quiescene muc 1 (sau muc nut la) co so nut gap b lan so nut la, muc 2 gap b^2 lan, ...
        //Co the bo sung nuoc di chieu tuong (de doa quan 0) them cho danh sach nuoc di
        public int Quiescence(int color, int alpha, int beta,int depth)
        {
            int value, best,i;
            Move[] moveable;
            int number = 0;

            //value = Eval.MaterialBalance(brd, color);
            //null-move pruning
            //

            //value = Eval.Evaluate(brd, color);
            if (MChess.Quickeval)
                value = 100 * (mtb[0,color] - mtb[0,3 - color]);
            else 
                value = 100*(mtb[0,color]-mtb[0,3-color])+Eval.BoardControl2 (brd ,st,color) +Eval.Attack2(brd ,st,color)  ;
            //temp2 = Eval.Evaluate(brd, color);
            //if (temp3!=temp2) Console.Write ("");
            if (value >= beta) return value;

            alpha = Math.Max(value, alpha);

            #region sinh nuoc di an quan
            //sinh cac nuoc di co the an duoc quan (capture)
            moveable = new Move[100];
            if(color==1)
                foreach( state.chess c in st.QuanXanh)
                {
                    if(c.value<=0) continue;
                    //kiem tra cac o cung hang
                    for(i=0;i<9;i++)
                        if(i!=c.x)
                            if(st.capture(brd ,c,i,c.y))
                            {
                                number++;
                                moveable[number]=new Move(c.x,c.y,i,c.y,true,brd.getInfo(i,c.y));
                            }
                    //kiem tra cac o cung cot
                    for(i=0;i<11;i++)
                        if(i!=c.y)
                            if(st.capture(brd ,c,c.x,i))
                            {
                                number++;
                                moveable[number]=new Move(c.x,c.y,c.x,i,true,brd.getInfo(c.x,i));
                            }
                    //kiem tra cac o cung duong cheo chinh
                    for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                        if(i!=0) 
                            if (st.capture(brd,c, c.x + i, c.y + i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x+ i ,c.y+ i, true, brd.getInfo(c.x+i,c.y+i));
                            }

                    //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if(i!=0)
                            if (st.capture(brd, c,c.x+i,c.y-i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y-i, true, brd.getInfo(c.x+i, c.y-i));
                            }

                        }
                }
            else     /// Quan do
                foreach (state.chess c in st.QuanDo)
                {
                    if (c.value <= 0) continue;
                    //kiem tra cac o cung hang
                    for (i = 0; i < 9; i++)
                        if (i != c.x)
                            if (st.capture(brd, c, i, c.y))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                            }
                    //kiem tra cac o cung cot
                    for (i = 0; i < 11; i++)
                        if (i != c.y)
                            if (st.capture(brd, c, c.x, i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                            }
                    //kiem tra cac o cung duong cheo chinh
                    for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                        if (i != 0)
                            if (st.capture(brd, c, c.x + i, c.y + i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                            }

                    //kiem tra cac o cung duong cheo phu
                    for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                    {
                        if(i!=0)
                        if (st.capture(brd, c, c.x + i, c.y - i))
                        {
                            number++;
                            moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                        }

                    }
                }
                        
            #endregion
            HisTable.SortMoveList2(color, moveable, number);
            HisTable.SortMoveList(color, moveable, number);
            
            #region Sinh moi nuoc di - khong su dung
            /* Chi su dung neu nhu co gioi han do sau toi da cho Quiescence */
            /*
            #region Sinh moi nuoc di
            //sinh nuoc di tu vi tri hien tai
            //tim nuoc di
            moveable = new Move[200];
            dem = 0;
            for (i = 0; i < 9; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    int temp = brd.getInfo(i, j);
                    if (temp != -1 && brd.getColor(i, j) == color && temp % 10 != 0)
                    {
                        //quan minh
                        dem++;
                        quan19 quan_co = new quan19(color, temp % 10, i, j);
                        //kiem tra cac o cung hang
                        for (int k = 0; k < 9; k++)
                        {
                            if (k != i)
                            {
                                if (quan_co.capture(brd, k, j))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, k, j, true, brd.getInfo(k, j));
                                }

                                if (quan_co.move(brd, k, j))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, k, j, false, -1);
                                }
                            }
                        }
                        //kiem tra cac o cung cot
                        for (int k = 0; k < 11; k++)
                        {
                            if (k != j)
                            {
                                if (quan_co.capture(brd, i, k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i, k, true, brd.getInfo(i, k));
                                }

                                if (quan_co.move(brd, i, k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i, k, false, -1);
                                }
                            }
                        }
                        //kiem tra cac o cung duong cheo chinh
                        for (int k = -Math.Min(i, j); k <= Math.Min(8 - i, 10 - j); k++)
                        {
                            if (quan_co.capture(brd, i + k, j + k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j + k, true, brd.getInfo(i + k, j + k));
                            }

                            if (quan_co.move(brd, i + k, j + k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j + k, false, -1);
                            }
                        }
                        //kiem tra cac o cung duong cheo phu
                        for (int k = Math.Max(-i, j - 10); k <= Math.Min(j, 8 - i); k++)
                        {
                            if (quan_co.capture(brd, i + k, j - k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j - k, true, brd.getInfo(i + k, j - k));
                            }

                            if (quan_co.move(brd, i + k, j - k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j - k, false, -1);
                            }
                        }
                        if (dem >= 9) break;
                    }
                }
                //da tim duoc du 9 quan, khong can xet nua
                if (dem >= 9) break;
            }
             */
            #endregion
            
            //khong co nuoc an quan
            if (number == 0)
                return value;

            best = Int32.MinValue + 1;
            i = 1;
            //bat dau search
            while (i <= number)
            {
                alpha = Math.Max(best, alpha);
                if(MakeMove(color,moveable[i],depth))
                    value = 100000  + 5000*depth;
                else
                    value = - Quiescence(3-color, -beta, -alpha,depth-1);
                UnMakeMove(color,moveable[i],depth);
                if (value > best)
                {
                    best = value;
                    if (best >= beta)
                    {
                        HisTable.AddCount(color, moveable[i]);
                        return best;
                    }
                }
                i++;
            }
            return best;
        }
        public int Quiescence1(int color, int alpha, int beta, int depth)
        {
            int value, best, i;
            Move[] moveable;
            int number = 0;

            //value = Eval.MaterialBalance(brd1, color);
            //null-move pruning
            //

            //value = Eval.Evaluate(brd1, color);
            if (MChess.Quickeval)
                value = 100 * (mtb[1,color] - mtb[1,3 - color]);
            else
                value = 100 * (mtb[1,color] - mtb[1,3 - color]) + Eval.BoardControl2(brd1, st1, color) + Eval.Attack2(brd1, st1, color);
            //temp2 = Eval.Evaluate(brd1, color);
            //if (temp3!=temp2) Console.Write ("");
            if (value >= beta) return value;

            alpha = Math.Max(value, alpha);

            #region sinh nuoc di an quan
            //sinh cac nuoc di co the an duoc quan (capture)
            moveable = new Move[100];
            if (color == 1)
                foreach (state.chess c in st1.QuanXanh)
                {
                    if (c.value <= 0) continue;
                    //kiem tra cac o cung hang
                    for (i = 0; i < 9; i++)
                        if (i != c.x)
                            if (st1.capture(brd1, c, i, c.y))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, i, c.y, true, brd1.getInfo(i, c.y));
                            }
                    //kiem tra cac o cung cot
                    for (i = 0; i < 11; i++)
                        if (i != c.y)
                            if (st1.capture(brd1, c, c.x, i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x, i, true, brd1.getInfo(c.x, i));
                            }
                    //kiem tra cac o cung duong cheo chinh
                    for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                        if (i != 0)
                            if (st1.capture(brd1, c, c.x + i, c.y + i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd1.getInfo(c.x + i, c.y + i));
                            }

                    //kiem tra cac o cung duong cheo phu
                    for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                    {
                        if (i != 0)
                            if (st1.capture(brd1, c, c.x + i, c.y - i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd1.getInfo(c.x + i, c.y - i));
                            }

                    }
                }
            else     /// Quan do
                foreach (state.chess c in st1.QuanDo)
                {
                    if (c.value <= 0) continue;
                    //kiem tra cac o cung hang
                    for (i = 0; i < 9; i++)
                        if (i != c.x)
                            if (st1.capture(brd1, c, i, c.y))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, i, c.y, true, brd1.getInfo(i, c.y));
                            }
                    //kiem tra cac o cung cot
                    for (i = 0; i < 11; i++)
                        if (i != c.y)
                            if (st1.capture(brd1, c, c.x, i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x, i, true, brd1.getInfo(c.x, i));
                            }
                    //kiem tra cac o cung duong cheo chinh
                    for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                        if (i != 0)
                            if (st1.capture(brd1, c, c.x + i, c.y + i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd1.getInfo(c.x + i, c.y + i));
                            }

                    //kiem tra cac o cung duong cheo phu
                    for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                    {
                        if (i != 0)
                            if (st1.capture(brd1, c, c.x + i, c.y - i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd1.getInfo(c.x + i, c.y - i));
                            }

                    }
                }
                       
            #endregion
            HisTable1.SortMoveList2(color, moveable, number);
            HisTable1.SortMoveList(color, moveable, number);

            #region Sinh moi nuoc di - khong su dung
            /* Chi su dung neu nhu co gioi han do sau toi da cho Quiescence */
            /*
            #region Sinh moi nuoc di
            //sinh nuoc di tu vi tri hien tai
            //tim nuoc di
            moveable = new Move[200];
            dem = 0;
            for (i = 0; i < 9; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    int temp = brd1.getInfo(i, j);
                    if (temp != -1 && brd1.getColor(i, j) == color && temp % 10 != 0)
                    {
                        //quan minh
                        dem++;
                        quan19 quan_co = new quan19(color, temp % 10, i, j);
                        //kiem tra cac o cung hang
                        for (int k = 0; k < 9; k++)
                        {
                            if (k != i)
                            {
                                if (quan_co.capture(brd1, k, j))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, k, j, true, brd1.getInfo(k, j));
                                }

                                if (quan_co.move(brd1, k, j))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, k, j, false, -1);
                                }
                            }
                        }
                        //kiem tra cac o cung cot
                        for (int k = 0; k < 11; k++)
                        {
                            if (k != j)
                            {
                                if (quan_co.capture(brd1, i, k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i, k, true, brd1.getInfo(i, k));
                                }

                                if (quan_co.move(brd1, i, k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i, k, false, -1);
                                }
                            }
                        }
                        //kiem tra cac o cung duong cheo chinh
                        for (int k = -Math.Min(i, j); k <= Math.Min(8 - i, 10 - j); k++)
                        {
                            if (quan_co.capture(brd1, i + k, j + k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j + k, true, brd1.getInfo(i + k, j + k));
                            }

                            if (quan_co.move(brd1, i + k, j + k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j + k, false, -1);
                            }
                        }
                        //kiem tra cac o cung duong cheo phu
                        for (int k = Math.Max(-i, j - 10); k <= Math.Min(j, 8 - i); k++)
                        {
                            if (quan_co.capture(brd1, i + k, j - k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j - k, true, brd1.getInfo(i + k, j - k));
                            }

                            if (quan_co.move(brd1, i + k, j - k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j - k, false, -1);
                            }
                        }
                        if (dem >= 9) break;
                    }
                }
                //da tim duoc du 9 quan, khong can xet nua
                if (dem >= 9) break;
            }
             */
            #endregion

            //khong co nuoc an quan
            if (number == 0)
                return value;

            best = Int32.MinValue + 1;
            i = 1;
            //bat dau search
            while (i <= number)
            {
                alpha = Math.Max(best, alpha);
                if (MakeMove1(color, moveable[i], depth))
                    value = 100000 + 5000 * depth;
                else
                    value = -Quiescence1(3 - color, -beta, -alpha, depth - 1);
                UnMakeMove1(color, moveable[i], depth);
                if (value > best)
                {
                    best = value;
                    if (best >= beta)
                    {
                        HisTable1.AddCount(color, moveable[i]);
                        return best;
                    }
                }
                i++;
            }
            return best;
        }
        public int Quiescence2(int color, int alpha, int beta, int depth)
        {
            int value, best, i;
            Move[] moveable;
            int number = 0;        
            
            //null-move pruning
           
            if (MChess.Quickeval)
                value = 100 * (mtb[2,color] - mtb[2,3 - color]);
            else
                value = 100 * (mtb[2,color] - mtb[2,3 - color]) + Eval.BoardControl2(brd2, st2, color) + Eval.Attack2(brd2, st2, color);
            //temp2 = Eval.Evaluate(brd2, color);
            //if (temp3!=temp2) Console.Write ("");
            if (value >= beta) return value;

            alpha = Math.Max(value, alpha);

            #region sinh nuoc di an quan
            //sinh cac nuoc di co the an duoc quan (capture)
            moveable = new Move[100];
            if (color == 1)
                foreach (state.chess c in st2.QuanXanh)
                {
                    if (c.value <= 0) continue;
                    //kiem tra cac o cung hang
                    for (i = 0; i < 9; i++)
                        if (i != c.x)
                            if (st2.capture(brd2, c, i, c.y))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, i, c.y, true, brd2.getInfo(i, c.y));
                            }
                    //kiem tra cac o cung cot
                    for (i = 0; i < 11; i++)
                        if (i != c.y)
                            if (st2.capture(brd2, c, c.x, i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x, i, true, brd2.getInfo(c.x, i));
                            }
                    //kiem tra cac o cung duong cheo chinh
                    for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                        if (i != 0)
                            if (st2.capture(brd2, c, c.x + i, c.y + i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd2.getInfo(c.x + i, c.y + i));
                            }

                    //kiem tra cac o cung duong cheo phu
                    for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                    {
                        if (i != 0)
                            if (st2.capture(brd2, c, c.x + i, c.y - i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd2.getInfo(c.x + i, c.y - i));
                            }

                    }
                }
            else     /// Quan do
                foreach (state.chess c in st2.QuanDo)
                {
                    if (c.value <= 0) continue;
                    //kiem tra cac o cung hang
                    for (i = 0; i < 9; i++)
                        if (i != c.x)
                            if (st2.capture(brd2, c, i, c.y))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, i, c.y, true, brd2.getInfo(i, c.y));
                            }
                    //kiem tra cac o cung cot
                    for (i = 0; i < 11; i++)
                        if (i != c.y)
                            if (st2.capture(brd2, c, c.x, i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x, i, true, brd2.getInfo(c.x, i));
                            }
                    //kiem tra cac o cung duong cheo chinh
                    for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                        if (i != 0)
                            if (st2.capture(brd2, c, c.x + i, c.y + i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd2.getInfo(c.x + i, c.y + i));
                            }

                    //kiem tra cac o cung duong cheo phu
                    for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                    {
                        if (i != 0)
                            if (st2.capture(brd2, c, c.x + i, c.y - i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd2.getInfo(c.x + i, c.y - i));
                            }

                    }
                }
                        
            #endregion
            HisTable2.SortMoveList2(color, moveable, number);
            HisTable2.SortMoveList(color, moveable, number);

            #region Sinh moi nuoc di - khong su dung
            /* Chi su dung neu nhu co gioi han do sau toi da cho Quiescence */
            /*
            #region Sinh moi nuoc di
            //sinh nuoc di tu vi tri hien tai
            //tim nuoc di
            moveable = new Move[200];
            dem = 0;
            for (i = 0; i < 9; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    int temp = brd2.getInfo(i, j);
                    if (temp != -1 && brd2.getColor(i, j) == color && temp % 10 != 0)
                    {
                        //quan minh
                        dem++;
                        quan19 quan_co = new quan19(color, temp % 10, i, j);
                        //kiem tra cac o cung hang
                        for (int k = 0; k < 9; k++)
                        {
                            if (k != i)
                            {
                                if (quan_co.capture(brd2, k, j))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, k, j, true, brd2.getInfo(k, j));
                                }

                                if (quan_co.move(brd2, k, j))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, k, j, false, -1);
                                }
                            }
                        }
                        //kiem tra cac o cung cot
                        for (int k = 0; k < 11; k++)
                        {
                            if (k != j)
                            {
                                if (quan_co.capture(brd2, i, k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i, k, true, brd2.getInfo(i, k));
                                }

                                if (quan_co.move(brd2, i, k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i, k, false, -1);
                                }
                            }
                        }
                        //kiem tra cac o cung duong cheo chinh
                        for (int k = -Math.Min(i, j); k <= Math.Min(8 - i, 10 - j); k++)
                        {
                            if (quan_co.capture(brd2, i + k, j + k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j + k, true, brd2.getInfo(i + k, j + k));
                            }

                            if (quan_co.move(brd2, i + k, j + k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j + k, false, -1);
                            }
                        }
                        //kiem tra cac o cung duong cheo phu
                        for (int k = Math.Max(-i, j - 10); k <= Math.Min(j, 8 - i); k++)
                        {
                            if (quan_co.capture(brd2, i + k, j - k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j - k, true, brd2.getInfo(i + k, j - k));
                            }

                            if (quan_co.move(brd2, i + k, j - k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j - k, false, -1);
                            }
                        }
                        if (dem >= 9) break;
                    }
                }
                //da tim duoc du 9 quan, khong can xet nua
                if (dem >= 9) break;
            }
             */
            #endregion

            //khong co nuoc an quan
            if (number == 0)
                return value;

            best = Int32.MinValue + 1;
            i = 1;
            //bat dau search
            while (i <= number)
            {
                alpha = Math.Max(best, alpha);
                if (MakeMove2(color, moveable[i], depth))
                    value = 100000 + 5000 * depth;
                else
                    value = -Quiescence2(3 - color, -beta, -alpha, depth - 1);
                UnMakeMove2(color, moveable[i], depth);
                if (value > best)
                {
                    best = value;
                    if (best >= beta)
                    {
                        HisTable2.AddCount(color, moveable[i]);
                        return best;
                    }
                }
                i++;
            }
            return best;
        }
        /*
        //******************************************************************
        //NEGASCOUT
        //Algorithm
        //link: (GT MTDF): //http://people.csail.mit.edu/plaat/mtdf.html 
        //pseu-do

        //      int NegaScout ( int p, alpha, beta );   
        //      {                        /* compute minimax value of position p */
        //          int a, b, t, i;

        //          determine successors p_1,...,p_w of p;
        //          if ( w = 0 )
        //          return ( Evaluate(p) );                      /* leaf node */
        //          a = alpha;
        //          b = beta;
        //          for ( i = 1; i <= w; i++ ) {
        //              t = -NegaScout ( p_i, -b, -a );
        //              if (t > a) && (t < beta) && (i > 1) && (d < maxdepth-1)
        //              a = -NegaScout ( p_i, -beta, -t );        /* re-search */
        //              a = max( a, t );
        //              if ( a >= beta ) 
        //                  return ( a );                               /* cut-off */
        //              b = a + 1;                         /* set new null window */
        //          }
        //          return ( a );
        //            }

        //Cai dat Negamax khong nho (no memory), cai tien bang Transposition Table luu ket qua
        //updated: Cai tien dung HistoryTable de sap xep danh sach nuoc di
        public int Negascout(int color, int alpha, int beta, int depth)
        {
            int best, i, value;
            Move[] moveable = new Move[200];
            int number = 0;

            //------------------------------------------------


            if (depth == 0)             //nut la
            {

                return Quiescence(color, alpha, beta,0);
                //return Eval.Evaluate(brd, color);
            }

            else
            {
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                if (color == 1)
                    foreach (state.chess c in st.QuanXanh)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i!=0)
                            if (st.move(brd, c, c.x + i, c.y - i))
                            {
                                number++;
                                moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                            }
                            else
                                if (st.capture(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                }

                        }
                    }
                else
                    foreach (state.chess c in st.QuanDo)
                    {
                        if (c.value <= 0) continue;
                        //kiem tra cac o cung hang
                        for (i = 0; i < 9; i++)
                            if (i != c.x)
                                if (st.move(brd, c, i, c.y))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, i, c.y, false, brd.getInfo(i, c.y));
                                }
                                else
                                    if (st.capture(brd, c, i, c.y))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, i, c.y, true, brd.getInfo(i, c.y));
                                    }

                        //kiem tra cac o cung cot
                        for (i = 0; i < 11; i++)
                            if (i != c.y)
                                if (st.move(brd, c, c.x, i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x, i, false, brd.getInfo(c.x, i));
                                }
                                else
                                    if (st.capture(brd, c, c.x, i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x, i, true, brd.getInfo(c.x, i));
                                    }
                        //kiem tra cac o cung duong cheo chinh
                        for (i = -Math.Min(c.x, c.y); i <= Math.Min(8 - c.x, 10 - c.y); i++)
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y + i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, false, brd.getInfo(c.x + i, c.y + i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y + i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y + i, true, brd.getInfo(c.x + i, c.y + i));
                                    }

                        //kiem tra cac o cung duong cheo phu
                        for (i = Math.Max(-c.x, c.y - 10); i <= Math.Min(c.y, 8 - c.x); i++)
                        {
                            if (i != 0)
                                if (st.move(brd, c, c.x + i, c.y - i))
                                {
                                    number++;
                                    moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, false, brd.getInfo(c.x + i, c.y - i));
                                }
                                else
                                    if (st.capture(brd, c, c.x + i, c.y - i))
                                    {
                                        number++;
                                        moveable[number] = new Move(c.x, c.y, c.x + i, c.y - i, true, brd.getInfo(c.x + i, c.y - i));
                                    }

                        }
                    }
                /*dem = 0;
                for (i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd.getInfo(i, j);
                        if (temp != -1 && brd.getColor(i, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, i, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != i)
                                {
                                    if (quan_co.capture(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, k, j, true, brd.getInfo(k, j));
                                    }
                                    else

                                    if (quan_co.move(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd, i, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, i, k, true, brd.getInfo(i, k));
                                    }
                                     else   
                                    if (quan_co.move(brd, i, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, i, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(i, j); k <= Math.Min(8 - i, 10 - j); k++)
                            {
                                if (quan_co.capture(brd, i + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i+k, j+k, true, brd.getInfo(i + k, j + k));
                                }
                                else
                                if (quan_co.move(brd, i + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-i, j - 10); k <= Math.Min(j, 8 - i); k++)
                            {
                                if (quan_co.capture(brd, i + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j - k, true, brd.getInfo(i + k, j - k));
                                }
                                else
                                if (quan_co.move(brd, i + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }*/

                //KET THUC TIM NUOC DI - Thu tu: cot, hang, duong cheo chinh, duong cheo phu
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;    //do re nhanh tai nut goc
                    
                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate
                //Sap xep lai mang moveable[1..number]
                //test cach sap xep don gian nhat (ko hieu qua lam) nhu sau:
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan
                //CAN TIM PP SAP XEP "TOT"
                //REUSE SHALLOWER SEARCH RESULT??? TRANSPOSITION TABLE!
                
                ///

                /*if (depth > MChess.MAX_PLY - 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                //*///

                //DUNG HISTORY TABLE DE SAP XEP LAI DANH SACH NUOC DI -------> DEPTH 5 POSSIBLE!
                HisTable.SortMoveList2(color, moveable, number);
                HisTable.SortMoveList(color, moveable, number);
                for( i= 1;i<number;i++) 
                if (moveable[i].value <moveable[i+1].value) Console.WriteLine(""); 
                //neu de best la Int32.MinValue thi se loi
                //vi sau buoc dau tien lat lai beta se la -(Int32.MinValue) = Int32.MinValue+1 (do tran so)
                //tuong tu nhu voi 8 bit bieu dien so co dau thi -(-128) = -127 (tran bit), -(-127) = 127
                best = Int32.MinValue + 1;
                int a, b;
                a = alpha;
                b = beta;

                i = 1;
                while (i <= number)
                {
                    if(MakeMove(color, moveable[i],depth ))
                        value = 100000 + 5000*depth ;
                    else
                        value = -Negascout(3 - color, -b, -a, depth - 1);

                    UnMakeMove(color, moveable[i],depth );

                    if ((value > a) && (value < beta) && (i > 1) && (depth < MChess.MAX_PLY - 1))
                    {
                        if (MakeMove(color, moveable[i],depth ))
                            value = 100000 + 5000*depth;
                        else
                            a = -Negascout(3 - color, -beta, -value, depth - 1);
                        UnMakeMove(color, moveable[i],depth );
                    }
                    if (value > a) a = value;
                    if (a < best) Console.Write("");
                    if (a > best)
                    {
                        best = a;
                        if (depth == MChess.MAX_PLY)
                            next_move = moveable[i];
                        //neu co the cat nhanh
                        if (a >= beta)
                        {
                            HisTable.AddCount(color, moveable[i]);
                            return a;
                        }
                    }
                    
                    b = a + 1;          //cua so moi
                    i++;
                }
                 //Console.WriteLine( "{0}", best);  
                return best;
            }
        }
        //END NEGASCOUT


        //***********************************************************************************/
        //                                   MTD(f) Algorithm                                //
        //***********************************************************************************//
        //MTDF Algorithm - Can phuong thuc AlphaBetaWithMemory(), transposition table
        //Tham khao tu:
        //http://people.csail.mit.edu/plaat/mtdf.html

        /*public int MTDF(int color, int f, int depth)
        {
            //Giai thuat
            //ALPHA_BETA_WITH_MEMORY

            //function AlphaBetaWithMemory(n : node_type; alpha , beta , d : integer) : integer;

            //if retrieve(n) == OK then /*Transposition table lookup */
            //if n.lowerbound >= beta then return n.lowerbound;
            //if n.upperbound <= alpha then return n.upperbound;
            //alpha := max(alpha, n.lowerbound);
            //beta := min(beta, n.upperbound);
            //if d == 0 then g := evaluate(n); /* leaf node */
            //else if n == MAXNODE then
            //g := -INFINITY; a := alpha; /* save original alpha value */
            //c := firstchild(n);
            //while (g < beta) and (c != NOCHILD) do
            //      g := max(g, AlphaBetaWithMemory(c, a, beta, d - 1));
            //      a := max(a, g);
            //      c := nextbrother(c);
            //else /* n is a MINNODE */
            //g := +INFINITY; b := beta; /* save original beta value */
            //c := firstchild(n);
            //while (g > alpha) and (c != NOCHILD) do
            //      g := min(g, AlphaBetaWithMemory(c, alpha, b, d - 1));
            //      b := min(b, g);
            //      c := nextbrother(c);
            ///* Traditional transposition table storing of bounds */
            ///* Fail low result implies an upper bound */
            //if g <= alpha then n.upperbound := g; store n.upperbound;
            ///* Found an accurate minimax value - will not occur if called with zero window */
            //if g >  alpha and g < beta then
            //n.lowerbound := g; n.upperbound := g; store n.lowerbound, n.upperbound;
            ///* Fail high result implies a lower bound */
            //if g >= beta then n.lowerbound := g; store n.lowerbound;
            //return g; 
            //

            //THUAT GIAI MTD(f)
            //function MTDF(root : node_type; f : integer; d : integer) : integer;

            //g := f;
            //upperbound := +INFINITY;
            //lowerbound := -INFINITY;
            //repeat
            //    if g == lowerbound then beta := g + 1 else beta := g;
            //    g := AlphaBetaWithMemory(root, beta - 1, beta, d);
            //    if g < beta then upperbound := g else lowerbound := g;
            //until lowerbound >= upperbound;
            //return g;

                /*int g = f;
                int beta;
                int upperbound = Int32.MaxValue;
                int lowerbound = Int32.MinValue + 1;
                while (lowerbound < upperbound)
                {
                    if (g == lowerbound)
                        beta = g+1;
                    else
                        beta = g;
                    g = alpha_beta2(color,beta - 1, beta, depth);
                    if(g < beta)
                        upperbound = g;
                    else
                        lowerbound = g;
                }
                return g;
            


            //function iterative_deepening(root : node_type) : integer;
            //firstguess := 0;
            //for d = 1 to MAX_SEARCH_DEPTH do
            //firstguess := MTDF(root, firstguess, d);
            //if times_up() then break;
            //return firstguess;

            //
        }

        int iterative_deepening(int color)
        {
            int firstguess = 0;
            for (int d = 1; d <= MChess.MAX_PLY; d++)
                firstguess = MTDF(color, firstguess, d);
            return firstguess;
        }

        //                                      END MTD(f)
        //************************************************************************************///

        //with memory! Cai dat nay khac voi alpha_beta mot chut o cho co phan biet nguoi di max, nguoi di min

        /*public int alpha_beta2(int color, int alpha, int beta, int depth)
        {
            int best, i, value;
            bool capture;
            int val;
            int dem;
            int temp = 0;
            int temp_eval, temp_depth, temp_type;

            Move[] moveable = new Move[200];
            int number = 0;


            //thu tim trong Hash Table
            if (TransTable.LookUp(brd, out temp_eval, out temp_depth, out temp_type) && temp_depth >= depth)
            {
                if (temp_eval >= beta)
                {
                    TTHitCount++;
                    return temp_eval;
                }
                alpha = Math.Max(alpha, temp_eval);
            }

            if (depth == 0)
            {
                counter++;
                return Quiescence2(color, alpha, beta);
            }
            else
            {
                #region Sinh nuoc di
                //sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                dem = 0;
                for (i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd.getInfo(i, j);
                        if (temp != -1 && brd.getColor(i, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, i, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != i)
                                {
                                    if (quan_co.capture(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, k, j, true, brd.getInfo(k, j));
                                    }

                                    if (quan_co.move(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd, i, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, i, k, true, brd.getInfo(i, k));
                                    }

                                    if (quan_co.move(brd, i, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, i, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(i, j); k <= Math.Min(8 - i, 10 - j); k++)
                            {
                                if (quan_co.capture(brd, i + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j + k, true, brd.getInfo(i + k, j + k));
                                }

                                if (quan_co.move(brd, i + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-i, j - 10); k <= Math.Min(j, 8 - i); k++)
                            {
                                if (quan_co.capture(brd, i + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j - k, true, brd.getInfo(i + k, j - k));
                                }

                                if (quan_co.move(brd, i + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;
                    tong_so_nuoc_di += so_nuoc_di;
                }

 
                HisTable.SortMoveList(color, moveable, number);
            
                best = Int32.MinValue + 1;
                //Algorithm
                //while(con lay duoc nuoc di) and (best < beta) do
                //begin
                //  if(best > alpha) then alpha:=best;
                //  thuc hien nuoc di;
                //  value:=-alpha_beta(-beta,-alpha, depth-1);
                //  bo thuc hien nuoc di;
                //  if(value>best then best:=value;
                //end;
                //alpha_beta:=best;

                //chu y dao bien color thanh 3-color cho phep goi de quy (chuyen tu muc Min <-> Max)
                i = 1;
                while (i <= number)
                {
                    if (best > alpha) alpha = best;
                    if (MakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, out capture, out val))
                        value = 1000 + depth - MChess.MAX_PLY;
                    else
                        value = -alpha_beta2(3 - color, -beta, -alpha, depth - 1);
                    UnMakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, capture, val);

                    if (value > best)
                    {
                        best = value;
                        //neu ply=0 thi luu lai nuoc di
                        if (depth == MChess.MAX_PLY)
                        {
                            next_move = moveable[i];
                        }
                        //neu co the cat nhanh
                        if (best >= beta)
                        {
                            TransTable.Store(brd, best, 1, depth, MChess.dem + 1);
                            HisTable.AddCount(color, moveable[i]);
                            return best;
                        }
                    }
                    i++;
                }
                TransTable.Store(brd, best, 0, depth, MChess.dem + 1);
                return best;
            }
        }

        public int Negascout2(int color, int alpha, int beta, int depth)
        {
            int best, i, value;
            bool capture;
            int val;
            int dem;
            Move[] moveable = new Move[200];
            int number = 0;
            int temp = 0;
            int temp_eval, temp_type, temp_depth;
            //------------------------------------------------

            //thu tim trong Hash Table
            if (TransTable.LookUp(brd, out temp_eval, out temp_depth, out temp_type) && temp_depth >= depth)
            {
                if (temp_eval >= beta)
                {
                    TTHitCount++;
                    return temp_eval;
                }
                alpha = Math.Max(alpha, temp_eval);
            }


            if (depth == 0)             //nut la
            {
                counter++;
                return Quiescence2(color,alpha,beta);
            }

            else
            {
                #region sinh nuoc di tu vi tri hien tai
                //tim nuoc di
                //CAI THIEN??
                dem = 0;
                for (i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        temp = brd.getInfo(i, j);
                        if (temp != -1 && brd.getColor(i, j) == color && temp % 10 != 0)
                        {
                            //quan minh
                            dem++;
                            quan19 quan_co = new quan19(color, temp % 10, i, j);
                            //kiem tra cac o cung hang
                            for (int k = 0; k < 9; k++)
                            {
                                if (k != i)
                                {
                                    if (quan_co.capture(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, k, j, true, brd.getInfo(k, j));
                                    }

                                    if (quan_co.move(brd, k, j))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, k, j, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung cot
                            for (int k = 0; k < 11; k++)
                            {
                                if (k != j)
                                {
                                    if (quan_co.capture(brd, i, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, i, k, true, brd.getInfo(i, k));
                                    }

                                    if (quan_co.move(brd, i, k))
                                    {
                                        number++;
                                        moveable[number] = new Move(i, j, i, k, false, -1);
                                    }
                                }
                            }
                            //kiem tra cac o cung duong cheo chinh
                            for (int k = -Math.Min(i, j); k <= Math.Min(8 - i, 10 - j); k++)
                            {
                                if (quan_co.capture(brd, i + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j + k, true, brd.getInfo(i + k, j + k));
                                }

                                if (quan_co.move(brd, i + k, j + k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j + k, false, -1);
                                }
                            }
                            //kiem tra cac o cung duong cheo phu
                            for (int k = Math.Max(-i, j - 10); k <= Math.Min(j, 8 - i); k++)
                            {
                                if (quan_co.capture(brd, i + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j - k, true, brd.getInfo(i + k, j - k));
                                }

                                if (quan_co.move(brd, i + k, j - k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i + k, j - k, false, -1);
                                }
                            }
                            if (dem >= 9) break;
                        }
                    }
                    //da tim duoc du 9 quan, khong can xet nua
                    if (dem >= 9) break;
                }

                //KET THUC TIM NUOC DI - Thu tu: cot, hang, duong cheo chinh, duong cheo phu
                #endregion

                if (depth == MChess.MAX_PLY)
                {
                    so_nuoc_di = number;    //do re nhanh tai nut goc
                    tong_so_nuoc_di += so_nuoc_di;
                }

                //Sap xep lai cac nuoc di vua tim duoc dua tren ham Evaluate
                //Sap xep lai mang moveable[1..number]
                //test cach sap xep don gian nhat (ko hieu qua lam) nhu sau:
                //neu o muc max, sap xep giam dan, neu o muc min, sap xep tang dan
                //CAN TIM PP SAP XEP "TOT"
                //REUSE SHALLOWER SEARCH RESULT??? TRANSPOSITION TABLE!
                /*
                if (depth > MChess.MAX_PLY - 2)
                {
                    int[] TT = new int[number + 1];
                    for (i = 1; i <= number; i++)
                        TT[i] = Evaluate_Move(color, moveable[i]);
                    TT[0] = Int32.MinValue;
                    Array.Sort(TT, moveable);
                    if ((MChess.MAX_PLY - depth) % 2 == 0)
                        Array.Reverse(moveable, 1, number);
                }
                //*/

                /*HisTable.SortMoveList(color, moveable, number);

                //neu de best la Int32.MinValue thi se loi
                //vi sau buoc dau tien lat lai beta se la -(Int32.MinValue) = Int32.MinValue+1 (do tran so)
                //tuong tu nhu voi 8 bit bieu dien so co dau thi -(-128) = -127 (tran bit), -(-127) = 127
                best = Int32.MinValue + 1;
                int a, b;
                a = alpha;
                b = beta;

                i = 1;
                while ((i <= number))
                {
                    if (MakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, out capture, out val))
                        value = 1000 + depth - MChess.MAX_PLY;
                    else
                        value = -Negascout2(3 - color, -b, -a, depth - 1);

                    UnMakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, capture, val);

                    if ((value > a) && (value < beta) && (i > 1) && (depth < MChess.MAX_PLY - 1))
                    {
                        if (MakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, out capture, out val))
                            value = 1000 + depth - MChess.MAX_PLY;
                        else
                            a = -Negascout2(3 - color, -beta, -value, depth - 1);
                        UnMakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, capture, val);
                    }
                    if (value > a) a = value;

                    if (a > best)
                    {
                        best = a;
                        if (depth == MChess.MAX_PLY)
                            next_move = moveable[i];
                        if (a >= beta)
                        {
                            TransTable.Store(brd, a, 1, depth,MChess.dem+1);
                            HisTable.AddCount(color, moveable[i]);
                            return best;
                        }
                    }

                    b = a + 1;          //cua so moi
                    i++;
                }

                TransTable.Store(brd, best, 0, depth, MChess.dem + 1);
                return best;
            }
        }
        //END NEGASCOUT2

        public int Quiescence2(int color, int alpha, int beta)
        {
            int value, best, i;
            Move[] moveable;
            int number = 0;
            int dem;
            bool capture;
            int val;

            int temp_eval, temp_depth, temp_type;

            QuiescenceNode++;

            //kiem tra trong Transposition Table
            if (TransTable.LookUp(brd, out temp_eval, out temp_depth, out temp_type))
            {
                if (temp_eval >= beta)
                {
                    TTHitCount++;
                    return temp_eval;
                }
                alpha = Math.Max(alpha, temp_eval);
            }

            //value = Eval.MaterialBalance(brd, color);
            //null-move pruning

            value = Eval.Evaluate(brd, color);
            if (value >= beta) return value;

            alpha = Math.Max(value, alpha);


            #region sinh nuoc di an quan
            //sinh cac nuoc di co the an duoc quan (capture)
            moveable = new Move[100];
            dem = 0;
            for (i = 0; i < 9; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    int temp = brd.getInfo(i, j);
                    if (temp != -1 && brd.getColor(i, j) == color && temp % 10 != 0)
                    {
                        //quan minh
                        dem++;
                        quan19 quan_co = new quan19(color, temp % 10, i, j);
                        //kiem tra cac o cung hang
                        for (int k = 0; k < 9; k++)
                        {
                            if (k != i)
                            {
                                if (quan_co.capture(brd, k, j))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, k, j, true, brd.getInfo(k, j));
                                }
                            }
                        }
                        //kiem tra cac o cung cot
                        for (int k = 0; k < 11; k++)
                        {
                            if (k != j)
                            {
                                if (quan_co.capture(brd, i, k))
                                {
                                    number++;
                                    moveable[number] = new Move(i, j, i, k, true, brd.getInfo(i, k));
                                }
                            }
                        }
                        //kiem tra cac o cung duong cheo chinh
                        for (int k = -Math.Min(i, j); k <= Math.Min(8 - i, 10 - j); k++)
                        {
                            if (quan_co.capture(brd, i + k, j + k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j + k, true, brd.getInfo(i + k, j + k));
                            }

                        }
                        //kiem tra cac o cung duong cheo phu
                        for (int k = Math.Max(-i, j - 10); k <= Math.Min(j, 8 - i); k++)
                        {
                            if (quan_co.capture(brd, i + k, j - k))
                            {
                                number++;
                                moveable[number] = new Move(i, j, i + k, j - k, true, brd.getInfo(i + k, j - k));
                            }

                        }
                        if (dem >= 9) break;
                    }
                }
                //da tim duoc du 9 quan, khong can xet cac o khac nua
                if (dem >= 9) break;
            }
            #endregion

            //khong co nuoc an quan
            if (number == 0)
                return value;

            best = Int32.MinValue + 1;
            i = 1;
            //bat dau search
            while (i <= number)
            {
                alpha = Math.Max(best, alpha);
                if (MakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, out capture, out val))
                    value = 1000 - MChess.MAX_PLY;
                else
                    value = -Quiescence2(3 - color, -beta, -alpha);
                UnMakeMove(color, moveable[i].x1, moveable[i].y1, moveable[i].x2, moveable[i].y2, capture, val);
                if (value > best)
                {
                    best = value;
                    if (best >= beta)
                    {
                        TransTable.Store(brd, best, 1, 0, MChess.dem + 1);
                        HisTable.AddCount(color, moveable[i]);
                        return best;
                    }
                }
                i++;
            }
            TransTable.Store(brd, best, 0, 0, MChess.dem + 1);
            return best;
        }

        public void PickMove(int color)
        {
 
        }*/
    
    }
}