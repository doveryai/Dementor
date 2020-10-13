using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dementor
{
    //from https://www.codeproject.com/Articles/23619/LoginHours-from-DirectoryEntry-as-a-boolean-array
    public class LoginHours
    {
        #region Fields

        private int _OffsetAmount;
        private BitArray _BitContainer;

        #endregion

        #region Properties

        public BitArray BitContainer
        {
            get { return _BitContainer; }
            set { _BitContainer = value; }
        }

        public int OffsetAmount
        {
            get { return _OffsetAmount; }
            set { _OffsetAmount = value; }
        }

        #endregion

        #region Contructors

        public LoginHours(BitArray bitcontainer)
        {
            _BitContainer = bitcontainer;
        }

        public LoginHours(bool[] boolvalues)
        {
            _BitContainer = new BitArray(boolvalues);
        }

        public LoginHours(byte[] bytevalues)
        {
            _BitContainer = new BitArray(bytevalues);
        }

        public LoginHours(int length)
        {
            _BitContainer = new BitArray(length);
        }

        public LoginHours(int[] intvalues)
        {
            _BitContainer = new BitArray(intvalues);
        }

        public LoginHours(BitArray bitcontainer, int Offsetamount)
        {
            _BitContainer = bitcontainer;
            this.Offset(Offsetamount);
        }

        public LoginHours(bool[] boolvalues, int Offsetamount)
        {
            _BitContainer = new BitArray(boolvalues);
            this.Offset(Offsetamount);
        }

        public LoginHours(byte[] bytevalues, int Offsetamount)
        {
            _BitContainer = new BitArray(bytevalues);
            this.Offset(Offsetamount);
        }

        public LoginHours(int length, int Offsetamount)
        {
            _BitContainer = new BitArray(length);
            this.Offset(Offsetamount);
        }

        public LoginHours(int[] intvalues, int Offsetamount)
        {
            _BitContainer = new BitArray(intvalues);
            this.Offset(Offsetamount);
        }

        #endregion

        #region Public Methods

        public void Offset()
        {
            _BitContainer = BitArrayOffset(_BitContainer, _OffsetAmount);
        }
        public void Offset(int amount)
        {
            _OffsetAmount = amount;
            _BitContainer = BitArrayOffset(_BitContainer, _OffsetAmount);
        }

        public void OffsetLeft()
        {
            this.Offset(-1);
        }

        public void OffsetRight()
        {
            this.Offset(1);
        }

        public void OffsetReverse()
        {
            this.Offset((-1 * _OffsetAmount));
        }

        public bool[] ToBooleanArray()
        {
            bool[] rtnboolarr = new bool[_BitContainer.Count];
            for (int b = 0; b < _BitContainer.Count; b++)
                rtnboolarr[b] = _BitContainer[b];
            return rtnboolarr;
        }
        public bool[] ToBooleanArray(int startpos, int lenght)
        {
            bool[] rtnboolarr = new bool[lenght];
            for (int b = startpos; b < (startpos + lenght); b++)
                rtnboolarr[b - startpos] = _BitContainer[b];
            return rtnboolarr;
        }
        public bool[] ToBooleanArray(DayOfWeek dayofweek)
        {
            int startpos = (((int)dayofweek) * 24);
            bool[] rtnboolarr = new bool[24];
            for (int b = startpos; b < (startpos + 24); b++)
                rtnboolarr[b - startpos] = _BitContainer[b];
            return rtnboolarr;
        }

        public byte[] ToByteArray()
        {
            int toplevel = _BitContainer.Count / 8;
            byte[] rtnbytearr = new byte[toplevel];
            bool[] conbin;
            int mult = 0;
            for (int i = 0; i < toplevel; i++)
            {
                conbin = new bool[8];
                for (int b = 0; b < 8; b++)
                    conbin[b] = _BitContainer.Get(b + mult);
                rtnbytearr[i] = BinToByte(conbin);
                mult += 8;
            }
            return rtnbytearr;
        }
        public byte[] ToByteArray(DayOfWeek dayofweek)
        {
            int startpos = ((int)dayofweek) * 24;
            byte[] rtnbytearr = new byte[3];
            for (int i = 0; i < 3; i++)
                rtnbytearr[i] = BinToByte(ToBooleanArray((startpos + (i * 8)), 8));
            return rtnbytearr;

        }

        public int[] ToInt32Array()
        {
            int toplevel = _BitContainer.Count / 32;
            int[] rtnbytearr = new int[toplevel];
            bool[] conbin;
            int mult = 0;
            for (int i = 0; i < toplevel; i++)
            {
                conbin = new bool[32];
                for (int b = 0; b < 32; b++)
                    conbin[b] = _BitContainer.Get(b + mult);
                rtnbytearr[i] = BinToByte(conbin);
                mult += 32;
            }
            return rtnbytearr;
        }

        #endregion

        #region Internal Methods

        internal BitArray BitArrayOffset(BitArray ArrayToOffsetFrom, int OffsetValue)
        {
            if (OffsetValue == 0)
                return ArrayToOffsetFrom;
            BitArray rtnbitArr = new BitArray(ArrayToOffsetFrom.Count);
            int Offset = 0;
            for (int i = 0; i < ArrayToOffsetFrom.Count; i++)
            {
                Offset = i + OffsetValue;
                if (Offset > ArrayToOffsetFrom.Count - 1)
                    Offset = Offset - ArrayToOffsetFrom.Count;
                if (Offset < 0)
                    Offset = ArrayToOffsetFrom.Count + Offset;
                rtnbitArr.Set(Offset, ArrayToOffsetFrom[i]);
            }
            return rtnbitArr;
        }

        internal byte BinToByte(bool[] boolvals)
        {
            int multiplier = 1;
            int rtnval = 0;

            for (int i = 0; i < boolvals.Length; i++)
            {
                rtnval = rtnval + (Convert.ToInt16(boolvals[i]) * multiplier);
                multiplier = multiplier * 2;
            }
            return (byte)rtnval;
        }

        #endregion
    }
}
